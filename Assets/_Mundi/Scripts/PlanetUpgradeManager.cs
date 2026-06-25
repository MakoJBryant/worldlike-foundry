using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Handles rolling upgrade options via spinning, slot machine cycling,
/// and applying chosen upgrades using Fortune.
/// </summary>
public class PlanetUpgradeManager : MonoBehaviour
{
    [Header("References")]
    public PlanetGenerator planetGenerator;
    public PlanetStats planetStats;
    public PlanetUpgradeDatabase database;
    public WorldSpacePanel panel;
    public SelectionManager selectionManager;
    public SolarSystemManager solarSystemManager;

    [Header("Current Upgrades (read-only at runtime)")]
    public PlanetUpgrade currentTerrainUpgrade;
    public PlanetUpgrade currentOceanUpgrade;
    public PlanetUpgrade currentAtmosphereUpgrade;

    [Header("Slot Machine")]
    [Tooltip("How fast the slot machine cycles through options at full spin speed.")]
    public float cycleSpeedMax = 10f;

    PlanetUpgrade[] rolledOptions = new PlanetUpgrade[3];
    bool optionsRolled = false;
    PlanetUpgrade lockedOffer = null;

    float cycleTimer = 0f;
    int cycleIndex = 0;
    bool wasSpinning = false;

    void Update()
    {
        if (selectionManager == null || selectionManager.selectedObject != transform) return;
        if (!planetStats.gameActive) return;

        bool isSpinning = planetStats.IsSpinning;

        // Force brake if Wonder runs out mid-spin
        if (isSpinning && planetStats.IsOutOfWonder)
        {
            var body = solarSystemManager.GetBodyData(transform);
            if (body != null)
                solarSystemManager.DampenSpin(body, 1f);
        }

        // Spin just started — roll options
        if (isSpinning && !wasSpinning)
        {
            RollOptions();
            lockedOffer = null;
        }

        // While spinning — cycle slot machine
        if (isSpinning && optionsRolled)
        {
            float cycleSpeed = Mathf.Lerp(1f, cycleSpeedMax,
                planetStats.currentSpinMagnitude / planetStats.fortuneSpinThreshold);

            cycleTimer += Time.deltaTime * cycleSpeed;
            if (cycleTimer >= 1f)
            {
                cycleTimer = 0f;
                cycleIndex = (cycleIndex + 1) % 3;
            }
        }

        // Spin just stopped — lock in current option
        if (!isSpinning && wasSpinning && optionsRolled)
        {
            lockedOffer = rolledOptions[cycleIndex];
        }

        wasSpinning = isSpinning;

        // B to buy locked offer
        if (lockedOffer != null && Keyboard.current.bKey.wasPressedThisFrame)
            TryBuyOffer();

        UpdatePanelDisplay();
    }

    void RollOptions()
    {
        List<PlanetUpgrade> pool = new List<PlanetUpgrade>();
        foreach (var upgrade in database.allUpgrades)
        {
            if (upgrade == currentTerrainUpgrade) continue;
            if (upgrade == currentOceanUpgrade) continue;
            if (upgrade == currentAtmosphereUpgrade) continue;
            pool.Add(upgrade);
        }

        rolledOptions = new PlanetUpgrade[3];
        for (int i = 0; i < 3 && pool.Count > 0; i++)
        {
            int idx = Random.Range(0, pool.Count);
            rolledOptions[i] = pool[idx];
            pool.RemoveAt(idx);
        }

        optionsRolled = true;
        cycleIndex = 0;
        cycleTimer = 0f;
    }

    void TryBuyOffer()
    {
        if (lockedOffer == null || database == null) return;

        float cost = database.rollCost;
        if (planetStats.fortune < cost)
        {
            panel.SetBody("Not enough Fortune to buy!");
            return;
        }

        planetStats.fortune -= cost;
        ApplyUpgrade(lockedOffer);
        lockedOffer = null;
        optionsRolled = false;
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

        if (planetStats.IsSpinning && optionsRolled)
        {
            // Slot machine cycling display
            panel.SetTitle("SPINNING...");
            string body = "";
            for (int i = 0; i < 3; i++)
            {
                if (rolledOptions[i] == null) continue;
                string arrow = (i == cycleIndex) ? ">>> " : "    ";
                body += $"{arrow}{rolledOptions[i].upgradeName} ({rolledOptions[i].slot})\n";
            }
            body += $"\nWonder: {planetStats.WonderInt}/{(int)planetStats.wonderCap}";
            panel.SetBody(body);
        }
        else if (lockedOffer != null)
        {
            // Offer locked in — show buy prompt
            panel.SetTitle("UPGRADE OFFER");
            string body = $"{lockedOffer.upgradeName}\n";
            body += $"{lockedOffer.slot}\n\n";
            body += $"{lockedOffer.description}\n\n";
            body += $"Cost: {(int)database.rollCost} Fortune\n";
            body += $"Yours: {planetStats.FortuneInt} Fortune\n\n";
            body += "[B] BUY\nSpin again to reroll";
            panel.SetBody(body);
        }
        else
        {
            // Idle — show current upgrades and wonder fuel level
            panel.SetTitle("UPGRADES");
            string terrain = currentTerrainUpgrade != null ? currentTerrainUpgrade.upgradeName : "None";
            string ocean = currentOceanUpgrade != null ? currentOceanUpgrade.upgradeName : "None";
            string atmo = currentAtmosphereUpgrade != null ? currentAtmosphereUpgrade.upgradeName : "None";

            string body = $"TERRAIN: {terrain}\n";
            body += $"OCEAN: {ocean}\n";
            body += $"ATMOSPHERE: {atmo}\n\n";
            body += $"Wonder: {planetStats.WonderInt}/{(int)planetStats.wonderCap}\n";
            body += $"Fortune: {planetStats.FortuneInt}\n\n";
            body += "Spin to roll upgrades!";
            panel.SetBody(body);
        }
    }

    public void OnSelected()
    {
        UpdatePanelDisplay();
    }
}