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
    public int bribeRiceCost = 2; // Scavenged food bribe
    public bool bribed = false;
    public bool isInterrogating = false;
    public float interrogationTimer = 0f;
    public float interrogationGracePeriod = 3.0f; // 3.0s inspection grace period before alarm
    public string interrogationDialogue = "";

    [Header("Guard Association")]
    public PatrolMover checkpointGuard;
    public float sirenBroadcastRadius = 35.0f;
    private AudioSource audioSource;

    private bool playerInRange = false;
    private Transform playerTransform;

    void Awake()
    {
        if (checkpointGuard == null)
        {
            checkpointGuard = FindFirstObjectByType<PatrolMover>();
        }
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.spatialBlend = 1.0f;
            audioSource.minDistance = 3.0f;
            audioSource.maxDistance = 40.0f;
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
            if (!isInterrogating)
            {
                isInterrogating = true;
                // If crouching in deep shadow (exposure <= 0.20), guard squints and takes 4.5s to inspect
                float effectiveGrace = (survival != null && survival.currentLightExposure <= 0.20f) ? 4.5f : interrogationGracePeriod;
                interrogationTimer = effectiveGrace;
                interrogationDialogue = "Halt! Curfew papers! Present identification immediately!";
                Debug.Log($"[Checkpoint] {checkpointName}: {interrogationDialogue} (Grace: {effectiveGrace:F1}s)");
            }
            else
            {
                interrogationTimer -= Time.deltaTime;
                if (interrogationTimer <= 0f)
                {
                    Debug.LogWarning($"[Checkpoint] {checkpointName}: Interrogation grace period expired without clearance!");
                    TriggerSecurityAlarm();
                }
            }

            // If player tries to pass through without credentials or bribe, alert guard
            if (Input.GetKeyDown(KeyCode.E))
            {
                // Attempt presentation of papers
                if (playerHasPass)
                {
                    isInterrogating = false;
                    interrogationDialogue = "Passage approved.";
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
                // Attempt bribe with scavenged rice
                if (survival != null && survival.riceCount >= bribeRiceCost)
                {
                    survival.riceCount -= bribeRiceCost;
                    bribed = true;
                    isInterrogating = false;
                    interrogationDialogue = "Curfew passage granted. Keep moving.";
                    Debug.Log($"[Checkpoint] {checkpointName}: Guard accepted bribe of {bribeRiceCost} rice bowls. Passage granted.");
                }
                else
                {
                    Debug.LogWarning($"[Checkpoint] {checkpointName}: Guard sneers: 'Need at least {bribeRiceCost} rice bowls to pass.'");
                }
            }
        }
        else if (!playerInRange && isInterrogating && !alarmTriggered)
        {
            // Player backed away from gate safely
            isInterrogating = false;
            interrogationTimer = 0f;
            interrogationDialogue = "";
        }
    }

    public void TriggerSecurityAlarm()
    {
        if (alarmTriggered) return;
        alarmTriggered = true;

        // Play security siren audio
        if (audioSource != null)
        {
            audioSource.clip = ProceduralAudio.CreateAlarmSirenClip(2.0f);
            audioSource.Play();
        }

        // Alert checkpoint guard and broadcast to all guards within 35m
        var allGuards = FindObjectsByType<PatrolMover>(FindObjectsSortMode.None);
        if (allGuards != null)
        {
            foreach (var g in allGuards)
            {
                if (g == null) continue;
                if (Vector3.Distance(transform.position, g.transform.position) <= sirenBroadcastRadius)
                {
                    g.ReceiveRadioAlert(transform.position);
                    g.currentState = PatrolMover.GuardState.Alert;
                    g.suspicionMeter = 1.0f;
                }
            }
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
                string timerStr = isInterrogating ? $" (Alarm in {interrogationTimer:F1}s)" : "";
                GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 95, 300, 25), $"[E] Present Papers{timerStr}");
                if (canBribe)
                {
                    GUI.Label(new Rect(Screen.width / 2 - 150, Screen.height - 75, 300, 25), $"[B] Bribe Guard ({bribeRiceCost} Rice Bowls)");
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
