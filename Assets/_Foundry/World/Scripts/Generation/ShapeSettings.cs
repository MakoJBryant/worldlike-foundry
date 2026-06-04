using UnityEngine;

[CreateAssetMenu(fileName = "New Shape Settings", menuName = "Worldlike Foundry/Shape Settings")]
public class ShapeSettings : ScriptableObject
{
    [Tooltip("Adjusts the overall height of the terrain relative to the base radius. Negative values create ocean basins.")]
    public float globalHeightOffset = 0f;

    [Tooltip("Noise layers used for procedural terrain deformation.")]
    public NoiseLayer[] noiseLayers;
}