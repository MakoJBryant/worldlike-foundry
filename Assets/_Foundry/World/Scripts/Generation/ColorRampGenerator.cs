using UnityEngine;

/// <summary>
/// Evaluates a ColorRampSettings to produce a color for a given normalized elevation.
/// Called per-vertex during planet generation to bake colors directly into the mesh.
/// </summary>
public static class ColorRampGenerator
{
    /// <summary>
    /// Returns the blended color at a given normalized elevation (0 = lowest, 1 = highest).
    /// </summary>
    public static Color SampleColor(ColorRampSettings settings, float normalizedElevation)
    {
        if (settings == null || settings.colorStops == null || settings.colorStops.Length == 0)
            return Color.white;

        // Sort stops by startHeight so blending always runs low to high
        var stops = settings.colorStops;
        System.Array.Sort(stops, (a, b) => a.startHeight.CompareTo(b.startHeight));

        Color result = stops[0].color;

        foreach (var stop in stops)
        {
            float blend = Mathf.Clamp01((normalizedElevation - stop.startHeight) / stop.blendAmount);
            result = Color.Lerp(result, stop.color, blend);
        }

        return result;
    }
}