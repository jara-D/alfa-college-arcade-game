using System.Collections;
using System.Threading;
using UnityEngine;

public class hazardDamage : MonoBehaviour
{
    [field: SerializeField] public int damageAmount { get; private set; }

    private void OnTriggerEnter2D(Collider2D trigger)
    {
        Debug.Log("Hazard collision with " + trigger.gameObject.name);
        Health health = trigger.gameObject.GetComponent<Health>();
        PlayerController playerController = trigger.gameObject.GetComponent<PlayerController>();

        
        if (health != null)
        {
            Debug.Log("Health component found on " + trigger.gameObject.name);
        //If the object has a health component, apply damage
            if (health.isInvincibleStatus() == false)
            {
                Debug.Log($"Calling TakeDamage({damageAmount}, {this.transform.name}, true) - with timeline");
                health.TakeDamage(damageAmount, this.transform, true); // true = play timeline animation
                
                // Start coroutine to wait for knockback to finish before respawning
                StartCoroutine(WaitForKnockbackThenRespawn(playerController, trigger.gameObject));
            }
            else
            {
                Debug.Log("Player is invincible, hazard damage blocked");
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
            Debug.Log("Player died to hazard, skipping last grounded respawn");
            yield break; // Exit the coroutine without calling LastGroundedRespawn
        }
        
        // Now call the respawn after knockback is finished (only if player didn't die)
        if (playerController != null)
        {
            playerController.LastGroundedRespawn();
        }
    }
}
