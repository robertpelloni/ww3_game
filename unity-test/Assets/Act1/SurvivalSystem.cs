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
    public bool hasForgedPapers = true;

    [Header("Environmental Status")]
    public bool isSheltered = false;
    public string currentShelterName = "Exposed (Curfew Alley)";
    [Range(0f, 1f)]
    public float currentLightExposure = 0.15f; // Ambient dark alley default

    [Header("Debuffs & Condition")]
    public bool isShivering = false;
    public bool isStarving = false;

    [Header("Game Loop State")]
    public bool isGameOver = false;
    public bool isGameWon = false;
    public string gameOverReason = "";

    [Header("Debug HUD")]
    public bool showDebugHUD = true;

    private PlayerMove playerMove;
    private PatrolMover nearestGuard;
    private ShelterZone[] shelterZones;
    private LightExposureZone[] lightZones;
    private SafehouseTrigger safehouseTrigger;

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

        // Check shelter proximity & ambient light exposure
        CheckShelterStatus();
        CheckLightExposure();

        // Base hunger accumulation
        hunger = Mathf.Clamp(hunger + hungerRate * dt, 0f, 100f);

        // Cold accumulation / recovery based on shelter
        if (isSheltered)
        {
            // Recover from cold while near warmth source
            cold = Mathf.Max(0f, cold - 6.0f * dt);
        }
        else
        {
            // Suffer outdoor curfew frostbite
            cold = Mathf.Clamp(cold + coldRate * dt, 0f, 100f);
        }

        // Shivering mechanic (Cold > 70%): generates acoustic noise & reduces stamina regen
        isShivering = cold >= 70f;
        isStarving = hunger >= 80f;

        // Health penalties for extreme cold/hunger
        if (cold >= 90f || hunger >= 90f)
        {
            health = Mathf.Max(0f, health - 2f * dt);
        }

        // Stamina logic
        bool isMoving = playerMove != null && playerMove.IsMoving;
        bool isSprinting = playerMove != null && playerMove.currentStance == PlayerMove.Stance.Sprint && isMoving;

        if (isSprinting)
        {
            // Starvation increases sprint drain significantly
            float starvationMultiplier = isStarving ? 1.75f : 1.0f;
            float drain = staminaDrainRate * (1f + (hunger / 100f) * 0.5f) * starvationMultiplier;
            stamina = Mathf.Max(0f, stamina - drain * dt);
            if (stamina <= 0.5f && playerMove != null)
            {
                playerMove.currentStance = PlayerMove.Stance.Walk;
            }
        }
        else
        {
            // Cold shivering reduces recovery rate
            float shiveringPenalty = isShivering ? 0.4f : 1.0f;
            float recovery = staminaRecoverRate * (1f - (cold / 100f) * 0.4f) * shiveringPenalty;
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
        Debug.Log($"[SurvivalSystem] VICTORY! Reached {safehouseName}.");
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
                    if (lz.exposureLevel > maxExposure)
                    {
                        maxExposure = lz.exposureLevel;
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

        GUI.Box(new Rect(10, 10, 275, 270), "Jun-ho Survival Monitor (Act 1)");

        GUI.Label(new Rect(20, 32, 235, 18), $"Stamina:  {stamina:F0}%");
        GUI.Label(new Rect(20, 50, 235, 18), $"Hunger:   {hunger:F0}% {(isStarving ? "[STARVING!]" : "")}");
        GUI.Label(new Rect(20, 68, 235, 18), $"Cold:     {cold:F0}% {(isShivering ? "[SHIVERING!]" : "")}");
        GUI.Label(new Rect(20, 86, 235, 18), $"Health:   {health:F0}%");

        string stanceName = playerMove != null ? playerMove.currentStance.ToString() : "N/A";
        float noise = playerMove != null ? playerMove.CurrentNoiseRadius : 0f;
        if (isShivering) noise += 1.5f; // Teeth chattering noise footprint
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

        GUI.Label(new Rect(20, 204, 235, 18), $"Papers: {(hasForgedPapers ? "Forged ID (Valid)" : "None")}");
        GUI.Label(new Rect(20, 222, 235, 18), "[Q] Whistle | [1] Rice | [2] Med | [3] Tea");
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
            GUI.Box(new Rect(Screen.width / 2f - 180, Screen.height / 2f - 70, 360, 140), "=== MISSION ACCOMPLISHED ===");
            GUIStyle winStyle = new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold };
            winStyle.normal.textColor = Color.green;
            GUI.Label(new Rect(Screen.width / 2f - 160, Screen.height / 2f - 35, 320, 25), "SAFED IN THE BUNKER!", winStyle);

            if (GUI.Button(new Rect(Screen.width / 2f - 80, Screen.height / 2f + 10, 160, 32), "PLAY AGAIN [R]"))
            {
                RestartLevel();
            }
        }
    }
}
