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

    [Header("Current Stance")]
    public Stance currentStance = Stance.Walk;

    [Header("Boundaries")]
    public float minX = -28f, maxX = 28f, minZ = -9f, maxZ = 9f;

    public bool IsMoving { get; private set; }

    private SurvivalSystem survival;

    void Awake()
    {
        survival = GetComponent<SurvivalSystem>();
    }

    void Update()
    {
        HandleStanceInput();

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
}
