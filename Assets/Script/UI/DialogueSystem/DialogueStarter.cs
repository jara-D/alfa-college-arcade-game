using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueStarter : MonoBehaviour
{
    [Header("Dialogue Manager")]
    public DialogueManager dialogueManager;
    
    [Header("UI Panels")]
    public GameObject DialogueTextContainer;
    public GameObject choicesContainer;
    public GameObject xButton;
    
    [Header("Interaction Settings")]
    public float interactionRange = 2f;
    
    [Header("Dialogue Selection")]
    [Tooltip("List of possible dialogues to choose from")]
    public List<DialogueData> availableDialogues = new List<DialogueData>();
    
    [Tooltip("Index of the dialogue to play (0-based)")]
    public int selectedDialogueIndex = 0;
    
    [Header("Fallback Dialogue (if no DialogueData is assigned)")]
    public bool useFallbackDialogue = true;
    
    private bool isPlayerInRange = false;
    private PlayerController playerController;
    private bool hasDisabledInput = false; // Track if we've disabled input for this dialogue
    
    // Static list to track all DialogueStarter instances
    private static List<DialogueStarter> allDialogueStarters = new List<DialogueStarter>();
    
    /// <summary>
    /// Check if any DialogueStarter has a player in range
    /// </summary>
    public static bool IsPlayerInAnyDialogueRange()
    {
        foreach (var starter in allDialogueStarters)
        {
            if (starter != null && starter.isPlayerInRange)
            {
                return true;
            }
        }
        return false;
    }

    void Start()
    {
        // Add this instance to the static list
        allDialogueStarters.Add(this);
        
        // Find the PlayerController in the scene
        playerController = FindFirstObjectByType<PlayerController>();
        if (playerController == null)
        {
            // PlayerController not found
        }
    }
    
    void OnDestroy()
    {
        // Remove this instance from the static list
        allDialogueStarters.Remove(this);
    }

    void Update()
    {
        if (isPlayerInRange && Input.GetButtonDown("Jump") && !dialogueManager.IsDialogueActive)
        {
            StartDialogue();
        }
        
        // Re-enable player input when dialogue ends
        CheckDialogueState();
    }
    
    private void CheckDialogueState()
    {
        if (dialogueManager != null && playerController != null && hasDisabledInput)
        {
            // If dialogue has ended, re-enable player input (only once)
            if (!dialogueManager.IsDialogueActive)
            {
                playerController.SetInputEnabled(true);
                hasDisabledInput = false; // Reset flag so we don't keep enabling input
            }
        }
    }

    private void StartDialogue()
    {
        List<DialogueNode> dialogueNodes = GetSelectedDialogue();
        
        if (dialogueNodes != null && dialogueNodes.Count > 0)
        {
            // Disable player movement when dialogue starts
            if (playerController != null)
            {
                playerController.SetInputEnabled(false);
                hasDisabledInput = true;
            }
            
            dialogueManager.StartDialogue(dialogueNodes);
            ActivatePanels();
        }
        else
        {
            // No valid dialogue found
        }
    }
    
    private List<DialogueNode> GetSelectedDialogue()
    {
        // First, try to use DialogueData assets
        if (availableDialogues != null && availableDialogues.Count > 0)
        {
            // Ensure selected index is within bounds
            int clampedIndex = Mathf.Clamp(selectedDialogueIndex, 0, availableDialogues.Count - 1);
            
            if (availableDialogues[clampedIndex] != null)
            {
                return availableDialogues[clampedIndex].dialogueNodes;
            }
        }
        
        // Fallback to hardcoded dialogue if no DialogueData is assigned
        if (useFallbackDialogue)
        {
            return GetFallbackDialogue();
        }
        
        return null;
    }
    
    private List<DialogueNode> GetFallbackDialogue()
    {
        return new List<DialogueNode>
        {
            // Node 0
            new DialogueNode
            {
                characterName = "DIALOGUE MANAGER",
                dialogueText = "ERROR: DialogueData has not been assigned, make/add a DialogueData asset by right clicking the asset folder > Create > Dialogue System > Dialogue Data, and then edit it from there! ",
            },
        };
    }

    private void ActivatePanels()
    {
        if (DialogueTextContainer != null)
        {
            DialogueTextContainer.SetActive(true);
        }

        if (choicesContainer != null)
        {
            choicesContainer.SetActive(true);
        }
        
        if (xButton != null)
        {
            xButton.SetActive(true);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }

    /// <summary>
    /// Switch to a different dialogue at runtime
    /// </summary>
    /// <param name="newIndex">Index of the dialogue to switch to</param>
    public void SwitchDialogue(int newIndex)
    {
        if (availableDialogues != null && newIndex >= 0 && newIndex < availableDialogues.Count)
        {
            selectedDialogueIndex = newIndex;
        }
        else
        {
            // Cannot switch to dialogue index - out of range
        }
    }
    
    /// <summary>
    /// Get the name of the currently selected dialogue
    /// </summary>
    /// <returns>Name of current dialogue or "Fallback" if using hardcoded dialogue</returns>
    public string GetCurrentDialogueName()
    {
        if (availableDialogues != null && selectedDialogueIndex < availableDialogues.Count && availableDialogues[selectedDialogueIndex] != null)
        {
            return availableDialogues[selectedDialogueIndex].dialogueName;
        }
        return "Fallback Dialogue";
    }
    
    /// <summary>
    /// Get total number of available dialogues
    /// </summary>
    /// <returns>Number of available dialogues</returns>
    public int GetDialogueCount()
    {
        return availableDialogues != null ? availableDialogues.Count : 0;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
        
        #if UNITY_EDITOR
        // Display current dialogue info in scene view
        if (availableDialogues != null && availableDialogues.Count > 0)
        {
            UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"Dialogue: {GetCurrentDialogueName()}");
        }
        #endif
    }
}