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
    public float viewAngle = 70f; // Half-angle FOV
    public bool isFlashlightActive = false;
    public float flashlightSweepAngle = 45f;
    public float flashlightSweepSpeed = 2.5f;
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
    private float searchTimer = 0f;

    private float t;
    private int dir = 1;
    private Vector3 lastKnownPlayerPos;

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
        bool playerSeen = CheckVision();
        bool playerSoundHeard = CheckHearing();
        Vector3 distractionPos;
        bool distractionHeard = CheckDistractionEvents(out distractionPos);

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

                var survival = targetPlayer.GetComponent<SurvivalSystem>();
                if (survival != null)
                {
                    lightExposure = survival.currentLightExposure;
                }
            }

            float detectionMultiplier = stanceModifier * Mathf.Lerp(0.3f, 1.8f, lightExposure);
            suspicionMeter = Mathf.Clamp01(suspicionMeter + (suspicionRate * detectionMultiplier) * Time.deltaTime);
            if (targetPlayer != null) lastKnownPlayerPos = targetPlayer.position;

            if (suspicionMeter >= 0.8f)
            {
                currentState = GuardState.Alert;
            }
            else if (suspicionMeter >= 0.25f)
            {
                currentState = GuardState.Suspicious;
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
            // Hearing player sound immediately raises interest to Investigating or Suspicious
            suspicionMeter = Mathf.Clamp01(suspicionMeter + 0.35f * Time.deltaTime);
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

        // Execute behavior based on state
        isFlashlightActive = (currentState == GuardState.Investigating || currentState == GuardState.Suspicious || currentState == GuardState.Alert);

        switch (currentState)
        {
            case GuardState.Patrolling:
                PatrolMovement();
                break;

            case GuardState.Investigating:
                // Move towards last heard or seen position with flashlight sweeping
                Vector3 investDir = (lastKnownPlayerPos - transform.position);
                investDir.y = 0;
                if (investDir.magnitude > 1.2f)
                {
                    Quaternion baseRot = Quaternion.LookRotation(investDir.normalized);
                    float sweep = Mathf.Sin(Time.time * flashlightSweepSpeed) * (flashlightSweepAngle * 0.4f);
                    Quaternion sweepRot = baseRot * Quaternion.Euler(0, sweep, 0);
                    transform.rotation = Quaternion.Slerp(transform.rotation, sweepRot, Time.deltaTime * 4f);
                    transform.position += transform.forward * investigateSpeed * Time.deltaTime;
                }
                else
                {
                    // Arrived at search origin: execute wider sweeping look-around
                    float wideSweep = Mathf.Sin(Time.time * flashlightSweepSpeed) * flashlightSweepAngle;
                    transform.Rotate(Vector3.up, (35f + wideSweep * 0.5f) * Time.deltaTime);
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

        // Acoustic ray occlusion check: if solid obstacle intervenes, attenuate effective hearing radius
        float effectiveRadius = baseNoise;
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

    private bool CheckVision()
    {
        if (targetPlayer == null) return false;

        Vector3 toPlayer = targetPlayer.position - transform.position;
        float dist = toPlayer.magnitude;

        if (dist > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, toPlayer);
        if (angle > viewAngle) return false;

        // Raycast check to ensure line of sight isn't blocked by obstacles (clamped to dist)
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, toPlayer.normalized, out hit, dist))
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
