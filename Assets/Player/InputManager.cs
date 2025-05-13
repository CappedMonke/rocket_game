using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    [Header("Input Asset")]
    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap playerMap;
    private InputActionMap rocketMap;

    [Header("Settings")]
    [SerializeField] private bool enablePlayerControlsOnStart = true;

    public static InputManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern (optional, remove if not needed)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        playerMap = inputActions.FindActionMap("Player", true);
        rocketMap = inputActions.FindActionMap("Rocket", true);

        if (enablePlayerControlsOnStart)
        {
            EnablePlayerControls();
        }
        else
        {
            EnableRocketControls();
        }
    }

    public void EnablePlayerControls()
    {
        rocketMap.Disable();
        playerMap.Enable();
    }

    public void EnableRocketControls()
    {
        playerMap.Disable();
        rocketMap.Enable();
    }
}