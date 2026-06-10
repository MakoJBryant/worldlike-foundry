using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Free fly camera for editor mode.
/// Right click + drag to orbit, scroll to zoom, middle click + drag to pan, F to focus target.
/// </summary>
[DisallowMultipleComponent]
public class EditorFlyCamera : MonoBehaviour
{
    [Header("Target")]
    public Transform focusTarget;

    [Header("Orbit")]
    public float orbitDistance = 2000f;
    public float orbitSensitivity = 1f;
    public float minDistance = 100f;
    public float maxDistance = 50000f;

    [Header("Zoom")]
    public float zoomSpeed = 200000f;
    public float zoomSmoothing = 8f;

    [Header("Pan")]
    public float panSensitivity = 1f;

    [Header("Focus")]
    public float focusSmoothing = 8f;

    [Header("Planet Collision")]
    public float planetRadius = 1000f;
    public float surfaceBuffer = 50f;

    private float currentYaw = 0f;
    private float currentPitch = 30f;
    private float targetDistance;
    private Vector3 focusPoint;

    void OnEnable()
    {
        if (focusTarget != null)
            focusPoint = focusTarget.position;

        targetDistance = orbitDistance;

        Vector3 angles = transform.eulerAngles;
        currentYaw = angles.y;
        currentPitch = angles.x;

        ApplyTransform();
    }

    void LateUpdate()
    {
        HandleFocus();
        HandleOrbit();
        HandleZoom();
        HandlePan();
        ApplyTransform();
    }

    void HandleFocus()
    {
        if (Keyboard.current.fKey.wasPressedThisFrame && focusTarget != null)
        {
            focusPoint = focusTarget.position;
            targetDistance = orbitDistance;
        }
    }

    void HandleOrbit()
    {
        if (Mouse.current.rightButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            float scaledSensitivity = orbitSensitivity * (orbitDistance / maxDistance);
            currentYaw += delta.x * scaledSensitivity;
            currentPitch -= delta.y * scaledSensitivity;
            currentPitch = Mathf.Clamp(currentPitch, -89f, 89f);
        }
    }

    void HandleZoom()
    {
        float scroll = Mouse.current.scroll.ReadValue().y;
        if (scroll != 0f)
        {
            float scaledZoom = zoomSpeed * (targetDistance / maxDistance);
            targetDistance -= scroll * scaledZoom * Time.deltaTime;
            targetDistance = Mathf.Clamp(targetDistance, minDistance, maxDistance);
        }

        orbitDistance = Mathf.Lerp(orbitDistance, targetDistance, zoomSmoothing * Time.deltaTime);
        // Hard clamp after lerp so it never exceeds bounds
        orbitDistance = Mathf.Clamp(orbitDistance, minDistance, maxDistance);
    }

    void HandlePan()
    {
        if (Mouse.current.middleButton.isPressed)
        {
            Vector2 delta = Mouse.current.delta.ReadValue();
            Vector3 right = transform.right * (-delta.x * panSensitivity * (orbitDistance / maxDistance));
            Vector3 up = transform.up * (-delta.y * panSensitivity * (orbitDistance / maxDistance));
            focusPoint += right + up;
        }
    }

    void ApplyTransform()
    {
        Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
        Vector3 offset = rotation * new Vector3(0f, 0f, -orbitDistance);
        Vector3 desiredPosition = focusPoint + offset;

        // Prevent camera from going inside planet
        float distFromPlanetCenter = Vector3.Distance(desiredPosition, focusPoint);
        if (distFromPlanetCenter < planetRadius + surfaceBuffer)
        {
            orbitDistance = planetRadius + surfaceBuffer;
            targetDistance = orbitDistance;
            offset = rotation * new Vector3(0f, 0f, -orbitDistance);
            desiredPosition = focusPoint + offset;
        }

        transform.position = desiredPosition;
        transform.rotation = rotation;
    }
}