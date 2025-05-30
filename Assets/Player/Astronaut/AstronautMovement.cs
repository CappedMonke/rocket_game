using UnityEngine;

public class AstronautMovement : MonoBehaviour
{
    public AstronautMovementData data;
    private Rigidbody2D rb;

    private Vector2 moveInput;
    private bool isJumping;
    private bool isJumpCut;
    private float lastOnGroundTime;
    private float lastPressedJumpTime;

    [SerializeField] private Transform groundCheck;
    [SerializeField] private Vector2 groundCheckSize = new Vector2(0.5f, 0.1f);
    [SerializeField] private LayerMask groundLayer;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        SetGravityScale(data.gravityScale);
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public void OnJumpInput()
    {
        lastPressedJumpTime = data.jumpInputBufferTime;
    }

    public void OnJumpUpInput()
    {
        if (CanJumpCut())
            isJumpCut = true;
    }

    private void Update()
    {
        lastOnGroundTime -= Time.deltaTime;
        lastPressedJumpTime -= Time.deltaTime;

        if (IsGrounded())
            lastOnGroundTime = data.coyoteTime;

        if (isJumping && rb.linearVelocity.y < 0)
            isJumping = false;
    }

    private void FixedUpdate()
    {
        Run();
        if (CanJump() && lastPressedJumpTime > 0)
        {
            Jump();
        }
        ApplyGravity();
    }

    private void Run()
    {
        float targetSpeed = moveInput.x * data.runMaxSpeed;
        float accelRate = (lastOnGroundTime > 0) ? data.runAccelAmount : data.runAccelAmount * data.accelInAir;

        float speedDiff = targetSpeed - rb.linearVelocity.x;
        float movement = speedDiff * accelRate;

        rb.AddForce(movement * Vector2.right, ForceMode2D.Force);
    }

    private void Jump()
    {
        lastPressedJumpTime = 0;
        lastOnGroundTime = 0;

        float force = data.jumpForce;
        if (rb.linearVelocity.y < 0)
            force -= rb.linearVelocity.y;

        rb.AddForce(Vector2.up * force, ForceMode2D.Impulse);
        isJumping = true;
        isJumpCut = false;
    }

    private void ApplyGravity()
    {
        if (isJumpCut)
        {
            SetGravityScale(data.gravityScale * data.jumpCutGravityMult);
        }
        else if (rb.linearVelocity.y < 0)
        {
            SetGravityScale(data.gravityScale * data.fallGravityMult);
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -data.maxFallSpeed));
        }
        else
        {
            SetGravityScale(data.gravityScale);
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapBox(groundCheck.position, groundCheckSize, 0, groundLayer);
    }

    private bool CanJump()
    {
        return lastOnGroundTime > 0 && !isJumping;
    }

    private bool CanJumpCut()
    {
        return isJumping && rb.linearVelocity.y > 0;
    }

    private void SetGravityScale(float scale)
    {
        rb.gravityScale = scale;
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }
    }
}
