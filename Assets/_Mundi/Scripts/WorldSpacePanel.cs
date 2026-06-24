using UnityEngine;
using TMPro;

/// <summary>
/// A world-space panel that floats to the side of a planet,
/// always faces the camera, and shows only when the planet is selected.
/// Content is set via SetContent() so both title screen and gameplay
/// systems can reuse the same panel without touching positioning code.
/// </summary>
public class WorldSpacePanel : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public Transform planet;
    public SelectionManager selectionManager;

    [Header("Positioning")]
    [Tooltip("How far from the planet center the panel sits along the camera-facing axis.")]
    public float distanceFromCenter = 300f;
    [Tooltip("How far to the right of the planet the panel sits.")]
    public float horizontalOffset = 400f;
    [Tooltip("Vertical offset from planet center.")]
    public float verticalOffset = 0f;

    [Header("Content")]
    public TextMeshPro titleText;
    public TextMeshPro bodyText;

    [Header("Placeholder")]
    public string placeholderTitle = "Planet Name";
    public string placeholderBody = "Details go here.";

    bool isVisible = false;

    void Start()
    {
        SetContent(placeholderTitle, placeholderBody);
        SetVisible(false);
    }

    void LateUpdate()
    {
        if (cam == null || planet == null) return;

        bool shouldShow = selectionManager != null &&
                          selectionManager.selectedObject == planet;

        if (shouldShow != isVisible)
            SetVisible(shouldShow);

        if (!isVisible) return;

        Vector3 toCam = (cam.transform.position - planet.position).normalized;
        Vector3 camRight = cam.transform.right;
        Vector3 camUp = cam.transform.up;

        transform.position = planet.position
            + toCam * distanceFromCenter
            + camRight * horizontalOffset
            + camUp * verticalOffset;

        transform.rotation = Quaternion.LookRotation(
            transform.position - cam.transform.position
        );
    }

    void SetVisible(bool visible)
    {
        isVisible = visible;
        if (titleText != null) titleText.gameObject.SetActive(visible);
        if (bodyText != null) bodyText.gameObject.SetActive(visible);
    }

    /// <summary>
    /// Call this to push content into the panel.
    /// Title screen uses it for options/controls.
    /// Gameplay uses it for upgrade shop listings.
    /// </summary>
    public void SetContent(string title, string body)
    {
        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = body;
    }

    public void SetTitle(string title)
    {
        if (titleText != null) titleText.text = title;
    }

    public void SetBody(string body)
    {
        if (bodyText != null) bodyText.text = body;
    }
}