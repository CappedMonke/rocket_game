using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using DG.Tweening;

public class InputManager : MonoBehaviour
{
    [Header("Input Asset")]
    [SerializeField] private InputActionAsset inputActions;

    private InputActionMap playerMap;
    private InputActionMap rocketMap;

    [Header("Settings")]
    [SerializeField] private bool enablePlayerControlsOnStart = true;

    [Header("References")]
    [SerializeField] private Player player;
    [SerializeField] private Rocket rocket;
    [SerializeField] private CinemachineCamera cinemachineCamera;

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

        player = FindFirstObjectByType<Player>();
        rocket = FindFirstObjectByType<Rocket>();
        cinemachineCamera = FindFirstObjectByType<CinemachineCamera>();

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
        UiManager.Instance.ShowPlayerUi();
        cinemachineCamera.Follow = player.transform;
        DOTween.To(() => cinemachineCamera.Lens.OrthographicSize, x => cinemachineCamera.Lens.OrthographicSize = x, 1f, 0.5f);
    }

    public void EnableRocketControls()
    {
        playerMap.Disable();
        rocketMap.Enable();
        UiManager.Instance.ShowRocketUi();
        cinemachineCamera.Follow = rocket.transform;
        DOTween.To(() => cinemachineCamera.Lens.OrthographicSize, x => cinemachineCamera.Lens.OrthographicSize = x, 10f, 0.5f);
    }
}