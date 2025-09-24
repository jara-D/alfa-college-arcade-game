using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    [Header("Movement")]
    public float moveSpeed = 5f;
    private float horizontalMovement;

    [Header("Jump")]
    public float jumpForce = 10f;
    public float coyoteTime = 0.30f; // time after leaving ground that jump is still allowed

    [Header("Dash")]
    public float dashForce = 8f;
    public float DashDelay = 0.1f;
    private bool DashReady = true;


    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckRadius = new Vector2(0.5f, 0.1f);
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 12f;
    public float fallSpeedMultiplier = 1f;

    [Header("Climbing")]
    public float climbSpeed = 5f;
    private float verticalMovement;
    public Transform wallCheckRight;
    public Transform wallCheckLeft;
    public Vector2 wallCheckRadius = new Vector2(0.2f, 1f);
    public LayerMask climbableLayer;
    private bool isClimbing = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);

        Gravity();
        if (!IsClimbable())
        {
            rb.gravityScale = baseGravity;
        }
        if (IsGrounded())
        {
            DashReady = true;
        }
    }

    private void Gravity()
    {
        if (isClimbing) return;
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        Vector2 movementInput = context.ReadValue<Vector2>();
        horizontalMovement = movementInput.x;
        verticalMovement = movementInput.y;
    }

    public void Jump(InputAction.CallbackContext context)
    {

        if (!IsGrounded() && !isClimbing) return;
        if (context.performed)
        {
            isClimbing = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

    }

    public void Climbing(InputAction.CallbackContext context)
    {
        if (!IsClimbable())
        {
            isClimbing = false;
            rb.gravityScale = baseGravity;
            return;
        }

        if (context.performed)
        {
            isClimbing = true;
            rb.gravityScale = 0f; // turns off gravity while climbing so the player doesn't fall
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalMovement * climbSpeed);
        }
        else if (context.canceled)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);
        }
        else if (IsGrounded())
        {
            isClimbing = false;
            rb.gravityScale = baseGravity;
        }
    }

    public void Dash(InputAction.CallbackContext context)
    {
        if (!IsGrounded() && DashReady)
        {
            DashReady = false;
            horizontalMovement = horizontalMovement * dashForce;
            verticalMovement = verticalMovement * dashForce;
            // stop after a short delay
            Invoke("ResetDash", DashDelay);
        }
    }
    private void ResetDash()
    {
        horizontalMovement = math.clamp(horizontalMovement, -1, 1);
        verticalMovement = math.clamp(verticalMovement, -1, 1);
    }
    private bool IsClimbable()
    {
        if (Physics2D.OverlapBox(wallCheckRight.position, wallCheckRadius, 0f, climbableLayer) ||
            Physics2D.OverlapBox(wallCheckLeft.position, wallCheckRadius, 0f, climbableLayer))
        {
            return true;
        }
        return false;
    }

    private bool IsGrounded()
    {
        float lastGrounded = 0f;
        if (Physics2D.OverlapBox(groundCheck.position, groundCheckRadius, 0f, groundLayer))
        {
            lastGrounded = Time.time;
        }

        return Time.time - lastGrounded < coyoteTime;
    }

    // Visualize ground and wall check areas in the editor when the player is selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(groundCheck.position, groundCheckRadius);
        Gizmos.DrawCube(wallCheckRight.position, wallCheckRadius);
        Gizmos.DrawCube(wallCheckLeft.position, wallCheckRadius);
    }
}
