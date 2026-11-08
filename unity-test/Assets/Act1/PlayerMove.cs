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

    public enum SurfaceType
    {
        Dirt,           // Soft mud / packed earth (0.7x noise)
        Concrete,       // Standard curfew pavement (1.0x noise)
        MetalGrate,     // Ventilation grates / catwalks (1.6x noise)
        GlassDebris     // Shattered glass / rubble (2.0x noise)
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

    [Header("Current Stance & Surface")]
    public Stance currentStance = Stance.Walk;
    public SurfaceType currentSurface = SurfaceType.Concrete;
    public float surfaceNoiseMultiplier = 1.0f;

    [Header("Distraction & Sound Mechanics")]
    public float whistleNoiseRadius = 20.0f;
    public float whistleDuration = 1.0f;
    public float whistleCooldown = 3.0f;
    public float whistleTimer = 0f;
    public float whistleCooldownTimer = 0f;

    [Header("Throw Distraction (Glass Bottles)")]
    public float throwDistance = 9.0f;
    public float throwNoiseRadius = 14.0f;
    public float throwCooldown = 2.0f;
    private float throwCooldownTimer = 0f;

    [Header("Boundaries")]
    public float minX = -28f, maxX = 28f, minZ = -9f, maxZ = 9f;

    [Header("Footstep Audio Pacing")]
    public float footstepTimer = 0f;

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
        HandleDistractionInputs();

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

        // Footstep audio pacing based on movement and stance
        if (IsMoving)
        {
            float strideInterval = currentStance switch
            {
                Stance.Sprint => 0.28f,
                Stance.Walk => 0.45f,
                Stance.Crouch => 0.65f,
                Stance.Prone => 0.90f,
                _ => 0.45f
            };

            footstepTimer += Time.deltaTime;
            if (footstepTimer >= strideInterval)
            {
                footstepTimer = 0f;
                PlayFootstepAudio();
            }
        }
        else
        {
            footstepTimer = 0f;
        }

        // Update surface material multiplier
        surfaceNoiseMultiplier = currentSurface switch
        {
            SurfaceType.Dirt => 0.70f,
            SurfaceType.Concrete => 1.00f,
            SurfaceType.MetalGrate => 1.60f,
            SurfaceType.GlassDebris => 2.00f,
            _ => 1.00f
        };

        // Calculate emitted noise radius
        if (whistleTimer > 0f)
        {
            whistleTimer -= Time.deltaTime;
            CurrentNoiseRadius = whistleNoiseRadius;
        }
        else
        {
            float baseNoise = IsMoving ? (GetBaseNoiseRadius() * surfaceNoiseMultiplier) : 0f;

            // Shivering chattering noise & exhaustion heavy breathing:
            if (survival != null)
            {
                if (survival.isShivering)
                {
                    baseNoise = IsMoving ? (baseNoise + 1.8f) : 1.8f;
                }
                if (survival.isExhausted)
                {
                    baseNoise = Mathf.Max(baseNoise, 2.5f);
                }
            }

            CurrentNoiseRadius = baseNoise;
        }

        if (whistleCooldownTimer > 0f)
        {
            whistleCooldownTimer -= Time.deltaTime;
        }
        if (throwCooldownTimer > 0f)
        {
            throwCooldownTimer -= Time.deltaTime;
        }
    }

    private void PlayFootstepAudio()
    {
        try
        {
            AudioClip step = ProceduralAudio.CreateFootstepClip(currentSurface);
            float vol = currentStance switch
            {
                Stance.Sprint => 0.85f,
                Stance.Walk => 0.50f,
                Stance.Crouch => 0.25f,
                Stance.Prone => 0.12f,
                _ => 0.50f
            };
            ProceduralAudio.PlayClipAtPoint(step, transform.position, vol);
        }
        catch (System.Exception) {}
    }

    void HandleDistractionInputs()
    {
        // [Q] Key triggers a loud whistle to lure or distract nearby guards (at player position)
        if (Input.GetKeyDown(KeyCode.Q) && whistleCooldownTimer <= 0f)
        {
            // Urban concrete alley reverberation: enclosed spaces sustain an acoustic echo bonus (+0.5s)
            float reverberationBonus = (currentSurface == SurfaceType.Concrete || currentSurface == SurfaceType.MetalGrate) ? 0.5f : 0f;
            whistleTimer = whistleDuration + reverberationBonus;
            whistleCooldownTimer = whistleCooldown;

            // Play procedural whistle audio
            try
            {
                AudioClip whistle = ProceduralAudio.CreateWhistleClip();
                ProceduralAudio.PlayClipAtPoint(whistle, transform.position, 0.8f);
            }
            catch (System.Exception) {}
            Debug.Log($"[PlayerMove] Whistled! Noise radius temporarily expanded to {whistleNoiseRadius}m (duration: {whistleTimer:F1}s).");
        }

        // [F] Key throws a scavenged glass bottle forward to distract guards to a remote position
        if (Input.GetKeyDown(KeyCode.F) && throwCooldownTimer <= 0f)
        {
            if (survival != null && survival.bottleCount > 0)
            {
                survival.bottleCount--;
                throwCooldownTimer = throwCooldown;

                // Determine throw landing point forward along player or movement heading
                Vector3 throwDirection = transform.forward;
                float h = Input.GetAxisRaw("Horizontal");
                float v = Input.GetAxisRaw("Vertical");
                if (Mathf.Abs(h) > 0.1f || Mathf.Abs(v) > 0.1f)
                {
                    throwDirection = new Vector3(h, 0f, v).normalized;
                }

                Vector3 targetLanding = transform.position + throwDirection * throwDistance;
                targetLanding.x = Mathf.Clamp(targetLanding.x, minX, maxX);
                targetLanding.z = Mathf.Clamp(targetLanding.z, minZ, maxZ);
                targetLanding.y = 1f;

                // Spawn environmental sound event & procedural shatter audio
                SoundDistractionEvent.Create(targetLanding, throwNoiseRadius, 3.0f);
                try
                {
                    AudioClip shatter = ProceduralAudio.CreateBottleShatterClip();
                    ProceduralAudio.PlayClipAtPoint(shatter, targetLanding, 0.9f);
                }
                catch (System.Exception) {}
                Debug.Log($"[PlayerMove] Threw bottle to {targetLanding}! Bottles left: {survival.bottleCount}");
            }
            else
            {
                Debug.Log("[PlayerMove] No glass bottles left to throw!");
            }
        }

        // [T] Key uses scavenged wire cutters to breach secondary perimeter fence shortcut
        if (Input.GetKeyDown(KeyCode.T))
        {
            if (survival != null && survival.hasWireCutters)
            {
                survival.hasCutPerimeterFence = true;
                try
                {
                    AudioClip snip = ProceduralAudio.CreateWireSnipClip();
                    ProceduralAudio.PlayClipAtPoint(snip, transform.position, 0.85f);
                }
                catch (System.Exception) {}
                Debug.Log("[PlayerMove] Snip! Wire fence breached. Secondary stealth shortcut opened to safehouse!");
            }
            else
            {
                Debug.LogWarning("[PlayerMove] Cannot cut fence without wire cutters!");
            }
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
        // Sprint when holding Shift (if sufficient stamina available and not exhausted)
        else if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (survival == null || (survival.stamina > 10f && !survival.isExhausted))
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
