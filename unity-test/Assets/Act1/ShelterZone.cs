using UnityEngine;

/// <summary>
/// Defines an environmental warming or shelter zone (e.g. burn barrel, indoor boiler room, underground alcove).
/// While inside this zone, Jun-ho recovers from cold/hypothermia instead of accumulating curfew frostbite.
/// </summary>
public class ShelterZone : MonoBehaviour
{
    [Header("Shelter Configuration")]
    [Tooltip("Descriptive name for HUD/log feedback (e.g., 'Alley Burn Barrel', 'Abandoned Boiler Room').")]
    public string zoneName = "Alley Burn Barrel";

    [Tooltip("Radius around this zone where warming and shelter apply.")]
    public float radius = 3.5f;

    [Tooltip("Warming rate in points of Cold removed per second while within zone.")]
    public float warmingRate = 8.0f;

    [Tooltip("Whether this zone provides full acoustic concealment/shadow coverage.")]
    public bool providesConcealment = true;

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
    /// Checks whether the specified position is within this shelter zone.
    /// </summary>
    public bool IsInside(Vector3 position)
    {
        return Vector3.Distance(transform.position, position) <= radius;
    }

    /// <summary>
    /// Checks whether the player is currently inside this shelter zone.
    /// </summary>
    public bool IsPlayerInside()
    {
        if (playerTransform == null) return false;
        return IsInside(playerTransform.position);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.4f);
        Gizmos.DrawWireSphere(transform.position, radius);
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }
}
