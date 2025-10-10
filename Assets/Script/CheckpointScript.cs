using UnityEngine;


public class CheckpointScript : MonoBehaviour
{


    void Start()
    {

    }

    private void OnTriggerEnter2D(Collider2D gameObject)
    {
        if (gameObject.CompareTag("Player"))
        {
            // Get the Health components
            Health playerHealth = gameObject.GetComponent<Health>();
            Health currentHealth = gameObject.GetComponent<Health>();
            Health maxHealth = gameObject.GetComponent<Health>();

            if (playerHealth != null)
            {
                // Set this checkpoint as new respawn point
                playerHealth.RespawnPoint = this.transform;
                playerHealth.currentHealth = playerHealth.maxHealth;
                Debug.Log("Checkpoint reached, Respawn updated");
            }
        }
    }
}