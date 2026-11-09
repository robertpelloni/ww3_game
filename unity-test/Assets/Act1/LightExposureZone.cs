using UnityEngine;

/// <summary>
/// LightExposureZone: Defines illumination zones (streetlamps, curfew spotlights, bonfires)
/// that scale player visibility / exposure in Act 1.
/// In dark alleys, baseline exposure is low (e.g. 0.05), but entering a streetlamp radius
/// elevates exposure (e.g. 0.50), and standing directly under a spotlight causes maximum exposure (1.0).
/// </summary>
public class LightExposureZone : MonoBehaviour
{
    [Header("Illumination Zone Configuration")]
    [Tooltip("Descriptive zone name (e.g., 'Curfew Spotlight', 'Flickering Streetlamp', 'Alley Burn Barrel').")]
    public string zoneName = "Streetlamp Illumination";

    [Tooltip("Effective radius of light emission.")]
    public float radius = 5.0f;
    public float baseRadius = 5.0f;
    public float alertFocusRadiusMultiplier = 1.4f; // Expands radius when locking on disturbance
    public float innerCoreRatio = 0.45f; // Inner 45% radius provides full glare

    [Header("Atmospheric Fog Scattering")]
    public bool hasDenseFog = true;
    public float fogScatteringFactor = 0.85f; // Fog slightly diffuses direct glare
    public float shadowCullingDistance = 22.0f; // Beyond 22m, cull shadow occlusion raycasts to preserve 30fps

    [Header("Oscillating Sweep Settings (Curfew Spotlight)")]
    public bool isSweepingSpotlight = false;
    public float sweepRangeX = 8.0f;
    public float sweepSpeed = 1.2f;
    public float baseSweepSpeed = 1.2f;
    public float motorMechanicalJitter = 0.25f; // Slight erratic motor vibration
    public float focusLingerTimer = 0f;
    public bool isAcquiringTarget = false;
    public float acquisitionWarningTimer = 0f;
    public bool isFlickering = false;
    public float flickerTimer = 0f;
    public float baseExposureLevel = 0.75f;
    public int sweepDirection = 1; // 1 = forward, -1 = reverse
    public float sweepDecelerationTimer = 0f;
    private Vector3 focusTargetPos;
    private Vector3 initialPosition;

    [Tooltip("Light exposure factor when within radius (0.0 = deep shadow, 1.0 = full direct glare).")]
    [Range(0f, 1f)]
    public float exposureLevel = 0.75f;

    [Tooltip("Target player transform; automatically resolves if unset.")]
    public Transform playerTransform;

    void Awake()
    {
        initialPosition = transform.position;
        baseRadius = radius;
        baseExposureLevel = exposureLevel;
        if (playerTransform == null)
        {
            var p = GameObject.FindWithTag("Player");
            if (p != null) playerTransform = p.transform;
            else
            {
                var pm = FindFirstObjectByType<PlayerMove>();
                if (pm != null) playerTransform = pm.transform;
            }
        }
    }

