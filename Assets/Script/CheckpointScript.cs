using UnityEngine;


public class CheckpointScript : MonoBehaviour
{
    [Header("Checkpoint Sprites")]
    [SerializeField] private Sprite[] checkpointSprites = new Sprite[3];
    private SpriteRenderer spriteRenderer;
    
    [Header("Interaction Settings")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private float standStillTime = 1f;
    [SerializeField] private float movementThreshold = 0.1f; // How much movement is considered "standing still"
    [SerializeField] private Vector3 playerSittingOffset = Vector3.up; // Offset from checkpoint position where player sits
    
    private Transform playerTransform;
    private Vector3 lastPlayerPosition;
    private float standStillTimer = 0f;
    private bool isPlayerInRange = false;
    
    // Animation functionality
    private Animator playerAnimator;
    private bool isPlayerSitting = false;


    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        if (checkpointSprites.Length > 0 && spriteRenderer != null)
        {
            int randomIndex = Random.Range(0, checkpointSprites.Length);
            spriteRenderer.sprite = checkpointSprites[randomIndex];
        }
    }

    void Update()
    {
        CheckForPlayerInRange();
        
        if (isPlayerInRange && playerTransform != null)
        {
            // If player is sitting, check for movement to stand up
            if (isPlayerSitting)
            {
                CheckForPlayerMovement();
            }
            else
            {
                // Only check for sitting if not already sitting
                CheckStandingStill();
            }
        }
        else
        {
            ResetStandStillTimer();
        }
    }

    private void CheckForPlayerInRange()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        
        if (player != null)
        {
            float distance = Vector3.Distance(transform.position, player.transform.position);
            
            if (distance <= interactionRadius)
            {
                if (!isPlayerInRange)
                {
                    // Player just entered range
                    isPlayerInRange = true;
                    playerTransform = player.transform;
                    lastPlayerPosition = playerTransform.position;
                    standStillTimer = 0f;
                }
            }
            else
            {
                if (isPlayerInRange)
                {
                    // Player left range
                    isPlayerInRange = false;
                    playerTransform = null;
                    ResetStandStillTimer();
                }
            }
        }
    }

    private void CheckStandingStill()
    {
        float movementDistance = Vector3.Distance(playerTransform.position, lastPlayerPosition);
        
        if (movementDistance <= movementThreshold)
        {
            // Player is standing still
            standStillTimer += Time.deltaTime;
            
            if (standStillTimer >= standStillTime)
            {
                ActivateCheckpoint();
            }
        }
        else
        {
            // Player moved too much, reset timer
            standStillTimer = 0f;
            lastPlayerPosition = playerTransform.position;
        }
    }

    private void ActivateCheckpoint()
    {
        // Don't activate if already sitting
        if (isPlayerSitting) return;
        
        Vector3 checkpointTop = transform.position + playerSittingOffset;
        playerTransform.position = checkpointTop;
        
        // Set player velocity to zero when sitting
        Rigidbody2D playerRb = playerTransform.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }
        
        playerAnimator = playerTransform.GetComponent<Animator>();
        
        if (playerAnimator != null)
        {
            playerAnimator.SetTrigger("sittingDown");
            playerAnimator.SetBool("isSitting", true);
        }
        
        // Set sitting state
        isPlayerSitting = true;

        // Set respawn point
        Health playerHealth = playerTransform.GetComponent<Health>();
        if (playerHealth != null)
        {
            playerHealth.RespawnPoint = this.transform;
            playerHealth.Heal(playerHealth.maxHealth); 
        }
        
        // Reset timer to allow reactivation
        ResetStandStillTimer();
    }

    private void ResetStandStillTimer()
    {
        standStillTimer = 0f;
    }
    
    private void CheckForPlayerMovement()
    {
        // Check for horizontal input
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        
        // If player is trying to move, stop sitting
        if (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f)
        {
            StopSitting();
        }
    }
    
    private void StopSitting()
    {
        if (playerAnimator != null)
        {
            // Set sitting state to false
            playerAnimator.SetBool("isSitting", false);
        }
        
        isPlayerSitting = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}