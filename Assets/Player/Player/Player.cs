using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerMovement))]
public class Player : MonoBehaviour
{
    [Header("Input")]
    public InputActionReference moveAction;
    public InputActionReference jumpAction;

    [Header("Interaction")]
    public InputActionReference enterRocketAction;

    [Header("Stats")]
    public int health = 100;
    public int maxHealth = 100;
    public float oxygen = 100f;
    public float maxOxygen = 100f;

    private PlayerMovement movement;

    void Awake()
    {
        movement = GetComponent<PlayerMovement>();
    }

    void OnEnable()
    {
        jumpAction.action.performed += ctx => movement.OnJumpInput();
        jumpAction.action.canceled += ctx => movement.OnJumpUpInput();
        enterRocketAction.action.performed += ctx => HandleEnterRocket();
    }

    void OnDisable()
    {
        jumpAction.action.performed -= ctx => movement.OnJumpInput();
        jumpAction.action.canceled -= ctx => movement.OnJumpUpInput();
        enterRocketAction.action.performed -= ctx => HandleEnterRocket();
    }

    void Update()
    {
        Vector2 input = new Vector2(moveAction.action.ReadValue<float>(), 0f);
        movement.SetMoveInput(input);

        UiManager.Instance.UpdatePlayerUi(health, maxHealth, Mathf.RoundToInt(oxygen), Mathf.RoundToInt(maxOxygen));
    }

    public void SetHealth(int newHealth)
    {
        health = Mathf.Clamp(newHealth, 0, maxHealth);
        UiManager.Instance.UpdatePlayerUi(health, maxHealth, Mathf.RoundToInt(oxygen), Mathf.RoundToInt(maxOxygen));
        if (health <= 0) Die();
    }

    public void SetOxygen(float newOxygen)
    {
        oxygen = Mathf.Clamp(newOxygen, 0f, maxOxygen);
        UiManager.Instance.UpdatePlayerUi(health, maxHealth, Mathf.RoundToInt(oxygen), Mathf.RoundToInt(maxOxygen));
        if (oxygen <= 0) Die();
    }

    public void SetSpeedMultiplier(float multiplier)
    {
        movement.data.runMaxSpeed *= multiplier;
        movement.data.runAcceleration *= multiplier;
    }

    void Die()
    {
        Debug.Log("Player has died.");
    }

    private void HandleEnterRocket()
    {
        Rocket rocket = FindFirstObjectByType<Rocket>();
        if (rocket != null && Vector2.Distance(transform.position, rocket.doorTransform.position) < 1f)
        {
            InputManager.Instance.EnableRocketControls();
            gameObject.SetActive(false);
            AudioSource.PlayClipAtPoint(rocket.launchSound, rocket.transform.position);
        }
    }
}