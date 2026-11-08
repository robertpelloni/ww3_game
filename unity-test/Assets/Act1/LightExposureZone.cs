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

    [Header("Oscillating Sweep Settings (Curfew Spotlight)")]
    public bool isSweepingSpotlight = false;
    public float sweepRangeX = 8.0f;
    public float sweepSpeed = 1.2f;
    public float focusLingerTimer = 0f;
    public bool isAcquiringTarget = false;
    public float acquisitionWarningTimer = 0f;
    public bool isFlickering = false;
    public float flickerTimer = 0f;
    public float baseExposureLevel = 0.75f;
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
            // Oscillate spotlight beam horizontally across alley smoothly
            float offsetX = Mathf.Sin(Time.time * sweepSpeed) * sweepRangeX;
            Vector3 sweepTarget = new Vector3(initialPosition.x + offsetX, initialPosition.y, initialPosition.z);
            transform.position = Vector3.Lerp(transform.position, sweepTarget, Time.deltaTime * 4.0f);
        }
    }

    public void FocusOnTarget(Vector3 targetPos, float duration = 5.0f)
    {
        focusTargetPos = new Vector3(targetPos.x, transform.position.y, targetPos.z);
        focusLingerTimer = Mathf.Max(focusLingerTimer, duration);
        if (!isAcquiringTarget && focusLingerTimer <= duration)
        {
            isAcquiringTarget = true;
            acquisitionWarningTimer = 0.5f; // 0.5s pre-lock strobe cue
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
