using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class TitleScreenController : MonoBehaviour
{
    [Header("References")]
    public SolarSystemManager solarSystemManager;
    public SelectionManager selectionManager;
    public EditorFlyCamera editorFlyCamera;
    public Camera cam;
    public RawImage rawImage;
    public Transform sun;

    [Header("Title Screen Planets")]
    public Transform startPlanet;
    public Transform optionsPlanet;
    public Transform exitPlanet;

    [Header("Sun Label")]
    public LabelFaceCamera sunLabel;

    [Header("Options Panel")]
    public GameObject optionsPanel;

    [Header("Controls Panel")]
    public GameObject controlsPanel;

    TitlePlanetHover sunHover;
    TitlePlanetHover startHover;
    TitlePlanetHover optionsHover;
    TitlePlanetHover exitHover;

    bool titleActive = true;

    void Start()
    {
        solarSystemManager.timeScale = 0f;
        solarSystemManager.gameActive = false;

        if (editorFlyCamera != null && sun != null)
            editorFlyCamera.focusTarget = sun;

        if (selectionManager != null)
            selectionManager.isLocked = true;

        sunHover = GetOrAddHover(sun);
        startHover = GetOrAddHover(startPlanet);
        optionsHover = GetOrAddHover(optionsPlanet);
        exitHover = GetOrAddHover(exitPlanet);
    }

    TitlePlanetHover GetOrAddHover(Transform t)
    {
        if (t == null) return null;
        var h = t.GetComponent<TitlePlanetHover>();
        if (h == null) h = t.gameObject.AddComponent<TitlePlanetHover>();
        return h;
    }

    void Update()
    {
        if (!titleActive) return;
        HandleHover();
        HandleClick();
    }

    void HandleHover()
    {
        Transform hovered = GetHoveredPlanet();

        if (sunHover != null) sunHover.isHovered = (hovered == sun);
        if (startHover != null) startHover.isHovered = (hovered == startPlanet);
        if (optionsHover != null) optionsHover.isHovered = (hovered == optionsPlanet);
        if (exitHover != null) exitHover.isHovered = (hovered == exitPlanet);

        if (sunLabel != null)
        {
            if (hovered == startPlanet) sunLabel.SetText("CONTROLS");
            else if (hovered == optionsPlanet) sunLabel.SetText("OPTIONS");
            else if (hovered == exitPlanet) sunLabel.SetText("EXIT");
            else if (hovered == sun) sunLabel.SetText("PLAY");
            else sunLabel.SetText("AXIS");
        }
    }

    void HandleClick()
    {
        if (!Mouse.current.leftButton.wasPressedThisFrame) return;

        Transform clicked = GetHoveredPlanet();
        if (clicked == null) return;

        if (clicked == sun) OnStartGame();
        // planets do nothing for now
    }

    Transform GetHoveredPlanet()
    {
        if (cam == null || rawImage == null) return null;

        Vector2 mousePos = Mouse.current.position.ReadValue();
        RectTransform rawImageRect = rawImage.rectTransform;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImageRect, mousePos, null, out localPoint);
        Rect rect = rawImageRect.rect;
        Vector2 normalized = new Vector2(
            (localPoint.x - rect.x) / rect.width,
            (localPoint.y - rect.y) / rect.height);

        if (normalized.x < 0 || normalized.x > 1 || normalized.y < 0 || normalized.y > 1)
            return null;

        int layerMask = ~LayerMask.GetMask("Ignore Raycast", "UI");
        Ray ray = cam.ViewportPointToRay(normalized);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask))
            return null;

        Transform root = hit.transform.root;
        if (root == sun) return sun;
        if (root == startPlanet) return startPlanet;
        if (root == optionsPlanet) return optionsPlanet;
        if (root == exitPlanet) return exitPlanet;
        return null;
    }

    void OnStartGame()
    {
        titleActive = false;
        solarSystemManager.timeScale = 1f;
        solarSystemManager.gameActive = true;

        if (selectionManager != null)
            selectionManager.isLocked = false;

        if (sunHover != null) sunHover.isHovered = false;
        if (startHover != null) startHover.isHovered = false;
        if (optionsHover != null) optionsHover.isHovered = false;
        if (exitHover != null) exitHover.isHovered = false;

        if (sunLabel != null)
            sunLabel.gameObject.SetActive(false);

        gameObject.SetActive(false);
    }

    void OnControls()
    {
        if (controlsPanel != null)
            controlsPanel.SetActive(!controlsPanel.activeSelf);
        else
            Debug.Log("[TitleScreen] Controls clicked — panel not yet built.");
    }

    void OnOptions()
    {
        if (optionsPanel != null)
            optionsPanel.SetActive(!optionsPanel.activeSelf);
    }

    void OnExit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}