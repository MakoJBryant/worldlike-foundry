using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global list of every possible upgrade in the game.
/// Create one instance of this asset and assign it to every planet's
/// PlanetUpgradeManager. Add new upgrades here as you create them.
/// </summary>
[CreateAssetMenu(fileName = "UpgradeDatabase", menuName = "Worldlike Foundry/Upgrade Database")]
public class PlanetUpgradeDatabase : ScriptableObject
{
    [Header("All Available Upgrades")]
    public List<PlanetUpgrade> allUpgrades = new List<PlanetUpgrade>();

    [Header("Roll Cost")]
    [Tooltip("Fortune spent each time the player rolls 3 new options.")]
    public float rollCost = 10f;
}