using UnityEngine;
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
public class ShapeGenerator : MonoBehaviour
{
    public ShapeSettings shapeSettings;
    public float MinElevation { get; private set; }
    public float MaxElevation { get; private set; }
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;
    private Mesh previewMesh;

    void Awake()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
    }

    public Mesh GenerateShape(float radius, int resolution)
    {
        if (shapeSettings == null ||
            shapeSettings.noiseLayers == null ||
            shapeSettings.noiseLayers.Length == 0)
        {
            Debug.LogWarning("ShapeGenerator: ShapeSettings missing or has no noise layers.");
            return null;
        }
        meshFilter = GetComponent<MeshFilter>();
        meshCollider = GetComponent<MeshCollider>();
        previewMesh = new Mesh { name = "Planet Shape Preview" };
        SphereCreator.CreateSphereMesh(
            resolution, 1f,
            out Vector3[] vertices,
            out int[] triangles,
            out Vector2[] uvs);
        TerrainGenerator.ApplyTerrainDeformation(
            vertices, shapeSettings,
            out Vector3[] displaced,
            out float min, out float max);
        MinElevation = min * radius;
        MaxElevation = max * radius;
        for (int i = 0; i < displaced.Length; i++)
            displaced[i] = displaced[i].normalized * displaced[i].magnitude * radius;
        previewMesh.Clear();
        previewMesh.vertices = displaced;
        previewMesh.triangles = triangles;
        previewMesh.uv = uvs;
        previewMesh.RecalculateNormals();
        previewMesh.RecalculateBounds();
        meshFilter.sharedMesh = previewMesh;
        meshCollider.sharedMesh = null;
        meshCollider.sharedMesh = previewMesh;
        return previewMesh;
    }
}