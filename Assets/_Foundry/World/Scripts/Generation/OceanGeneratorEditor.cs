using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(OceanGenerator))]
public class OceanGeneratorEditor : Editor
{
    private OceanGenerator oceanGenerator;

    void OnEnable()
    {
        oceanGenerator = (OceanGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ── Settings Asset Reference ─────────────────────────
        EditorGUILayout.LabelField("Ocean", EditorStyles.boldLabel);
        oceanGenerator.settings = (OceanSettings)EditorGUILayout.ObjectField(
            "Ocean Settings", oceanGenerator.settings, typeof(OceanSettings), false);

        EditorGUILayout.Space();

        if (oceanGenerator.settings == null)
        {
            EditorGUILayout.HelpBox(
                "Assign an OceanSettings asset to begin editing.",
                MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        // ── Ocean Settings ───────────────────────────────────
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            oceanGenerator.settings.oceanMaterial = (Material)EditorGUILayout.ObjectField(
                "Ocean Material", oceanGenerator.settings.oceanMaterial, typeof(Material), false);
            oceanGenerator.settings.seaLevel = EditorGUILayout.Slider(
                "Sea Level", oceanGenerator.settings.seaLevel, 0f, 1f);

            if (check.changed)
            {
                EditorUtility.SetDirty(oceanGenerator.settings);
            }
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        // ── Actions ──────────────────────────────────────────
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Save & Regenerate", GUILayout.Height(32)))
        {
            EditorUtility.SetDirty(oceanGenerator.settings);
            AssetDatabase.SaveAssets();

            PlanetGenerator pg = oceanGenerator.GetComponentInParent<PlanetGenerator>();
            if (pg != null)
            {
                pg.GeneratePlanet();
                SceneView.RepaintAll();
            }
            else
            {
                Debug.LogWarning("[OceanGeneratorEditor] No PlanetGenerator found in parent.");
            }
        }
        GUI.backgroundColor = Color.white;

        serializedObject.ApplyModifiedProperties();
    }
}