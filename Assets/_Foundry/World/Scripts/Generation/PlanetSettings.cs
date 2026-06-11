using UnityEngine;
[CreateAssetMenu(fileName = "New Planet", menuName = "Worldlike Foundry/Planet")]
public class PlanetSettings : ScriptableObject
{
    [Header("Planet")]
    public float radius = 100f;

    [Header("Settings")]
    public TerrainSettings terrainSettings;
    public OceanSettings oceanSettings;
    public AtmosphereSettings atmosphereSettings;

    [Header("Rotation")]
    [Tooltip("Degrees per second the planet rotates around its axis.")]
    public float rotationSpeed = 10f;
    [Tooltip("Tilt of the rotation axis in degrees. 0 = straight up, 23.5 = Earth-like.")]
    [Range(0f, 90f)]
    public float axialTilt = 23.5f;

    [Header("Baked Assets")]
    [Tooltip("The baked mesh generated from the settings above. Assigned automatically when you bake.")]
    public Mesh bakedMesh;
}