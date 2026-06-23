using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class SelectionManager : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public EditorFlyCamera editorFlyCamera;
    public RawImage rawImage;

    public Transform selectedObject { get; private set; }

    // While true, clicks are ignored and the camera stays on whatever
    // focusTarget EditorFlyCamera currently has (the Sun at title screen).
    [HideInInspector]
    public bool isLocked = false;

    void Update()
    {
        if (isLocked) return;
        if (Mouse.current.leftButton.wasPressedThisFrame)
            TrySelect();
    }

    void TrySelect()
    {
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