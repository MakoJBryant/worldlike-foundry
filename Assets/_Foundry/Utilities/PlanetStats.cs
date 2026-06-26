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
    public float fortuneSpinThreshold = 5f;
    public float fortuneAccrualWindow = 2f;
    public float fortuneAccrualRate = 1f;

    [Header("Wonder Regen")]
    [Tooltip("Wonder regenerated per second while selected and not spinning.")]
    public float wonderRegenRate = 0.5f;

    [HideInInspector] public float currentSpinMagnitude = 0f;
    [HideInInspector] public bool isSelected = false;
    [HideInInspector] public bool gameActive = false;

    public bool IsOutOfWonder => wonder <= 0f;
    public bool IsSpinning => fortuneWindowTimer > 0f;

    float fortuneWindowTimer = 0f;

    void Update()
    {
        if (!gameActive || !isSelected)
        {
            fortuneWindowTimer = 0f;
            return;
        }

        bool spinningFast = currentSpinMagnitude >= fortuneSpinThreshold;

        if (spinningFast)
            fortuneWindowTimer = fortuneAccrualWindow;

        if (fortuneWindowTimer > 0f)
        {
            // Earn Fortune while spinning
            if (fortune < fortuneCap)
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