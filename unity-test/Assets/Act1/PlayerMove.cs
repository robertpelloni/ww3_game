using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public enum Stance
    {
        Prone,
        Crouch,
        Walk,
        Sprint
    }

    [Header("Movement Speeds")]
    public float proneSpeed = 1.5f;
    public float crouchSpeed = 3.5f;
    public float walkSpeed = 6.0f;
    public float sprintSpeed = 9.5f;

    [Header("Noise Profiles (Radius in meters)")]
    public float proneNoise = 0.5f;
    public float crouchNoise = 2.0f;
    public float walkNoise = 5.0f;
    public float sprintNoise = 12.0f;

    [Header("Current Stance")]
    public Stance currentStance = Stance.Walk;

    [Header("Distraction & Sound Mechanics")]
    public float whistleNoiseRadius = 20.0f;
    public float whistleDuration = 1.0f;
    public float whistleCooldown = 3.0f;
    public float whistleTimer = 0f;
    public float whistleCooldownTimer = 0f;

    [Header("Boundaries")]
    public float minX = -28f, maxX = 28f, minZ = -9f, maxZ = 9f;

    public bool IsMoving { get; private set; }
    public float CurrentNoiseRadius { get; private set; }
    public bool IsWhistling => whistleTimer > 0f;

    private SurvivalSystem survival;

    void Awake()
    {
        survival = GetComponent<SurvivalSystem>();
    }

    void Update()
    {
        HandleStanceInput();
        HandleWhistleInput();

        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");

        Vector3 moveInput = new Vector3(h, 0f, v);
        IsMoving = moveInput.sqrMagnitude > 0.01f;

        float targetSpeed = GetCurrentSpeed();

        // Penalty for high cold / hunger
        if (survival != null)
        {
            if (survival.cold > 70f || survival.hunger > 70f)
            {
                targetSpeed *= 0.75f;
            }
        }

        Vector3 p = transform.position + moveInput.normalized * targetSpeed * Time.deltaTime;
        p.x = Mathf.Clamp(p.x, minX, maxX);
        p.z = Mathf.Clamp(p.z, minZ, maxZ);
        p.y = 1f;
        transform.position = p;

        // Calculate emitted noise radius
        if (whistleTimer > 0f)
        {
            whistleTimer -= Time.deltaTime;
            CurrentNoiseRadius = whistleNoiseRadius;
        }
        else
        {
            CurrentNoiseRadius = IsMoving ? GetBaseNoiseRadius() : 0f;
        }

        if (whistleCooldownTimer > 0f)
        {
            whistleCooldownTimer -= Time.deltaTime;
        }
    }

    void HandleWhistleInput()
    {
        // [Q] Key triggers a loud whistle to lure or distract nearby guards
        if (Input.GetKeyDown(KeyCode.Q) && whistleCooldownTimer <= 0f)
        {
            whistleTimer = whistleDuration;
            whistleCooldownTimer = whistleCooldown;
            Debug.Log($"[PlayerMove] Whistled! Noise radius temporarily expanded to {whistleNoiseRadius}m.");
        }
    }

    void HandleStanceInput()
    {
        // Toggle Crouch with C
        if (Input.GetKeyDown(KeyCode.C))
        {
            currentStance = currentStance == Stance.Crouch ? Stance.Walk : Stance.Crouch;
        }
        // Toggle Prone with Z
        else if (Input.GetKeyDown(KeyCode.Z))
        {
            currentStance = currentStance == Stance.Prone ? Stance.Walk : Stance.Prone;
        }
        // Sprint when holding Shift (if sufficient stamina available)
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (survival == null || survival.stamina > 5f)
            {
                currentStance = Stance.Sprint;
            }
            else if (currentStance == Stance.Sprint)
            {
                currentStance = Stance.Walk;
            }
        }
        else if (currentStance == Stance.Sprint)
        {
            // Drop back to Walk when releasing sprint
            currentStance = Stance.Walk;
        }
    }

    public float GetCurrentSpeed()
    {
        return currentStance switch
        {
            Stance.Prone => proneSpeed,
            Stance.Crouch => crouchSpeed,
            Stance.Walk => walkSpeed,
            Stance.Sprint => sprintSpeed,
            _ => walkSpeed
        };
    }

    public float GetBaseNoiseRadius()
    {
        return currentStance switch
        {
            Stance.Prone => proneNoise,
            Stance.Crouch => crouchNoise,
            Stance.Walk => walkNoise,
            Stance.Sprint => sprintNoise,
            _ => walkNoise
        };
    }
}
