using UnityEngine;
using UnityEngine.Rendering;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class OceanGenerator : MonoBehaviour
{
    public OceanSettings settings;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private Mesh mesh;

    /// <summary>
    /// Generates the ocean sphere mesh sized to sit at sea level between min and max elevation.
    /// </summary>
    public void Generate(int resolution, float minElevation, float maxElevation)
    {
        if (settings == null)
        {
            Debug.LogWarning("[OceanGenerator] No OceanSettings assigned.");
            return;
        }

        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();

        if (mesh == null)
            mesh = new Mesh { name = "Ocean Mesh" };

        mesh.Clear();

        float oceanRadius = Mathf.Lerp(minElevation, maxElevation, settings.seaLevel);

        SphereCreator.CreateSphereMesh(
            resolution, oceanRadius,
            out Vector3[] vertices,
            out int[] triangles,
            out Vector2[] uvs);

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        meshFilter.sharedMesh = mesh;

        if (settings.oceanMaterial != null)
        {
            meshRenderer.sharedMaterial = settings.oceanMaterial;
        }
        else
        {
            Debug.LogWarning("[OceanGenerator] OceanSettings has no material assigned.");
        }

        meshRenderer.shadowCastingMode = ShadowCastingMode.Off;
        meshRenderer.receiveShadows = false;

        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
    }
}