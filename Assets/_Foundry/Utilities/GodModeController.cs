using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class GodModeController : MonoBehaviour
{
    [Header("References")]
    public PlayerController playerController;
    public PlanetCameraController planetCameraController;
    public GravityBody gravityBody;
    public Camera cam;

    [Header("Clipping Planes")]
    public float playModeFarClip = 10000f;
    public float editorModeFarClip = 100000f;

    private bool isGodMode = false;
    private Vector3 savedPlayerPosition;
    private Quaternion savedPlayerRotation;
    private Rigidbody rb;
    private Transform originalCameraParent;
    private Vector3 originalCameraLocalPosition;
    private Quaternion originalCameraLocalRotation;
    private EditorFlyCamera editorFlyCamera;

    void Awake()
    {
        rb = playerController.GetComponent<Rigidbody>();

        originalCameraParent = cam.transform.parent;
        originalCameraLocalPosition = cam.transform.localPosition;
        originalCameraLocalRotation = cam.transform.localRotation;

        editorFlyCamera = cam.GetComponent<EditorFlyCamera>();
        editorFlyCamera.enabled = false;
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (isGodMode)
                ExitGodMode();
            else
                EnterGodMode();
        }
    }

    void EnterGodMode()
    {
        isGodMode = true;

        // Save player state
        savedPlayerPosition = playerController.transform.position;
        savedPlayerRotation = playerController.transform.rotation;

        // Freeze player
        gravityBody.enabled = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        playerController.enabled = false;
        planetCameraController.enabled = false;

        // Detach camera from player hierarchy
        cam.transform.SetParent(null);

        // Extend far clip for solar system scale
        cam.farClipPlane = editorModeFarClip;

        // Set editor camera focus to current planet
        if (playerController.planet != null)
            editorFlyCamera.focusTarget = playerController.planet.transform;

        // Enable editor camera
        editorFlyCamera.enabled = true;

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ExitGodMode()
    {
        isGodMode = false;

        // Disable editor camera
        editorFlyCamera.enabled = false;

        // Restore far clip for play mode
        cam.farClipPlane = playModeFarClip;

        // Reattach camera to player
        cam.transform.SetParent(originalCameraParent);
        cam.transform.localPosition = originalCameraLocalPosition;
        cam.transform.localRotation = originalCameraLocalRotation;

        // Restore player state
        playerController.transform.position = savedPlayerPosition;
        playerController.transform.rotation = savedPlayerRotation;
        rb.linearVelocity = Vector3.zero;
        gravityBody.enabled = true;
        playerController.enabled = true;
        planetCameraController.enabled = true;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}