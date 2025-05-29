using UnityEngine;

public class Astronaut : MonoBehaviour
{
    [Header("References")]
    private Rocket rocket;

    private Controls controls;

    private void Awake()
    {
        rocket = FindFirstObjectByType<Rocket>();

        controls = new Controls();
    }

    private void Start()
    {

    }

    private void Update()
    {

    }

    public void EnterRocket()
    {
        if (rocket != null)
        {
            controls.Astronaut.Disable();
            rocket.OnEnterRocket();
        }
    }

    public void OnExitRocket()
    {
        controls.Astronaut.Enable();
    }
}
