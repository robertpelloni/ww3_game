using UnityEngine;

[RequireComponent(typeof(PlayerMove))]
public class SurvivalSystem : MonoBehaviour
{
    [Header("Survival Meters (0 - 100)")]
    [Tooltip("Hunger: 0 = full, 100 = starving. Increases over time.")]
    public float hunger = 10f;
    [Tooltip("Cold: 0 = warm, 100 = hypothermia. Increases in outdoor curfew weather.")]
    public float cold = 15f;
    [Tooltip("Stamina: 100 = full stamina, 0 = exhausted.")]
    public float stamina = 100f;
    [Tooltip("Player Health: 100 = full health, 0 = death from hypothermia/starvation.")]
    public float health = 100f;

    [Header("Drain Rates (per second)")]
    public float hungerRate = 0.2f;
    public float coldRate = 0.35f;
    public float staminaDrainRate = 25f;
    public float staminaRecoverRate = 12f;

    [Header("Inventory")]
    public int riceCount = 3;
    public int medicineCount = 1;
    public int hotTeaCount = 2;
    public int bottleCount = 2;
    public bool hasForgedPapers = true;
    public bool hasWireCutters = true;
    public bool hasCutPerimeterFence = false;

    [Header("Environmental Status")]
    public bool isSheltered = false;
    public string currentShelterName = "Exposed (Curfew Alley)";
    [Range(0f, 1f)]
    public float currentLightExposure = 0.15f; // Ambient dark alley default

    [Header("Debuffs & Condition")]
    public bool isShivering = false;
    public bool isStarving = false;
    public bool isExhausted = false;
    public float exhaustionCooldown = 0f;

    [Header("Game Loop State")]
    public bool isGameOver = false;
    public bool isGameWon = false;
    public string gameOverReason = "";
    public float missionTimer = 0f;
    public string extractionRoute = "Checkpoint Transit Gate";

    [Header("Debug HUD")]
    public bool showDebugHUD = true;
    public float rollingFPS = 60f;
    private float fpsAccumulator = 0f;
    private int fpsFrameCount = 0;
    private float fpsTimer = 0f;

    [Header("Performance Throttling (Intel HD 520 / Low-Spec)")]
    public float envCheckInterval = 0.10f; // 10Hz throttle on shelter & light calculations
    private float envCheckTimer = 0f;

    private PlayerMove playerMove;
    private PatrolMover nearestGuard;
    private ShelterZone[] shelterZones;
    private LightExposureZone[] lightZones;

