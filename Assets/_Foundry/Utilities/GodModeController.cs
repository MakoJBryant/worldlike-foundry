using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class GodModeController : MonoBehaviour
{
    [Header("Mode Toggles")]
    [Tooltip("Enable if this scene has a player that walks on planet surfaces (PlayerController, PlanetCameraController, GravityBody). Uncheck if there's no player in this game.")]
    public bool usePlayerView = true;
    [Tooltip("Enable if this scene has the space/solar editor view (EditorFlyCamera, SelectionManager).")]
    public bool useSpaceView = true;

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
        if (usePlayerView && playerController != null)
            rb = playerController.GetComponent<Rigidbody>();

        originalCameraParent = cam.transform.parent;
        originalCameraLocalPosition = cam.transform.localPosition;
        originalCameraLocalRotation = cam.transform.localRotation;

        editorFlyCamera = cam.GetComponent<EditorFlyCamera>();

        // Disable everything that belongs to play mode at the start
        if (useSpaceView)
        {
            if (editorFlyCamera != null) editorFlyCamera.enabled = false;
            if (selectionManager != null) selectionManager.enabled = false;
        }
        if (usePlayerView)
        {
            if (planetCameraController != null) planetCameraController.enabled = false;
            if (playerController != null) playerController.enabled = false;
            if (gravityBody != null) gravityBody.enabled = false;
        }
    }

    void Start()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnModeChanged += OnGameModeChanged;
            // GameManager may have already fired its initial mode before we subscribed,
            // depending on script execution order. Sync to whatever the current mode is now.
            OnGameModeChanged(GameManager.Instance.CurrentMode);
        }
    }

    void OnDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.OnModeChanged -= OnGameModeChanged;
    }

    void OnGameModeChanged(GameMode mode)
    {
        if (mode == GameMode.SolarEditor && !isGodMode)
            EnterGodMode();
        else if (mode == GameMode.PlayerSurface && isGodMode)
            ExitGodMode();
    }

    void Update()
    {
        if (!usePlayerView) return; // nothing to toggle to without a player

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

        if (usePlayerView && playerController != null)
        {
            // Save player state
            savedPlayerPosition = playerController.transform.position;
            savedPlayerRotation = playerController.transform.rotation;

            // Unparent player from planet
            playerController.transform.SetParent(null);

            // Freeze player
            if (gravityBody != null) gravityBody.enabled = false;
            if (rb != null)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
            playerController.enabled = false;
            if (planetCameraController != null) planetCameraController.enabled = false;
        }

        // Detach camera from player hierarchy
        cam.transform.SetParent(null);

        // Extend far clip for solar system scale
        cam.farClipPlane = editorModeFarClip;

        if (useSpaceView)
        {
            // Set editor camera focus to current planet
            if (usePlayerView && playerController != null && playerController.planet != null)
                editorFlyCamera.focusTarget = playerController.planet.transform;

            // Enable god mode systems
            if (editorFlyCamera != null) editorFlyCamera.enabled = true;
            if (selectionManager != null) selectionManager.enabled = true;
        }

        // Unlock cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ExitGodMode()
    {
        isGodMode = false;

        if (useSpaceView)
        {
            // Disable god mode systems
            if (editorFlyCamera != null) editorFlyCamera.enabled = false;
            if (selectionManager != null) selectionManager.enabled = false;
        }

        // Restore far clip for play mode
        cam.farClipPlane = playModeFarClip;

        if (usePlayerView && playerController != null)
        {
            // Reattach camera to player
            cam.transform.SetParent(originalCameraParent);
            cam.transform.localPosition = originalCameraLocalPosition;
            cam.transform.localRotation = originalCameraLocalRotation;

            // Restore player position and rotation
            playerController.transform.position = savedPlayerPosition;
            playerController.transform.rotation = savedPlayerRotation;
            if (rb != null) rb.linearVelocity = Vector3.zero;

            // Reparent player to planet
            if (playerController.planet != null)
                playerController.transform.SetParent(playerController.planet.transform);

            // Re-enable player
            if (gravityBody != null) gravityBody.enabled = true;
            playerController.enabled = true;
            if (planetCameraController != null) planetCameraController.enabled = true;

            // Lock cursor
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // No player to hand control back to — keep cursor free for the space view
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}