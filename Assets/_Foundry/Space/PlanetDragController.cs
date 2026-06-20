using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Click-and-drag input for planets. Mode is decided by what's currently selected:
/// dragging the selected planet itself spins it, dragging any other planet
/// (including while the sun or nothing is selected) adjusts its orbit.
/// </summary>
public class PlanetDragController : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public RawImage rawImage;
    public SelectionManager selectionManager;
    public SolarSystemManager solarSystemManager;

    [Header("Tuning")]
    public float orbitRadiusSensitivity = 0.5f;
    public float orbitSpeedFlingSensitivity = 0.05f;
    public float spinFlingSensitivity = 50f;

    Transform draggedPlanet;
    SolarBodyData draggedBody;
    bool isSpinMode;
    Vector3 lastPlanePoint;
    Vector2 lastScreenPoint;
    Vector2 smoothedScreenVelocity;
    float smoothedTangentVelocity;

    void Update()
    {
        if (Mouse.current.leftButton.wasPressedThisFrame)
            TryBeginDrag();
        else if (draggedPlanet != null && Mouse.current.leftButton.isPressed)
            ContinueDrag();
        else if (draggedPlanet != null && Mouse.current.leftButton.wasReleasedThisFrame)
            EndDrag();
    }

    bool TryGetViewportPoint(out Vector2 normalized)
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        RectTransform rt = rawImage.rectTransform;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, screenPos, null, out Vector2 local);
        Rect rect = rt.rect;
        normalized = new Vector2((local.x - rect.x) / rect.width, (local.y - rect.y) / rect.height);
        return normalized.x >= 0 && normalized.x <= 1 && normalized.y >= 0 && normalized.y <= 1;
    }

    void TryBeginDrag()
    {
        if (!TryGetViewportPoint(out Vector2 viewport)) return;
        Ray ray = cam.ViewportPointToRay(viewport);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) return;

        Transform root = hit.transform.root;
        SolarBodyData body = solarSystemManager.GetBodyData(root);
        if (body == null) return; // not a draggable planet

        draggedPlanet = root;
        draggedBody = body;
        isSpinMode = root == selectionManager.selectedObject;

        Plane plane = new Plane(Vector3.up, draggedPlanet.position);
        plane.Raycast(ray, out float dist);
        lastPlanePoint = ray.GetPoint(dist);
        lastScreenPoint = Mouse.current.position.ReadValue();
        smoothedScreenVelocity = Vector2.zero;
        smoothedTangentVelocity = 0f;
    }

    void ContinueDrag()
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        Vector2 screenDelta = screenPos - lastScreenPoint;
        smoothedScreenVelocity = Vector2.Lerp(smoothedScreenVelocity, screenDelta, 0.5f);
        lastScreenPoint = screenPos;

        if (isSpinMode)
        {
            // Holding brakes the existing spin a little every frame, like gripping a
            // spinning ball. If you also drag, the release impulse below adds fresh
            // momentum on top — drag hard enough and you overcome the brake.
            solarSystemManager.DampenSpin(draggedBody, solarSystemManager.holdBrakeStrength * Time.deltaTime);
            return;
        }

        if (!TryGetViewportPoint(out Vector2 viewport)) return;
        Ray ray = cam.ViewportPointToRay(viewport);
        Plane plane = new Plane(Vector3.up, draggedPlanet.position);
        if (!plane.Raycast(ray, out float dist)) return;
        Vector3 worldPoint = ray.GetPoint(dist);
        Vector3 worldDelta = worldPoint - lastPlanePoint;
        lastPlanePoint = worldPoint;

        Vector3 toPlanet = draggedPlanet.position - solarSystemManager.sun.position;
        Vector3 radialDir = toPlanet.normalized;
        Vector3 tangentDir = Vector3.Cross(Vector3.up, radialDir).normalized;

        float radialAmount = Vector3.Dot(worldDelta, radialDir);
        float tangentAmount = Vector3.Dot(worldDelta, tangentDir);

        solarSystemManager.AdjustOrbitRadius(draggedBody, radialAmount * orbitRadiusSensitivity);
        smoothedTangentVelocity = Mathf.Lerp(smoothedTangentVelocity, tangentAmount, 0.5f);
    }

    void EndDrag()
    {
        if (isSpinMode)
        {
            Vector3 axis = cam.transform.right * smoothedScreenVelocity.y
                         - cam.transform.up * smoothedScreenVelocity.x;
            solarSystemManager.AddSpin(draggedBody, axis * spinFlingSensitivity);
        }
        else
        {
            solarSystemManager.BoostOrbitSpeed(draggedBody, smoothedTangentVelocity * orbitSpeedFlingSensitivity);
        }

        draggedPlanet = null;
        draggedBody = null;
    }
}