using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlanetUpgradeManager : MonoBehaviour
{
    [Header("References")]
    public PlanetGenerator planetGenerator;
    public PlanetStats planetStats;
    public PlanetUpgradeDatabase database;
    public UpgradeLabel upgradeLabel;
    public SelectionManager selectionManager;
    public SolarSystemManager solarSystemManager;

    [Header("Current Upgrades (read-only at runtime)")]
    public PlanetUpgrade currentTerrainUpgrade;
    public PlanetUpgrade currentOceanUpgrade;
    public PlanetUpgrade currentAtmosphereUpgrade;

    [Header("Slot Machine")]
    public float cycleSpeedMax = 10f;

    PlanetUpgrade[] rolledOptions = new PlanetUpgrade[3];
    bool optionsRolled = false;

    // Public so UpgradeLabel can read it for preview
    public PlanetUpgrade LockedOffer { get; private set; }

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
            LockedOffer = null;
            if (upgradeLabel != null) upgradeLabel.ClearOffer();
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

            // Update label to show cycling name
            if (upgradeLabel != null && rolledOptions[cycleIndex] != null)
                upgradeLabel.SetOffer(rolledOptions[cycleIndex]);
        }

        // Spin just stopped — lock in current option
        if (!isSpinning && wasSpinning && optionsRolled)
        {
            LockedOffer = rolledOptions[cycleIndex];
            if (upgradeLabel != null)
                upgradeLabel.SetOffer(LockedOffer);
        }

        wasSpinning = isSpinning;
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

    public void TryBuyOffer()
    {
        if (LockedOffer == null || database == null) return;

        float cost = database.rollCost;
        if (planetStats.fortune < cost)
        {
            Debug.Log("[Upgrades] Not enough Fortune to buy.");
            return;
        }

        planetStats.fortune -= cost;
        ApplyUpgrade(LockedOffer);
        LockedOffer = null;
        optionsRolled = false;

        if (upgradeLabel != null) upgradeLabel.ClearOffer();
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

    public void OnSelected()
    {
        // Refresh label on reselect
        if (upgradeLabel != null && LockedOffer != null)
            upgradeLabel.SetOffer(LockedOffer);
    }
}