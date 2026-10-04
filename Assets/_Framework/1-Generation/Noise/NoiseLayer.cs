using UnityEngine;

public enum NoiseType { Standard, Ridge }

[CreateAssetMenu(fileName = "NoiseLayer", menuName = "Worldlike Foundry/Noise Layer")]
public class NoiseLayer : ScriptableObject
{
    public bool enabled = true;
    public NoiseType noiseType = NoiseType.Standard;
    public bool useFirstLayerAsMask = false;

    [Header("Strength")]
    public float strength = 1f;        // How much this layer displaces the surface
    public float minValue = 0f;        // Noise floor — values below this are clamped (useful for flat oceans)

    [Header("Frequency")]
    public float roughness = 2f;       // Base frequency of the noise (higher = more jagged terrain)
    public float lacunarity = 2f;      // Frequency multiplier per octave

    [Header("Detail")]
    public int octaves = 4;            // Number of noise layers stacked (more = finer detail)
    [Range(0, 1)]
    public float persistence = 0.5f;   // Amplitude decay per octave (lower = smoother)

    [Header("Offset")]
    public Vector3 offset = Vector3.zero; // Shifts the noise sample point (use for variation between planets)
}