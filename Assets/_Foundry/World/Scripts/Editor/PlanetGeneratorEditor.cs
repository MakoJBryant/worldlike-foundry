using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PlanetGenerator))]
public class PlanetGeneratorEditor : Editor
{
    private PlanetGenerator generator;

    // Foldout states
    private bool showShape = true;
    private bool showColorRamp = true;
    private bool showOcean = true;
    private bool showAtmosphere = true;
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

        // ── Subsystems ───────────────────────────────────────
        EditorGUILayout.LabelField("Subsystems", EditorStyles.boldLabel);
        generator.shapeGenerator = (ShapeGenerator)EditorGUILayout.ObjectField(
            "Shape", generator.shapeGenerator, typeof(ShapeGenerator), true);
        generator.oceanGenerator = (OceanGenerator)EditorGUILayout.ObjectField(
            "Ocean", generator.oceanGenerator, typeof(OceanGenerator), true);
        generator.atmosphereGenerator = (AtmosphereGenerator)EditorGUILayout.ObjectField(
            "Atmosphere", generator.atmosphereGenerator, typeof(AtmosphereGenerator), true);

        EditorGUILayout.Space();

        if (generator.planetData == null)
        {
            EditorGUILayout.HelpBox(
                "Assign a PlanetData asset to begin editing.",
                MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        // ── Settings ─────────────────────────────────────────
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);

        DrawFoldout("Shape", ref showShape, generator.planetData.shapeSettings);
        DrawFoldout("Color Ramp", ref showColorRamp, generator.planetData.colorRampSettings);
        DrawFoldout("Ocean", ref showOcean, generator.planetData.oceanSettings);
        DrawFoldout("Atmosphere", ref showAtmosphere, generator.planetData.atmosphereSettings);

        EditorGUILayout.Space();

        // ── Generate ─────────────────────────────────────────
        EditorGUILayout.LabelField("Generate", EditorStyles.boldLabel);

        if (GUILayout.Button("Generate Preview", GUILayout.Height(32)))
        {
            generator.GeneratePlanet();
            SceneView.RepaintAll();
        }

        EditorGUILayout.Space();

        // ── Bake ─────────────────────────────────────────────
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

    private void DrawFoldout(string label, ref bool foldout, ScriptableObject settings)
    {
        foldout = EditorGUILayout.Foldout(foldout, label, true, EditorStyles.foldoutHeader);
        if (!foldout) return;

        EditorGUI.indentLevel++;

        if (settings == null)
        {
            EditorGUILayout.HelpBox($"No {label} settings assigned on PlanetData.", MessageType.Warning);
            EditorGUI.indentLevel--;
            return;
        }

        using (var check = new EditorGUI.ChangeCheckScope())
        {
            Editor settingsEditor = CreateEditor(settings);
            settingsEditor.OnInspectorGUI();

            if (check.changed)
            {
                EditorUtility.SetDirty(settings);
                generator.GeneratePlanet();
                SceneView.RepaintAll();
            }
        }

        EditorGUI.indentLevel--;
        EditorGUILayout.Space();
    }

    private void BakePlanet()
    {
        if (generator.planetData == null) return;

        // Make sure we have a fresh generation before baking
        generator.GeneratePlanet();

        MeshFilter mf = generator.shapeGenerator?.GetComponent<MeshFilter>();
        if (mf == null || mf.sharedMesh == null)
        {
            Debug.LogWarning("[PlanetGeneratorEditor] No mesh found to bake.");
            return;
        }

        // Build save path
        string folder = "Assets/_Foundry/World/Meshes";
        string assetName = generator.planetData.name + "_Baked.asset";
        string path = $"{folder}/{assetName}";

        // Ensure folder exists
        if (!AssetDatabase.IsValidFolder(folder))
            AssetDatabase.CreateFolder("Assets/_Foundry/World", "Meshes");

        // Copy the preview mesh into a new saved mesh
        Mesh bakedMesh = Instantiate(mf.sharedMesh);
        bakedMesh.name = generator.planetData.name + "_Baked";

        Mesh existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing != null)
        {
            // Update existing asset in place
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