using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    private Rigidbody2D rb;
    private CapsuleCollider2D col;
    private bool InputEnabled = true;

    private Animator animator;
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
    public float dashingTime = 1f;
    public float dashAnimationMinDuration = 1f; // Minimum time isDashing animation bool should stay true (0.2 seconds)
    public float dashingCooledown = 1f;
    public LayerMask dashStopLayer;
    private TrailRenderer tr;
    private float dashAnimationEndTime = 0f; // Time when dash animation should end


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
    private float climbableObjectXPosition; // Store the X position of the climbable object
    private bool isXPositionLocked = false; // Track if X position is locked during climbing

    [Header("Facing Direction")]
    private int facingDirection = 0; // 0 = right, 1 = left
    private float lastHorizontalInput = 0f; // Track last input to determine facing

    [Header("Dialogue")]
    private DialogueManager dialogueManager;
    public Transform wallCheckRight;
    public Transform wallCheckLeft;
    public Vector2 wallCheckRadius = new Vector2(0.2f, 1f);
    public LayerMask climbableLayer;
    private bool isClimbing = false;
    private bool ClimbingEnabled = true;

    [Header("Knockback")]
    private Knockback knockback;

    [Header("Health")]
    private Health health;

    public Rigidbody2D movingTileRigidbody;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        tr = GetComponent<TrailRenderer>();
        animator = GetComponent<Animator>();
        knockback = GetComponent<Knockback>();
        health = GetComponent<Health>();

        // Find DialogueManager in scene
        dialogueManager = FindFirstObjectByType<DialogueManager>();

        // Initialize facing direction (default to right)
        facingDirection = 0;
        if (animator != null)
        {
            animator.SetInteger("facingDirection", facingDirection);
        }

        // Check for missing components
        if (knockback == null)
        {
            // Knockback component not found
        }

        if (health == null)
        {
            // Health component not found
        }

        if (dialogueManager == null)
        {
            // DialogueManager not found in scene
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Reset movement if input is disabled
        if (!InputEnabled)
        {
            horizontalMovement = 0;
            verticalMovement = 0;
        }

        realGrounded = Physics2D.OverlapBox(groundCheck.position, groundCheckRadius, 0f, groundLayer);

        if (isDashing) return;

        // If currently being knocked back, do not override velocity or process movement
        if (knockback != null && knockback.IsBeingKnockedBack)
        {
            return;
        }

        // While climbing and X is locked, force horizontal velocity to zero and preserve Y
        if (isXPositionLocked && isClimbing)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }

        // Remove velocity assignment from Update. Final horizontal velocity is set in FixedUpdate.
        Movement();

        // Always call UpdateAnimationStates, but let it handle its own protection logic
        UpdateAnimationStates();

        Gravity();

        if (IsGrounded())
        {
            isJumping = false;
        }
    }
    void FixedUpdate()
    {
        if (isDashing) return;

        // Don't override velocity if being knocked back
        if (knockback != null && knockback.IsBeingKnockedBack) return;

        float platformVelX = 0f;
        if (movingTileRigidbody != null)
        {
            platformVelX = movingTileRigidbody.linearVelocity.x;
        }

        if (isXPositionLocked && isClimbing)
        {
            // During climbing, don't apply horizontal movement - position is locked
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
        else
        {
            // Normal horizontal movement when not climbing
            rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed + platformVelX, rb.linearVelocity.y);
        }
    }

    private void Movement()
    {
        if (!InputEnabled) return;
        horizontalMovement = Input.GetAxisRaw("Horizontal");
        verticalMovement = Input.GetAxisRaw("Vertical");

        // Update facing direction based on horizontal input
        UpdateFacingDirection();

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

    private void UpdateFacingDirection()
    {
        // Update facing direction if there's any horizontal input (even small amounts)
        if (Mathf.Abs(horizontalMovement) > 0.01f)
        {
            int newFacingDirection = horizontalMovement > 0 ? 0 : 1; // 0 = right, 1 = left

            // Update immediately when direction changes
            if (newFacingDirection != facingDirection)
            {
                facingDirection = newFacingDirection;

                // Update animator parameter immediately
                if (animator != null)
                {
                    animator.SetInteger("facingDirection", facingDirection);
                }
            }
        }
        // If no input, maintain the current facing direction (don't change it)

        lastHorizontalInput = horizontalMovement;
    }

    /// Get the current facing direction
    public int GetFacingDirection()
    {
        return facingDirection;
    }

    /// Get facing direction as a multiplier
    /// 0 = right, 1 = left
    public float GetFacingDirectionMultiplier()
    {
        return facingDirection == 0 ? 1f : -1f;
    }

    private void UpdateAnimationStates()
    {
        if (animator == null) return;

        // Check if player is taking damage - this takes priority over other animations
        if (health != null && health.isInvincibleStatus())
        {
            // Don't override damage animation - let Health component handle it
            return;
        }

        // Check if player is sitting - this takes priority over other animations
        bool isSitting = animator.GetBool("isSitting");
        if (isSitting)
        {
            // Don't override sitting animation - let CheckpointScript handle it
            return;
        }

        bool startDashAnimation = isDashing || Time.time < dashAnimationEndTime;

        if (startDashAnimation)
        {
            animator.SetBool("isWalking", false);
            animator.SetBool("isJumping", false);
            animator.SetBool("isFalling", false);
            animator.SetBool("isClimbing", false);
            animator.SetBool("isDashing", true);
            return; // Don't process any other animations
        }

        animator.SetBool("isWalking", false);
        animator.SetBool("isJumping", false);
        animator.SetBool("isFalling", false);
        animator.SetBool("isClimbing", false);
        animator.SetBool("isDashing", false);

        bool isAirborne = !IsGrounded() && !isClimbing;

        if (isAirborne)
        {
            // Player is in the air
            if (rb.linearVelocity.y > 0.05f)
            {
                animator.SetBool("isJumping", true);
            }
            else if (rb.linearVelocity.y < -0.05f)
            {
                animator.SetBool("isFalling", true);
            }
            else
            {

                if (!animator.GetBool("isJumping") && !animator.GetBool("isFalling"))
                {
                    animator.SetBool("isJumping", true);
                }
            }
        }
        else if (isClimbing)
        {
            animator.SetBool("isClimbing", true);
        }
        else if (Mathf.Abs(horizontalMovement) > 0.1f && IsGrounded() && !isClimbing)
        {
            animator.SetBool("isWalking", true);
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

        // Update facing direction immediately when input changes
        if (InputEnabled && Mathf.Abs(horizontalMovement) > 0.01f)
        {
            int newFacingDirection = horizontalMovement > 0 ? 0 : 1; // 0 = right, 1 = left

            if (newFacingDirection != facingDirection)
            {
                facingDirection = newFacingDirection;

                if (animator != null)
                {
                    animator.SetInteger("facingDirection", facingDirection);
                }
            }
        }
    }

    public void Jump()
    {
        cutJumpShort();
        if (!Input.GetButtonDown("Jump")) return;

        // Prevent jumping if dialogue is active OR if player is in dialogue interaction range
        if (dialogueManager != null && dialogueManager.IsDialogueActive) return;
        if (DialogueStarter.IsPlayerInAnyDialogueRange()) return;

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
            isXPositionLocked = true;
            rb.gravityScale = 0f; // turns off gravity while climbing so the player doesn't fall

            // Lock X position to the climbable object and only allow Y movement
            Vector3 lockedPosition = new Vector3(climbableObjectXPosition, transform.position.y, transform.position.z);
            transform.position = lockedPosition;
            rb.linearVelocity = new Vector2(0f, verticalMovement * climbSpeed); // Force X velocity to 0
        }

        if (verticalMovement == 0 && isClimbing)
        {
            rb.gravityScale = 0f;
            // Keep X position locked and stop Y movement
            Vector3 lockedPosition = new Vector3(climbableObjectXPosition, transform.position.y, transform.position.z);
            transform.position = lockedPosition;
            rb.linearVelocity = new Vector2(0f, 0f); // Stop all movement
        }

        if (Input.GetButtonDown("Jump") && isClimbing)
        {
            // Prevent climbing jump if dialogue is active OR if player is in dialogue interaction range
            if (dialogueManager != null && dialogueManager.IsDialogueActive) return;
            if (DialogueStarter.IsPlayerInAnyDialogueRange()) return;

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
        isXPositionLocked = false; // Unlock X position
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
        // Set the minimum time the dash animation should stay active using the configurable duration
        dashAnimationEndTime = Time.time + dashAnimationMinDuration;
        animator.SetBool("isDashing", true);
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
        Collider2D rightWall = Physics2D.OverlapBox(wallCheckRight.position, wallCheckRadius, 0f, climbableLayer);
        Collider2D leftWall = Physics2D.OverlapBox(wallCheckLeft.position, wallCheckRadius, 0f, climbableLayer);

        if (rightWall != null)
        {
            climbableObjectXPosition = GetClimbableTileXPosition(rightWall, wallCheckRight.position);
            return true;
        }
        else if (leftWall != null)
        {
            climbableObjectXPosition = GetClimbableTileXPosition(leftWall, wallCheckLeft.position);
            return true;
        }

        return false;
    }

    private float GetClimbableTileXPosition(Collider2D climbableCollider, Vector3 checkPosition)
    {
        // Check if it's a tilemap
        TilemapCollider2D tilemapCollider = climbableCollider.GetComponent<TilemapCollider2D>();
        if (tilemapCollider != null)
        {
            // Get the tilemap and grid components
            Tilemap tilemap = climbableCollider.GetComponent<Tilemap>();
            Grid grid = tilemap.layoutGrid;

            if (tilemap != null && grid != null)
            {
                // Convert world position to cell position
                Vector3Int cellPosition = grid.WorldToCell(checkPosition);

                // Get the world position of the center of this specific tile
                Vector3 tileWorldPos = grid.CellToWorld(cellPosition);

                // Add half cell size to get the center of the tile
                tileWorldPos.x += grid.cellSize.x * 0.5f;

                return tileWorldPos.x;
            }
        }

        // Fallback for regular colliders (not tilemaps)
        return climbableCollider.transform.position.x;
    }

    // check ground methods

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

    public void ResetAnimations()
    {
        if (animator == null) return;

        animator.SetBool("isWalking", false);
        animator.SetBool("isJumping", false);
        animator.SetBool("isFalling", false);
        animator.SetBool("isClimbing", false);
        animator.SetBool("isDashing", false);
        animator.SetBool("isSitting", false);

        animator.ResetTrigger("takeDamage");
        animator.ResetTrigger("sittingDown");

        dashAnimationEndTime = 0f;

    }

    // Method to control player input
    public void SetInputEnabled(bool enabled)
    {
        InputEnabled = enabled;
    }

    public void LastGroundedRespawn()
    {
        StartCoroutine(LastGroundedRespawnCoroutine());
    }

    private IEnumerator LastGroundedRespawnCoroutine()
    {
        SetInputEnabled(false);

        yield return new WaitForSeconds(0.3f);

        if (knockback != null)
        {
            knockback.StopKnockback();
        }
        rb.linearVelocity = Vector2.zero;

        // Move to last grounded position
        transform.position = new Vector3(lastGroundedPosition.x, lastGroundedPosition.y, transform.position.z);

        // Reset animations and states
        ResetAnimations();
        isJumping = false;
        isDashing = false;
        isClimbing = false;

        yield return new WaitForSeconds(0.2f);

        // Re-enable player input
        SetInputEnabled(true);

    }

}