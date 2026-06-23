using UnityEngine;

/// <summary>
/// Holds Fortune and Wonder values for a planet.
/// Fortune accrues from fast spinning (player-driven spin velocity).
/// Wonder accrues from slow natural rotation.
/// Both caps are independently tunable in the inspector.
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
    [Tooltip("Spin velocity magnitude above this threshold counts as 'fast spin' for Fortune.")]
    public float fortuneSpinThreshold = 50f;
    [Tooltip("Fortune gained per second while spinning fast.")]
    public float fortuneAccrualRate = 1f;
    [Tooltip("Wonder gained per second while spinning naturally (below threshold).")]
    public float wonderAccrualRate = 0.5f;

    [HideInInspector] public float currentSpinMagnitude = 0f;

    void Update()
    {
        if (currentSpinMagnitude >= fortuneSpinThreshold)
        {
            if (fortune < fortuneCap)
                fortune = Mathf.Min(fortune + fortuneAccrualRate * Time.deltaTime, fortuneCap);
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