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


    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponentInChildren<Rigidbody2D>();
        player = FindFirstObjectByType<Player>();
    }

    void Update()
    {
        HandleExitRocket();
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