    void Update()
    {
        // Strobe / flicker dynamics during electrical grid instability
        if (isFlickering)
        {
            flickerTimer -= Time.deltaTime;
            exposureLevel = (Mathf.Sin(Time.time * 24f) > 0f) ? baseExposureLevel : (baseExposureLevel * 0.2f);
            if (flickerTimer <= 0f)
            {
                isFlickering = false;
                exposureLevel = baseExposureLevel;
            }
        }

        // Pre-focus strobe warning cue (0.5s acquisition delay)
        if (isAcquiringTarget)
        {
            acquisitionWarningTimer -= Time.deltaTime;
            // High-frequency strobe warning: beam flickers rapidly to telegraph lock
            exposureLevel = (Mathf.Sin(Time.time * 30f) > 0f) ? 1.0f : 0.4f;
            if (acquisitionWarningTimer <= 0f)
            {
                isAcquiringTarget = false;
                exposureLevel = baseExposureLevel;
            }
        }

        if (focusLingerTimer > 0f)
        {
            focusLingerTimer -= Time.deltaTime;
            // Expand beam radius during active focus lock
            radius = Mathf.Lerp(radius, baseRadius * alertFocusRadiusMultiplier, Time.deltaTime * 3.0f);
            // Smoothly track towards focused disturbance location
            transform.position = Vector3.Lerp(transform.position, focusTargetPos, Time.deltaTime * 3.0f);
        }
        else if (isSweepingSpotlight)
        {
            // Recover baseline radius
            radius = Mathf.Lerp(radius, baseRadius, Time.deltaTime * 2.0f);

            // Variable sweep velocity dynamics: if decelerated by nearby disturbance, slow sweep by 50%
            if (sweepDecelerationTimer > 0f)
            {
                sweepDecelerationTimer -= Time.deltaTime;
                sweepSpeed = baseSweepSpeed * 0.50f;
            }
            else
            {
                sweepSpeed = baseSweepSpeed;
            }

            // Oscillate spotlight beam horizontally across alley with realistic motor jitter & direction
            float jitter = Mathf.Sin(Time.time * 17.3f) * motorMechanicalJitter;
            float offsetX = Mathf.Sin(Time.time * sweepSpeed * sweepDirection) * sweepRangeX + jitter;
            Vector3 sweepTarget = new Vector3(initialPosition.x + offsetX, initialPosition.y, initialPosition.z);
            transform.position = Vector3.Lerp(transform.position, sweepTarget, Time.deltaTime * 4.0f);
        }
    }

    public void FocusOnTarget(Vector3 targetPos, float duration = 5.0f)
    {
        focusTargetPos = new Vector3(targetPos.x, transform.position.y, targetPos.z);
        focusLingerTimer = Mathf.Max(focusLingerTimer, duration);
        sweepDecelerationTimer = duration + 2.0f; // Decelerate post-focus sweep
        sweepDirection *= -1; // Reverse sweep direction to re-check sector
        if (!isAcquiringTarget && focusLingerTimer <= duration)
        {
            isAcquiringTarget = true;
            acquisitionWarningTimer = 0.5f; // 0.5s pre-lock strobe cue

            // Play searchlight mechanical motor whine audio cue
            try
            {
                AudioClip motorWhine = ProceduralAudio.CreateSearchlightHumClip();
                ProceduralAudio.PlayClipAtPoint(motorWhine, transform.position, 0.40f);
            }
            catch (System.Exception) {}
        }
    }

    public void TriggerGridFlicker(float duration = 2.0f)
    {
        isFlickering = true;
        flickerTimer = duration;
    }

    /// <summary>
    /// Checks whether the specified position falls within this light volume.
    /// </summary>
    public bool IsInside(Vector3 position)
    {
        return Vector3.Distance(transform.position, position) <= radius;
    }

    public float GetEffectiveExposure()
    {
        return hasDenseFog ? (exposureLevel * fogScatteringFactor) : exposureLevel;
    }

    /// <summary>
    /// Computes exposure with smooth penumbra edge falloff based on distance from light center.
    /// Returns 1.0 in inner core, falling off smoothly to baseline at the outer rim.
    /// Distance-based glare raycast throttling: ignores targets beyond shadowCullingDistance (22m)
    /// to preserve CPU cycles on dual-core low-spec machines.
    /// </summary>
    public float GetExposureAtPosition(Vector3 position)
    {
        float dist = Vector3.Distance(transform.position, position);
        // Distance-based shadow and glare culling: if outside emission radius or beyond 22m cull boundary
        if (dist > radius || dist > shadowCullingDistance) return 0.15f;

        float coreRadius = radius * innerCoreRatio;
        float baseExp = GetEffectiveExposure();

        if (dist <= coreRadius)
        {
            return baseExp;
        }

        // Smooth cosine penumbra falloff from core to outer rim with clamped lookup
        float t = Mathf.Clamp01((dist - coreRadius) / Mathf.Max(0.01f, radius - coreRadius));
        // Smoothstep polynomial S-curve falloff to eliminate expensive trigonometric calls
        float smoothT = t * t * (3f - 2f * t);
        return Mathf.Lerp(baseExp, 0.15f, smoothT);
    }

    /// <summary>
    /// Checks whether the player is currently inside this light volume.
    /// </summary>
    public bool IsPlayerInside()
    {
        if (playerTransform == null) return false;
        return IsInside(playerTransform.position);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 1f, 0.2f, 0.35f);
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.25f);
    }
}
