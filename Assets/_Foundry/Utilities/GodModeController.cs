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
    public SelectionManager selectionManager;

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

        // Disable everything that belongs to play mode at the start
        editorFlyCamera.enabled = false;
        planetCameraController.enabled = false;
        playerController.enabled = false;
        gravityBody.enabled = false;
        selectionManager.enabled = false;
    }

    void Start()
    {
        // Subscribe to GameManager mode changes
        if (GameManager.Instance != null)
            GameManager.Instance.OnModeChanged += OnGameModeChanged;
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnModeChanged -= OnGameModeChanged;
    }

    void OnGameModeChanged(GameMode mode)
    {
        if (mode == GameMode.SolarEditor)
            EnterGodMode();
        else if (mode == GameMode.PlayerSurface)
            ExitGodMode();
    }

    void Update()
    {
        if (Keyboard.current.tabKey.wasPressedThisFrame)
        {
            if (isGodMode)
                GameManager.Instance.EnterPlayerSurface();
            else
                GameManager.Instance.EnterSolarEditor();
        }
    }

    void EnterGodMode()
    {
        isGodMode = true;

        // Save player state
        savedPlayerPosition = playerController.transform.position;
        savedPlayerRotation = playerController.transform.rotation;

        // Unparent player from planet
        playerController.transform.SetParent(null);

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

        // Enable god mode systems
        editorFlyCamera.enabled = true;
        selectionManager.enabled = true;

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ExitGodMode()
    {
        isGodMode = false;

        // Disable god mode systems
        editorFlyCamera.enabled = false;
        selectionManager.enabled = false;

        // Restore far clip for play mode
        cam.farClipPlane = playModeFarClip;

        // Reattach camera to player
        cam.transform.SetParent(originalCameraParent);
        cam.transform.localPosition = originalCameraLocalPosition;
        cam.transform.localRotation = originalCameraLocalRotation;

        // Restore player position and rotation
        playerController.transform.position = savedPlayerPosition;
        playerController.transform.rotation = savedPlayerRotation;
        rb.linearVelocity = Vector3.zero;

        // Reparent player to planet
        if (playerController.planet != null)
            playerController.transform.SetParent(playerController.planet.transform);

        // Re-enable player
        gravityBody.enabled = true;
        playerController.enabled = true;
        planetCameraController.enabled = true;

        // Lock cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}