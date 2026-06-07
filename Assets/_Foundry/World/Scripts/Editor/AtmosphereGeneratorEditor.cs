using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(AtmosphereGenerator))]
public class AtmosphereGeneratorEditor : Editor
{
    private AtmosphereGenerator atmosphereGenerator;

    void OnEnable()
    {
        atmosphereGenerator = (AtmosphereGenerator)target;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        // ── Settings Asset Reference ─────────────────────────
        EditorGUILayout.LabelField("Atmosphere", EditorStyles.boldLabel);
        atmosphereGenerator.settings = (AtmosphereSettings)EditorGUILayout.ObjectField(
            "Atmosphere Settings", atmosphereGenerator.settings, typeof(AtmosphereSettings), false);

        EditorGUILayout.Space();

        if (atmosphereGenerator.settings == null)
        {
            EditorGUILayout.HelpBox(
                "Assign an AtmosphereSettings asset to begin editing.",
                MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        // ── Atmosphere Settings ──────────────────────────────
        EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
        EditorGUI.indentLevel++;
        using (var check = new EditorGUI.ChangeCheckScope())
        {
            atmosphereGenerator.settings.atmosphereMaterial = (Material)EditorGUILayout.ObjectField(
                "Atmosphere Material", atmosphereGenerator.settings.atmosphereMaterial, typeof(Material), false);
            atmosphereGenerator.settings.sizeMultiplier = EditorGUILayout.FloatField(
                "Size Multiplier", atmosphereGenerator.settings.sizeMultiplier);
            atmosphereGenerator.settings.nightOpacity = EditorGUILayout.Slider(
                "Night Opacity", atmosphereGenerator.settings.nightOpacity, 0f, 1f);

            if (check.changed)
            {
                EditorUtility.SetDirty(atmosphereGenerator.settings);
            }
        }
        EditorGUI.indentLevel--;

        EditorGUILayout.Space();

        // ── Actions ──────────────────────────────────────────
        EditorGUILayout.LabelField("Actions", EditorStyles.boldLabel);

        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Save & Regenerate", GUILayout.Height(32)))
        {
            EditorUtility.SetDirty(atmosphereGenerator.settings);
            AssetDatabase.SaveAssets();

            PlanetGenerator pg = atmosphereGenerator.GetComponentInParent<PlanetGenerator>();
            if (pg != null)
            {
                pg.GeneratePlanet();
                SceneView.RepaintAll();
            }
            else
            {
                Debug.LogWarning("[AtmosphereGeneratorEditor] No PlanetGenerator found in parent.");
            }
        }
        GUI.backgroundColor = Color.white;

        serializedObject.ApplyModifiedProperties();
    }
}