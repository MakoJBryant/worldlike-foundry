using UnityEngine;
/// <summary>
/// Applies procedural terrain deformation to a base sphere using TerrainSettings.
/// Operates in normalized (radius = 1) space — scaling to world radius happens in TerrainGenerator.
/// </summary>
public static class SphereDeformer
{
    public static void ApplyTerrainDeformation(
        Vector3[] baseVertices,
        TerrainSettings terrainSettings,
        out Vector3[] displacedVertices,
        out float minElevation,
        out float maxElevation)
    {
        displacedVertices = new Vector3[baseVertices.Length];
        minElevation = float.MaxValue;
        maxElevation = -float.MaxValue;

        if (terrainSettings == null)
        {
            Debug.LogError("[SphereDeformer] TerrainSettings is null.");
            return;
        }

        for (int i = 0; i < baseVertices.Length; i++)
        {
            Vector3 normal = baseVertices[i].normalized;
            float displacement = terrainSettings.globalHeightOffset;
            float firstLayerValue = 0f;

            if (terrainSettings.noiseLayers != null)
            {
                for (int layerIndex = 0; layerIndex < terrainSettings.noiseLayers.Length; layerIndex++)
                {
                    NoiseLayer layer = terrainSettings.noiseLayers[layerIndex];
                    if (layer == null || !layer.enabled) continue;

                    float layerValue = EvaluateLayer(layer, normal);

                    if (layerIndex == 0)
                        firstLayerValue = layerValue;

                    if (layer.useFirstLayerAsMask && firstLayerValue <= 0f)
                        layerValue = 0f;

                    displacement += layerValue * layer.strength;
                }
            }

            Vector3 displaced = normal * (baseVertices[i].magnitude + displacement);
            displacedVertices[i] = displaced;

            float height = displaced.magnitude;
            minElevation = Mathf.Min(minElevation, height);
            maxElevation = Mathf.Max(maxElevation, height);
        }
    }

    private static float EvaluateLayer(NoiseLayer layer, Vector3 normal)
    {
        float noise = 0f;
        float frequency = layer.roughness;
        float amplitude = 1f;
        float totalAmplitude = 0f;

        // Push samples far from the origin to break the point-symmetry in Perlin noise
        // that causes quad mirroring on cube-spheres. The offset is applied after frequency
        // scaling so it stays spatially consistent across octaves.
        Vector3 seedOffset = layer.offset + new Vector3(1000f, 1000f, 1000f);

        for (int o = 0; o < layer.octaves; o++)
        {
            Vector3 p = (normal * frequency) + seedOffset;
            float v = PerlinNoise3D.GenerateNoise(p.x, p.y, p.z);

            // Standard: remap [0,1] to [-1,1]
            // Ridge: fold the noise to create sharp ridgelines
            v = layer.noiseType == NoiseType.Ridge
                ? 1f - Mathf.Abs(v * 2f - 1f)
                : v * 2f - 1f;

            noise += v * amplitude;
            totalAmplitude += amplitude;
            amplitude *= layer.persistence;
            frequency *= layer.lacunarity;
        }

        return (totalAmplitude == 0f ? 0f : noise / totalAmplitude) + layer.minValue;
    }
}