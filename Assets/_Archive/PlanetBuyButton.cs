using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Floating 3D buy cube.
/// Positions itself against planet surface.
/// Hover previews current offer.
/// Click purchases.
/// </summary>
public class PlanetBuyButton : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform planet;

    public PlanetGenerator planetGenerator;
    public PlanetUpgradeManager upgradeManager;
    public SelectionManager selectionManager;

    [Header("Positioning")]
    public float distanceFromCenter = 300f;
    public float horizontalOffset = -350f;
    public float verticalOffset = -120f;

    [Header("Hover")]
    public float hoverPixelRadius = 60f;

    [Header("Animation")]
    public float hoverScale = 1.15f;

    bool isPreviewing;

    Vector3 originalScale;

    TerrainSettings savedTerrain;
    OceanSettings savedOcean;
    AtmosphereSettings savedAtmosphere;

    void Start()
    {
        originalScale = transform.localScale;
    }

    void LateUpdate()
    {
        if (cam == null || planet == null)
            return;

        bool isSelected =
            selectionManager == null ||
            selectionManager.selectedObject == planet;

        // Keep cube active for positioning
        MeshRenderer renderer =
            GetComponent<MeshRenderer>();

        Collider col =
            GetComponent<Collider>();

        if (renderer != null)
            renderer.enabled = isSelected;

        if (col != null)
            col.enabled = isSelected;

        if (!isSelected)
        {
            if (isPreviewing)
                EndPreview();

            return;
        }

        //------------------------------------------------
        // POSITION EXACTLY LIKE UpgradeLabel
        //------------------------------------------------

        Vector3 toCam =
            (cam.transform.position - planet.position).normalized;

        Vector3 camRight =
            cam.transform.right;

        Vector3 camUp =
            cam.transform.up;

        Vector3 baseFacePos =
            planet.position +
            toCam * distanceFromCenter;

        Quaternion faceCamera =
            Quaternion.LookRotation(
                baseFacePos - cam.transform.position
            );

        transform.position =
            baseFacePos +
            camRight * horizontalOffset +
            camUp * verticalOffset;

        transform.rotation =
            faceCamera;

        //------------------------------------------------
        // HOVER
        //------------------------------------------------

        bool hovering =
            IsMouseOverThis();

        if (hovering && !isPreviewing)
        {
            transform.localScale =
                originalScale * hoverScale;

            StartPreview();
        }

        if (!hovering && isPreviewing)
        {
            transform.localScale =
                originalScale;

            EndPreview();
        }

        //------------------------------------------------
        // BUY
        //------------------------------------------------

        if (
            hovering &&
            Mouse.current.leftButton.wasPressedThisFrame
        )
        {
            upgradeManager?.TryBuyOffer();
        }
    }

    bool IsMouseOverThis()
    {
        Vector3 screenPos =
            cam.WorldToScreenPoint(
                transform.position
            );

        if (screenPos.z < 0)
            return false;

        Vector2 mouse =
            Mouse.current.position.ReadValue();

        float dist =
            Vector2.Distance(
                mouse,
                new Vector2(
                    screenPos.x,
                    screenPos.y
                )
            );

        return dist < hoverPixelRadius;
    }

    void StartPreview()
    {
        if (
            upgradeManager == null ||
            upgradeManager.LockedOffer == null
        )
            return;

        if (
            planetGenerator == null ||
            planetGenerator.planetSettings == null
        )
            return;

        isPreviewing = true;

        PlanetSettings settings =
            planetGenerator.planetSettings;

        savedTerrain =
            settings.terrainSettings;

        savedOcean =
            settings.oceanSettings;

        savedAtmosphere =
            settings.atmosphereSettings;

        PlanetUpgrade offer =
            upgradeManager.LockedOffer;

        switch (offer.slot)
        {
            case UpgradeSlot.Terrain:
                settings.terrainSettings =
                    offer.terrainSettings;
                break;

            case UpgradeSlot.Ocean:
                settings.oceanSettings =
                    offer.oceanSettings;
                break;

            case UpgradeSlot.Atmosphere:
                settings.atmosphereSettings =
                    offer.atmosphereSettings;
                break;
        }

        planetGenerator.GeneratePlanet();
    }

    void EndPreview()
    {
        if (!isPreviewing)
            return;

        isPreviewing = false;

        PlanetSettings settings =
            planetGenerator.planetSettings;

        settings.terrainSettings =
            savedTerrain;

        settings.oceanSettings =
            savedOcean;

        settings.atmosphereSettings =
            savedAtmosphere;

        planetGenerator.GeneratePlanet();
    }
}