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
            // Get the Health component
            Health playerHealth = gameObject.GetComponent<Health>();

            if (playerHealth != null)
            {
                // Set this checkpoint as new respawn point
                playerHealth.RespawnPoint = this.transform;
                Debug.Log("CHeckpoint reached, Respawn updated");
            }
        }
    }
}