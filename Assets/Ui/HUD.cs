using UnityEngine;

public class HUD : MonoBehaviour
{
    public AstronautUI astronautUI;
    public RocketUI rocketUI;
    public GameEndScreen gameEndScreen;

    private void Start()
    {
        if (astronautUI == null)
        {
            Debug.LogError("PlayerUI component not found in children.");
        }

        if (rocketUI == null)
        {
            Debug.LogError("RocketUI component not found in children.");
        }

        if (gameEndScreen == null)
        {
            Debug.LogError("GameEndScreen component not found in children.");
        }
    }

    private void OnEnable()
    {
        gameEndScreen.gameObject.SetActive(false);
    }

    public void EnableAstronautUI()
    {
        astronautUI.gameObject.SetActive(true);
        rocketUI.gameObject.SetActive(false);
    }

    public void DisableAstronautUI()
    {
        astronautUI.gameObject.SetActive(false);
    }

    public void EnableRocketUI()
    {
        rocketUI.gameObject.SetActive(true);
        astronautUI.gameObject.SetActive(false);
    }

    public void DisableRocketUI()
    {
        rocketUI.gameObject.SetActive(false);
    }
}
