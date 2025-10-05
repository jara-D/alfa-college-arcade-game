using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private bool InputEnabled = true;

    [Header("Movement")]
    public float moveSpeed = 5f;
    private float horizontalMovement;

    [Header("Jump")]
    public float jumpForce = 5f;
    public float coyoteTime = 0.30f; // time after leaving ground that jump is still allowed
    private bool isJumping = false;
    private float jumpTime = 0f;
    private float maxJumpTime = 0.4f;

    [Header("Dash")]
    public float dashForce = 8f;
    public float DashDelay = 0.1f;
    private bool DashReady = true;
    private bool isDashing = false;


    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckRadius = new Vector2(0.5f, 0.1f);
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float baseGravity;
    public float maxFallSpeed;
    public float fallSpeedMultiplier;

    [Header("Climbing")]
    public float climbSpeed = 5f;
    private float verticalMovement;
    public Transform wallCheckRight;
    public Transform wallCheckLeft;
    public Vector2 wallCheckRadius = new Vector2(0.2f, 1f);
    public LayerMask climbableLayer;
    private bool isClimbing = false;
    private bool ClimbingEnabled = true;

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
            isJumping = false;
        }

        Movement();
    }
    private void Movement()
    {
        if (!InputEnabled) return;
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");
        Jump();
        Climbing();
        Dash();

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

    public void Jump()
    {
        cutJumpShort();
        if (!Input.GetButtonDown("Jump")) return;
        if (!IsGrounded() && !isClimbing) return;


        Debug.Log("Jump performed");
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isJumping = true;
    }
    public void cutJumpShort()
    {
        if (Input.GetButtonUp("Jump") && rb.linearVelocity.y > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        }
    }

    public void Climbing()
    {
        if (!IsClimbable() || !ClimbingEnabled)
        {
            StopClimbing();
            return;
        }
        if (verticalMovement != 0)
        {
            isClimbing = true;
            rb.gravityScale = 0f; // turns off gravity while climbing so the player doesn't fall
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, verticalMovement * climbSpeed);
        }

        if (verticalMovement == 0 && isClimbing)
        {
            rb.gravityScale = 0f;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0);
        }

        if (Input.GetButtonDown("Jump") && isClimbing)
        {
            Debug.Log("Jumping off wall");
            StopClimbing();
            StartCoroutine(ClimbCooldown(0.2f));
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce * 1.2f);
        }

        if (IsGrounded())
        {
            StopClimbing();
        }
    }

    private void StopClimbing()
    {
        isClimbing = false;
        rb.gravityScale = baseGravity;
    }

    IEnumerator ClimbCooldown(float duration)
    {
        ClimbingEnabled = false;
        yield return new WaitForSeconds(duration);
        ClimbingEnabled = true;
    }

    public void Dash()
    {
        if (!IsGrounded() && DashReady && !isDashing )
        {
            DashReady = false;
            // horizontalMovement = horizontalMovement * dashForce;
            // verticalMovement = verticalMovement * dashForce;
            rb.AddForce(new Vector2(dashForce * Mathf.Sign(horizontalMovement), 0), ForceMode2D.Impulse);
            Debug.Log("Dash performed");
            // stop after a short delay
            // Invoke("ResetDash", DashDelay);
            StartCoroutine(DashCoroutine(1f));
        }
    }
    // private void ResetDash()
    // {
    //     horizontalMovement = math.clamp(horizontalMovement, -1, 1);
    //     verticalMovement = math.clamp(verticalMovement, -1, 1);
    // }

    IEnumerator DashCoroutine(float dashDuration)
    {
       if(isDashing)
          yield break; 

       isDashing = true;
       rb.linearVelocity = new Vector2(dashForce, 0);

       yield return new WaitForSeconds(dashDuration);

       rb.linearVelocity = new Vector2(0,0);

       isDashing = false;
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


    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("Player triggered with " + collision.name);
    }

}
