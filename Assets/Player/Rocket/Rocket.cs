using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class Rocket : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference rotateAction;
    public InputActionReference thrustAction;
    public InputActionReference exitRocketAction;

    [Header("Movement")]
    public float rotationSpeed = 10f;
    public float thrustSpeed = 10f;

    [Header("References")]
    public Transform doorTransform;
    private Player player;

    [Header("Stats")]
    public int health = 100;
    public int maxHealth = 100;
    public float oxygen = 100f;
    public float maxOxygen = 100f;
    public float fuel = 100f;
    public float maxFuel = 100f;
    public float fuelLossRate = 1f;

    [Header("Sounds")]
    public AudioClip perfectLandingSound;

    [Header("Landing")]
    public float perfectLandingVelocityThreshold = 1f;
    public float perfectLandingAngleThreshold = 5f;

    private Rigidbody2D rb;
    private bool hasFlown = false; 
    private const float minVelocityThreshold = 0.1f; 
    private const float speedBoostDuration = 5f; 
    private const float speedBoostMultiplier = 2f; 
    private float flightTime = 0f; 
    private bool hasRotated = false; 

    void Awake()
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        player = FindFirstObjectByType<Player>();
    }

    void Update()
    {
        HandleExitRocket();
        UiManager.Instance.UpdateRocketUi(health, maxHealth, Mathf.RoundToInt(oxygen), Mathf.RoundToInt(maxOxygen), Mathf.RoundToInt(fuel), Mathf.RoundToInt(maxFuel));
        if (hasFlown)
        {
            flightTime += Time.deltaTime;
        }
    }

    void FixedUpdate()
    {
        HandleRotation();
        HandleThrust();
    }

    private void HandleRotation()
    {
        float rotateInput = rotateAction.action.ReadValue<float>();
        if (Mathf.Abs(rotateInput) > 0.1f) 
        {
            hasRotated = true;
        }
        rb.MoveRotation(rb.rotation - rotateInput * rotationSpeed * Time.fixedDeltaTime);
    }

    private void HandleThrust()
    {
        float thrustInput = thrustAction.action.ReadValue<float>();
        if (thrustInput > 0 && fuel > 0)
        {
            hasFlown = true;
            Vector2 thrustVector = thrustInput * thrustSpeed * rb.transform.up;
            rb.AddForce(thrustVector);
            fuel -= fuelLossRate * thrustInput * Time.fixedDeltaTime; 
            fuel = Mathf.Clamp(fuel, 0f, maxFuel); 
        }
    }
    
    private void HandleExitRocket()
    {
        if (player != null && exitRocketAction.action.WasPressedThisFrame())
        {
            InputManager.Instance.EnablePlayerControls();
            player.gameObject.SetActive(true);
            player.transform.position = doorTransform.position;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") && hasFlown)
        {
            float landingVelocity = rb.linearVelocity.magnitude; 
            float landingAngle = Mathf.Abs(rb.rotation % 360);

            if (landingAngle > 180) landingAngle = 360 - landingAngle;

            if (landingVelocity <= perfectLandingVelocityThreshold && 
                landingAngle <= perfectLandingAngleThreshold && 
                flightTime >= 2f && 
                hasRotated)
            {
                AudioSource.PlayClipAtPoint(perfectLandingSound, transform.position);
                StartCoroutine(ApplyPlayerSpeedBoost());
            }
        }
    }

    private IEnumerator ApplyPlayerSpeedBoost()
    {
        if (player != null)
        {
            player.SetSpeedMultiplier(speedBoostMultiplier);
            yield return new WaitForSeconds(speedBoostDuration);
            player.SetSpeedMultiplier(1f);
        }
    }
}
