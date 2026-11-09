using UnityEngine;

/// <summary>
/// Guard AI state machine: Patrolling, Investigating, Suspicious, or Alert.
/// Integrates vision cone checking, acoustic hearing (footsteps & whistle), stance modifiers from PlayerMove,
/// search investigation, and suspicion escalation.
/// </summary>
public class PatrolMover : MonoBehaviour
{
    public enum GuardState { Patrolling, Investigating, Suspicious, Alert }

    [Header("Patrol Waypoints")]
    public Vector3 pointA = new Vector3(-5f, 1f, -4f);
    public Vector3 pointB = new Vector3(-5f, 1f, 4f);
    public float speed = 3f;
    public float investigateSpeed = 3.5f;
    public float chaseSpeed = 4.5f;

    [Header("Vision & Detection")]
    public float viewDistance = 12f;
    public float baseViewDistance = 12f;
    public float viewAngle = 70f; // Half-angle FOV
    public bool isFlashlightActive = false;
    public bool hasDenseFog = true;
    public float fogExtinctionDistanceReduction = 0.166f; // Reduces unassisted view distance from 12m to 10m
    public float flashlightSweepAngle = 45f;
    public float flashlightSweepSpeed = 2.5f;
    public float flashlightRange = 10f;
    public float flashlightAngle = 50f;
    public float searchlightAlertMultiplier = 2.5f;
    public float silhouetteDetectionMultiplier = 1.8f;
    public float glareBlindnessDistanceReduction = 0.30f; // -30% view distance when looking into light source
    public Transform targetPlayer;
    public GuardState currentState = GuardState.Patrolling;
    [Range(0f, 1f)]
    public float suspicionMeter = 0f;
    public float suspicionRate = 0.8f;   // Rate per second when player in line-of-sight
    public float cooldownRate = 0.35f;   // Decay per second when sight lost

    [Header("Acoustic / Hearing Settings")]
    public float hearingMultiplier = 1.0f;
    public float searchWaitTime = 4.0f;
    [Tooltip("Acoustic transmission factor through solid walls and concrete obstacles (0.35 = 65% dampening).")]
    public float wallSoundOcclusionFactor = 0.35f;
    public float radioBroadcastRadius = 25.0f;
    public float tacticalFlankOffset = 0f; // Lateral fanning offset for pincer convergence
    public int investigationStage = 0; // 0 = primary origin, 1 = secondary probe point
    private Vector3 secondarySearchPos;
    private float searchTimer = 0f;
    private float alertBroadcastCooldown = 0f;
    private float searchlightSignalCooldown = 0f;

    [Header("Performance & Sensor Budgeting (Intel HD 520 / Potato Target)")]
    public float sensorTickInterval = 0.066f; // ~15Hz sensor update when patrolling far
    public float spatialSleepDistance = 35.0f; // Beyond 35m, drop to 2Hz sensory sleep mode
    public float sleepTickInterval = 0.50f;   // ~2Hz update when distant
    public float dynamicLoadDecimationMultiplier = 1.0f; // Multiplier when frame time > 33.3ms
    private float sensorTickTimer = 0f;
    private bool cachedPlayerSeen = false;
    private bool cachedSoundHeard = false;
    private bool cachedDistractionHeard = false;
    private Vector3 cachedDistractionPos = Vector3.zero;

    private float t;
    private int dir = 1;
    private Vector3 lastKnownPlayerPos;

    // Static preallocated cache to eliminate per-frame GC allocations
    private static LightExposureZone[] cachedLightZones;
    private static float lastLightZoneCacheTime = -999f;
    private static PatrolMover[] cachedGuards;
    private static float lastGuardCacheTime = -999f;

    private static void UpdateCachedLightZones()
    {
        // Decoupled time-stamp invalidation: refreshes exactly once per second regardless of guard count
        if (cachedLightZones == null || (Time.unscaledTime - lastLightZoneCacheTime) >= 1.0f)
        {
            lastLightZoneCacheTime = Time.unscaledTime;
            cachedLightZones = FindObjectsByType<LightExposureZone>(FindObjectsSortMode.None);
        }
    }

