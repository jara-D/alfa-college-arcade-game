using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    [Header("Movement")]
    public float _moveSpeed = 5f;
    public float _horizontalMovement;

    [Header("Jump")]
    public float _jumpForce = 15f;
    public float coyoteTime = 0.15f;


    [Header("Ground Check")]
    public Transform _groundCheck;
    public Vector2 _groundCheckRadius = new Vector2(0.5f, 0.1f);
    public LayerMask _groundLayer;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 12f;
    public float fallSpeedMultiplier = 1f;

    [Header("Climbing")]
    public float _climbSpeed = 3f;
    public float _verticalMovement;
    public Transform _wallCheckRight;
    public Transform _wallCheckLeft;
    public Vector2 _wallCheckRadius = new Vector2(0.1f, 0.5f);
    public LayerMask _climbableLayer;
    private bool isClimbing = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = new Vector2(_horizontalMovement * _moveSpeed, rb.linearVelocity.y);
        
        Gravity();
        if (!IsClimbable())
        {
            rb.gravityScale = baseGravity;
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
        _horizontalMovement = movementInput.x;
        _verticalMovement = movementInput.y;
    }

    public void Jump(InputAction.CallbackContext context)
    {

        if (!IsGrounded() && !isClimbing) return;
        if (context.performed)
        {
            isClimbing = false;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, _jumpForce);
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
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, _verticalMovement * _climbSpeed);
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

    private bool IsClimbable()
    {
        if (Physics2D.OverlapBox(_wallCheckRight.position, _wallCheckRadius, 0f, _climbableLayer) ||
            Physics2D.OverlapBox(_wallCheckLeft.position, _wallCheckRadius, 0f, _climbableLayer))
        {
            return true;
        }
        return false;
    }

    private bool IsGrounded()
    {
        float lastGrounded = 0f;
        if (Physics2D.OverlapBox(_groundCheck.position, _groundCheckRadius, 0f, _groundLayer))
        {
            lastGrounded = Time.time;
        }

        return Time.time - lastGrounded < coyoteTime;
    }

    // Visualize ground and wall check areas in the editor when the player is selected
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawCube(_groundCheck.position, _groundCheckRadius);
        Gizmos.DrawCube(_wallCheckRight.position, _wallCheckRadius);
        Gizmos.DrawCube(_wallCheckLeft.position, _wallCheckRadius);
    }
}
