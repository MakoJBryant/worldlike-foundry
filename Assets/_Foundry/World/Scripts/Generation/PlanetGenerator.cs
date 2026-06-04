using UnityEngine;

[DisallowMultipleComponent]
public class PlanetGenerator : MonoBehaviour
{
    [Header("Planet Data")]
    public PlanetData planetData;

    [Header("Subsystems")]
    public ShapeGenerator shapeGenerator;
    public OceanGenerator oceanGenerator;
    public AtmosphereGenerator atmosphereGenerator;

    /// <summary>
    /// Generates the full planet in memory as a preview.
    /// Does not save anything to disk.
    /// </summary>
    public void GeneratePlanet()
    {
        if (planetData == null)
        {
            Debug.LogWarning("[PlanetGenerator] No PlanetData assigned.");
            return;
        }

        if (shapeGenerator == null)
        {
            Debug.LogWarning("[PlanetGenerator] No ShapeGenerator assigned.");
            return;
        }

        // 1. Sync shape generator with planet data
        shapeGenerator.resolution = planetData.resolution;
        shapeGenerator.radius = planetData.radius;
        shapeGenerator.shapeSettings = planetData.shapeSettings;

        // 2. Generate terrain shape, get the mesh back
        Mesh planetMesh = shapeGenerator.GenerateShape();
        if (planetMesh == null) return;

        // 3. Apply vertex colors from color ramp
        if (planetData.colorRampSettings != null)
        {
            ApplyVertexColors(planetMesh, shapeGenerator.MinElevation, shapeGenerator.MaxElevation);
        }

        // 4. Generate ocean
        if (oceanGenerator != null && planetData.oceanSettings != null)
        {
            oceanGenerator.settings = planetData.oceanSettings;
            oceanGenerator.Generate(
                planetData.resolution,
                shapeGenerator.MinElevation,
                shapeGenerator.MaxElevation);
        }

        // 5. Generate atmosphere
        if (atmosphereGenerator != null && planetData.atmosphereSettings != null)
        {
            atmosphereGenerator.settings = planetData.atmosphereSettings;
            atmosphereGenerator.Generate(
                planetData.resolution,
                shapeGenerator.MaxElevation);
        }

        // 6. Apply axial tilt
        transform.rotation = Quaternion.Euler(planetData.axialTilt, 0f, 0f);
    }

    private void ApplyVertexColors(Mesh mesh, float minElevation, float maxElevation)
    {
        Vector3[] vertices = mesh.vertices;
        Color[] colors = new Color[vertices.Length];
        float elevRange = maxElevation - minElevation;

        for (int i = 0; i < vertices.Length; i++)
        {
            float height = vertices[i].magnitude;
            float normalizedElev = elevRange > 0f
                ? (height - minElevation) / elevRange
                : 0f;

            colors[i] = ColorRampGenerator.SampleColor(
                planetData.colorRampSettings, normalizedElev);
        }

        mesh.colors = colors;
    }
}