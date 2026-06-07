using UnityEngine;
[DisallowMultipleComponent]
public class PlanetGenerator : MonoBehaviour
{
    [Header("Planet Settings")]
    public PlanetSettings planetSettings;
    [Header("Subsystems")]
    public TerrainGenerator terrainGenerator;
    public OceanGenerator oceanGenerator;
    public AtmosphereGenerator atmosphereGenerator;

    public void GeneratePlanet()
    {
        if (planetSettings == null)
        {
            Debug.LogWarning("[PlanetGenerator] No PlanetSettings assigned.");
            return;
        }
        if (terrainGenerator == null)
        {
            Debug.LogWarning("[PlanetGenerator] No TerrainGenerator assigned.");
            return;
        }

        // 1. Sync terrain generator with planet settings
        terrainGenerator.terrainSettings = planetSettings.terrainSettings;

        // 2. Generate terrain, get the mesh back
        Mesh planetMesh = terrainGenerator.GenerateTerrain(planetSettings.radius);
        if (planetMesh == null) return;

        // 3. Generate ocean
        if (oceanGenerator != null && planetSettings.oceanSettings != null)
        {
            oceanGenerator.settings = planetSettings.oceanSettings;
            oceanGenerator.Generate(
                planetSettings.terrainSettings.resolution,
                terrainGenerator.MinElevation,
                terrainGenerator.MaxElevation);
        }

        // 4. Generate atmosphere
        if (atmosphereGenerator != null && planetSettings.atmosphereSettings != null)
        {
            atmosphereGenerator.settings = planetSettings.atmosphereSettings;
            atmosphereGenerator.Generate(
                planetSettings.terrainSettings.resolution,
                terrainGenerator.MaxElevation);
        }

        // 5. Apply axial tilt
        transform.rotation = Quaternion.Euler(planetSettings.axialTilt, 0f, 0f);
    }
}