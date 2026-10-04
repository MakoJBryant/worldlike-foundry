using UnityEngine;

public enum UpgradeSlot { Terrain, Ocean, Atmosphere }

/// <summary>
/// A single upgrade option. Create these as assets in the Project window.
/// Fill only the settings field that matches your slot type.
/// </summary>
[CreateAssetMenu(fileName = "New Planet Upgrade", menuName = "Worldlike Foundry/Planet Upgrade")]
public class PlanetUpgrade : ScriptableObject
{
    [Header("Identity")]
    public string upgradeName;
    [TextArea(2, 4)]
    public string description;

    [Header("Slot")]
    public UpgradeSlot slot;

    [Header("Settings — fill only the one matching your slot")]
    public TerrainSettings terrainSettings;
    public OceanSettings oceanSettings;
    public AtmosphereSettings atmosphereSettings;
}