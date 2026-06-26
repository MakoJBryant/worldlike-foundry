using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlanetDragController : MonoBehaviour
{
    [Header("References")]
    public Camera cam;
    public RawImage rawImage;
    public SelectionManager selectionManager;
    public SolarSystemManager solarSystemManager;

    [Header("Tuning")]
    public float orbitRadiusSensitivity = 1f;
    public float orbitSpeedFlingSensitivity = 0.05f;
    public float spinFlingSensitivity = 4f;

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

    bool IsMouseOverUpgradeLabel(Transform planetRoot)
    {
        if (planetRoot == null || cam == null) return false;
        var label = planetRoot.GetComponentInChildren<UpgradeLabel>();
        if (label == null) return false;

        // Only block drag if there's actually an offer to click
        if (label.upgradeManager == null || label.upgradeManager.LockedOffer == null) return false;

        Vector3 labelScreenPos = cam.WorldToScreenPoint(label.transform.position);
        if (labelScreenPos.z < 0) return false;

        Vector2 screenMouse = Mouse.current.position.ReadValue();
        float dist = Vector2.Distance(
            new Vector2(labelScreenPos.x, labelScreenPos.y), screenMouse);

        return dist < label.hoverPixelRadius;
    }

    void TryBeginDrag()
    {
        if (!TryGetViewportPoint(out Vector2 viewport)) return;
        Ray ray = cam.ViewportPointToRay(viewport);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) return;

        Transform root = hit.transform.root;
        SolarBodyData body = solarSystemManager.GetBodyData(root);
        if (body == null) return;

        // Don't start a drag if clicking the upgrade label
        if (IsMouseOverUpgradeLabel(root)) return;

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
            solarSystemManager.DampenSpin(draggedBody,
                solarSystemManager.holdBrakeStrength * Time.deltaTime);
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
            float currentSpin = draggedBody.spinVelocity.magnitude;
            if (currentSpin > 5f)
            {
                Debug.Log("[Drag] Planet still spinning — wait for it to slow down.");
                draggedPlanet = null;
                draggedBody = null;
                return;
            }

            // Only count as a fling if mouse was actually moving
            // prevents hold-to-brake from being charged as a spin
            float flingStrength = smoothedScreenVelocity.magnitude;
            if (flingStrength > 2f)
            {
                var stats = draggedPlanet.GetComponent<PlanetStats>();
                if (stats != null)
                {
                    if (stats.wonder < 1f)
                    {
                        Debug.Log("[Drag] Not enough Wonder to spin — need 1.");
                        draggedPlanet = null;
                        draggedBody = null;
                        return;
                    }
                    stats.wonder -= 1f;
                }

                Vector3 axis = cam.transform.right * smoothedScreenVelocity.y
                             - cam.transform.up * smoothedScreenVelocity.x;
                solarSystemManager.AddSpin(draggedBody, axis * spinFlingSensitivity);
            }
            // If flingStrength <= 2f it was just a hold — no cost, no spin added
        }
        else
        {
            solarSystemManager.BoostOrbitSpeed(draggedBody,
                smoothedTangentVelocity * orbitSpeedFlingSensitivity);
        }

        draggedPlanet = null;
        draggedBody = null;
    }
}