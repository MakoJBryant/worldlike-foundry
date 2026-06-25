using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// During gameplay, detects which planet the cursor is hovering over
/// and sets its TitlePlanetHover.isHovered flag — same hover scale
/// effect as the title screen, but active during normal play.
/// </summary>
public class GameplayHover : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public RawImage rawImage;
    public SolarSystemManager solarSystemManager;
    public SelectionManager selectionManager;

    TitlePlanetHover lastHovered;

    void Update()
    {
        Transform hovered = GetHoveredPlanet();

        // Don't hover-highlight the currently selected planet
        if (hovered != null && selectionManager != null &&
            hovered == selectionManager.selectedObject)
            hovered = null;

        TitlePlanetHover newHover = hovered != null
            ? hovered.GetComponent<TitlePlanetHover>()
            : null;

        if (newHover != lastHovered)
        {
            if (lastHovered != null) lastHovered.isHovered = false;
            if (newHover != null) newHover.isHovered = true;
            lastHovered = newHover;
        }
    }

    Transform GetHoveredPlanet()
    {
        if (cam == null || rawImage == null) return null;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        RectTransform rt = rawImage.rectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt, mousePos, null, out Vector2 local);
        Rect rect = rt.rect;
        Vector2 normalized = new Vector2(
            (local.x - rect.x) / rect.width,
            (local.y - rect.y) / rect.height);

        if (normalized.x < 0 || normalized.x > 1 ||
            normalized.y < 0 || normalized.y > 1)
            return null;

        int layerMask = ~LayerMask.GetMask("Ignore Raycast", "UI");
        Ray ray = cam.ViewportPointToRay(normalized);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            return null;

        Transform root = hit.transform.root;

        // Only hover planets that are in the solar system manager
        if (solarSystemManager.GetBodyData(root) != null) return root;
        if (solarSystemManager.sun == root) return root;
        return null;
    }
}