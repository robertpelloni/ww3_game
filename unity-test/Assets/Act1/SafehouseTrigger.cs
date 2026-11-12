using UnityEngine;

/// <summary>
/// Safehouse extraction trigger for Act 1 Chongjin.
/// When the player reaches the safehouse door within interaction distance,
/// it triggers mission victory via SurvivalSystem.
/// </summary>
public class SafehouseTrigger : MonoBehaviour
{
    public string safehouseName = "Chongjin Safehouse";
    public float triggerRadius = 3.0f;
    public bool victoryTriggered = false;

    private Transform playerTransform;

    void Update()
    {
        if (victoryTriggered) return;

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

        if (playerTransform != null)
        {
            // Use squared magnitude comparison to avoid square root calls on low-spec hardware
            float sqrDist = (transform.position - playerTransform.position).sqrMagnitude;
            if (sqrDist <= (triggerRadius * triggerRadius))
            {
                victoryTriggered = true;
                var survival = playerTransform.GetComponent<SurvivalSystem>();
                if (survival != null)
                {
                    survival.TriggerVictory(safehouseName);
                }
                Debug.Log($"[SafehouseTrigger] Player entered {safehouseName}! Victory achieved.");
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, triggerRadius);
    }
}
