using UnityEngine;

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
    public bool hasForgedPapers = true;

    [Header("Debug HUD")]
    public bool showDebugHUD = true;

    private PlayerMove playerMove;

    void Awake()
    {
        playerMove = GetComponent<PlayerMove>();
    }

    void Update()
    {
        float dt = Time.deltaTime;

        // Base hunger accumulation
        hunger = Mathf.Clamp(hunger + hungerRate * dt, 0f, 100f);

        // Base cold accumulation
        cold = Mathf.Clamp(cold + coldRate * dt, 0f, 100f);

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
            float drain = staminaDrainRate * (1f + (hunger / 100f) * 0.5f);
            stamina = Mathf.Max(0f, stamina - drain * dt);
            if (stamina <= 0.5f && playerMove != null)
            {
                playerMove.currentStance = PlayerMove.Stance.Walk;
            }
        }
        else
        {
            float recovery = staminaRecoverRate * (1f - (cold / 100f) * 0.4f);
            stamina = Mathf.Min(100f, stamina + recovery * dt);
        }

        // Quick input for item consumption
        if (Input.GetKeyDown(KeyCode.Alpha1)) EatRice();
        if (Input.GetKeyDown(KeyCode.Alpha2)) TakeMedicine();
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

    void OnGUI()
    {
        if (!showDebugHUD) return;

        GUI.Box(new Rect(10, 10, 240, 180), "Jun-ho Survival Monitor (Act 1)");

        GUI.Label(new Rect(20, 35, 200, 20), $"Stamina:  {stamina:F0}%");
        GUI.Label(new Rect(20, 55, 200, 20), $"Hunger:   {hunger:F0}%");
        GUI.Label(new Rect(20, 75, 200, 20), $"Cold:     {cold:F0}%");
        GUI.Label(new Rect(20, 95, 200, 20), $"Health:   {health:F0}%");

        string stanceName = playerMove != null ? playerMove.currentStance.ToString() : "N/A";
        GUI.Label(new Rect(20, 115, 200, 20), $"Stance:   {stanceName}");

        if (GUI.Button(new Rect(20, 140, 95, 25), $"Eat Rice ({riceCount})"))
        {
            EatRice();
        }

        if (GUI.Button(new Rect(125, 140, 110, 25), $"Medicine ({medicineCount})"))
        {
            TakeMedicine();
        }
    }
}
