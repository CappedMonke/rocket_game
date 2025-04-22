using UnityEngine;
using UnityEngine.InputSystem;

public class SpaceshipControls : MonoBehaviour
{
    public InputActionReference rotate;
    public InputActionReference thrust;
    public float rotationForce;
    public float thrustForce;

    private Rigidbody2D _rb;
    private float _rotation;
    private bool _isThrustButtonPressed = false;

    private void OnEnable() {
        rotate.action.Enable();
        thrust.action.Enable();
        thrust.action.started += OnThrustButtonPressed;
        thrust.action.canceled += OnThrustButtonReleased;

        _rb = GetComponent<Rigidbody2D>();
        _rotation = 0.0f;
        _rb.gravityScale = 0.2f;
    }

    private void Update() {
        _rotation = rotate.action.ReadValue<float>();

        _rb.AddTorque(-_rotation * Time.deltaTime * rotationForce);

        if(_rotation == 0.0f) {
            _rb.angularVelocity = 0.0f;
        }

    }

    private void FixedUpdate() {
        if(_isThrustButtonPressed) {
            _rb.AddForce(transform.up * thrustForce);
        }
    }

    private void OnThrustButtonPressed(InputAction.CallbackContext context) {
        _isThrustButtonPressed = true;
    }

    private void OnThrustButtonReleased(InputAction.CallbackContext context) {
        _isThrustButtonPressed = false;
    }
}
