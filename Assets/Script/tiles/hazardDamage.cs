using System.Collections;
using System.Threading;
using UnityEngine;

public class hazardDamage : MonoBehaviour
{
    [field: SerializeField] public int damageAmount { get; private set; }
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject damageEffectPrefab;
    [SerializeField] private float effectDuration = 0.2f;
    [SerializeField] private float yOffset = 0.5f;
    [SerializeField] private float xOffset = 0.3f;

    private void OnTriggerEnter2D(Collider2D trigger)
    {
        Health health = trigger.gameObject.GetComponent<Health>();
        PlayerController playerController = trigger.gameObject.GetComponent<PlayerController>();

        
        if (health != null)
        {
        //If the object has a health component, apply damage
            if (health.isInvincibleStatus() == false)
            {
                
                // Calculate collision position (between player and hazard)
                Vector3 collisionPosition = Vector3.Lerp(trigger.transform.position, this.transform.position, 0.5f);
                
                // Get player's facing direction from PlayerController
                float facingDirection = 1f; // Default to right
                if (playerController != null)
                {
                    // Access the horizontalMovement to determine facing direction
                    // If player is moving left (negative), face left. If moving right (positive) or not moving, face right
                    Rigidbody2D playerRb = trigger.gameObject.GetComponent<Rigidbody2D>();
                    if (playerRb != null)
                    {
                        facingDirection = playerRb.linearVelocity.x < 0 ? -1f : 1f;
                    }
                }
                
                // Instantiate damage effect at collision position with facing direction
                if (damageEffectPrefab != null)
                {
                    StartCoroutine(SpawnDamageEffect(collisionPosition, facingDirection));
                }
                
                health.TakeDamage(damageAmount, this.transform, true); // true = play timeline animation
                
                // Start coroutine to wait for knockback to finish before respawning
                StartCoroutine(WaitForKnockbackThenRespawn(playerController, trigger.gameObject));
            }
            else
            {
                // Player is invincible
            }
        }
    }

    private IEnumerator WaitForKnockbackThenRespawn(PlayerController playerController, GameObject player)
    {
        // Get the knockback component to monitor its state
        Knockback knockback = player.GetComponent<Knockback>();
        Health health = player.GetComponent<Health>();
        
        if (knockback != null)
        {
            // Wait while the player is being knocked back
            while (knockback.IsBeingKnockedBack)
            {
                yield return new WaitForFixedUpdate();
            }
            
            // Add a small buffer to ensure knockback has fully completed
            yield return new WaitForSeconds(0.1f);
        }
        
        // Check if the player died during the damage - if so, don't respawn to last grounded
        if (health != null && health.IsDeadOrRespawning())
        {
            yield break; // Exit the coroutine without calling LastGroundedRespawn
        }
        
        // Now call the respawn after knockback is finished (only if player didn't die)
        if (playerController != null)
        {
            playerController.LastGroundedRespawn();
        }
    }
    
    private IEnumerator SpawnDamageEffect(Vector3 position, float facingDirection)
    {
        // Apply Y offset to move the effect slightly upward
        // Apply X offset based on facing direction (negative for left, positive for right)
        Vector3 effectPosition = position + Vector3.up * yOffset + Vector3.right * (xOffset * facingDirection);
        
        // Instantiate the damage effect prefab at the offset position
        GameObject effectInstance = Instantiate(damageEffectPrefab, effectPosition, Quaternion.identity);
        
        // Wait for the specified duration
        yield return new WaitForSeconds(effectDuration);
        
        // Destroy the effect
        if (effectInstance != null)
        {
            Destroy(effectInstance);
        }
    }
}
