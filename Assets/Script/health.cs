using UnityEditor.UI;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class Health : MonoBehaviour
{
    public Transform RespawnPoint;
    public int maxHealth = 10;
    [SerializeField] public int currentHealth;

    private bool isInvincible = false;
    public float invincibilityDuration = 1.5f;
    private float invincibilityTimer;
    private Coroutine invincibilityCoroutine;
    private Knockback knockback;
    
    // Flag to track if player is currently dead/respawning
    private bool isDead = false;
    
    // Timeline animation for damage before respawn
    [Header("Timeline Animation")]
    public PlayableDirector timelineDirector;
    public TimelineAsset damageBeforeRespawnTimeline;
    public GameObject transitionImage; // The image GameObject that gets animated
    
    // Animation
    private Animator animator;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
        knockback = GetComponent<Knockback>();
        
        // Debug check for missing knockback component
        if (knockback == null)
        {
            Debug.LogError("Knockback component not found on " + gameObject.name + "! Make sure to add the Knockback script to this GameObject.");
        }
        else
        {
            Debug.Log("Knockback component found on " + gameObject.name);
        }
    }
    public bool isInvincibleStatus()
    {
        return isInvincible;
    }

    public bool IsDeadOrRespawning()
    {
        return isDead;
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        
        // Trigger damage animation
        if (animator != null)
        {
            animator.SetTrigger("takeDamage");
        }

        if (currentHealth <= 0)
        {
            Die();
        }

        // Store the coroutine reference to manage it properly
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
        }
        invincibilityCoroutine = StartCoroutine(BecomeTemporarilyInvincible());
    }

    public void TakeDamage(int damage, Transform damageSource)
    {
        TakeDamage(damage, damageSource, false); // Default to not playing timeline
    }

    public void TakeDamage(int damage, Transform damageSource, bool playTimelineOnRespawn)
    {
        Debug.Log($"TakeDamage called with damage: {damage}, source: {(damageSource != null ? damageSource.name : "null")}, timeline: {playTimelineOnRespawn}");
        
        if (isInvincible) 
        {
            Debug.Log("Player is invincible, damage blocked");
            return;
        }

        // Check if this damage will cause death or if this is a respawn-triggering damage
        bool willCauseDeath = (currentHealth - damage) <= 0;
        bool shouldPlayTimeline = playTimelineOnRespawn || willCauseDeath;

        // Play timeline animation if this damage will result in respawn
        if (shouldPlayTimeline && timelineDirector != null && damageBeforeRespawnTimeline != null)
        {
            StartCoroutine(PlayTimelineWithImageActivation());
        }

        currentHealth -= damage;
        Debug.Log($"Health reduced to: {currentHealth}");
        
        // Trigger damage animation
        if (animator != null)
        {
            animator.SetTrigger("takeDamage");
        }

        if (currentHealth <= 0)
        {
            Die();
        }

        Vector2 hitDirection = CalculateHitDirection(damageSource);
        Debug.Log($"Hit direction calculated: {hitDirection}");
        
        if (knockback != null)
        {
            Debug.Log("Calling knockback...");
            knockback.CallKnockback(hitDirection, Vector2.up, Input.GetAxisRaw("Horizontal"));
        }
        else
        {
            Debug.LogError("Knockback component is null! Cannot apply knockback.");
        }

        // Store the coroutine reference to manage it properly
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
        }
        invincibilityCoroutine = StartCoroutine(BecomeTemporarilyInvincible());
    }

    // Calculates the hit direction from a damage source to the player.
    // Returns a normalized Vector2 indicating the direction the player should be knocked back.
    private Vector2 CalculateHitDirection(Transform damageSource)
    {
        if (damageSource == null)
        {
            // Default knockback direction (left) if no source is provided
            return Vector2.left;
        }

        // Calculate direction from damage source to player
        Vector2 directionToPlayer = (transform.position - damageSource.position).normalized;
        
        // Add a slight upward component to make knockback feel better
        Vector2 hitDirection = new Vector2(directionToPlayer.x, Mathf.Abs(directionToPlayer.y) + 0.5f);
        
        // Normalize the result
        return hitDirection.normalized;
    }

    // Alternative hit direction calculation with custom upward force.
    private Vector2 CalculateHitDirection(Transform damageSource, float upwardForce)
    {
        if (damageSource == null)
        {
            return new Vector2(-1f, upwardForce).normalized;
        }

        // Calculate horizontal direction from damage source to player
        float horizontalDirection = transform.position.x - damageSource.position.x;
        
        // Determine knockback direction (away from damage source)
        Vector2 hitDirection = new Vector2(
            horizontalDirection > 0 ? 1f : -1f, // Push right if source is left, push left if source is right
            upwardForce
        );
        
        return hitDirection.normalized;
    }
    
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
    }
    // On death, run Die()
    private void Die()
    {
        Health playerHealth = gameObject.GetComponent<Health>();
        if (gameObject.CompareTag("Player"))
        {
            // Set the dead flag to prevent other respawn methods
            isDead = true;
            
            //UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex); // On death, reset scene
            //If the player dies, start the respawn coroutine
            StartCoroutine(RespawnCoroutine(playerHealth));
            return;
        }
    }

    private System.Collections.IEnumerator RespawnCoroutine(Health playerHealth)
    {
        // Get player controller to disable input
        PlayerController playerController = gameObject.GetComponent<PlayerController>();
        if (playerController != null)
        {
            // Disable player input immediately
            playerController.SetInputEnabled(false);
        }
        
        // Stop any ongoing knockback when respawning
        if (knockback != null)
        {
            knockback.StopKnockback();
        }
        
        // Reset player velocity immediately
        Rigidbody2D playerRb = gameObject.GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.linearVelocity = Vector2.zero;
        }
        
        // Wait for 0.3 seconds before respawning
        yield return new WaitForSeconds(0.3f);
        
        // Set the player gameObject to the RespawnPoint location
        transform.position = RespawnPoint.position;
        playerHealth.Heal(playerHealth.maxHealth);
        
        // Reset player animations when respawning after death
        if (playerController != null)
        {
            playerController.ResetAnimations();
        }
        
        // Wait additional 0.2 seconds (total 0.5 seconds of disabled input)
        yield return new WaitForSeconds(0.2f);
        
        // Re-enable player input
        if (playerController != null)
        {
            playerController.SetInputEnabled(true);
        }
        
        // Reset the dead flag
        isDead = false;
        
        Debug.Log("Player respawned at checkpoint");
    }

    private System.Collections.IEnumerator BecomeTemporarilyInvincible()
    {
        isInvincible = true;
        UnityEngine.Debug.Log($"Player became invincible for {invincibilityDuration} seconds");
        
        yield return new WaitForSeconds(invincibilityDuration);
        
        isInvincible = false;
        invincibilityCoroutine = null; // Clear the reference when done
        UnityEngine.Debug.Log("Player is no longer invincible.");
    }

    private System.Collections.IEnumerator PlayTimelineWithImageActivation()
    {
        Debug.Log("Playing damage before respawn timeline animation");
        
        // Activate the transition image
        if (transitionImage != null)
        {
            transitionImage.SetActive(true);
            Debug.Log("Transition image activated");
        }
        
        // Play the timeline
        timelineDirector.Play(damageBeforeRespawnTimeline);
        
        // Wait for the timeline to finish
        yield return new WaitForSeconds((float)damageBeforeRespawnTimeline.duration);
        
        // Deactivate the transition image
        if (transitionImage != null)
        {
            transitionImage.SetActive(false);
            Debug.Log("Transition image deactivated");
        }
    }
}
