using UnityEngine;

/// <summary>
/// Represents a temporary acoustic noise distraction in the environment (e.g. shattered bottle, thrown rock).
/// Guards in Act 1 detect these events via PatrolMover and investigate the impact location.
/// </summary>
public class SoundDistractionEvent : MonoBehaviour
{
    public Vector3 originPosition;
    public float noiseRadius = 14.0f;
    public float duration = 2.5f;
    public float elapsed = 0f;

    public static SoundDistractionEvent Create(Vector3 position, float radius = 14f, float lifetime = 2.5f)
    {
        var go = new GameObject("SoundDistraction_Impulse");
        go.transform.position = position;
        var sde = go.AddComponent<SoundDistractionEvent>();
        sde.originPosition = position;
        sde.noiseRadius = radius;
        sde.duration = lifetime;
        return sde;
    }

    void Update()
    {
        elapsed += Time.deltaTime;
        if (elapsed >= duration)
        {
            Destroy(gameObject);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, noiseRadius);
    }
}
