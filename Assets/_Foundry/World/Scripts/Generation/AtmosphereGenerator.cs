using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class AtmosphereGenerator : MonoBehaviour
{
    public AtmosphereSettings settings;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;
    private MaterialPropertyBlock propertyBlock;

    /// <summary>
    /// Generates the atmosphere glow sphere around the planet.
    /// </summary>
    public void Generate(int resolution, float maxElevation)
    {
        if (settings == null)
        {
            Debug.LogWarning("[AtmosphereGenerator] No AtmosphereSettings assigned.");
            return;
        }

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        propertyBlock = new MaterialPropertyBlock();

        if (mesh == null)
            mesh = new Mesh { name = "Atmosphere Mesh" };

        mesh.Clear();

        float atmosphereRadius = maxElevation * (1f + settings.thicknessMultiplier);

        SphereCreator.CreateSphereMesh(
            resolution, atmosphereRadius,
            out Vector3[] vertices,
            out int[] triangles,
            out Vector2[] uvs);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;

        if (settings.atmosphereMaterial != null)
        {
            meshRenderer.sharedMaterial = settings.atmosphereMaterial;
            propertyBlock.SetColor("_Color", settings.atmosphereColor);
            meshRenderer.SetPropertyBlock(propertyBlock);
        }
        else
        {
            Debug.LogWarning("[AtmosphereGenerator] No atmosphere material assigned.");
        }

        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Updates the sun direction on the atmosphere shader each frame.
    /// Called by AtmosphereSunController.
    /// </summary>
    public void SetSunDirection(Vector3 direction)
    {
        if (meshRenderer == null || propertyBlock == null) return;

        propertyBlock.SetVector("_SunDirection", direction);
        meshRenderer.SetPropertyBlock(propertyBlock);
    }
}