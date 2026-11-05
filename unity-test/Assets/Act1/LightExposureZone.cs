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

    [Tooltip("Light exposure factor when within radius (0.0 = deep shadow, 1.0 = full direct glare).")]
    [Range(0f, 1f)]
    public float exposureLevel = 0.75f;

    [Tooltip("Target player transform; automatically resolves if unset.")]
    public Transform playerTransform;

    void Awake()
    {
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
