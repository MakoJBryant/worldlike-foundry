using UnityEngine;

[CreateAssetMenu(fileName = "New Atmosphere Settings", menuName = "Worldlike Foundry/Atmosphere Settings")]
public class AtmosphereSettings : ScriptableObject
{
    [Header("Glow Sphere")]
    public Material atmosphereMaterial;

    [Tooltip("How much larger the atmosphere sphere is relative to the planet radius.")]
    public float sizeMultiplier = 0f;

    [Header("Day/Night — handled by Sky system, not here")]
    [Tooltip("How transparent the atmosphere is at night (0 = invisible, 1 = full color).")]
    [Range(0f, 1f)]
    public float nightOpacity = 0.1f;
}