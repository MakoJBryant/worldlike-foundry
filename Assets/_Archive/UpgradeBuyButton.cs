using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Attach to a 3D cube child of a planet.
/// Hover to preview the current upgrade offer.
/// Click to buy it.
/// Positions itself relative to the planet like wonder/fortune labels.
/// </summary>
public class UpgradeBuyButton : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform planet;
    public PlanetUpgradeManager upgradeManager;
    public PlanetGenerator planetGenerator;

    [Header("Positioning")]
    public float distanceFromCenter = 300f;
    public float horizontalOffset = -350f;
    public float verticalOffset = -100f;

    [Header("Hover Detection")]
    public float hoverPixelRadius = 50f;

    [Header("Visual")]
    public Renderer cubeRenderer;
    public Color defaultColor = Color.white;
    public Color hoverColor = Color.yellow;
    public Color noOfferColor = Color.grey;

    bool isPreviewing = false;
    TerrainSettings savedTerrain;
    OceanSettings savedOcean;
    AtmosphereSettings savedAtmosphere;

    void Start()
    {
        if (cubeRenderer == null)
            cubeRenderer = GetComponent<Renderer>();
    }

    void LateUpdate()
    {
        if (cam == null || planet == null) return;

        // Position same pattern as fortune/wonder labels
        Vector3 toCam = (cam.transform.position - planet.position).normalized;
        Vector3 camRight = cam.transform.right;
        Vector3 camUp = cam.transform.up;

        Vector3 baseFacePos = planet.position + toCam * distanceFromCenter;

        transform.position = baseFacePos
            + camRight * horizontalOffset
            + camUp * verticalOffset;

        // Cube faces camera
        transform.rotation = Quaternion.LookRotation(
            transform.position - cam.transform.position);

        // No offer — grey out and do nothing
        bool hasOffer = upgradeManager != null &&
                        upgradeManager.LockedOffer != null &&
                        !upgradeManager.planetStats.IsSpinning;

        if (!hasOffer)
        {
            if (isPreviewing) EndPreview();
            SetColor(noOfferColor);
            return;
        }

        bool hovering = IsMouseOverThis();

        if (hovering && !isPreviewing)
            StartPreview();
        else if (!hovering && isPreviewing)
            EndPreview();

        SetColor(hovering ? hoverColor : defaultColor);

        // Click to buy
        if (hovering && Mouse.current.leftButton.wasPressedThisFrame)
            upgradeManager.TryBuyOffer();
    }

    bool IsMouseOverThis()
    {
        Vector3 screenPos = cam.WorldToScreenPoint(transform.position);
        if (screenPos.z < 0) return false;

        float dist = Vector2.Distance(
            new Vector2(screenPos.x, screenPos.y),
            Mouse.current.position.ReadValue());

        return dist < hoverPixelRadius;
    }

    void SetColor(Color color)
    {
        if (cubeRenderer != null)
            cubeRenderer.material.color = color;
    }

    void StartPreview()
    {
        if (upgradeManager?.LockedOffer == null) return;
        if (planetGenerator?.planetSettings == null) return;

        isPreviewing = true;

        PlanetSettings settings = planetGenerator.planetSettings;
        savedTerrain = settings.terrainSettings;
        savedOcean = settings.oceanSettings;
        savedAtmosphere = settings.atmosphereSettings;

        PlanetUpgrade offer = upgradeManager.LockedOffer;
        switch (offer.slot)
        {
            case UpgradeSlot.Terrain:
                settings.terrainSettings = offer.terrainSettings;
                break;
            case UpgradeSlot.Ocean:
                settings.oceanSettings = offer.oceanSettings;
                break;
            case UpgradeSlot.Atmosphere:
                settings.atmosphereSettings = offer.atmosphereSettings;
                break;
        }

        planetGenerator.GeneratePlanet();
    }

    void EndPreview()
    {
        if (!isPreviewing) return;
        isPreviewing = false;

        if (planetGenerator?.planetSettings == null) return;

        PlanetSettings settings = planetGenerator.planetSettings;
        settings.terrainSettings = savedTerrain;
        settings.oceanSettings = savedOcean;
        settings.atmosphereSettings = savedAtmosphere;

        planetGenerator.GeneratePlanet();
    }
}