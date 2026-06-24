using UnityEngine;
/// <summary>
/// Holds Fortune and Wonder values for a planet.
/// Fortune accrues from fast spinning (player-driven spin velocity).
/// Wonder accrues from slow natural rotation.
/// Both only accrue while the planet is selected and the game is active.
/// </summary>
public class PlanetStats : MonoBehaviour
{
    [Header("Values")]
    public float fortune = 0f;
    public float wonder = 0f;

    [Header("Caps")]
    public float fortuneCap = 100f;
    public float wonderCap = 10f;

    [Header("Accrual Settings")]
    [Tooltip("Spin velocity magnitude above this threshold counts as fast spin for Fortune.")]
    public float fortuneSpinThreshold = 5f;
    [Tooltip("How long after a fling Fortune continues to accrue, in seconds.")]
    public float fortuneAccrualWindow = 2f;
    [Tooltip("Fortune gained per second while spinning fast.")]
    public float fortuneAccrualRate = 1f;
    [Tooltip("Wonder gained per second while spinning naturally (below threshold).")]
    public float wonderAccrualRate = 0.5f;

    [HideInInspector] public float currentSpinMagnitude = 0f;
    [HideInInspector] public bool isSelected = false;
    [HideInInspector] public bool gameActive = false;

    float fortuneWindowTimer = 0f;

    void Update()
    {
        if (!gameActive || !isSelected)
        {
            fortuneWindowTimer = 0f;
            return;
        }

        if (currentSpinMagnitude >= fortuneSpinThreshold)
            fortuneWindowTimer = fortuneAccrualWindow;

        if (fortuneWindowTimer > 0f)
        {
            if (fortune < fortuneCap)
                fortune = Mathf.Min(fortune + fortuneAccrualRate * Time.deltaTime, fortuneCap);
            fortuneWindowTimer -= Time.deltaTime;
        }
        else
        {
            if (wonder < wonderCap)
                wonder = Mathf.Min(wonder + wonderAccrualRate * Time.deltaTime, wonderCap);
        }
    }

    public int FortuneInt => Mathf.FloorToInt(fortune);
    public int WonderInt => Mathf.FloorToInt(wonder);
}