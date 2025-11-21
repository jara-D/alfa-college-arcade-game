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
    
    [Header("Character Facing")]
    [Tooltip("Should the character turn to face the player when they approach?")]
    public bool facePlayer = true;
    
    [Tooltip("Which direction is the character's default facing direction? (1 = right, -1 = left)")]
    public int defaultFacingDirection = -1;
    
    [Header("Dialogue Selection")]
    [Tooltip("List of possible dialogues to choose from")]
    public List<DialogueData> availableDialogues = new List<DialogueData>();
    
    [Tooltip("Index of the dialogue to play (0-based)")]
    public int selectedDialogueIndex = 0;
    
    [Header("Fallback Dialogue (if no DialogueData is assigned)")]
    public bool useFallbackDialogue = true;
    
    [Header("Character Selection")]
    [Tooltip("Select which character this dialogue starter represents")]
    public bool Ids = false;
    public bool Johan = false;
    public bool Christiaan = false;
    public bool Carlo = false;
    public bool Maik = false;
    public bool Sjoerd = false;
    [SerializeField] private bool EvilIds = false; // Using SerializeField with different name for "Evil Ids"
    
    [Header("Character Animations")]
    [Tooltip("Animation booleans for each character")]
    // Ids animations
    public string IdsIdle = "IdsIdle";
    public string IdsTalking = "IdsTalking";
    
    // Johan animations (has 3: idle, talking, knee)
    public string JohanIdle = "JohanIdle";
    public string JohanTalking = "JohanTalking";
    public string JohanKnee = "JohanKnee";
    
    // Christiaan animations
    public string ChristiaanIdle = "ChristiaanIdle";
    public string ChristiaanTalking = "ChristiaanTalking";
    
    // Carlo animations
    public string CarloIdle = "CarloIdle";
    public string CarloTalking = "CarloTalking";
    
    // Maik animations
    public string MaikIdle = "MaikIdle";
    public string MaikTalking = "MaikTalking";
    
    // Sjoerd animations
    public string SjoerdIdle = "SjoerdIdle";
    public string SjoerdTalking = "SjoerdTalking";
    
    // Evil Ids animations
    public string EvilIdsIdle = "EvilIdsIdle";
    public string EvilIdsTalking = "EvilIdsTalking";
    
    [Header("Animator Component")]
    [Tooltip("Animator component to control character animations")]
    public Animator characterAnimator;
    
    private bool isPlayerInRange = false;
    private PlayerController playerController;
    private bool hasDisabledInput = false; // Track if we've disabled input for this dialogue
    private bool hasFlippedToFacePlayer = false; // Track if we've flipped to face player
    private Vector3 originalScale; // Store original scale
    
    // Static list to track all DialogueStarter instances
    private static List<DialogueStarter> allDialogueStarters = new List<DialogueStarter>();
    
    // Check if any DialogueStarter has a player in range
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
        
        // Store original scale for flipping
        originalScale = transform.localScale;
        
        // Set idle animation for the selected character
        SetCharacterIdleAnimation();
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
        
        // Emergency dialogue exit (Escape key) - for debugging the softlock issue
        if (dialogueManager.IsDialogueActive && Input.GetKeyDown(KeyCode.Escape))
        {
            ForceEndDialogue();
        }
        
        // Continuously face the player if face player is enabled
        if (facePlayer && playerController != null)
        {
            FacePlayer(playerController.transform);
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
                // Set character back to idle animation (this will reset talking animation)
                SetCharacterTalkingAnimation(false);
                
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
            // Activate UI panels first
            ActivatePanels();
            
            // Disable player movement when dialogue starts
            if (playerController != null)
            {
                playerController.SetInputEnabled(false);
                hasDisabledInput = true;
            }
            
            // Handle Johan's special knee animation - but start dialogue immediately
            if (Johan)
            {
                // Start dialogue immediately
                dialogueManager.StartDialogue(dialogueNodes);
                
                // Play knee animation sequence in background
                StartCoroutine(JohanKneeAnimationSequence());
            }
            else
            {
                // Set talking animation for other characters
                SetCharacterTalkingAnimation(true);
                dialogueManager.StartDialogue(dialogueNodes);
            }
        }
        else
        {
            Debug.LogError("No valid dialogue found!");
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

    // Activate panels and ensure Canvas Groups are properly configured for interaction
    private void ActivatePanels()
    {
        if (DialogueTextContainer != null)
        {
            DialogueTextContainer.SetActive(true);
            
            // Check for Canvas Group blocking
            var canvasGroup = DialogueTextContainer.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }

        if (choicesContainer != null)
        {
            choicesContainer.SetActive(true);
            
            // Check for Canvas Group blocking
            var canvasGroup = choicesContainer.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
        }
        
        if (xButton != null)
        {
            xButton.SetActive(true);
            
            // Check for Canvas Group blocking
            var canvasGroup = xButton.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.interactable = true;
                canvasGroup.blocksRaycasts = true;
            }
            
            // Also check the button component itself
            var button = xButton.GetComponent<UnityEngine.UI.Button>();
            if (button != null)
            {
                button.interactable = true;
            }
        }
        
        // Force UI refresh
        Canvas.ForceUpdateCanvases();
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // Player facing is now handled continuously in Update()
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // Player facing continues even when out of interaction range
        }
    }

    /// <summary>
    /// Set the idle animation for the selected character
    /// </summary>
    private void SetCharacterIdleAnimation()
    {
        if (characterAnimator == null) return;
        
        // Reset all animation bools to false first
        ResetAllAnimations();
        
        // Set the appropriate idle animation based on selected character
        if (Ids)
        {
            characterAnimator.SetBool(IdsIdle, true);
        }
        else if (Johan)
        {
            characterAnimator.SetBool(JohanIdle, true);
        }
        else if (Christiaan)
        {
            characterAnimator.SetBool(ChristiaanIdle, true);
        }
        else if (Carlo)
        {
            characterAnimator.SetBool(CarloIdle, true);
        }
        else if (Maik)
        {
            characterAnimator.SetBool(MaikIdle, true);
        }
        else if (Sjoerd)
        {
            characterAnimator.SetBool(SjoerdIdle, true);
        }
        else if (EvilIds)
        {
            characterAnimator.SetBool(EvilIdsIdle, true);
        }
    }
    
    /// <summary>
    /// Coroutine to handle Johan's knee animation sequence (runs in background while dialogue is active)
    /// </summary>
    private System.Collections.IEnumerator JohanKneeAnimationSequence()
    {
        if (characterAnimator != null)
        {
            // Reset all animations and start knee animation
            ResetAllAnimations();
            characterAnimator.SetBool(JohanKnee, true);
        }
        
        // Wait for 0.5 seconds
        yield return new WaitForSeconds(0.5f);
        
        if (characterAnimator != null)
        {
            // Switch to talking animation
            ResetAllAnimations();
            characterAnimator.SetBool(JohanTalking, true);
        }
    }

    /// <summary>
    /// Coroutine to handle Johan's knee animation sequence before dialogue (DEPRECATED - keeping for reference)
    /// </summary>
    /// <param name="dialogueNodes">The dialogue to start after knee animation</param>
    private System.Collections.IEnumerator JohanDialogueSequence(List<DialogueNode> dialogueNodes)
    {
        if (characterAnimator != null)
        {
            // Reset all animations and start knee animation
            ResetAllAnimations();
            characterAnimator.SetBool(JohanKnee, true);
        }
        
        // Wait for 0.5 seconds
        yield return new WaitForSeconds(0.5f);
        
        if (characterAnimator != null)
        {
            // Switch to talking animation
            ResetAllAnimations();
            characterAnimator.SetBool(JohanTalking, true);
        }
        
        // Start dialogue - let DialogueManager handle UI
        dialogueManager.StartDialogue(dialogueNodes);
    }

    /// <summary>
    /// Set the appropriate animation state for the selected character
    /// </summary>
    /// <param name="isTalking">True for talking animation, false for idle animation</param>
    private void SetCharacterTalkingAnimation(bool isTalking)
    {
        if (characterAnimator == null) return;
        
        // Reset all animations first to prevent conflicts
        ResetAllAnimations();
        
        // Set the appropriate animation based on selected character
        if (Ids)
        {
            if (isTalking)
                characterAnimator.SetBool(IdsTalking, true);
            else
                characterAnimator.SetBool(IdsIdle, true);
        }
        else if (Johan)
        {
            if (isTalking)
                characterAnimator.SetBool(JohanTalking, true);
            else
                characterAnimator.SetBool(JohanIdle, true);
        }
        else if (Christiaan)
        {
            if (isTalking)
                characterAnimator.SetBool(ChristiaanTalking, true);
            else
                characterAnimator.SetBool(ChristiaanIdle, true);
        }
        else if (Carlo)
        {
            if (isTalking)
                characterAnimator.SetBool(CarloTalking, true);
            else
                characterAnimator.SetBool(CarloIdle, true);
        }
        else if (Maik)
        {
            if (isTalking)
                characterAnimator.SetBool(MaikTalking, true);
            else
                characterAnimator.SetBool(MaikIdle, true);
        }
        else if (Sjoerd)
        {
            if (isTalking)
                characterAnimator.SetBool(SjoerdTalking, true);
            else
                characterAnimator.SetBool(SjoerdIdle, true);
        }
        else if (EvilIds)
        {
            if (isTalking)
                characterAnimator.SetBool(EvilIdsTalking, true);
            else
                characterAnimator.SetBool(EvilIdsIdle, true);
        }
    }
    
    /// <summary>
    /// Reset all character animation bools to false
    /// </summary>
    private void ResetAllAnimations()
    {
        if (characterAnimator == null) return;
        
        // Reset Ids animations
        characterAnimator.SetBool(IdsIdle, false);
        characterAnimator.SetBool(IdsTalking, false);
        
        // Reset Johan animations
        characterAnimator.SetBool(JohanIdle, false);
        characterAnimator.SetBool(JohanTalking, false);
        characterAnimator.SetBool(JohanKnee, false);
        
        // Reset Christiaan animations
        characterAnimator.SetBool(ChristiaanIdle, false);
        characterAnimator.SetBool(ChristiaanTalking, false);
        
        // Reset Carlo animations
        characterAnimator.SetBool(CarloIdle, false);
        characterAnimator.SetBool(CarloTalking, false);
        
        // Reset Maik animations
        characterAnimator.SetBool(MaikIdle, false);
        characterAnimator.SetBool(MaikTalking, false);
        
        // Reset Sjoerd animations
        characterAnimator.SetBool(SjoerdIdle, false);
        characterAnimator.SetBool(SjoerdTalking, false);
        
        // Reset Evil Ids animations
        characterAnimator.SetBool(EvilIdsIdle, false);
        characterAnimator.SetBool(EvilIdsTalking, false);
    }
    
    /// <summary>
    /// Make the character face towards the player
    /// </summary>
    /// <param name="playerTransform">The player's transform</param>
    private void FacePlayer(Transform playerTransform)
    {
        // Calculate direction to player
        float directionToPlayer = playerTransform.position.x - transform.position.x;
        
        // Determine which way the character should face
        // If player X is lower than NPC X, look right (1)
        // If player X is higher than NPC X, look left (-1)
        int targetFacingDirection = directionToPlayer < 0 ? 1 : -1;
        int currentFacingDirection = GetCurrentFacingDirection();
        
        // Flip if we need to change direction
        if (targetFacingDirection != currentFacingDirection)
        {
            FlipCharacter();
            hasFlippedToFacePlayer = true;
        }
    }
    
    /// <summary>
    /// Return character to their default facing direction
    /// </summary>
    private void ReturnToDefaultFacing()
    {
        // Only flip back if current direction doesn't match default
        if (GetCurrentFacingDirection() != defaultFacingDirection)
        {
            FlipCharacter();
        }
        hasFlippedToFacePlayer = false;
    }
    
    /// <summary>
    /// Get the character's current facing direction based on scale
    /// </summary>
    /// <returns>1 for right, -1 for left</returns>
    private int GetCurrentFacingDirection()
    {
        return transform.localScale.x > 0 ? 1 : -1;
    }
    
    /// <summary>
    /// Flip the character by inverting the X scale
    /// </summary>
    private void FlipCharacter()
    {
        Vector3 newScale = transform.localScale;
        newScale.x *= -1;
        transform.localScale = newScale;
    }
    
    /// <summary>
    /// Manually set the character to face a specific direction
    /// </summary>
    /// <param name="direction">1 for right, -1 for left</param>
    public void SetFacingDirection(int direction)
    {
        direction = direction > 0 ? 1 : -1; // Clamp to 1 or -1
        
        if (GetCurrentFacingDirection() != direction)
        {
            FlipCharacter();
        }
    }
    
    /// <summary>
    /// Force end dialogue - emergency method to fix softlock issues
    /// </summary>
    public void ForceEndDialogue()
    {
        if (dialogueManager != null)
        {
            dialogueManager.EndDialogue();
        }
        
        // Reset character animation
        SetCharacterTalkingAnimation(false);
        
        // Re-enable player input
        if (playerController != null)
        {
            playerController.SetInputEnabled(true);
            hasDisabledInput = false;
        }
        
        // Force update UI state
        Canvas.ForceUpdateCanvases();
    }
    
    /// <summary>
    /// Trigger Johan's knee animation specifically
    /// </summary>
    public void TriggerJohanKneeAnimation()
    {
        if (characterAnimator != null && Johan)
        {
            ResetAllAnimations();
            characterAnimator.SetBool(JohanKnee, true);
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