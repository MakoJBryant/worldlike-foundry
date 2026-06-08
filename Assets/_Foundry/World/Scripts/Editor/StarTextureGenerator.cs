using UnityEngine;
using UnityEditor;
using System.IO;

public class StarTextureGenerator : EditorWindow
{
    [MenuItem("Worldlike Foundry/Generate Star Texture")]
    public static void ShowWindow()
    {
        GetWindow<StarTextureGenerator>("Star Texture Generator");
    }

    private int textureWidth = 2048;
    private int textureHeight = 1024;
    private int starCount = 300;
    private float starSize = 2.5f;
    private float coloredStarChance = 0.1f;
    private Color blueStarColor = new Color(0.6f, 0.8f, 1f);
    private Color redStarColor = new Color(1f, 0.4f, 0.3f);
    private int seed = 42;
    private string savePath = "Assets/_Foundry/Rendering/T_Starfield.png";

    void OnGUI()
    {
        GUILayout.Label("Star Texture Generator", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        textureWidth = EditorGUILayout.IntField("Texture Width", textureWidth);
        textureHeight = EditorGUILayout.IntField("Texture Height", textureHeight);
        starCount = EditorGUILayout.IntField("Star Count", starCount);
        starSize = EditorGUILayout.Slider("Star Size (px)", starSize, 0.5f, 8f);
        coloredStarChance = EditorGUILayout.Slider("Colored Star Chance", coloredStarChance, 0f, 0.5f);
        blueStarColor = EditorGUILayout.ColorField("Blue Star Color", blueStarColor);
        redStarColor = EditorGUILayout.ColorField("Red Star Color", redStarColor);
        seed = EditorGUILayout.IntField("Seed", seed);
        savePath = EditorGUILayout.TextField("Save Path", savePath);

        EditorGUILayout.Space();

        GUI.backgroundColor = new Color(0.4f, 0.8f, 0.4f);
        if (GUILayout.Button("Generate & Save", GUILayout.Height(32)))
        {
            GenerateAndSave();
        }
        GUI.backgroundColor = Color.white;
    }

    void GenerateAndSave()
    {
        Random.InitState(seed);

        Texture2D tex = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);

        // Fill with black
        Color[] pixels = new Color[textureWidth * textureHeight];
        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = Color.black;

        // Place stars using spherical distribution
        // Generate random points on a sphere and project to equirectangular UV
        for (int i = 0; i < starCount; i++)
        {
            // Uniform sphere distribution using Gaussian method
            float u = Random.Range(0f, 1f);
            float v = Random.Range(0f, 1f);

            // Convert to spherical angles — this gives even distribution across sphere
            float theta = 2f * Mathf.PI * u;             // longitude: 0 to 2PI
            float phi = Mathf.Acos(2f * v - 1f);         // latitude: 0 to PI, uniform on sphere

            // Convert spherical to equirectangular UV
            float uvX = theta / (2f * Mathf.PI);          // 0 to 1
            float uvY = phi / Mathf.PI;                    // 0 to 1

            int x = Mathf.Clamp(Mathf.RoundToInt(uvX * textureWidth), 0, textureWidth - 1);
            int y = Mathf.Clamp(Mathf.RoundToInt(uvY * textureHeight), 0, textureHeight - 1);

            // Determine star color
            float colorRoll = Random.value;
            Color starColor;

            if (colorRoll < coloredStarChance * 0.5f)
                starColor = blueStarColor;
            else if (colorRoll < coloredStarChance)
                starColor = redStarColor;
            else
            {
                float brightness = Random.Range(0.7f, 1.0f);
                starColor = new Color(brightness, brightness, brightness);
            }

            // Paint star as a circle with given radius
            int radius = Mathf.RoundToInt(starSize);
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (dx * dx + dy * dy <= radius * radius)
                    {
                        int px = (x + dx + textureWidth) % textureWidth;
                        int py = Mathf.Clamp(y + dy, 0, textureHeight - 1);
                        pixels[py * textureWidth + px] = starColor;
                    }
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        // Save to disk
        byte[] png = tex.EncodeToPNG();
        string fullPath = Path.Combine(Application.dataPath, "../", savePath);

        string dir = Path.GetDirectoryName(fullPath);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);

        File.WriteAllBytes(fullPath, png);
        AssetDatabase.Refresh();

        // Set texture import settings
        TextureImporter importer = AssetImporter.GetAtPath(savePath) as TextureImporter;
        if (importer != null)
        {
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.wrapMode = TextureWrapMode.Repeat;
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.SaveAndReimport();
        }

        Debug.Log($"[StarTextureGenerator] Saved to {savePath}");
        DestroyImmediate(tex);

        EditorUtility.DisplayDialog(
            "Star Texture Generator",
            $"Starfield saved to {savePath}",
            "OK");
    }
}