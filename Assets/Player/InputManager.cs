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
    private Player player;
    private Rocket rocket;
    private CinemachineCamera cinemachineCamera;
    private DesaturationController desaturationController;

    [Header("Oxygen Settings")]
    [SerializeField] private float oxygenDepletionRate = 1f;
    [SerializeField] private float oxygenRefillRate = 10f;

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
        desaturationController = FindFirstObjectByType<DesaturationController>();

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

    private void Update()
    {
        HandleOxygenManagement();
    }

    private void HandleOxygenManagement()
    {
        if (player != null)
        {
            float oxygenLoss = oxygenDepletionRate * Time.deltaTime;

            if (player.gameObject.activeSelf)
            {
                player.SetOxygen(player.oxygen - oxygenLoss);
            }
            else if (rocket != null && rocket.gameObject.activeSelf)
            {
                if (rocket.oxygen > 0 && player.oxygen < player.maxOxygen)
                {
                    float oxygenToRefill = oxygenRefillRate * Time.deltaTime;
                    float actualRefill = Mathf.Min(oxygenToRefill, rocket.oxygen, player.maxOxygen - player.oxygen);
                    player.SetOxygen(player.oxygen + actualRefill - oxygenLoss);
                    rocket.oxygen -= actualRefill;
                }
                else
                {
                    player.SetOxygen(player.oxygen - oxygenLoss);
                }
            }

            desaturationController.inputValue = player.oxygen;
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

        if (player != null)
        {
            player.gameObject.SetActive(false); // Ensure player is disabled when entering the rocket
        }
    }
}