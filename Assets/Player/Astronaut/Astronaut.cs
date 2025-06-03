using System;
using UnityEditor.Callbacks;
using UnityEngine;

public class Astronaut : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private int health = 100;
    [SerializeField] private int maxHealth = 100;
    [SerializeField] private int oxygen = 100;
    [SerializeField] private int maxOxygen = 100;

    [Header("Controls")]
    [SerializeField] private bool startWithControlsEnabled = true;

    [Header("References")]
    [SerializeField] private AudioClip heartbeatSound;
    [SerializeField] private AudioClip heavyBreathingSound;
    private Rigidbody2D rb;
    private Rocket rocket;
    private AstronautMovement movement;
    private Controls controls;
    private CameraManager cameraManager;

    private bool canEnterRocket = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        movement = GetComponent<AstronautMovement>();
        rocket = FindFirstObjectByType<Rocket>();
        cameraManager = FindFirstObjectByType<CameraManager>();

        controls = new Controls();
        controls.Astronaut.EnterRocket.performed += ctx => EnterRocket();
        controls.Astronaut.Jump.performed += ctx => movement.OnJumpInput();
        controls.Astronaut.Jump.canceled += ctx => movement.OnJumpUpInput();
        controls.Astronaut.Move.performed += ctx => movement.SetMoveInput(ctx.ReadValue<Vector2>());
        controls.Astronaut.Move.canceled += ctx => movement.SetMoveInput(Vector2.zero);

        if (startWithControlsEnabled)
        {
            controls.Astronaut.Enable();

            if (cameraManager != null)
            {
                cameraManager.SwitchToAstronautCam();
            }
        }
        else
        {
            controls.Astronaut.Disable();
        }
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
            gameObject.SetActive(false);

            if (cameraManager != null)
            {
                cameraManager.SwitchToRocketCam();
            }
        }
    }

    public void OnExitRocket(Vector2 spawnPosition)
    {
        gameObject.SetActive(true);
        transform.position = spawnPosition;
        rb.linearVelocity = Vector2.zero;

        controls.Astronaut.Enable();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("RocketDoor"))
        {
            canEnterRocket = true;
            if (rocket != null)
            {
                rocket.EnableOutline();
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("RocketDoor"))
        {
            canEnterRocket = false;
            if (rocket != null)
            {
                rocket.DisableOutline();
            }
        }
    }

    public void ApplyPerfectLandingBoost()
    {
        movement.ApplyPerfectLandingBoost();
    }

    public void ApplyStats(UpgradeBuff buff)
    {
        if (buff == null) return;

        movement.data.jumpHeight = buff.jumpHeight;
        maxOxygen = buff.maxOxygenAstronaut;
    }
}
