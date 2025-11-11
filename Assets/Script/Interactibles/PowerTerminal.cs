using UnityEngine;
using System.Collections;

public class PowerTerminal : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private PowerTerminalMinigame minigameScript;
    
    [Header("Animation")]
    [SerializeField] private Animator terminalAnimator;
    
    [Header("UI References")]
    [SerializeField] private HealthBar playerHealthBar;
    [SerializeField] private GameObject[] uiElementsToHide;
    
    [Header("Interaction")]
    [SerializeField] private float interactionRadius = 2f;
    [SerializeField] private int terminalID = 0; // 0, 1, or 2 for the three terminals
    private bool playerInside = false;
    private bool isTerminalOn = false;
    private bool interactionEnabled = true;
    
    private void Start()
    {
        // Ensure the minigame panel is initially disabled
        if (minigamePanel != null)
        {
            minigamePanel.SetActive(false);
        }
        
        // Get the animator component if not assigned
        if (terminalAnimator == null)
        {
            terminalAnimator = GetComponent<Animator>();
        }
    }
    
    private void Update()
    {
        // Check for E key press when player is inside trigger area, terminal is not already on, and interaction is enabled
        if (playerInside && Input.GetKeyDown(KeyCode.E) && !isTerminalOn && interactionEnabled)
        {
            OpenMinigamePanel();
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = true;
        }
    }
    
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInside = false;
        }
    }
    
    private void OpenMinigamePanel()
    {
        // Get the minigame script if not assigned
        if (minigameScript == null && minigamePanel != null)
        {
            minigameScript = minigamePanel.GetComponent<PowerTerminalMinigame>();
        }
        
        // Hide the health bar and other UI elements
        HideHealthBar();
        
        // Start the code editing minigame
        if (minigameScript != null)
        {
            minigameScript.StartMinigame(terminalID, this);
        }
        else if (minigamePanel != null)
        {
            // Fallback to just showing the panel
            minigamePanel.SetActive(true);
        }
        
        // Optionally pause the game while in minigame
        // Time.timeScale = 0;
    }
    
    public void CloseMinigamePanel()
    {
        if (minigamePanel != null)
        {
            minigamePanel.SetActive(false);
            
            // Show the health bar and other UI elements again
            ShowHealthBar();
            
            // Resume game if it was paused
            // Time.timeScale = 1;
        }
    }
    
    // Call this method when the minigame is completed successfully
    public void OnMinigameComplete()
    {
        // Close the minigame panel
        CloseMinigamePanel();
        
        // Start the terminal activation sequence
        StartCoroutine(ActivateTerminal());
    }
    
    private IEnumerator ActivateTerminal()
    {
        // Set the turning on trigger
        if (terminalAnimator != null)
        {
            terminalAnimator.SetTrigger("IsTurningOn");
        }
        
        // Wait for 0.2 seconds
        yield return new WaitForSeconds(0.2f);
        
        // Set the IsOn bool to true
        if (terminalAnimator != null)
        {
            terminalAnimator.SetBool("IsOn", true);
        }
        
        // Mark terminal as activated
        isTerminalOn = true;
    }
    
    /// <summary>
    /// Hides the player's health bar and additional UI elements during minigame
    /// </summary>
    private void HideHealthBar()
    {
        if (playerHealthBar == null)
        {
            // Try to find the health bar automatically
            playerHealthBar = FindFirstObjectByType<HealthBar>();
        }
        
        if (playerHealthBar != null)
        {
            playerHealthBar.gameObject.SetActive(false);
        }
        
        // Hide additional UI elements
        if (uiElementsToHide != null)
        {
            foreach (GameObject uiElement in uiElementsToHide)
            {
                if (uiElement != null)
                {
                    uiElement.SetActive(false);
                }
            }
        }
    }
    
    /// <summary>
    /// Shows the player's health bar and additional UI elements when minigame ends
    /// </summary>
    private void ShowHealthBar()
    {
        if (playerHealthBar != null)
        {
            playerHealthBar.gameObject.SetActive(true);
        }
        
        // Show additional UI elements
        if (uiElementsToHide != null)
        {
            foreach (GameObject uiElement in uiElementsToHide)
            {
                if (uiElement != null)
                {
                    uiElement.SetActive(true);
                }
            }
        }
    }
    
    /// <summary>
    /// Enable or disable terminal interaction
    /// </summary>
    /// <param name="enabled">Whether the terminal can be interacted with</param>
    public void SetInteractionEnabled(bool enabled)
    {
        interactionEnabled = enabled;
    }
    
    /// <summary>
    /// Check if terminal interaction is enabled
    /// </summary>
    /// <returns>True if terminal can be interacted with</returns>
    public bool IsInteractionEnabled()
    {
        return interactionEnabled;
    }
    
    // Draw the interaction radius in the Scene view
    private void OnDrawGizmosSelected()
    {
        // Set the gizmo color
        Gizmos.color = Color.yellow;
        
        // Draw a wire sphere to represent the interaction radius
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
