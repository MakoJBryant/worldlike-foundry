using UnityEngine;

/// <summary>
/// Holds Fortune and Wonder values for a planet.
/// Fortune accrues from fast spinning (player-driven spin velocity).
/// Wonder accrues from slow natural rotation.
/// Both cap at 9.
/// </summary>
public class PlanetStats : MonoBehaviour
{
    [Header("Values")]
    public float fortune = 0f;
    public float wonder = 0f;

    [Header("Accrual Settings")]
    [Tooltip("Spin velocity magnitude above this threshold counts as 'fast spin' for Fortune.")]
    public float fortuneSpinThreshold = 50f;
    [Tooltip("Fortune gained per second while spinning fast.")]
    public float fortuneAccrualRate = 1f;
    [Tooltip("Wonder gained per second while spinning naturally (below threshold).")]
    public float wonderAccrualRate = 0.5f;

    public const int MaxValue = 9;

    // Set by SolarSystemManager each frame
    [HideInInspector] public float currentSpinMagnitude = 0f;

    void Update()
    {
        if (currentSpinMagnitude >= fortuneSpinThreshold)
        {
            if (fortune < MaxValue)
                fortune = Mathf.Min(fortune + fortuneAccrualRate * Time.deltaTime, MaxValue);
        }
        else
        {
            if (wonder < MaxValue)
                wonder = Mathf.Min(wonder + wonderAccrualRate * Time.deltaTime, MaxValue);
        }
    }

    public int FortuneInt => Mathf.FloorToInt(fortune);
    public int WonderInt => Mathf.FloorToInt(wonder);
}