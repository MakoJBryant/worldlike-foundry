using UnityEngine;

[CreateAssetMenu(fileName = "New Color Ramp Settings", menuName = "Worldlike Foundry/Color Ramp Settings")]
public class ColorRampSettings : ScriptableObject
{
    public ColorStop[] colorStops;

    [System.Serializable]
    public struct ColorStop
    {
        public string name;
        public Color color;

        [Tooltip("Normalized elevation threshold (0 = lowest point, 1 = highest point) where this color begins.")]
        [Range(0, 1)]
        public float startHeight;

        [Tooltip("How much this color blends into the one below it.")]
        [Range(0.01f, 1)]
        public float blendAmount;
    }
}