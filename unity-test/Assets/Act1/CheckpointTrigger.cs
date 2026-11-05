using UnityEngine;

/// <summary>
/// Checkpoint Security Gate / Guard Interrogation Trigger.
/// Represents a militarized checkpoint where guards demand identification papers,
/// confiscate contraband, or raise the alarm if the player attempts to bypass or fails inspection.
/// </summary>
public class CheckpointTrigger : MonoBehaviour
{
    [Header("Checkpoint Parameters")]
    public string checkpointName = "Chongjin Sector 4 Gate";
    public bool requiresPass = true;
    public bool playerHasPass = false;
    public bool alarmTriggered = false;
    public float interactionDistance = 3.5f;

    [Header("Bribe / Dialogue")]
    public bool canBribe = true;
    public int bribeCost = 50;
    public bool bribed = false;

    [Header("Guard Association")]
    public PatrolMover checkpointGuard;

    private bool playerInRange = false;
    private Transform playerTransform;

    void Awake()
    {
        if (checkpointGuard == null)
        {
            checkpointGuard = FindFirstObjectByType<PatrolMover>();
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

        if (playerTransform != null)
        {
            float dist = Vector3.Distance(transform.position, playerTransform.position);
            playerInRange = dist <= interactionDistance;
        }

        // Synchronize with player's SurvivalSystem if present
        var survival = FindFirstObjectByType<SurvivalSystem>();
        if (survival != null && survival.hasForgedPapers)
        {
            playerHasPass = true;
        }

        if (playerInRange && !alarmTriggered && !playerHasPass && !bribed)
        {
            // If player tries to pass through without credentials or bribe, alert guard
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Attempt presentation of papers
                if (playerHasPass)
                {
                    Debug.Log($"[Checkpoint] {checkpointName}: Papers inspected and approved. Passage granted.");
                }
                else
                {
                    Debug.Log($"[Checkpoint] {checkpointName}: Papers missing or invalid!");
                    TriggerSecurityAlarm();
                }
            }
            else if (Input.GetKeyDown(KeyCode.B) && canBribe)
            {
                // Attempt bribe
                bribed = true;
                Debug.Log($"[Checkpoint] {checkpointName}: Guard accepted bribe of {bribeCost} won. Passage granted.");
            }
        }
    }

    public void TriggerSecurityAlarm()
    {
        alarmTriggered = true;
        if (checkpointGuard != null)
        {
            checkpointGuard.currentState = PatrolMover.GuardState.Alert;
            checkpointGuard.suspicionMeter = 1.0f;
        }
        Debug.LogWarning($"[ALARM] Checkpoint breach detected at {checkpointName}!");
    }

    void OnGUI()
    {
        if (playerInRange)
        {
            GUI.Box(new Rect(Screen.width / 2 - 160, Screen.height - 120, 320, 80), $"=== {checkpointName} ===");
            if (playerHasPass || bribed)
            {
                GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 95, 300, 25), "STATUS: Clearance Verified. Gate Open.");
            }
            else
            {
                GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 95, 300, 25), "[E] Present Transit Papers (Missing/Invalid)");
                if (canBribe)
                {
                    GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 75, 300, 25), $"[B] Bribe Guard ({bribeCost} Won)");
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = alarmTriggered ? Color.red : (playerHasPass || bribed ? Color.green : Color.yellow);
        Gizmos.DrawWireCube(transform.position, new Vector3(interactionDistance * 2f, 2f, 1f));
    }
}
