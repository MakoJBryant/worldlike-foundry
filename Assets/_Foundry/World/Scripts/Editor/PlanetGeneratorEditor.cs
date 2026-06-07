using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlanetGenerator))]
public class PlanetGeneratorEditor : Editor
{
    private PlanetGenerator generator;
    private bool showBake = false;

    void OnEnable()
    {
        generator = (PlanetGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ── Planet Data ──────────────────────────────────────
        EditorGUILayout.LabelField("Planet", EditorStyles.boldLabel);
        generator.planetData = (PlanetData)EditorGUILayout.ObjectField(
            "Planet Data", generator.planetData, typeof(PlanetData), false);

        EditorGUILayout.Space();

        if (generator.planetData == null)
        {
            EditorGUILayout.HelpBox(
                "Assign a PlanetData asset to begin editing.",
                MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        // ── Planet Settings ──────────────────────────────────
        EditorGUILayout.LabelField("Planet Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            generator.planetData.radius = EditorGUILayout.FloatField("Radius", generator.planetData.radius);
            generator.planetData.axialTilt = EditorGUILayout.Slider("Axial Tilt", generator.planetData.axialTilt, 0f, 90f);
            generator.planetData.rotationSpeed = EditorGUILayout.FloatField("Rotation Speed", generator.planetData.rotationSpeed);
            generator.planetData.orbitRadius = EditorGUILayout.FloatField("Orbit Radius", generator.planetData.orbitRadius);
            generator.planetData.orbitSpeed = EditorGUILayout.FloatField("Orbit Speed", generator.planetData.orbitSpeed);
            if (check.changed)
            {
                EditorUtility.SetDirty(generator.planetData);
                generator.GeneratePlanet();
                SceneView.RepaintAll();
            }
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        // ── Subsystems ───────────────────────────────────────
        EditorGUILayout.LabelField("Subsystems", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        generator.terrainGenerator = (TerrainGenerator)EditorGUILayout.ObjectField(
            "Terrain", generator.terrainGenerator, typeof(TerrainGenerator), true);
        generator.oceanGenerator = (OceanGenerator)EditorGUILayout.ObjectField(
            "Ocean", generator.oceanGenerator, typeof(OceanGenerator), true);
        generator.atmosphereGenerator = (AtmosphereGenerator)EditorGUILayout.ObjectField(
            "Atmosphere", generator.atmosphereGenerator, typeof(AtmosphereGenerator), true);
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        // ── Actions ──────────────────────────────────────────
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Preview", GUILayout.Height(32)))
        {
            generator.GeneratePlanet();
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space();

        showBake = EditorGUILayout.Foldout(showBake, "Bake", true, EditorStyles.foldoutHeader);
        if (showBake)
        {
            EditorGUI.indentLevel++;

            if (generator.planetData.bakedMesh != null)
            {
                EditorGUILayout.HelpBox(
                    $"Baked mesh: {generator.planetData.bakedMesh.vertexCount} vertices.",
                    MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox(
                    "No baked mesh yet. Generate a preview then bake.",
                    MessageType.Warning);
            }

            EditorGUILayout.Space();

            GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
            if (GUILayout.Button("Bake Planet", GUILayout.Height(32)))
            {
                BakePlanet();
            }
            GUI.backgroundColor = Color.white;

            EditorGUI.indentLevel--;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void BakePlanet()
    {
        if (generator.planetData == null) return;

        generator.GeneratePlanet();

        MeshFilter mf = generator.terrainGenerator?.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogWarning("[PlanetGeneratorEditor] No mesh found to bake.");
            return;
        }

        string folder = "Assets/_Foundry/World/Meshes";
        string assetName = generator.planetData.name + "_Baked.asset";
        string path = $"{folder}/{assetName}";

        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Foundry/World", "Meshes");

        Mesh bakedMesh = Instantiate(mf.sharedMesh);
        bakedMesh.name = generator.planetData.name + "_Baked";

        Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null)
        {
            existing.Clear();
            existing.vertices = bakedMesh.vertices;
            existing.triangles = bakedMesh.triangles;
            existing.uv = bakedMesh.uv;
            existing.colors = bakedMesh.colors;
            existing.RecalculateNormals();
            existing.RecalculateBounds();
            EditorUtility.SetDirty(existing);
            generator.planetData.bakedMesh = existing;
        }
        else
        {
            AssetDatabase.CreateAsset(bakedMesh, path);
            generator.planetData.bakedMesh = bakedMesh;
        }

        EditorUtility.SetDirty(generator.planetData);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[PlanetGeneratorEditor] Planet baked to {path}");
    }
}