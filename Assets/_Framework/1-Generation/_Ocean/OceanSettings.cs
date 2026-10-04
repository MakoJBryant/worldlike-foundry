using UnityEngine;

[CreateAssetMenu(fileName = "New Ocean Settings", menuName = "Worldlike Foundry/Ocean Settings")]
public class OceanSettings : ScriptableObject
{
    [Header("Visual")]
    public Material oceanMaterial;

    [Header("Surface")]
    [Tooltip("Normalized elevation (0-1) where the ocean surface sits. Terrain below this level will be underwater.")]
    [Range(0f, 1f)]
    public float seaLevel = 0.65f;
}