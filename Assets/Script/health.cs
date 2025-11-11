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
        
        // Check for missing knockback component
        if (knockback == null)
        {
            // Knockback component not found
        }
        else
        {
            // Knockback component found
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
        
        // Clamp health to not go below 0
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        
        // Set invincible immediately to prevent animation interference
        isInvincible = true;
        
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
        
        if (isInvincible) 
        {
            return;
        }

        // Check if this damage will cause death or if this is a respawn-triggering damage
        bool willCauseDeath = (currentHealth - damage) <= 0;
        bool shouldPlayTimeline = playTimelineOnRespawn || willCauseDeath;

        // Play timeline animation if this damage will result in respawn
        if (shouldPlayTimeline && timelineDirector != null && damageBeforeRespawnTimeline != null)
        {
            StartCoroutine(PlayDamageTimeline());
        }

        currentHealth -= damage;
        
        // Clamp health to not go below 0
        if (currentHealth < 0)
        {
            currentHealth = 0;
        }
        
        
        // Set invincible immediately to prevent animation interference
        isInvincible = true;
        
        // Update health bar immediately
        HealthBar healthBar = FindFirstObjectByType<HealthBar>();
        if (healthBar != null)
        {
            healthBar.ForceUpdate();
        }
        
        // Trigger screen shake when player takes damage
        if (ScreenShake.Instance != null)
        {
            ScreenShake.Instance.Shake();
        }
        
        // Set invincible immediately to prevent animation interference
        isInvincible = true;
        
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
        
        if (knockback != null)
        {
            knockback.CallKnockback(hitDirection, Vector2.up, Input.GetAxisRaw("Horizontal"));
        }
        else
        {
            // Knockback component is null
        }

        // Store the coroutine reference to manage it properly
        if (invincibilityCoroutine != null)
        {
            StopCoroutine(invincibilityCoroutine);
        }
        invincibilityCoroutine = StartCoroutine(BecomeTemporarilyInvincible());
    }

    private Vector2 CalculateHitDirection(Transform damageSource)
    {
        if (damageSource == null)
        {
            return Vector2.left;
        }

        // Calculate direction from damage source to player
        Vector2 directionToPlayer = (transform.position - damageSource.position).normalized;

        float minUpwardForce = 0.6f;
        Vector2 hitDirection = new Vector2(directionToPlayer.x, Mathf.Max(Mathf.Abs(directionToPlayer.y), minUpwardForce));
        
        return hitDirection.normalized;
    }
    
    public void Heal(int amount)
    {
        currentHealth += amount;
        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }
        
        // Update health bar immediately after healing
        HealthBar healthBar = FindFirstObjectByType<HealthBar>();
        if (healthBar != null)
        {
            healthBar.ForceUpdate();
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
        
    }

    private System.Collections.IEnumerator BecomeTemporarilyInvincible()
    {
        // isInvincible is already set to true before this coroutine starts
        
        yield return new WaitForSeconds(invincibilityDuration);
        
        isInvincible = false;
        invincibilityCoroutine = null; // Clear the reference when done
    }

    private System.Collections.IEnumerator PlayDamageTimeline()
    {
        
        // Activate the transition image
        if (transitionImage != null)
        {
            transitionImage.SetActive(true);
        }
        
        // Play the timeline
        timelineDirector.Play(damageBeforeRespawnTimeline);
        
        // Wait for the timeline to finish
        yield return new WaitForSeconds((float)damageBeforeRespawnTimeline.duration);
        
        // Deactivate the transition image
        if (transitionImage != null)
        {
            transitionImage.SetActive(false);
        }
    }
}
