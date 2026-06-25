using UnityEngine;

public class PlanetStats : MonoBehaviour
{
    [Header("Values")]
    public float fortune = 0f;
    public float wonder = 0f;

    [Header("Caps")]
    public float fortuneCap = 100f;
    public float wonderCap = 10f;

    [Header("Fortune Accrual")]
    [Tooltip("Spin velocity magnitude above this threshold counts as fast spin for Fortune.")]
    public float fortuneSpinThreshold = 5f;
    [Tooltip("How long after a fling Fortune continues to accrue, in seconds.")]
    public float fortuneAccrualWindow = 2f;
    [Tooltip("Fortune gained per second while spinning fast.")]
    public float fortuneAccrualRate = 1f;

    [Header("Wonder (Fuel)")]
    [Tooltip("Wonder drained per second while spinning above threshold.")]
    public float wonderDrainRate = 1f;
    [Tooltip("Wonder regenerated per second while selected and not spinning.")]
    public float wonderRegenRate = 0.5f;

    [HideInInspector] public float currentSpinMagnitude = 0f;
    [HideInInspector] public bool isSelected = false;
    [HideInInspector] public bool gameActive = false;

    // Read by PlanetUpgradeManager to know when to brake
    public bool IsOutOfWonder => wonder <= 0f;
    // True while actively earning fortune
    public bool IsSpinning => fortuneWindowTimer > 0f;

    float fortuneWindowTimer = 0f;

    void Update()
    {
        if (!gameActive || !isSelected)
        {
            fortuneWindowTimer = 0f;
            return;
        }

        bool spinningFast = currentSpinMagnitude >= fortuneSpinThreshold && wonder > 0f;

        if (spinningFast)
            fortuneWindowTimer = fortuneAccrualWindow;

        if (fortuneWindowTimer > 0f)
        {
            // Drain Wonder while spinning
            wonder = Mathf.Max(0f, wonder - wonderDrainRate * Time.deltaTime);

            // Earn Fortune while Wonder holds out
            if (wonder > 0f && fortune < fortuneCap)
                fortune = Mathf.Min(fortune + fortuneAccrualRate * Time.deltaTime, fortuneCap);

            fortuneWindowTimer -= Time.deltaTime;
        }
        else
        {
            // Not spinning — regenerate Wonder slowly
            if (wonder < wonderCap)
                wonder = Mathf.Min(wonder + wonderRegenRate * Time.deltaTime, wonderCap);
        }
    }

    public int FortuneInt => Mathf.FloorToInt(fortune);
    public int WonderInt => Mathf.FloorToInt(wonder);
}