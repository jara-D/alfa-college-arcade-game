using System.Threading;
using UnityEngine;

public class hazardDamage : MonoBehaviour
{
    [field: SerializeField] public int damageAmount { get; private set; }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        Health health = collision.gameObject.GetComponent<Health>();
        PlayerController playerController = collision.gameObject.GetComponent<PlayerController>();

        Debug.Log("Collided with " + collision.gameObject.name);
        //If the object has a health component, apply damage
        if (health != null && health.isInvincibleStatus() == false)
        {
            health.TakeDamage(damageAmount);

            // If the object is the player and has health over 0, respawn at last grounded
            if (collision.gameObject.CompareTag("Player") && health.currentHealth > 0 && playerController != null)
            {
                playerController.LastGroundedRespawn();
            }
        }
    }
}
