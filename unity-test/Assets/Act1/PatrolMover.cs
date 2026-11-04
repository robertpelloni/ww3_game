using UnityEngine;

/// <summary>
/// Guard AI state machine: Patrolling, Suspicious, or Alert.
/// Integrates vision cone checking, stance modifiers from PlayerMove, and suspicion escalation.
/// </summary>
public class PatrolMover : MonoBehaviour
{
    public enum GuardState { Patrolling, Suspicious, Alert }

    [Header("Patrol Waypoints")]
    public Vector3 pointA = new Vector3(-5f, 1f, -4f);
    public Vector3 pointB = new Vector3(-5f, 1f, 4f);
    public float speed = 3f;
    public float chaseSpeed = 4.5f;

    [Header("Vision & Detection")]
    public float viewDistance = 12f;
    public float viewAngle = 70f; // Half-angle FOV
    public Transform targetPlayer;
    public GuardState currentState = GuardState.Patrolling;
    [Range(0f, 1f)]
    public float suspicionMeter = 0f;
    public float suspicionRate = 0.8f;   // Rate per second when player in line-of-sight
    public float cooldownRate = 0.35f;   // Decay per second when sight lost

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

        // Update suspicion meter based on vision check
        if (playerSeen)
        {
            // Detection rate scaled by player stance if PlayerMove is present
            float stanceModifier = 1.0f;
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
            }

            suspicionMeter = Mathf.Clamp01(suspicionMeter + (suspicionRate * stanceModifier) * Time.deltaTime);
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
        else
        {
            suspicionMeter = Mathf.Clamp01(suspicionMeter - cooldownRate * Time.deltaTime);
            if (suspicionMeter < 0.2f && currentState != GuardState.Patrolling)
            {
                currentState = GuardState.Patrolling;
            }
            else if (suspicionMeter < 0.75f && currentState == GuardState.Alert)
            {
                currentState = GuardState.Suspicious;
            }
        }

        // Execute behavior based on state
        switch (currentState)
        {
            case GuardState.Patrolling:
                PatrolMovement();
                break;

            case GuardState.Suspicious:
                // Look toward last known position or pause
                Vector3 lookDir = (lastKnownPlayerPos - transform.position);
                lookDir.y = 0;
                if (lookDir != Vector3.zero)
                {
                    transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 3f);
                }
                break;

            case GuardState.Alert:
                // Move towards last known player position
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

        // Raycast check to ensure line of sight isn't blocked by obstacles
        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up * 0.5f, toPlayer.normalized, out hit, viewDistance))
        {
            if (hit.transform == targetPlayer || hit.transform.IsChildOf(targetPlayer))
            {
                return true;
            }
        }
        else
        {
            // If raycast didn't hit any obstacle, target in clear sight
            return true;
        }

        return false;
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
    }
}
