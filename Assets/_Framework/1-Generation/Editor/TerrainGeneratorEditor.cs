using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TerrainGenerator))]
public class TerrainGeneratorEditor : Editor
{
    private TerrainGenerator terrainGenerator;

    void OnEnable()
    {
        terrainGenerator = (TerrainGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ── Settings Asset Reference ─────────────────────────
        EditorGUILayout.LabelField("Terrain", EditorStyles.boldLabel);
        terrainGenerator.terrainSettings = (TerrainSettings)EditorGUILayout.ObjectField(
            "Terrain Settings", terrainGenerator.terrainSettings, typeof(TerrainSettings), false);

        EditorGUILayout.Space();

        if (terrainGenerator.terrainSettings == null)
        {
            EditorGUILayout.HelpBox(
                "Assign a TerrainSettings asset to begin editing.",
                MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        // ── Terrain Settings ─────────────────────────────────
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            terrainGenerator.terrainSettings.resolution = EditorGUILayout.IntSlider(
                "Resolution", terrainGenerator.terrainSettings.resolution, 2, 256);
            terrainGenerator.terrainSettings.globalHeightOffset = EditorGUILayout.FloatField(
                "Global Height Offset", terrainGenerator.terrainSettings.globalHeightOffset);

            EditorGUILayout.Space();

            // ── Noise Layers ─────────────────────────────────
            EditorGUILayout.LabelField("Noise Layers", EditorStyles.boldLabel);

            SerializedObject settingsSO = new SerializedObject(terrainGenerator.terrainSettings);
            settingsSO.Update();
            SerializedProperty noiseLayers = settingsSO.FindProperty("noiseLayers");
            EditorGUILayout.PropertyField(noiseLayers, new GUIContent("Noise Layers"), true);

            if (check.changed || settingsSO.ApplyModifiedProperties())
            {
                EditorUtility.SetDirty(terrainGenerator.terrainSettings);
            }
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        // ── Actions ──────────────────────────────────────────
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Save & Regenerate", GUILayout.Height(32)))
        {
            EditorUtility.SetDirty(terrainGenerator.terrainSettings);
            AssetDatabase.SaveAssets();

            PlanetGenerator pg = terrainGenerator.GetComponentInParent<PlanetGenerator>();
            if (pg != null)
            {
                pg.GeneratePlanet();
                SceneView.RepaintAll();
            }
            else
            {
                Debug.LogWarning("[TerrainGeneratorEditor] No PlanetGenerator found in parent.");
            }
        }
        GUI.backgroundColor = Color.white;

        serializedObject.ApplyModifiedProperties();
    }
}