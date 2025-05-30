using UnityEngine;

public class Astronaut : MonoBehaviour
{
    [Header("References")]
    private Rocket rocket;
    private AstronautMovement movement;
    private Controls controls;

    private void Awake()
    {
        movement = GetComponent<AstronautMovement>();
        rocket = FindFirstObjectByType<Rocket>();

        controls = new Controls();
        controls.Astronaut.EnterRocket.performed += ctx => EnterRocket();
        controls.Astronaut.Jump.performed += ctx => movement.OnJumpInput();
        controls.Astronaut.Jump.canceled += ctx => movement.OnJumpUpInput();
        controls.Astronaut.Move.performed += ctx => movement.SetMoveInput(ctx.ReadValue<Vector2>());
        controls.Astronaut.Move.canceled += ctx => movement.SetMoveInput(Vector2.zero);
    }

    private void OnEnable()
    {
        controls.Astronaut.Enable();
    }

    private void OnDisable()
    {
        controls.Astronaut.Disable();
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
