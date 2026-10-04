using UnityEngine;
using UnityEngine.InputSystem;

public class PlanetCameraController : MonoBehaviour
{
    [Header("References")]
    public PlayerController player;
    public Transform cameraPivot;
    public Camera cam;

    [Header("Pitch")]
    public float mouseSensitivity = 2f;
    public float minPitch = -80f;
    public float maxPitch = 80f;

    [Header("Distance")]
    public float cameraDistance = 4f;
    public float cameraCollisionRadius = 0.3f;
    public LayerMask collisionMask;

    [Header("Smoothing")]
    public float positionSmoothing = 20f;

    private InputSystem_Actions input;
    private float pitch;
    private Vector3 smoothedPivotPosition;

    void Awake()
    {
        input = new InputSystem_Actions();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable()
    {
        input.Enable();
        if (player != null)
            smoothedPivotPosition = player.transform.position;
    }

    void OnDisable() => input.Disable();

    void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            bool locked = Cursor.lockState == CursorLockMode.Locked;
            Cursor.lockState = locked ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = locked;
        }
    }

    void LateUpdate()
    {
        if (player == null || cameraPivot == null || cam == null) return;

        // Pitch
        Vector2 lookInput = input.Player.Look.ReadValue<Vector2>();
        pitch -= lookInput.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Smooth pivot position to reduce jitter from physics/render mismatch
        smoothedPivotPosition = Vector3.Lerp(
            smoothedPivotPosition,
            player.transform.position,
            positionSmoothing * Time.deltaTime);

        cameraPivot.position = smoothedPivotPosition;
        cameraPivot.rotation = player.transform.rotation * Quaternion.Euler(pitch, 0f, 0f);

        // Camera position with collision
        Vector3 desiredPos = cameraPivot.position - cameraPivot.forward * cameraDistance;

        if (Physics.SphereCast(
            cameraPivot.position,
            cameraCollisionRadius,
            -cameraPivot.forward,
            out RaycastHit hit,
            cameraDistance,
            collisionMask,
            QueryTriggerInteraction.Ignore))
        {
            desiredPos = hit.point + hit.normal * cameraCollisionRadius;
        }

        cam.transform.position = desiredPos;
        cam.transform.LookAt(cameraPivot.position, player.GravityUp);
    }
}