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

    private InputSystem_Actions input;
    private float pitch;

    void Awake()
    {
        input = new InputSystem_Actions();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnEnable() => input.Enable();
    void OnDisable() => input.Disable();

    void Update()
    {
        // Toggle cursor lock with Escape
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

        // Pitch — rotate camera pivot vertically
        Vector2 lookInput = input.Player.Look.ReadValue<Vector2>();
        pitch -= lookInput.y * mouseSensitivity;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);

        // Position pivot at player, apply pitch on top of player rotation
        cameraPivot.position = player.transform.position;
        cameraPivot.rotation = player.transform.rotation * Quaternion.Euler(pitch, 0f, 0f);

        // Camera position with collision
        Vector3 desiredPos = cameraPivot.position - cameraPivot.forward * cameraDistance;

        if (Physics.SphereCast(
            cameraPivot.position,
            cameraCollisionRadius,
            -cameraPivot.forward,
            out RaycastHit hit,
            cameraDistance,
            ~0,
            QueryTriggerInteraction.Ignore))
        {
            desiredPos = hit.point + hit.normal * cameraCollisionRadius;
        }

        cam.transform.position = desiredPos;
        cam.transform.LookAt(cameraPivot.position, player.GravityUp);
    }
}