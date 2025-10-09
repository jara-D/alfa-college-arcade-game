using UnityEditor.UI;
using UnityEngine;

public class Health : MonoBehaviour
{
    public Transform RespawnPoint;
    public int maxHealth = 10;
    [SerializeField] private int currentHealth;

    private bool isInvincible = false;
    public float invincibilityDuration = 1.5f;
    private float invincibilityTimer;
    private Coroutine invincibilityCoroutine;
    
    // Animation
    private Animator animator;

    private void Start()
    {
        currentHealth = maxHealth;
        animator = GetComponent<Animator>();
    }
    public bool isInvincibleStatus()
    {
        return isInvincible;
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
        UnityEngine.Debug.Log($"Player became invincible for {invincibilityDuration} seconds");
        
        yield return new WaitForSeconds(invincibilityDuration);
        
        isInvincible = false;
        invincibilityCoroutine = null; // Clear the reference when done
        UnityEngine.Debug.Log("Player is no longer invincible.");
    }
}
