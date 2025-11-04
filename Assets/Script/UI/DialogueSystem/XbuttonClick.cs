using UnityEngine;
using UnityEngine.UI;

public class XbuttonClick : MonoBehaviour
{
    [Header("Dialogue Manager Reference")]
    [Tooltip("The DialogueManager to end dialogue on. If not assigned, will search for one automatically.")]
    public DialogueManager dialogueManager;
    
    private Button button;

    void Start()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            // No Button component found
            return;
        }
        
        // Find DialogueManager if not assigned
        if (dialogueManager == null)
        {
            dialogueManager = FindFirstObjectByType<DialogueManager>();
            if (dialogueManager == null)
            {
                // No DialogueManager found
                return;
            }
        }
        
        button.onClick.AddListener(EndDialogue);
    }
    
    public void EndDialogue()
    {
        if (dialogueManager != null)
        {
            dialogueManager.EndDialogue();
        }
        else
        {
            // DialogueManager reference is null
        }
    }
}
