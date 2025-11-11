using UnityEngine;
using UnityEngine.UI;

public class XbuttonClick : MonoBehaviour
{
    [Header("Action Type")]
    [Tooltip("Choose what this X button should do when clicked.")]
    public ActionType actionType = ActionType.EndDialogue;
    
    [Header("References")]
    [Tooltip("The DialogueManager to end dialogue on. If not assigned, will search for one automatically.")]
    public DialogueManager dialogueManager;
    
    [Tooltip("The GameObject panel to disable when clicked.")]
    public GameObject panelToDisable;
    
    [Tooltip("The PowerTerminal to close minigame panel on.")]
    public PowerTerminal powerTerminal;
    
    [Tooltip("The PowerTerminalMinigame to close minigame on.")]
    public PowerTerminalMinigame powerTerminalMinigame;
    
    private Button button;

    public enum ActionType
    {
        EndDialogue,
        DisablePanel,
        CloseMinigamePanel
    }

    void Start()
    {
        button = GetComponent<Button>();
        if (button == null)
        {
            // No Button component found
            return;
        }
        
        // Setup based on action type
        switch (actionType)
        {
            case ActionType.EndDialogue:
                SetupDialogueAction();
                break;
            case ActionType.DisablePanel:
                SetupPanelAction();
                break;
            case ActionType.CloseMinigamePanel:
                SetupMinigameAction();
                break;
        }
        
        button.onClick.AddListener(ExecuteAction);
    }
    
    private void SetupDialogueAction()
    {
        // Find DialogueManager if not assigned
        if (dialogueManager == null)
        {
            dialogueManager = FindFirstObjectByType<DialogueManager>();
            if (dialogueManager == null)
            {
                // No DialogueManager found
            }
        }
    }
    
    private void SetupPanelAction()
    {
        // Panel reference should be assigned in inspector
        if (panelToDisable == null)
        {
            // No panel assigned
        }
    }
    
    private void SetupMinigameAction()
    {
        // Find PowerTerminalMinigame if not assigned
        if (powerTerminalMinigame == null)
        {
            powerTerminalMinigame = FindFirstObjectByType<PowerTerminalMinigame>();
            if (powerTerminalMinigame == null)
            {
                // No PowerTerminalMinigame found
            }
        }
        
        // Find PowerTerminal if not assigned (backup)
        if (powerTerminal == null)
        {
            powerTerminal = FindFirstObjectByType<PowerTerminal>();
            if (powerTerminal == null)
            {
                // No PowerTerminal found
            }
        }
    }
    
    public void ExecuteAction()
    {
        switch (actionType)
        {
            case ActionType.EndDialogue:
                EndDialogue();
                break;
            case ActionType.DisablePanel:
                DisablePanel();
                break;
            case ActionType.CloseMinigamePanel:
                CloseMinigamePanel();
                break;
        }
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
    
    public void DisablePanel()
    {
        if (panelToDisable != null)
        {
            panelToDisable.SetActive(false);
        }
        else
        {
            // Panel reference is null
        }
    }
    
    public void CloseMinigamePanel()
    {
        // Prefer PowerTerminalMinigame's CloseMinigame method as it properly handles player controls
        if (powerTerminalMinigame != null)
        {
            powerTerminalMinigame.CloseMinigame();
        }
        else if (powerTerminal != null)
        {
            // Fallback to PowerTerminal method (but this won't re-enable player controls)
            powerTerminal.CloseMinigamePanel();
        }
        else
        {
            // No references found
        }
    }
}
