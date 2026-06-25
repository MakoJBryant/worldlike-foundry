using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Floating upgrade label that sits next to a planet.
/// Shows the current upgrade offer. Hover to preview.
/// Click to buy.
/// Sits flat under the planet root, same as WonderLabel and FortuneLabel.
/// </summary>
public class UpgradeLabel : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public RawImage rawImage;
    public Transform planet;
    public PlanetGenerator planetGenerator;
    public PlanetUpgradeManager upgradeManager;
    public SelectionManager selectionManager;

    [Header("Positioning")]
    public float distanceFromCenter = 300f;
    public float horizontalOffset = -350f;
    public float verticalOffset = 0f;

    [Header("Hover Name Label")]
    [Tooltip("Sibling TMP object that appears to the right on hover.")]
    public TextMeshPro nameLabel;
    public float nameHorizontalOffset = 200f;

    [Header("Hover Detection")]
    [Tooltip("Screen pixel radius that counts as hovering over this label.")]
    public float hoverPixelRadius = 60f;

    TextMeshPro tmp;
    bool isPreviewing = false;

    TerrainSettings savedTerrain;
    OceanSettings savedOcean;
    AtmosphereSettings savedAtmosphere;

    void Start()
    {
        tmp = GetComponent<TextMeshPro>();
        gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");

        if (nameLabel != null)
        {
            nameLabel.gameObject.SetActive(false);
            nameLabel.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
        }
    }

    void LateUpdate()
    {
        if (cam == null || planet == null) return;

        Vector3 toCam = (cam.transform.position - planet.position).normalized;
        Vector3 camRight = cam.transform.right;
        Vector3 camUp = cam.transform.up;

        // Position main label
        transform.position = planet.position
            + toCam * distanceFromCenter
            + camRight * horizontalOffset
            + camUp * verticalOffset;

        Quaternion faceCam = Quaternion.LookRotation(
            transform.position - cam.transform.position);
        transform.rotation = faceCam;

        // Position name label as sibling — to the right of main label
        if (nameLabel != null && nameLabel.gameObject.activeSelf)
        {
            nameLabel.transform.position = transform.position
                + camRight * nameHorizontalOffset;
            nameLabel.transform.rotation = faceCam;
        }

        // Hover detection
        bool hovering = IsMouseOverThis();

        if (hovering && !isPreviewing)
            StartPreview();
        else if (!hovering && isPreviewing)
            EndPreview();

        // Click to buy
        if (hovering && Mouse.current.leftButton.wasPressedThisFrame)
            upgradeManager?.TryBuyOffer();
    }

    public void SetOffer(PlanetUpgrade upgrade)
    {
        if (tmp != null)
            tmp.text = upgrade != null ? upgrade.upgradeName : "";

        if (nameLabel != null)
            nameLabel.text = upgrade != null ? upgrade.upgradeName : "";

        if (isPreviewing)
            EndPreview();
    }

    public void ClearOffer()
    {
        if (tmp != null) tmp.text = "";
        if (nameLabel != null)
        {
            nameLabel.text = "";
            nameLabel.gameObject.SetActive(false);
        }

        if (isPreviewing)
            EndPreview();
    }

    bool IsMouseOverThis()
    {
        if (rawImage == null || cam == null) return false;

        Vector3 labelScreenPos = cam.WorldToScreenPoint(transform.position);
        if (labelScreenPos.z < 0) return false;

        Vector2 screenMouse = Mouse.current.position.ReadValue();
        float dist = Vector2.Distance(
            new Vector2(labelScreenPos.x, labelScreenPos.y),
            screenMouse);

        return dist < hoverPixelRadius;
    }

    void StartPreview()
    {
        if (upgradeManager == null || upgradeManager.LockedOffer == null) return;
        if (planetGenerator == null || planetGenerator.planetSettings == null) return;

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

        if (nameLabel != null)
            nameLabel.gameObject.SetActive(true);
    }

    void EndPreview()
    {
        if (!isPreviewing) return;
        isPreviewing = false;

        if (planetGenerator == null || planetGenerator.planetSettings == null) return;

        PlanetSettings settings = planetGenerator.planetSettings;
        settings.terrainSettings = savedTerrain;
        settings.oceanSettings = savedOcean;
        settings.atmosphereSettings = savedAtmosphere;

        planetGenerator.GeneratePlanet();

        if (nameLabel != null)
            nameLabel.gameObject.SetActive(false);
    }
}