    void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
        if (playerMove == null)
        {
            // Resilient runtime safeguard: ensures player movement is always attached
            playerMove = gameObject.AddComponent<PlayerMove>();
        }
    }

    void Start()
    {
        shelterZones = FindObjectsByType<ShelterZone>(FindObjectsSortMode.None);
        lightZones = FindObjectsByType<LightExposureZone>(FindObjectsSortMode.None);
    }

    void Update()
    {
        float dt = Time.deltaTime;

        if (!isGameOver && !isGameWon)
        {
            missionTimer += dt;
        }

        // Rolling FPS monitoring
        fpsAccumulator += (dt > 0f) ? (1f / dt) : 60f;
        fpsFrameCount++;
        fpsTimer += dt;
        if (fpsTimer >= 0.5f)
        {
            rollingFPS = fpsAccumulator / Mathf.Max(1, fpsFrameCount);
            fpsAccumulator = 0f;
            fpsFrameCount = 0;
            fpsTimer = 0f;
        }

        // Throttled shelter proximity & ambient light exposure evaluation (10Hz instead of 60Hz)
        envCheckTimer += dt;
        if (envCheckTimer >= envCheckInterval)
        {
            envCheckTimer = 0f;
            CheckShelterStatus();
            CheckLightExposure();
        }

        // Base hunger accumulation
        hunger = Mathf.Clamp(hunger + hungerRate * dt, 0f, 100f);

        // Cold accumulation / recovery based on shelter & radiant light exposure
        if (isSheltered)
        {
            // Recover from cold while inside shelter or near fire/heaters
            cold = Mathf.Max(0f, cold - 6.0f * dt);
        }
        else
        {
            // Outdoor curfew frostbite with light-exposure thermal radiant dynamic:
            // High exposure (spotlights/lamps, >= 0.70) emits radiant halogen heat slowing frostbite (-50%),
            // whereas pitch-black wind tunnels (<= 0.15) induce increased wind chill (+25%).
            float exposureThermalFactor = 1.0f;
            if (currentLightExposure >= 0.70f)
            {
                exposureThermalFactor = 0.5f; // Radiant heat from industrial curfew lamps
            }
            else if (currentLightExposure <= 0.15f)
            {
                exposureThermalFactor = 1.25f; // Shadow alley wind tunnel chill
            }

            cold = Mathf.Clamp(cold + coldRate * exposureThermalFactor * dt, 0f, 100f);
        }

        // Shivering mechanic (Cold >= 70%): generates acoustic noise & reduces stamina regen
        isShivering = cold >= 70f;
        isStarving = hunger >= 80f;

        // Health penalties for extreme cold/hunger
        if (cold >= 90f || hunger >= 90f)
        {
            health = Mathf.Max(0f, health - 2f * dt);
        }

        // Stamina logic: temperature-based drain and shivering exhaustion
        bool isMoving = playerMove != null && playerMove.IsMoving;
        bool isSprinting = playerMove != null && playerMove.currentStance == PlayerMove.Stance.Sprint && isMoving;

        // Temperature-based continuous baseline exhaustion:
        // As cold rises above 60%, maintaining body temperature burns stamina continuously.
        if (cold >= 60f)
        {
            float coldExhaustionFactor = (cold - 60f) / 40f; // 0.0 at 60% cold -> 1.0 at 100% cold
            float coldStaminaBurn = 3.5f * coldExhaustionFactor;
            stamina = Mathf.Max(0f, stamina - coldStaminaBurn * dt);
        }

        if (isSprinting)
        {
            // Starvation and extreme cold compound sprint exhaustion
            float starvationMultiplier = isStarving ? 1.75f : 1.0f;
            float coldSprintMultiplier = 1f + (cold / 100f) * 0.75f;
            float drain = staminaDrainRate * (1f + (hunger / 100f) * 0.5f) * starvationMultiplier * coldSprintMultiplier;
            stamina = Mathf.Max(0f, stamina - drain * dt);
            if (stamina <= 0.5f && playerMove != null)
            {
                playerMove.currentStance = PlayerMove.Stance.Walk;
                isExhausted = true;
                exhaustionCooldown = 1.5f; // Enforce 1.5s recovery before sprint can resume
            }
        }
        else
        {
            // If exhausted, cooldown must expire before sprint is allowed again
            if (exhaustionCooldown > 0f)
            {
                exhaustionCooldown -= dt;
                if (exhaustionCooldown <= 0f)
                {
                    isExhausted = false;
                }
            }

            // Cold shivering sharply reduces recovery rate; extreme shivering (>=80%) nearly halts it
            float shiveringPenalty = isShivering ? (cold >= 80f ? 0.15f : 0.4f) : 1.0f;
            float exhaustionPenalty = isExhausted ? 0.5f : 1.0f;
            float recovery = staminaRecoverRate * (1f - (cold / 100f) * 0.5f) * shiveringPenalty * exhaustionPenalty;
            stamina = Mathf.Min(100f, stamina + recovery * dt);
        }

        // Quick input for item consumption
        if (Input.GetKeyDown(KeyCode.Alpha1)) EatRice();
        if (Input.GetKeyDown(KeyCode.Alpha2)) TakeMedicine();
        if (Input.GetKeyDown(KeyCode.Alpha3)) DrinkHotTea();

        // Query nearest guard for HUD status
        if (nearestGuard == null)
        {
            nearestGuard = FindFirstObjectByType<PatrolMover>();
        }

        // Check fail conditions
        CheckGameOverConditions();

        // Quick restart on 'R' key
        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartLevel();
        }
    }

    private void CheckGameOverConditions()
    {
        if (isGameOver || isGameWon) return;

        // Health depleted
        if (health <= 0f)
        {
            isGameOver = true;
            gameOverReason = cold >= 90f ? "FROZEN TO DEATH (Hypothermia)" : "STARVATION";
            return;
        }

        // Guard capture check: if guard is in Alert state and within 1.8 meters
        if (nearestGuard != null && nearestGuard.currentState == PatrolMover.GuardState.Alert)
        {
            float dist = Vector3.Distance(transform.position, nearestGuard.transform.position);
            if (dist <= 1.8f)
            {
                isGameOver = true;
                gameOverReason = "CAPTURED BY CURFEW PATROL";
                return;
            }
        }
    }

    public void TriggerVictory(string safehouseName = "Border Safehouse")
    {
        isGameWon = true;
        extractionRoute = hasCutPerimeterFence ? "Wire-Cut Side Alley Shortcut" : "Sector 4 Checkpoint Gate";
        Debug.Log($"[SurvivalSystem] VICTORY! Reached {safehouseName} in {missionTimer:F1}s via {extractionRoute}.");
    }

    public void RestartLevel()
    {
        Debug.Log("[SurvivalSystem] Quick restarting Act 1 scene...");
        try
        {
            string currentScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (!string.IsNullOrEmpty(currentScene))
            {
                UnityEngine.SceneManagement.SceneManager.LoadScene(currentScene);
                return;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[SurvivalSystem] SceneManager reload fallback: {ex.Message}");
        }

        // Fallback in-memory reset if SceneManager isn't bound in testing
        hunger = 10f;
        cold = 15f;
        stamina = 100f;
        health = 100f;
        isGameOver = false;
        isGameWon = false;
        gameOverReason = "";
        if (nearestGuard != null)
        {
            nearestGuard.currentState = PatrolMover.GuardState.Patrolling;
            nearestGuard.suspicionMeter = 0f;
        }
    }

    private void CheckShelterStatus()
    {
        if (shelterZones == null || shelterZones.Length == 0)
        {
            shelterZones = FindObjectsByType<ShelterZone>(FindObjectsSortMode.None);
        }

        isSheltered = false;
        currentShelterName = "Exposed (Curfew Alley)";

        if (shelterZones != null)
        {
            foreach (var zone in shelterZones)
            {
                if (zone != null && zone.IsPlayerInside())
                {
                    isSheltered = true;
                    currentShelterName = zone.zoneName;
                    break;
                }
            }
        }
    }

    private void CheckLightExposure()
    {
        if (lightZones == null || lightZones.Length == 0)
        {
            lightZones = FindObjectsByType<LightExposureZone>(FindObjectsSortMode.None);
        }

        // Deep shadow default baseline in curfew night alleys
        float maxExposure = isSheltered ? 0.05f : 0.15f;

        if (lightZones != null)
        {
            foreach (var lz in lightZones)
            {
                if (lz != null && lz.IsPlayerInside())
                {
                    float zoneExp = lz.GetExposureAtPosition(transform.position);
                    if (zoneExp > maxExposure)
                    {
                        maxExposure = zoneExp;
                    }
                }
            }
        }

        currentLightExposure = maxExposure;
    }

    public void EatRice()
    {
        if (riceCount > 0)
        {
            riceCount--;
            hunger = Mathf.Max(0f, hunger - 35f);
            Debug.Log($"[SurvivalSystem] Ate rice. Hunger reduced to {hunger:F0}%. Rice remaining: {riceCount}");
        }
    }

    public void TakeMedicine()
    {
        if (medicineCount > 0)
        {
            medicineCount--;
            cold = Mathf.Max(0f, cold - 30f);
            health = Mathf.Min(100f, health + 20f);
            Debug.Log($"[SurvivalSystem] Took medicine. Cold reduced to {cold:F0}%. Health restored to {health:F0}%.");
        }
    }

    public void DrinkHotTea()
    {
        if (hotTeaCount > 0)
        {
            hotTeaCount--;
            cold = Mathf.Max(0f, cold - 45f);
            stamina = Mathf.Min(100f, stamina + 25f);
            health = Mathf.Min(100f, health + 10f);
            Debug.Log($"[SurvivalSystem] Drank hot tea. Cold reduced to {cold:F0}%. Stamina +25. Teas remaining: {hotTeaCount}");
        }
    }

    void OnGUI()
    {
        if (!showDebugHUD) return;

        GUI.Box(new Rect(10, 10, 285, 285), $"Jun-ho Survival Monitor [FPS: {rollingFPS:F0}]");

        GUI.Label(new Rect(20, 32, 235, 18), $"Stamina:  {stamina:F0}%");
        GUI.Label(new Rect(20, 50, 235, 18), $"Hunger:   {hunger:F0}% {(isStarving ? "[STARVING!]" : "")}");
        GUI.Label(new Rect(20, 68, 235, 18), $"Cold:     {cold:F0}% {(isShivering ? "[SHIVERING!]" : "")}");
        GUI.Label(new Rect(20, 86, 235, 18), $"Health:   {health:F0}%");

        string stanceName = playerMove != null ? playerMove.currentStance.ToString() : "N/A";
        float noise = playerMove != null ? playerMove.CurrentNoiseRadius : 0f;
        string whistleNote = (playerMove != null && playerMove.IsWhistling) ? " [WHISTLE!]" : "";
        GUI.Label(new Rect(20, 104, 235, 18), $"Stance:   {stanceName} (Noise: {noise:F1}m){whistleNote}");

        GUI.Label(new Rect(20, 122, 235, 18), $"Area:     {(isSheltered ? "SHELTERED: " : "")}{currentShelterName}");
        string lightPercent = $"{currentLightExposure * 100f:F0}%";
        GUI.Label(new Rect(20, 138, 235, 18), $"Light:    {lightPercent} exposure");

        string guardStatus = "None";
        if (nearestGuard != null)
        {
            guardStatus = $"{nearestGuard.currentState} ({nearestGuard.suspicionMeter * 100f:F0}%)";
        }
        GUI.Label(new Rect(20, 154, 235, 18), $"Guard AI: {guardStatus}");

        if (GUI.Button(new Rect(18, 174, 75, 24), $"Rice ({riceCount})"))
        {
            EatRice();
        }

        if (GUI.Button(new Rect(98, 174, 80, 24), $"Meds ({medicineCount})"))
        {
            TakeMedicine();
        }

        if (GUI.Button(new Rect(183, 174, 90, 24), $"Tea ({hotTeaCount})"))
        {
            DrinkHotTea();
        }

        GUI.Label(new Rect(20, 204, 235, 18), $"Papers: {(hasForgedPapers ? "Valid" : "None")} | Cutters: {(hasWireCutters ? "Equipped" : "None")} | Bottles: {bottleCount}");
        GUI.Label(new Rect(20, 222, 235, 18), "[Q] Whistle | [F] Bottle | [T] Cut Wire Fence | [1] Rice | [2] Med | [3] Tea");
        GUI.Label(new Rect(20, 240, 235, 18), "[E] Checkpoint Papers | [B] Bribe Guard | [R] Retry");

        // Overlay for Game Over or Victory
        if (isGameOver)
        {
            GUI.Box(new Rect(Screen.width / 2f - 180, Screen.height / 2f - 70, 360, 140), "=== MISSION FAILED ===");
            GUIStyle failStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            failStyle.normal.textColor = Color.red;
            GUI.Label(new Rect(Screen.width / 2f - 160, Screen.height / 2f - 35, 320, 25), gameOverReason, failStyle);

            if (GUI.Button(new Rect(Screen.width / 2f - 80, Screen.height / 2f + 10, 160, 32), "RETRY [R] (< 3s)"))
            {
                RestartLevel();
            }
        }
        else if (isGameWon)
        {
            GUI.Box(new Rect(Screen.width / 2f - 180, Screen.height / 2f - 85, 360, 170), "=== ACT 1 MISSION ACCOMPLISHED ===");
            GUIStyle winStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            winStyle.normal.textColor = Color.green;
            GUI.Label(new Rect(Screen.width / 2f - 160, Screen.height / 2f - 60, 320, 22), "SAFED IN THE CHONGJIN BUNKER!", winStyle);

            GUI.Label(new Rect(Screen.width / 2f - 150, Screen.height / 2f - 38, 300, 18), $"Time: {missionTimer:F1}s | Route: {extractionRoute}");
            GUI.Label(new Rect(Screen.width / 2f - 150, Screen.height / 2f - 18, 300, 18), $"Health: {health:F0}% | Rations Left: {riceCount} Rice, {hotTeaCount} Tea");

            if (GUI.Button(new Rect(Screen.width / 2f - 80, Screen.height / 2f + 15, 160, 32), "PLAY AGAIN [R]"))
            {
                RestartLevel();
            }
        }
    }
}
