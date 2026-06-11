using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Handles object selection in god mode.
/// Left click to select a planet or sun, camera snaps to focus on it.
/// </summary>
[DisallowMultipleComponent]
public class SelectionManager : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public EditorFlyCamera editorFlyCamera;
    public RawImage rawImage;

    public Transform selectedObject { get; private set; }

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
            TrySelect();
    }

    void TrySelect()
    {
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // Convert screen position to render texture viewport space
        RectTransform rawImageRect = rawImage.rectTransform;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rawImageRect, mousePos, null, out localPoint);

        Rect rect = rawImageRect.rect;
        Vector2 normalized = new Vector2(
            (localPoint.x - rect.x) / rect.width,
            (localPoint.y - rect.y) / rect.height);

        // Clamp to valid viewport range
        if (normalized.x < 0 || normalized.x > 1 || normalized.y < 0 || normalized.y > 1)
        {
            Debug.Log("[SelectionManager] Click outside viewport");
            return;
        }

        Ray ray = cam.ViewportPointToRay(normalized);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            Debug.Log($"[SelectionManager] Hit: {hit.transform.name} root: {hit.transform.root.name}");
            SelectObject(hit.transform.root);
        }
        else
        {
            Debug.Log("[SelectionManager] Raycast hit nothing");
        }
    }

    void SelectObject(Transform obj)
    {
        selectedObject = obj;

        if (editorFlyCamera != null)
            editorFlyCamera.focusTarget = obj;

        Debug.Log($"[SelectionManager] Selected: {obj.name}");
    }

    public void Deselect()
    {
        selectedObject = null;
    }
}