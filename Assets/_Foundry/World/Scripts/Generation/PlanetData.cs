using UnityEngine;

[CreateAssetMenu(fileName = "New Planet", menuName = "Worldlike Foundry/Planet")]
public class PlanetData : ScriptableObject
{
    [Header("Generation")]
    [Range(2, 256)]
    public int resolution = 64;
    public float radius = 1000f;

    [Header("Settings")]
    public ShapeSettings shapeSettings;
    public ColorRampSettings colorRampSettings;
    public OceanSettings oceanSettings;
    public AtmosphereSettings atmosphereSettings;

    [Header("Rotation")]
    [Tooltip("Degrees per second the planet rotates around its axis.")]
    public float rotationSpeed = 10f;
    [Tooltip("Tilt of the rotation axis in degrees. 0 = straight up, 23.5 = Earth-like.")]
    [Range(0f, 90f)]
    public float axialTilt = 23.5f;

    [Header("Orbit")]
    [Tooltip("Distance from the star this planet orbits.")]
    public float orbitRadius = 5000f;
    [Tooltip("Degrees per second this planet moves along its orbit.")]
    public float orbitSpeed = 5f;

    [Header("Baked Assets")]
    [Tooltip("The baked mesh generated from the settings above. Assigned automatically when you bake.")]
    public Mesh bakedMesh;
}