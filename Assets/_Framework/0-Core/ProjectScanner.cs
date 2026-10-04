// Place this file in:  Assets/Editor/ProjectScanner.cs
// Then use the menu:   Tools > Project Scanner
// Output:              <ProjectRoot>/ProjectScans/<ProjectName>_scan_<date>_<time>.txt
//                      (outside Assets, so Unity never imports the reports)

#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public static class ProjectScanner
{
    // Folders inside Assets that are usually third-party noise. Listed, but not expanded.
    static readonly HashSet<string> CollapsedFolders = new HashSet<string>
    {
        "TextMesh Pro", "Samples", "StreamingAssets"
    };

    const int MaxFilesShownPerFolder = 40;
    const int MaxScriptBytes = 20000;

    static readonly Regex ClassRegex = new Regex(
        @"(?:public|internal|private|protected)?\s*(?:abstract\s+|static\s+|sealed\s+|partial\s+)*(class|struct|enum|interface)\s+(\w+)(?:\s*:\s*([\w\.,\s<>]+?))?\s*(?:\{|where|$)",
        RegexOptions.Multiline);

    [MenuItem("Tools/Project Scanner/Scan Structure")]
    public static void ScanStructure() => Scan(false);

    [MenuItem("Tools/Project Scanner/Scan Structure + Script Contents")]
    public static void ScanWithContents() => Scan(true);

    static void Scan(bool includeContents)
    {
        var sb = new StringBuilder();
        string projectRoot = Path.GetDirectoryName(Application.dataPath);
        string assetsPath = Application.dataPath;

        DateTime now = DateTime.Now;
        sb.AppendLine($"PROJECT SCAN: {PlayerSettings.productName}");
        sb.AppendLine($"Scanned: {now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine($"Mode: {(includeContents ? "Structure + script contents" : "Structure only")}");
        sb.AppendLine();

        WriteProjectInfo(sb);
        WriteBuildScenes(sb);
        WritePackages(sb, projectRoot);

        sb.AppendLine("=== ASSETS FOLDER TREE ===");
        sb.AppendLine("Assets/");
        WriteTree(sb, assetsPath, 1);
        sb.AppendLine();

        WriteTypeSummary(sb, assetsPath);
        WriteScenes(sb);
        WritePrefabs(sb);
        WriteScripts(sb, assetsPath, includeContents);

        string safeName = Regex.Replace(PlayerSettings.productName, @"[^\w\-]+", "_");
        string suffix = includeContents ? "_with_scripts" : "";
        string outDir = Path.Combine(projectRoot, "ProjectScans");
        Directory.CreateDirectory(outDir);
        string outPath = Path.Combine(outDir, $"{safeName}_scan_{now:yyyy-MM-dd_HHmm}{suffix}.txt");
        File.WriteAllText(outPath, sb.ToString(), Encoding.UTF8);

        Debug.Log($"[ProjectScanner] Scan complete. Report saved to: {outPath}");
        EditorUtility.RevealInFinder(outPath);
    }

    // ---------- Sections ----------

    static void WriteProjectInfo(StringBuilder sb)
    {
        sb.AppendLine("=== PROJECT INFO ===");
        sb.AppendLine($"Unity version:    {Application.unityVersion}");
        sb.AppendLine($"Company:          {PlayerSettings.companyName}");
        sb.AppendLine($"Color space:      {PlayerSettings.colorSpace}");
        sb.AppendLine($"Active build target: {EditorUserBuildSettings.activeBuildTarget}");

        var rp = GraphicsSettings.defaultRenderPipeline;
        sb.AppendLine($"Render pipeline:  {(rp != null ? rp.GetType().Name + " (" + rp.name + ")" : "Built-in")}");

        string defines = PlayerSettings.GetScriptingDefineSymbols(
            UnityEditor.Build.NamedBuildTarget.FromBuildTargetGroup(
                BuildPipeline.GetBuildTargetGroup(EditorUserBuildSettings.activeBuildTarget)));
        sb.AppendLine($"Scripting defines: {(string.IsNullOrEmpty(defines) ? "(none)" : defines)}");
        sb.AppendLine();
    }

    static void WriteBuildScenes(StringBuilder sb)
    {
        sb.AppendLine("=== SCENES IN BUILD SETTINGS ===");
        var scenes = EditorBuildSettings.scenes;
        if (scenes.Length == 0) sb.AppendLine("  (none)");
        for (int i = 0; i < scenes.Length; i++)
            sb.AppendLine($"  [{i}] {(scenes[i].enabled ? "ON " : "OFF")} {scenes[i].path}");
        sb.AppendLine();
    }

    static void WritePackages(StringBuilder sb, string projectRoot)
    {
        sb.AppendLine("=== PACKAGES (Packages/manifest.json) ===");
        string manifest = Path.Combine(projectRoot, "Packages", "manifest.json");
        if (File.Exists(manifest))
        {
            foreach (Match m in Regex.Matches(File.ReadAllText(manifest), "\"([\\w\\.\\-]+)\"\\s*:\\s*\"([^\"]+)\""))
            {
                string name = m.Groups[1].Value;
                if (name.StartsWith("com.") || name.StartsWith("net.") || name.StartsWith("org."))
                    sb.AppendLine($"  {name}  {m.Groups[2].Value}");
            }
        }
        else sb.AppendLine("  (manifest not found)");
        sb.AppendLine();
    }

    static void WriteTree(StringBuilder sb, string dir, int depth)
    {
        string indent = new string(' ', depth * 2);

        foreach (string sub in Directory.GetDirectories(dir).OrderBy(d => d))
        {
            string name = Path.GetFileName(sub);
            if (name.StartsWith(".") || name.EndsWith("~")) continue;

            if (CollapsedFolders.Contains(name))
            {
                int count = Directory.GetFiles(sub, "*", SearchOption.AllDirectories)
                                     .Count(f => !f.EndsWith(".meta"));
                sb.AppendLine($"{indent}{name}/  [collapsed, {count} files]");
                continue;
            }

            sb.AppendLine($"{indent}{name}/");
            WriteTree(sb, sub, depth + 1);
        }

        var files = Directory.GetFiles(dir).Where(f => !f.EndsWith(".meta")).OrderBy(f => f).ToList();
        foreach (string f in files.Take(MaxFilesShownPerFolder))
            sb.AppendLine($"{indent}{Path.GetFileName(f)}");
        if (files.Count > MaxFilesShownPerFolder)
            sb.AppendLine($"{indent}... and {files.Count - MaxFilesShownPerFolder} more files");
    }

    static void WriteTypeSummary(StringBuilder sb, string assetsPath)
    {
        sb.AppendLine("=== FILE TYPES IN ASSETS ===");
        var groups = Directory.GetFiles(assetsPath, "*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(".meta"))
            .GroupBy(f => Path.GetExtension(f).ToLower())
            .OrderByDescending(g => g.Count());
        foreach (var g in groups)
            sb.AppendLine($"  {(g.Key == "" ? "(no ext)" : g.Key),-12} {g.Count(),5} files");
        sb.AppendLine();
    }

    static void WriteScenes(StringBuilder sb)
    {
        sb.AppendLine("=== ALL SCENES ===");
        foreach (string guid in AssetDatabase.FindAssets("t:Scene", new[] { "Assets" }))
            sb.AppendLine($"  {AssetDatabase.GUIDToAssetPath(guid)}");
        sb.AppendLine();
    }

    static void WritePrefabs(StringBuilder sb)
    {
        sb.AppendLine("=== PREFABS ===");
        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var comps = go != null
                ? go.GetComponents<Component>().Where(c => c != null).Select(c => c.GetType().Name)
                : Enumerable.Empty<string>();
            sb.AppendLine($"  {path}  [{string.Join(", ", comps)}]");
        }
        sb.AppendLine();
    }

    static void WriteScripts(StringBuilder sb, string assetsPath, bool includeContents)
    {
        sb.AppendLine("=== SCRIPTS (types and base classes) ===");
        var scripts = Directory.GetFiles(assetsPath, "*.cs", SearchOption.AllDirectories)
                               .OrderBy(f => f).ToList();

        foreach (string file in scripts)
        {
            string rel = "Assets" + file.Substring(assetsPath.Length).Replace('\\', '/');
            string text = File.ReadAllText(file);
            var types = ClassRegex.Matches(text).Cast<Match>()
                .Select(m => m.Groups[3].Success && m.Groups[3].Value.Trim() != ""
                    ? $"{m.Groups[1].Value} {m.Groups[2].Value} : {m.Groups[3].Value.Trim()}"
                    : $"{m.Groups[1].Value} {m.Groups[2].Value}");
            sb.AppendLine($"  {rel}");
            foreach (string t in types) sb.AppendLine($"      - {t}");
        }
        sb.AppendLine($"\nTotal scripts: {scripts.Count}");
        sb.AppendLine();

        if (!includeContents) return;

        sb.AppendLine("=== SCRIPT CONTENTS ===");
        foreach (string file in scripts)
        {
            string rel = "Assets" + file.Substring(assetsPath.Length).Replace('\\', '/');
            if (rel.Contains("/TextMesh Pro/") || rel.Contains("/Samples/")) continue;

            string text = File.ReadAllText(file);
            sb.AppendLine();
            sb.AppendLine($"----- {rel} -----");
            sb.AppendLine(text.Length > MaxScriptBytes
                ? text.Substring(0, MaxScriptBytes) + "\n... [truncated]"
                : text);
        }
    }
}
#endif