    private static void UpdateCachedGuards()
    {
        // Decoupled time-stamp invalidation: refreshes exactly once per second regardless of guard count
        if (cachedGuards == null || (Time.unscaledTime - lastGuardCacheTime) >= 1.0f)
        {
            lastGuardCacheTime = Time.unscaledTime;
            cachedGuards = FindObjectsByType<PatrolMover>(FindObjectsSortMode.None);
        }
    }

    void Awake()
    {
        if (targetPlayer == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) targetPlayer = p.transform;
            else
            {
                var pm = FindFirstObjectByType<PlayerMove>();
                if (pm != null) targetPlayer = pm.transform;
            }
        }
    }

    void Update()
    {
        // Dynamic load decimation: if frame rate drops below 30fps (dt > 33.3ms), scale intervals
        dynamicLoadDecimationMultiplier = (Time.deltaTime > 0.0333f) ? 1.5f : 1.0f;

        // Check if low-spec dynamic quality adaptation is active on survival system
        var survival = targetPlayer != null ? targetPlayer.GetComponent<SurvivalSystem>() : null;
        bool lowSpecActive = (survival != null && survival.isLowSpecSafeguardActive);

        // Adaptive sensor frequency: 60Hz when active / suspicious, 15Hz when cruising on patrol, 2Hz if distant (>35m, or >25m in low-spec mode)
        float effectiveSleepDist = lowSpecActive ? (spatialSleepDistance * 0.714f) : spatialSleepDistance; // 25.0m threshold
        float distToPlayer = targetPlayer != null ? Vector3.Distance(transform.position, targetPlayer.position) : 999f;
        bool isDistant = distToPlayer > effectiveSleepDist && currentState == GuardState.Patrolling && suspicionMeter <= 0.05f;

        if (lowSpecActive)
        {
            dynamicLoadDecimationMultiplier = 1.5f; // Automatically engage sensor decimation safeguard
        }
        else if (Time.deltaTime <= 0.0333f)
        {
            dynamicLoadDecimationMultiplier = 1.0f;
        }

        float currentSensorInterval = 0f;
        if (isDistant)
        {
            currentSensorInterval = sleepTickInterval * dynamicLoadDecimationMultiplier; // 2Hz low-power sensory sleep
        }
        else if (currentState == GuardState.Patrolling && suspicionMeter <= 0.1f)
        {
            currentSensorInterval = sensorTickInterval * dynamicLoadDecimationMultiplier; // 15Hz cruising
        }

        sensorTickTimer += Time.deltaTime;

        if (sensorTickTimer >= currentSensorInterval)
        {
            sensorTickTimer = 0f;
            cachedPlayerSeen = isDistant ? false : CheckVision();
            cachedSoundHeard = isDistant ? false : CheckHearing();
            cachedDistractionHeard = CheckDistractionEvents(out cachedDistractionPos);
        }

        bool playerSeen = cachedPlayerSeen;
        bool playerSoundHeard = cachedSoundHeard;
        bool distractionHeard = cachedDistractionHeard;
        Vector3 distractionPos = cachedDistractionPos;

        // Update suspicion meter based on vision check and hearing
        if (playerSeen)
        {
            // Detection rate scaled by player stance and ambient light exposure
            float stanceModifier = 1.0f;
            float lightExposure = 0.25f; // Baseline ambient alley exposure

            if (targetPlayer != null)
            {
                var pm = targetPlayer.GetComponent<PlayerMove>();
                if (pm != null)
                {
                    switch (pm.currentStance)
                    {
                        case PlayerMove.Stance.Prone: stanceModifier = 0.25f; break;
                        case PlayerMove.Stance.Crouch: stanceModifier = 0.60f; break;
                        case PlayerMove.Stance.Walk: stanceModifier = 1.00f; break;
                        case PlayerMove.Stance.Sprint: stanceModifier = 1.50f; break;
                    }
                }

                if (survival != null)
                {
                    lightExposure = survival.currentLightExposure;
                }
            }

            float lightFactor = Mathf.Lerp(0.3f, 1.8f, lightExposure);
            if (lightExposure >= 0.75f)
            {
                // Spotlight glare alert: high-visibility searchlight direct exposure
                lightFactor *= searchlightAlertMultiplier;
            }

            // Flashlight direct illumination synergy: if flashlight is active and aiming within cone of player
            if (isFlashlightActive && targetPlayer != null)
            {
                Vector3 toPlayer = (targetPlayer.position - transform.position);
                toPlayer.y = 0;
                if (toPlayer.magnitude <= flashlightRange && Vector3.Angle(transform.forward, toPlayer) <= (flashlightAngle * 0.5f))
                {
                    lightFactor *= 1.50f;
                }
            }

            // Silhouette detection: if player is back-lit by an intense light source behind them
            if (CheckSilhouetteBacklit())
            {
                stanceModifier = Mathf.Max(stanceModifier, 0.75f); // Negates prone/crouch invisibility in silhouette
                lightFactor *= silhouetteDetectionMultiplier;
            }

            float detectionMultiplier = stanceModifier * lightFactor;
            suspicionMeter = Mathf.Clamp01(suspicionMeter + (suspicionRate * detectionMultiplier) * Time.deltaTime);
            if (targetPlayer != null) lastKnownPlayerPos = targetPlayer.position;

            if (suspicionMeter >= 0.8f)
            {
                if (currentState != GuardState.Alert)
                {
                    // Synthesize guard alert vocal shout
                    try
                    {
                        AudioClip shout = ProceduralAudio.CreateGuardAlertShoutClip();
                        ProceduralAudio.PlayClipAtPoint(shout, transform.position, 0.9f);
                    }
                    catch (System.Exception) {}
                }
                currentState = GuardState.Alert;
                BroadcastRadioAlert();
                SignalNearbySearchlights(lastKnownPlayerPos, 5.0f);
            }
            else if (suspicionMeter >= 0.25f)
            {
                currentState = GuardState.Suspicious;
                SignalNearbySearchlights(lastKnownPlayerPos, 3.5f);
            }
        }
        else if (distractionHeard)
        {
            // Hearing an environmental sound distraction (e.g. shattered bottle) pulls guard away
            // to investigate the distraction point without revealing player's true position
            lastKnownPlayerPos = distractionPos;
            suspicionMeter = Mathf.Clamp01(suspicionMeter + 0.40f * Time.deltaTime);

            if (currentState == GuardState.Patrolling || currentState == GuardState.Suspicious)
            {
                currentState = GuardState.Investigating;
                searchTimer = searchWaitTime;
            }
        }
        else if (playerSoundHeard)
        {
            // Hearing player sound immediately raises interest to Investigating or Suspicious.
            // Sharp glass shard crunching alerts guards significantly faster than muffled dirt/concrete thuds.
            float soundSuspicionRate = 0.35f;
            if (targetPlayer != null)
            {
                var pm = targetPlayer.GetComponent<PlayerMove>();
                if (pm != null && pm.currentSurface == PlayerMove.SurfaceType.GlassDebris)
                {
                    soundSuspicionRate = 0.50f; // 1.43x sharper suspicion reaction to shattering glass shards
                }
            }

            suspicionMeter = Mathf.Clamp01(suspicionMeter + soundSuspicionRate * Time.deltaTime);
            if (targetPlayer != null) lastKnownPlayerPos = targetPlayer.position;

            if (currentState == GuardState.Patrolling)
            {
                currentState = GuardState.Investigating;
                searchTimer = searchWaitTime;
            }
        }
        else
        {
            suspicionMeter = Mathf.Clamp01(suspicionMeter - cooldownRate * Time.deltaTime);
            if (suspicionMeter < 0.15f && currentState != GuardState.Patrolling)
            {
                if (currentState == GuardState.Investigating)
                {
                    searchTimer -= Time.deltaTime;
                    if (searchTimer <= 0f)
                    {
                        currentState = GuardState.Patrolling;
                    }
                }
                else
                {
                    currentState = GuardState.Patrolling;
                }
            }
            else if (suspicionMeter < 0.75f && currentState == GuardState.Alert)
            {
                currentState = GuardState.Suspicious;
            }
        }

        if (alertBroadcastCooldown > 0f)
        {
            alertBroadcastCooldown -= Time.deltaTime;
        }
        if (searchlightSignalCooldown > 0f)
        {
            searchlightSignalCooldown -= Time.deltaTime;
        }

        // Execute behavior based on state
        isFlashlightActive = (currentState == GuardState.Investigating || currentState == GuardState.Suspicious || currentState == GuardState.Alert);

        switch (currentState)
        {
            case GuardState.Patrolling:
                // If a nearby sweeping searchlight illuminates an active target or lingers, guard peeks towards illuminated zone
                Vector3 searchlightHotspot;
                if (CheckNearbySearchlightBeam(out searchlightHotspot))
                {
                    Vector3 glanceDir = (searchlightHotspot - transform.position);
                    glanceDir.y = 0;
                    if (glanceDir != Vector3.zero)
                    {
                        Quaternion glanceRot = Quaternion.LookRotation(glanceDir);
                        transform.rotation = Quaternion.Slerp(transform.rotation, glanceRot, Time.deltaTime * 2.0f);
                    }
                }
                PatrolMovement();
                break;

            case GuardState.Investigating:
                // Multi-stage investigation: primary origin probe then secondary vantage point probe
                Vector3 currentTarget = (investigationStage == 0) ? lastKnownPlayerPos : secondarySearchPos;
                Vector3 investDir = (currentTarget - transform.position);
                investDir.y = 0;

                // Searchlight-guard intersect synergy: if searchlight beam sweeps within 5.0m of search target, accelerate tempo by 1.35x
                float effectiveInvestigateSpeed = investigateSpeed;
                Vector3 searchlightHotspotPos;
                if (CheckNearbySearchlightBeam(out searchlightHotspotPos) && Vector3.Distance(currentTarget, searchlightHotspotPos) <= 5.0f)
                {
                    effectiveInvestigateSpeed *= 1.35f;
                }

                if (investDir.magnitude > 1.2f)
                {
                    Quaternion baseRot = Quaternion.LookRotation(investDir.normalized);
                    float sweep = Mathf.Sin(Time.time * flashlightSweepSpeed) * (flashlightSweepAngle * 0.4f);
                    Quaternion sweepRot = baseRot * Quaternion.Euler(0, sweep, 0);
                    transform.rotation = Quaternion.Slerp(transform.rotation, sweepRot, Time.deltaTime * 4f);
                    transform.position += transform.forward * effectiveInvestigateSpeed * Time.deltaTime;
                }
                else
                {
                    // Arrived at primary waypoint: probe secondary vantage point
                    if (investigationStage == 0)
                    {
                        investigationStage = 1;
                        secondarySearchPos = lastKnownPlayerPos + transform.right * 3.5f;
                    }
                    else
                    {
                        // Arrived at secondary search origin: execute wide sweeping look-around
                        float wideSweep = Mathf.Sin(Time.time * flashlightSweepSpeed) * flashlightSweepAngle;
                        transform.Rotate(Vector3.up, (35f + wideSweep * 0.5f) * Time.deltaTime);
                    }
                }
                break;

            case GuardState.Suspicious:
                // Look toward last known position with alert search sweep
                Vector3 lookDir = (lastKnownPlayerPos - transform.position);
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                {
                    Quaternion targetRot = Quaternion.LookRotation(lookDir);
                    float sweep = Mathf.Sin(Time.time * flashlightSweepSpeed * 1.5f) * (flashlightSweepAngle * 0.5f);
                    Quaternion sweepRot = targetRot * Quaternion.Euler(0, sweep, 0);
                    transform.rotation = Quaternion.Slerp(transform.rotation, sweepRot, Time.deltaTime * 3.5f);
                }
                break;

            case GuardState.Alert:
                // Move rapidly towards last known player position with beam locked forward
                Vector3 targetDir = (lastKnownPlayerPos - transform.position);
                targetDir.y = 0;
                if (targetDir.magnitude > 1f)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDir), Time.deltaTime * 5f);
                    transform.position += transform.forward * chaseSpeed * Time.deltaTime;
                }
                break;
        }
    }

    private bool CheckHearing()
    {
        if (targetPlayer == null) return false;

        var pm = targetPlayer.GetComponent<PlayerMove>();
        if (pm == null) return false;

        float baseNoise = pm.CurrentNoiseRadius * hearingMultiplier;
        if (baseNoise <= 0f) return false;

        float dist = Vector3.Distance(transform.position, targetPlayer.position);
        if (dist > baseNoise) return false;

        // Environmental wind masking: howling curfew wind gusts mask subtle footstep acoustics (-25% hearing range)
        // while loud events (whistles, bottle shatters) maintain high penetration
        float windMaskingFactor = 1.0f;
        var survival = targetPlayer.GetComponent<SurvivalSystem>();
        if (survival != null && !survival.isSheltered && !pm.IsWhistling)
        {
            // When outdoor wind volume is elevated, acoustic footprint of footsteps is partially masked
            windMaskingFactor = 0.75f;
        }

        // Acoustic ray occlusion check: if solid obstacle intervenes, attenuate effective hearing radius
        float effectiveRadius = baseNoise * windMaskingFactor;
        Vector3 soundOrigin = targetPlayer.position + Vector3.up * 0.5f;
        Vector3 guardEar = transform.position + Vector3.up * 1.5f;
        Vector3 soundDir = soundOrigin - guardEar;
        float rayDist = soundDir.magnitude;

        RaycastHit hit;
        if (Physics.Raycast(guardEar, soundDir.normalized, out hit, rayDist))
        {
            // Ignore hits against guard itself or target player
            if (hit.transform != transform && !hit.transform.IsChildOf(transform) &&
                hit.transform != targetPlayer && !hit.transform.IsChildOf(targetPlayer))
            {
                effectiveRadius *= wallSoundOcclusionFactor;
            }
        }

        return dist <= effectiveRadius;
    }

    private void BroadcastRadioAlert()
    {
        if (alertBroadcastCooldown > 0f) return;
        alertBroadcastCooldown = 3.0f;

        // Play radio chirp audio
        try
        {
            AudioClip click = ProceduralAudio.CreateRadioClickClip();
            ProceduralAudio.PlayClipAtPoint(click, transform.position, 0.75f);
        }
        catch (System.Exception) {}

        UpdateCachedGuards();
        var allGuards = cachedGuards;
        if (allGuards == null) return;

        int guardIndex = 0;
        foreach (var other in allGuards)
        {
            if (other == null || other == this) continue;
            float d = Vector3.Distance(transform.position, other.transform.position);
            if (d <= radioBroadcastRadius)
            {
                guardIndex++;
                // Alternate lateral flanking offset: +4.5m or -4.5m
                float flank = (guardIndex % 2 == 1) ? 4.5f : -4.5f;
                Vector3 toDisturbance = (lastKnownPlayerPos - transform.position).normalized;
                Vector3 flankPerpendicular = new Vector3(-toDisturbance.z, 0, toDisturbance.x);
                Vector3 flankPosition = lastKnownPlayerPos + flankPerpendicular * flank;

                other.ReceiveRadioAlert(flankPosition);
            }
        }
    }

    private void SignalNearbySearchlights(Vector3 targetPos, float lingerDuration)
    {
        if (searchlightSignalCooldown > 0f) return;
        searchlightSignalCooldown = 2.0f; // Debounce searchlight signal to prevent timer resetting

        UpdateCachedLightZones();
        var lights = cachedLightZones;
        if (lights == null) return;

        foreach (var lz in lights)
        {
            if (lz != null && lz.isSweepingSpotlight)
            {
                float dist = Vector3.Distance(transform.position, lz.transform.position);
                if (dist <= 20.0f)
                {
                    lz.FocusOnTarget(targetPos, lingerDuration);
                }
            }
        }
    }

    public void ReceiveRadioAlert(Vector3 targetLocation)
    {
        // When receiving a radio / vocal shout from an allied guard, investigate the disturbance
        lastKnownPlayerPos = targetLocation;
        suspicionMeter = Mathf.Max(suspicionMeter, 0.60f);

        if (currentState == GuardState.Patrolling || currentState == GuardState.Investigating)
        {
            currentState = GuardState.Suspicious;
            investigationStage = 0;
            searchTimer = searchWaitTime;
            Debug.Log($"[PatrolMover] {gameObject.name} received radio alert! Converging on {targetLocation}");
        }
    }

    private bool CheckDistractionEvents(out Vector3 eventPosition)
    {
        eventPosition = Vector3.zero;
        var distractions = FindObjectsByType<SoundDistractionEvent>(FindObjectsSortMode.None);
        if (distractions == null || distractions.Length == 0) return false;

        float closestDist = float.MaxValue;
        SoundDistractionEvent closestEvent = null;

        foreach (var sde in distractions)
        {
            if (sde == null) continue;
            float d = Vector3.Distance(transform.position, sde.originPosition);
            float baseRadius = sde.noiseRadius * hearingMultiplier;
            if (d > baseRadius) continue;

            // Acoustic ray occlusion check for environmental sound
            float effectiveRadius = baseRadius;
            Vector3 soundOrigin = sde.originPosition + Vector3.up * 0.5f;
            Vector3 guardEar = transform.position + Vector3.up * 1.5f;
            Vector3 soundDir = soundOrigin - guardEar;
            float rayDist = soundDir.magnitude;

            RaycastHit hit;
            if (Physics.Raycast(guardEar, soundDir.normalized, out hit, rayDist))
            {
                // Ignore hits against guard itself or the distraction event object
                if (hit.transform != transform && !hit.transform.IsChildOf(transform) &&
                    hit.transform != sde.transform && !hit.transform.IsChildOf(sde.transform))
                {
                    effectiveRadius *= wallSoundOcclusionFactor;
                }
            }

            if (d <= effectiveRadius && d < closestDist)
            {
                closestDist = d;
                closestEvent = sde;
            }
        }

        if (closestEvent != null)
        {
            eventPosition = closestEvent.originPosition;
            return true;
        }

        return false;
    }

    private void PatrolMovement()
    {
        float dist = Vector3.Distance(pointA, pointB);
        t += dir * speed * Time.deltaTime / Mathf.Max(0.01f, dist);
        if (t >= 1f) { t = 1f; dir = -1; }
        if (t <= 0f) { t = 0f; dir = 1; }

        Vector3 targetPos = Vector3.Lerp(pointA, pointB, t);
        Vector3 moveDelta = targetPos - transform.position;
        moveDelta.y = 0;
        if (moveDelta.sqrMagnitude > 0.0001f)
        {
            transform.rotation = Quaternion.LookRotation(moveDelta.normalized);
        }
        transform.position = targetPos;
    }

    private bool CheckNearbySearchlightBeam(out Vector3 beamPosition)
    {
        beamPosition = Vector3.zero;
        UpdateCachedLightZones();
        var lights = cachedLightZones;
        if (lights == null) return false;

        foreach (var lz in lights)
        {
            if (lz != null && lz.isSweepingSpotlight && (lz.focusLingerTimer > 0f || lz.exposureLevel >= 0.85f))
            {
                float d = Vector3.Distance(transform.position, lz.transform.position);
                // Interlocking searchlight-patrol sector coordination within 18m
                if (d <= 18.0f)
                {
                    beamPosition = lz.transform.position;
                    return true;
                }
            }
        }
        return false;
    }

    private bool CheckSilhouetteBacklit()
    {
        if (targetPlayer == null) return false;
        UpdateCachedLightZones();
        var lights = cachedLightZones;
        if (lights == null) return false;

        Vector3 toPlayer = (targetPlayer.position - transform.position).normalized;

        foreach (var lz in lights)
        {
            if (lz == null || lz.exposureLevel < 0.70f) continue;
            Vector3 toLight = (lz.transform.position - targetPlayer.position).normalized;
            // If light is behind the player from guard's perspective (dot product > 0.5)
            if (Vector3.Dot(toPlayer, toLight) > 0.5f && Vector3.Distance(lz.transform.position, targetPlayer.position) <= lz.radius * 1.5f)
            {
                return true;
            }
        }
        return false;
    }

    private bool CheckVision()
    {
        if (targetPlayer == null) return false;

        Vector3 toPlayer = targetPlayer.position - transform.position;
        float dist = toPlayer.magnitude;

        // Apply dense fog atmospheric extinction (reduced range unless illuminated by flashlight)
        float effectiveViewDistance = hasDenseFog && !isFlashlightActive
            ? (viewDistance * (1f - fogExtinctionDistanceReduction))
            : viewDistance;

        // Apply glare blindness if guard is looking directly into an intense light beam within 6m
        UpdateCachedLightZones();
        var lights = cachedLightZones;
        if (lights != null)
        {
            foreach (var lz in lights)
            {
                if (lz != null && lz.exposureLevel >= 0.75f)
                {
                    Vector3 toLight = lz.transform.position - transform.position;
                    if (toLight.magnitude <= 6.0f && Vector3.Angle(transform.forward, toLight) <= 35f)
                    {
                        effectiveViewDistance *= (1f - glareBlindnessDistanceReduction);
                        break;
                    }
                }
            }
        }

        if (dist > effectiveViewDistance) return false;

        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > viewAngle) return false;

        // Raycast check to ensure line of sight isn't blocked by obstacles (clamped to dist)
        Vector3 guardEye = transform.position + Vector3.up * 1.5f;
        Vector3 playerChest = targetPlayer.position + Vector3.up * 0.7f;
        Vector3 rayDir = playerChest - guardEye;
        float rayDist = rayDir.magnitude;

        RaycastHit hit;
        if (Physics.Raycast(guardEye, rayDir.normalized, out hit, rayDist))
        {
            if (hit.transform == targetPlayer || hit.transform.IsChildOf(targetPlayer))
            {
                return true;
            }
        }
        else
        {
            // If raycast didn't hit any obstacle before reaching the player, target in clear sight
            return true;
        }

        return false;
    }

    void OnGUI()
    {
        // Render overhead suspicion meter and status marker in screen space
        if (currentState == GuardState.Patrolling && suspicionMeter <= 0.05f) return;

        Camera cam = Camera.main;
        if (cam == null) cam = FindFirstObjectByType<Camera>();
        if (cam == null) return;

        Vector3 headPos = transform.position + Vector3.up * 2.2f;
        Vector3 screenPos = cam.WorldToScreenPoint(headPos);

        // Discard if behind camera
        if (screenPos.z <= 0) return;

        // Invert Y coordinate for IMGUI
        float x = screenPos.x;
        float y = Screen.height - screenPos.y;

        // Status icon
        string marker = "";
        Color markerColor = Color.white;
        if (currentState == GuardState.Alert)
        {
            marker = "[ ! ALERT ! ]";
            markerColor = Color.red;
        }
        else if (currentState == GuardState.Suspicious)
        {
            marker = "[ ? SUSPICIOUS ]";
            markerColor = Color.yellow;
        }
        else if (currentState == GuardState.Investigating)
        {
            marker = "[ ? INVESTIGATING ]";
            markerColor = new Color(1f, 0.6f, 0.2f);
        }

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = markerColor;
        style.fontStyle = FontStyle.Bold;

        GUI.Label(new Rect(x - 75, y - 24, 150, 20), marker, style);

        // Suspicion progress bar background & fill
        float barWidth = 70f;
        float barHeight = 8f;
        GUI.Box(new Rect(x - barWidth / 2f, y - 4, barWidth, barHeight), GUIContent.none);

        Color oldColor = GUI.color;
        GUI.color = currentState == GuardState.Alert ? Color.red : (currentState == GuardState.Suspicious ? Color.yellow : Color.cyan);
        GUI.DrawTexture(new Rect(x - barWidth / 2f + 1, y - 3, (barWidth - 2) * suspicionMeter, barHeight - 2), Texture2D.whiteTexture);
        GUI.color = oldColor;
    }

    void OnDrawGizmosSelected()
    {
        // Waypoint line
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(pointA, pointB);
        Gizmos.DrawWireSphere(pointA, 0.3f);
        Gizmos.DrawWireSphere(pointB, 0.3f);

        // Vision cone visualization
        Gizmos.color = currentState == GuardState.Alert ? Color.red : (currentState == GuardState.Suspicious ? Color.yellow : Color.green);
        Vector3 leftRayDirection = Quaternion.Euler(0, -viewAngle, 0) * transform.forward;
        Vector3 rightRayDirection = Quaternion.Euler(0, viewAngle, 0) * transform.forward;

        Gizmos.DrawRay(transform.position, leftRayDirection * viewDistance);
        Gizmos.DrawRay(transform.position, rightRayDirection * viewDistance);
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
        Gizmos.DrawWireSphere(transform.position, 0.5f);

        // Flashlight beam visualization when active
        if (isFlashlightActive)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawRay(transform.position + Vector3.up * 1.2f, transform.forward * (viewDistance * 1.15f));
        }
    }
}
