using UnityEngine;

/// <summary>
/// Shifts the camera's projection so whatever it's looking at (its forward direction)
/// renders off-center on screen instead of dead center, while the scene still fills
/// the entire viewport edge-to-edge — no black bars, no resized render area. Used in
/// the space view to leave visual room for a planet detail UI panel.
/// </summary>
[RequireComponent(typeof(Camera))]
public class CameraFrameOffset : MonoBehaviour
{
    [Tooltip("0 = centered. 1 = focus point sits at the left edge of the screen. 0.75 lands it about an eighth of the way in from the left edge.")]
    [Range(0f, 1f)]
    public float horizontalOffset = 0.5f;

    Camera cam;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void OnDisable()
    {
        if (cam != null)
            cam.ResetProjectionMatrix(); // hand control back to normal auto FOV-based projection
    }

    void LateUpdate()
    {
        float near = cam.nearClipPlane;
        float far = cam.farClipPlane;
        float verticalFov = cam.fieldOfView * Mathf.Deg2Rad;
        float top = near * Mathf.Tan(verticalFov * 0.5f);
        float bottom = -top;
        float right = top * cam.aspect;
        float left = -right;

        // Shifting the frustum window to the right pushes whatever the camera is
        // looking straight at (camera-space x = 0) toward the left of the resulting
        // image. Window width is unchanged, so FOV doesn't change — only where
        // it's centered does.
        float shift = horizontalOffset * right;
        cam.projectionMatrix = Matrix4x4.Frustum(left + shift, right + shift, bottom, top, near, far);
    }
}