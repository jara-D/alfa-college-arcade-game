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
    private bool canDash = true;
    private bool isDashing = false;
    public float dashingPower = 20f;
    public float dashingTime = 0.2f;
    public float dashingCooledown = 1f;
    public LayerMask dashStopLayer;
    private TrailRenderer tr;

    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckRadius = new Vector2(0.5f, 0.1f);
    public LayerMask groundLayer;

    public Vector3 lastGroundedPosition;    //tracks the vector of the last location the player was grounded
    public bool realGrounded;               //Grounded bool without coyote time

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
        tr = GetComponent<TrailRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
        realGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckRadius, 0f, groundLayer);

        if (isDashing) return;


        Gravity();

        if (IsGrounded())
        {
            isJumping = false;
        }

        Movement();
    }
    void FixedUpdate()
    {
        if (isDashing) return;
        rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
    }

    private void Movement()
    {
        if (!InputEnabled) return;
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");
        Jump();
        Climbing();

        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash && horizontalMovement != 0)
        {
            StartCoroutine(Dash());
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
        //Update last grounded position if the player is really grounded
        if (realGrounded)
        {
            lastGroundedPosition = transform.position;
        }
        Vector2 movementInput = context.ReadValue<Vector2>();
        horizontalMovement = movementInput.x;
        verticalMovement = movementInput.y;
    }

    public void Jump()
    {
        cutJumpShort();
        if (!Input.GetButtonDown("Jump")) return;
        if (!IsGrounded() && !isClimbing) return;


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

    private IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        tr.emitting = true;
        float dashStartTime = Time.time;
        while (Time.time < dashStartTime + dashingTime)
        {
            rb.linearVelocity = new Vector2(horizontalMovement * dashingPower, 0f);

            // Cast a ray in the dash direction
            Vector2 dashDirection = new Vector2(horizontalMovement, -0.364f).normalized; // -0.364 ≈ tan(20°)
            RaycastHit2D hit = Physics2D.Raycast(transform.position, dashDirection, 1.5f, dashStopLayer);

            if (hit.collider != null)
            {
                float angle = Vector2.Angle(hit.normal, Vector2.up);
                // Stop dash if surface is slanted (not flat or vertical)
                if (angle > 10f && angle < 80f)
                {
                    break;
                }
            }
            yield return null;
        }

        tr.emitting = false;
        rb.gravityScale = originalGravity;
        isDashing = false;
        yield return new WaitForSeconds(dashingCooledown);
        canDash = true;
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


    /*
     * check ground methods
     */
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

    // Respawn at the last grounded position
    public void LastGroundedRespawn()
    {
        transform.position = new Vector3(lastGroundedPosition.x, lastGroundedPosition.y, transform.position.z);

    }

}
