using UnityEngine;

[CreateAssetMenu(fileName = "New Atmosphere Settings", menuName = "Worldlike Foundry/Atmosphere Settings")]
public class AtmosphereSettings : ScriptableObject
{
    [Header("Glow Sphere")]
    public Material atmosphereMaterial;

    [Tooltip("How much larger the atmosphere sphere is relative to the planet radius.")]
    public float thicknessMultiplier = 0.3f;

    [Tooltip("Base color of the atmosphere glow.")]
    public Color atmosphereColor = new Color(0.4f, 0.6f, 1f, 1f);

    [Header("Day/Night — handled by Sky system, not here")]
    [Tooltip("Color of the atmosphere at sunrise/sunset.")]
    public Color sunsetColor = new Color(1f, 0.5f, 0.2f, 1f);

    [Tooltip("How transparent the atmosphere is at night (0 = invisible, 1 = full color).")]
    [Range(0f, 1f)]
    public float nightOpacity = 0.1f;
}