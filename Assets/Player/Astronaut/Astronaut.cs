using UnityEditor.Callbacks;
using UnityEngine;

public class Astronaut : MonoBehaviour
{
    [Header("References")]
    private Rigidbody2D rb;
    private Rocket rocket;
    private AstronautMovement movement;
    private Controls controls;

    private bool canEnterRocket = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
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

    private void FixedUpdate()
    {
        if (Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Sign(rb.linearVelocity.x) * Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
    }

    public void EnterRocket()
    {
        if (rocket != null && canEnterRocket)
        {
            controls.Astronaut.Disable();
            rocket.OnEnterRocket();
        }
    }

    public void OnExitRocket()
    {
        controls.Astronaut.Enable();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("RocketDoor"))
        {
            canEnterRocket = true;
        } 
    }
}
