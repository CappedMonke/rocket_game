using UnityEngine;
using UnityEngine.InputSystem;

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
    public int oxygen = 100;
    public int maxOxygen = 100;
    public int fuel = 100;
    public int maxFuel = 100;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        player = FindFirstObjectByType<Player>();
    }

    void Update()
    {
        HandleExitRocket();
        UiManager.Instance.UpdateRocketUi(health, maxHealth, oxygen, maxOxygen, fuel, maxFuel);
    }

    void FixedUpdate()
    {
        HandleRotation();
        HandleThrust();
    }

    private void HandleRotation()
    {
        float rotateInput = rotateAction.action.ReadValue<float>();
        rb.MoveRotation(rb.rotation - rotateInput * rotationSpeed * Time.fixedDeltaTime);
    }

    private void HandleThrust()
    {
        float thrustInput = thrustAction.action.ReadValue<float>();
        Vector2 thrustVector = thrustInput * thrustSpeed * rb.transform.up;
        rb.AddForce(thrustVector);
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
}
