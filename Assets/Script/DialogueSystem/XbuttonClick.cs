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
            Debug.LogError("XbuttonClick: No Button component found on this GameObject! Make sure this script is attached to a GameObject with a Button component.");
            return;
        }
        
        // Find DialogueManager if not assigned
        if (dialogueManager == null)
        {
            dialogueManager = FindFirstObjectByType<DialogueManager>();
            if (dialogueManager == null)
            {
                Debug.LogError("XbuttonClick: No DialogueManager found in the scene!");
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
            Debug.LogWarning("XbuttonClick: DialogueManager reference is null!");
        }
    }
}
