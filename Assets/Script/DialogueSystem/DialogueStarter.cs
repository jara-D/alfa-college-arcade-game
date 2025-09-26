using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueStarter : MonoBehaviour
{
    [Header("Dialogue Manager")]
    public DialogueManager dialogueManager;
    
    [Header("UI Panels")]
    public GameObject panel1;
    public GameObject panel2;
    public GameObject choicesContainer;
    
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

    void Update()
    {
        if (isPlayerInRange && Input.GetKeyDown(KeyCode.E))
        {
            StartDialogue();
        }
    }

    private void StartDialogue()
    {
        List<DialogueNode> dialogueNodes = GetSelectedDialogue();
        
        if (dialogueNodes != null && dialogueNodes.Count > 0)
        {
            dialogueManager.StartDialogue(dialogueNodes);
            ActivatePanels();
        }
        else
        {
            Debug.LogWarning("No valid dialogue found! Check DialogueStarter configuration.");
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
        if (panel1 != null)
        {
            panel1.SetActive(true);
        }

        if (panel2 != null)
        {
            panel2.SetActive(true);
        }

        if (choicesContainer != null)
        {
            choicesContainer.SetActive(true);
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
            Debug.Log($"Switched to dialogue: {availableDialogues[newIndex].dialogueName}");
        }
        else
        {
            Debug.LogWarning($"Cannot switch to dialogue index {newIndex}. Index out of range.");
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