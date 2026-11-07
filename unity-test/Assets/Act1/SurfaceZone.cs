using UnityEngine;

/// <summary>
/// Defines an environmental surface zone (e.g. mud/dirt, metal catwalk grates, broken glass).
/// Automatically modulates player movement acoustic footprint upon entry.
/// </summary>
public class SurfaceZone : MonoBehaviour
{
    public string zoneName = "Catwalk Grate";
    public PlayerMove.SurfaceType surfaceType = PlayerMove.SurfaceType.MetalGrate;
    public Vector3 zoneCenter = Vector3.zero;
    public Vector3 zoneSize = new Vector3(4f, 1f, 4f);

    private Transform playerTransform;

    void Awake()
    {
        if (zoneCenter == Vector3.zero)
        {
            zoneCenter = transform.position;
        }
    }

    void Update()
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

        if (playerTransform != null && IsPlayerInside())
        {
            var pm = playerTransform.GetComponent<PlayerMove>();
            if (pm != null)
            {
                pm.currentSurface = surfaceType;
            }
        }
    }

    public bool IsPlayerInside()
    {
        if (playerTransform == null) return false;
        Vector3 pos = playerTransform.position;
        Bounds bounds = new Bounds(zoneCenter, zoneSize);
        return bounds.Contains(pos);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = surfaceType switch
        {
            PlayerMove.SurfaceType.Dirt => new Color(0.6f, 0.4f, 0.2f, 0.5f),
            PlayerMove.SurfaceType.Concrete => new Color(0.7f, 0.7f, 0.7f, 0.5f),
            PlayerMove.SurfaceType.MetalGrate => new Color(0.3f, 0.7f, 1.0f, 0.5f),
            PlayerMove.SurfaceType.GlassDebris => new Color(1.0f, 0.3f, 0.3f, 0.5f),
            _ => Color.white
        };
        Gizmos.DrawWireCube(zoneCenter, zoneSize);
    }
}
