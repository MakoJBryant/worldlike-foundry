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

        // ── Planet Settings ──────────────────────────────────
        EditorGUILayout.LabelField("Planet", EditorStyles.boldLabel);
        generator.planetSettings = (PlanetSettings)EditorGUILayout.ObjectField(
            "Planet Settings", generator.planetSettings, typeof(PlanetSettings), false);

        EditorGUILayout.Space();

        if (generator.planetSettings == null)
        {
            EditorGUILayout.HelpBox(
                "Assign a PlanetSettings asset to begin editing.",
                MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        // ── Planet Settings Fields ───────────────────────────
        EditorGUILayout.LabelField("Planet Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            generator.planetSettings.radius = EditorGUILayout.FloatField("Radius", generator.planetSettings.radius);
            generator.planetSettings.axialTilt = EditorGUILayout.Slider("Axial Tilt", generator.planetSettings.axialTilt, 0f, 90f);
            generator.planetSettings.rotationSpeed = EditorGUILayout.FloatField("Rotation Speed", generator.planetSettings.rotationSpeed);
            generator.planetSettings.orbitRadius = EditorGUILayout.FloatField("Orbit Radius", generator.planetSettings.orbitRadius);
            generator.planetSettings.orbitSpeed = EditorGUILayout.FloatField("Orbit Speed", generator.planetSettings.orbitSpeed);
            if (check.changed)
            {
                EditorUtility.SetDirty(generator.planetSettings);
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

        if (GUILayout.Button("Save Settings", GUILayout.Height(32)))
        {
            EditorUtility.SetDirty(generator.planetSettings);
            AssetDatabase.SaveAssets();
            Debug.Log("[PlanetGeneratorEditor] Planet settings saved.");
        }

        EditorGUILayout.Space();

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

            if (generator.planetSettings.bakedMesh != null)
            {
                EditorGUILayout.HelpBox(
                    $"Baked mesh: {generator.planetSettings.bakedMesh.vertexCount} vertices.",
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
        if (generator.planetSettings == null) return;

        generator.GeneratePlanet();

        MeshFilter mf = generator.terrainGenerator?.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogWarning("[PlanetGeneratorEditor] No mesh found to bake.");
            return;
        }

        string folder = "Assets/_Foundry/World/Meshes";
        string assetName = generator.planetSettings.name + "_Baked.asset";
        string path = $"{folder}/{assetName}";

        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Foundry/World", "Meshes");

        Mesh bakedMesh = Instantiate(mf.sharedMesh);
        bakedMesh.name = generator.planetSettings.name + "_Baked";

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
            generator.planetSettings.bakedMesh = existing;
        }
        else
        {
            AssetDatabase.CreateAsset(bakedMesh, path);
            generator.planetSettings.bakedMesh = bakedMesh;
        }

        EditorUtility.SetDirty(generator.planetSettings);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log($"[PlanetGeneratorEditor] Planet baked to {path}");
    }
}