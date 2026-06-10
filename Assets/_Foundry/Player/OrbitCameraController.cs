using UnityEngine;
using UnityEngine.InputSystem;

public class OrbitCameraController : MonoBehaviour
{
    [Header("Target")]
    public Transform target;

    [Header("Orbit")]
    public float distance = 15f;
    public float xSpeed = 120f;
    public float ySpeed = 80f;

    public float yMinLimit = -20f;
    public float yMaxLimit = 80f;

    float x = 0f;
    float y = 0f;

    [Header("Zoom")]
    public float zoomSpeed = 250f;
    public float minDistance = 2f;
    public float maxDistance = 50f;

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;
    }

    void LateUpdate()
    {
        if (!target) return;

        HandleZoom();
        HandleOrbit();

        ApplyCameraTransform();
    }

    void HandleZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;

        if (scroll != 0)
        {
            distance -= scroll * zoomSpeed * Time.deltaTime;
            distance = Mathf.Clamp(distance, minDistance, maxDistance);
        }
    }

    void HandleOrbit()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();

            x += delta.x * xSpeed * Time.deltaTime;
            y -= delta.y * ySpeed * Time.deltaTime;

            y = ClampAngle(y, yMinLimit, yMaxLimit);
        }
    }

    void ApplyCameraTransform()
    {
        Quaternion rotation = Quaternion.Euler(y, x, 0);

        Vector3 negDistance = new Vector3(0f, 0f, -distance);
        Vector3 position = rotation * negDistance + target.position;

        transform.rotation = rotation;
        transform.position = position;
    }

    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360) angle += 360;
        if (angle > 360) angle -= 360;
        return Mathf.Clamp(angle, min, max);
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }
}