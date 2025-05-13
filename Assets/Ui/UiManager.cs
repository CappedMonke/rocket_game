using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance { get; private set; }

    [Header("Player")]
    public GameObject playerUi;
    public TMP_Text playerHealthText;
    public TMP_Text playerOxygenText;

    [Header("Rocket")]
    public GameObject rocketUi;
    public TMP_Text rocketHealthText;
    public TMP_Text rocketOxygenText;
    public TMP_Text rocketFuelText;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    internal void ShowPlayerUi()
    {
        playerUi.SetActive(true);
        rocketUi.SetActive(false);
    }

    internal void ShowRocketUi()
    {
        rocketUi.SetActive(true);
        playerUi.SetActive(false);
    }

    internal void UpdatePlayerUi(int health, int maxHealth, int oxygen, int maxOxygen)
    {
        playerHealthText.text = $"Health: {health}/{maxHealth}";
        playerOxygenText.text = $"Oxygen: {oxygen}/{maxOxygen}";
    }

    internal void UpdateRocketUi(int health, int maxHealth, int oxygen, int maxOxygen, int fuel, int maxFuel)
    {
        rocketHealthText.text = $"Health: {health}/{maxHealth}";
        rocketOxygenText.text = $"Oxygen: {oxygen}/{maxOxygen}";
        rocketFuelText.text = $"Fuel: {fuel}/{maxFuel}";
    }
}
