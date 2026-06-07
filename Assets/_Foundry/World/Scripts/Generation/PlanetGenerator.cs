using UnityEngine;
[DisallowMultipleComponent]
public class PlanetGenerator : MonoBehaviour
{
    [Header("Planet Data")]
    public PlanetData planetData;
    [Header("Subsystems")]
    public TerrainGenerator terrainGenerator;
    public OceanGenerator oceanGenerator;
    public AtmosphereGenerator atmosphereGenerator;

    public void GeneratePlanet()
    {
        if (planetData == null)
        {
            Debug.LogWarning("[PlanetGenerator] No PlanetData assigned.");
            return;
        }
        if (terrainGenerator == null)
        {
            Debug.LogWarning("[PlanetGenerator] No TerrainGenerator assigned.");
            return;
        }

        // 1. Sync terrain generator with planet data
        terrainGenerator.terrainSettings = planetData.terrainSettings;

        // 2. Generate terrain, get the mesh back
        Mesh planetMesh = terrainGenerator.GenerateTerrain(planetData.radius);
        if (planetMesh == null) return;

        // 3. Generate ocean
        if (oceanGenerator != null && planetData.oceanSettings != null)
        {
            oceanGenerator.settings = planetData.oceanSettings;
            oceanGenerator.Generate(
                planetData.terrainSettings.resolution,
                terrainGenerator.MinElevation,
                terrainGenerator.MaxElevation);
        }

        // 4. Generate atmosphere
        if (atmosphereGenerator != null && planetData.atmosphereSettings != null)
        {
            atmosphereGenerator.settings = planetData.atmosphereSettings;
            atmosphereGenerator.Generate(
                planetData.terrainSettings.resolution,
                terrainGenerator.MaxElevation);
        }

        // 5. Apply axial tilt
        transform.rotation = Quaternion.Euler(planetData.axialTilt, 0f, 0f);
    }
}