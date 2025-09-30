using UnityEditor.UI;
using UnityEngine;

public class Health : MonoBehaviour
{
    public Transform RespawnPoint;
    public int maxHealth = 10;
    [SerializeField] private int currentHealth;

    private bool isInvincible = false;
    public float invincibilityDuration = 1f;
    private float invincibilityTimer;

    private void Start()
    {
        currentHealth = maxHealth;
    }
    public bool isInvincibleStatus()
    {
        return isInvincible;
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }

        StartCoroutine(BecomeTemporarilyInvincible());
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
        if (gameObject.CompareTag("Player"))
        {
            //UnityEngine.SceneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex); // On death, reset scene
            //If the player dies, set the player gameObject to the RespawnPoint location
            transform.position = RespawnPoint.position;
            return;
        }
    }

    private System.Collections.IEnumerator BecomeTemporarilyInvincible()
    {
        isInvincible = true;
        yield return new WaitForSeconds(invincibilityDuration);
        isInvincible = false;
        UnityEngine.Debug.Log("Player is no longer invincible.");
    }
}
