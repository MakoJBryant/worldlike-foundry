using UnityEngine;

/// <summary>
/// Manages global game state and persists across scenes.
/// Controls which mode the game starts in and handles mode transitions.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Settings")]
    public GameMode startingMode = GameMode.SolarEditor;

    public GameMode CurrentMode { get; private set; }

    // Event other systems can subscribe to when mode changes
    public event System.Action<GameMode> OnModeChanged;

    void Awake()
    {
        // Singleton pattern — persist across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        SetMode(startingMode);
    }

    public void SetMode(GameMode mode)
    {
        CurrentMode = mode;
        OnModeChanged?.Invoke(mode);
        Debug.Log($"[GameManager] Mode set to: {mode}");
    }

    public void EnterSolarEditor()
    {
        SetMode(GameMode.SolarEditor);
    }

    public void EnterPlayerSurface()
    {
        SetMode(GameMode.PlayerSurface);
    }

    public bool IsInSolarEditor => CurrentMode == GameMode.SolarEditor;
    public bool IsInPlayerSurface => CurrentMode == GameMode.PlayerSurface;
}

public enum GameMode
{
    SolarEditor,
    PlayerSurface
}