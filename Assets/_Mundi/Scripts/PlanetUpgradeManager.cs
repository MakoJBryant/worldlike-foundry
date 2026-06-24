using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles rolling upgrade options, applying chosen upgrades,
/// and pushing current state to the planet's WorldSpacePanel.
/// Attach to each planet root alongside PlanetGenerator and PlanetStats.
/// </summary>
public class PlanetUpgradeManager : MonoBehaviour
{
    [Header("References")]
    public PlanetGenerator planetGenerator;
    public PlanetStats planetStats;
    public PlanetUpgradeDatabase database;
    public WorldSpacePanel panel;
    public SelectionManager selectionManager;

    [Header("Current Upgrades (read-only at runtime)")]
    public PlanetUpgrade currentTerrainUpgrade;
    public PlanetUpgrade currentOceanUpgrade;
    public PlanetUpgrade currentAtmosphereUpgrade;

    PlanetUpgrade[] currentOptions = new PlanetUpgrade[3];
    bool optionsActive = false;
    bool isDirty = true; // forces panel refresh on first frame selected

    void Update()
    {
        if (selectionManager == null || selectionManager.selectedObject != transform) return;

        // R to roll
        if (Keyboard.current.rKey.wasPressedThisFrame)
        {
            TryRoll();
            isDirty = true;
        }

        // 1/2/3 to pick an option
        if (optionsActive)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame) TryApplyUpgrade(0);
            else if (Keyboard.current.digit2Key.wasPressedThisFrame) TryApplyUpgrade(1);
            else if (Keyboard.current.digit3Key.wasPressedThisFrame) TryApplyUpgrade(2);
        }

        // Only rebuild panel text when something changed
        if (isDirty)
        {
            UpdatePanelDisplay();
            isDirty = false;
        }
    }

    void TryRoll()
    {
        if (database == null) return;

        if (planetStats.fortune < database.rollCost)
        {
            Debug.Log("[Upgrades] Not enough fortune to roll.");
            return;
        }

        planetStats.fortune -= database.rollCost;

        // Pool: everything except what's already applied in each slot
        List<PlanetUpgrade> pool = new List<PlanetUpgrade>();
        foreach (var upgrade in database.allUpgrades)
        {
            if (upgrade == currentTerrainUpgrade) continue;
            if (upgrade == currentOceanUpgrade) continue;
            if (upgrade == currentAtmosphereUpgrade) continue;
            pool.Add(upgrade);
        }

        // Pick up to 3 without duplicates
        currentOptions = new PlanetUpgrade[3];
        for (int i = 0; i < 3 && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            currentOptions[i] = pool[idx];
            pool.RemoveAt(idx);
        }

        optionsActive = true;
    }

    void TryApplyUpgrade(int index)
    {
        if (index >= currentOptions.Length || currentOptions[index] == null) return;

        ApplyUpgrade(currentOptions[index]);

        currentOptions = new PlanetUpgrade[3];
        optionsActive = false;
        isDirty = true;
    }

    void ApplyUpgrade(PlanetUpgrade upgrade)
    {
        if (planetGenerator == null || planetGenerator.planetSettings == null) return;

        PlanetSettings settings = planetGenerator.planetSettings;

        switch (upgrade.slot)
        {
            case UpgradeSlot.Terrain:
                settings.terrainSettings = upgrade.terrainSettings;
                currentTerrainUpgrade = upgrade;
                break;
            case UpgradeSlot.Ocean:
                settings.oceanSettings = upgrade.oceanSettings;
                currentOceanUpgrade = upgrade;
                break;
            case UpgradeSlot.Atmosphere:
                settings.atmosphereSettings = upgrade.atmosphereSettings;
                currentAtmosphereUpgrade = upgrade;
                break;
        }

        planetGenerator.GeneratePlanet();
    }

    void UpdatePanelDisplay()
    {
        if (panel == null) return;

        if (optionsActive)
        {
            panel.SetTitle("CHOOSE UPGRADE");

            string body = "";
            for (int i = 0; i < 3; i++)
            {
                if (currentOptions[i] != null)
                {
                    body += $"[{i + 1}] {currentOptions[i].upgradeName}\n";
                    body += $"    {currentOptions[i].slot} — {currentOptions[i].description}\n\n";
                }
            }
            body += "Press 1, 2 or 3 to apply.";
            panel.SetBody(body);
        }
        else
        {
            panel.SetTitle("UPGRADES");

            string terrain = currentTerrainUpgrade != null ? currentTerrainUpgrade.upgradeName : "None";
            string ocean = currentOceanUpgrade != null ? currentOceanUpgrade.upgradeName : "None";
            string atmo = currentAtmosphereUpgrade != null ? currentAtmosphereUpgrade.upgradeName : "None";
            int fortuneNeeded = database != null ? (int)database.rollCost : 0;
            int currentFortune = planetStats != null ? planetStats.FortuneInt : 0;

            string body = $"TERRAIN: {terrain}\n";
            body += $"OCEAN: {ocean}\n";
            body += $"ATMOSPHERE: {atmo}\n\n";
            body += $"[R] ROLL OPTIONS\n";
            body += $"Cost: {fortuneNeeded} Fortune\n";
            body += $"Yours: {currentFortune} Fortune";
            panel.SetBody(body);
        }
    }

    // Called externally when selection changes so panel refreshes immediately
    public void OnSelected()
    {
        isDirty = true;
    }
}