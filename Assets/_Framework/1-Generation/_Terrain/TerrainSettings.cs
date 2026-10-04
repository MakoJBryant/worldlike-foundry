using UnityEngine;
[CreateAssetMenu(fileName = "New Terrain Settings", menuName = "Worldlike Foundry/Terrain Settings")]
public class TerrainSettings : ScriptableObject
{
    [Range(2, 256)]
    [Tooltip("Number of vertices used to generate the terrain mesh.")]
    public int resolution = 64;
    [Tooltip("Adjusts the overall height of the terrain relative to the base radius. Negative values create ocean basins.")]
    public float globalHeightOffset = 0f;
    [Tooltip("Noise layers used for procedural terrain deformation.")]
    public NoiseLayer[] noiseLayers;
}