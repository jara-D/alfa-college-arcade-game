using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Timeline;
using UnityEngine.Playables;

[System.Serializable]
public class CodePuzzle
{
    [Header("Puzzle Configuration")]
    public string puzzleName;
    [TextArea(3, 5)]
    public string brokenCode;
    [TextArea(3, 5)]
    public string correctCode;
    [TextArea(2, 3)]
    public string hintText;
}

public class PowerTerminalMinigame : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject minigamePanel;
    [SerializeField] private TMP_InputField codeInputField;
    [SerializeField] private TextMeshProUGUI hintText;
    [SerializeField] private TextMeshProUGUI puzzleNameText;
    [SerializeField] private Button submitButton;
    [SerializeField] private Button closeButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private TextMeshProUGUI feedbackText;
    
    [Header("Puzzle System")]
    [SerializeField] private static List<CodePuzzle> sharedPuzzles = new List<CodePuzzle>();
    [SerializeField] private static List<bool> puzzlesSolved = new List<bool>();
    [SerializeField] private static int totalTerminals = 3;
    
    [Header("Terminal Configuration")]
    private int currentTerminalID = -1; // Set dynamically when minigame starts
    private PowerTerminal currentPowerTerminal;
    
    [Header("Static Puzzle Data")]
    [SerializeField] private bool initializePuzzles = false;
    
    [Header("Global Lighting")]
    [Tooltip("Global Light 2D to set intensity to 1 when all terminals complete")]
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D globalLight;
    
    [Tooltip("Spotlight 2D to disable when all terminals complete")]
    [SerializeField] private UnityEngine.Rendering.Universal.Light2D spotlightToDisable;
    
    [Header("Background Activation")]
    [Tooltip("Background GameObject to activate when all terminals are completed")]
    [SerializeField] private GameObject backgroundToActivate;
    
    [Header("Fog Control")]
    [Tooltip("Fog GameObjects to disable when all terminals are completed")]
    [SerializeField] private GameObject fogObject1;
    [SerializeField] private GameObject fogObject2;
    
    [Header("Timeline Control")]
    [Tooltip("PlayableDirector to control the timeline animation")]
    [SerializeField] private PlayableDirector timelineDirector;
    
    [Tooltip("TimelineAsset to play when all terminals are completed")]
    [SerializeField] private TimelineAsset completionTimeline;
    
    [Tooltip("Image GameObject that gets animated by the timeline (will be activated/deactivated)")]
    [SerializeField] private GameObject transitionImage;
    
    private CodePuzzle currentPuzzle;
    [Header("Player Control")]
    private PlayerController playerController;
    private PowerTerminal[] allTerminals;
    
    private bool isMinigameActive = false;
    private bool isInitialized = false;
    private bool timelineStartedForFinalTerminal = false;
    
    void Start()
    {
        InitializePuzzleSystem();
        SetupUI();
    }
    
    private void InitializePuzzleSystem()
    {
        // Only initialize once across all terminals
        if (sharedPuzzles.Count == 0 || initializePuzzles)
        {
            InitializeSharedPuzzles();
        }
        
        // Ensure the right number of solved states
        while (puzzlesSolved.Count < totalTerminals)
        {
            puzzlesSolved.Add(false);
        }
        
        isInitialized = true;
    }
    
    private void InitializeSharedPuzzles()
    {
        sharedPuzzles.Clear();
        
        // Puzzle 1
        CodePuzzle puzzle1 = new CodePuzzle
        {
            puzzleName = "Power Core Initialization",
            brokenCode = "int powerLevel = 0;\nif (powerLevel = 100)\n ActivateCore();",
            correctCode = "int powerLevel = 100;\nif (powerLevel = 100)\n ActivateCore();",
            hintText = "The power level needs to be sufficient to activate the core. Try a higher value."
        };
        
        // Puzzle 2
        CodePuzzle puzzle2 = new CodePuzzle
        {
            puzzleName = "Open Energy Gates",
            brokenCode = "openGate(0);\n// (0 = Closed  |  1 = Open)\n",
            correctCode = "openGate(1);\n// (0 = Closed  |  1 = Open)\n",
            hintText = "The gates need to be opened to allow passage. Change the parameter to open the gate."
        };
        
        // Puzzle 3
        CodePuzzle puzzle3 = new CodePuzzle
        {
            puzzleName = "System Activation",
            brokenCode = "bool StartSystem(False)",
            correctCode = "bool StartSystem(True)",
            hintText = "The system needs to be started with a true parameter. Change the boolean value."
        };
        
        sharedPuzzles.Add(puzzle1);
        sharedPuzzles.Add(puzzle2);
        sharedPuzzles.Add(puzzle3);
    }
    
    private void SetupUI()
    {
        if (submitButton != null)
            submitButton.onClick.AddListener(SubmitCode);
        
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseMinigame);
        
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetPuzzle);
    }
    
    public void StartMinigame(int terminalID, PowerTerminal powerTerminal)
    {
        currentTerminalID = terminalID;
        currentPowerTerminal = powerTerminal;
        
        if (!isInitialized)
        {
            InitializePuzzleSystem();
        }
        
        // Check if the terminal is already solved
        if (currentTerminalID < puzzlesSolved.Count && puzzlesSolved[currentTerminalID])
        {
            ShowAlreadySolvedMessage();
            return;
        }
        
        LoadPuzzle();
        
        if (minigamePanel != null)
        {
            minigamePanel.SetActive(true);
        }
        
        DisablePlayerControls();
    }
    
    private void LoadPuzzle()
    {
        if (currentTerminalID >= 0 && currentTerminalID < sharedPuzzles.Count)
        {
            currentPuzzle = sharedPuzzles[currentTerminalID];
            
            // Update UI with the puzzle data
            if (puzzleNameText != null)
                puzzleNameText.text = currentPuzzle.puzzleName;
            
            if (codeInputField != null)
                codeInputField.text = currentPuzzle.brokenCode;
            
            if (hintText != null)
                hintText.text = currentPuzzle.hintText;
            
            if (feedbackText != null)
                feedbackText.text = "";
        }
    }
    
    public void SubmitCode()
    {
        if (currentPuzzle == null || codeInputField == null) return;
        
        string submittedCode = codeInputField.text.Trim();
        string correctCode = currentPuzzle.correctCode.Trim();
        
        // Check if the code matches
        if (NormalizeCode(submittedCode) == NormalizeCode(correctCode))
        {
            OnPuzzleSolved();
        }
        else
        {
            OnPuzzleFailed();
        }
    }
    
    private string NormalizeCode(string code)
    {
        // Remove extra whitespaces, normalize line endings, and make capital letter insensitive for the comparison
        return code.Replace("\r\n", "\n")
                   .Replace("\r", "\n")
                   .Replace(" ", "")
                   .Replace("\t", "")
                   .ToLowerInvariant();
    }
    
    public void ResetPuzzle()
    {
        if (currentPuzzle != null && codeInputField != null)
        {
            codeInputField.text = currentPuzzle.brokenCode;
        }
        
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }
    
    private void OnPuzzleSolved()
    {
        // Mark the terminal as solved FIRST
        if (currentTerminalID < puzzlesSolved.Count)
        {
            puzzlesSolved[currentTerminalID] = true;
        }
        
        // NOW check if this makes all terminals completed
        bool willBeAllCompleted = true;
        int completedCount = 0;
        for (int i = 0; i < totalTerminals; i++)
        {
            if (i < puzzlesSolved.Count && puzzlesSolved[i])
            {
                completedCount++;
            }
            else
            {
                willBeAllCompleted = false;
            }
        }
        
        if (willBeAllCompleted)
        {
            // This is the final terminal - make panel invisible IMMEDIATELY
            MakePanelInvisible();
            
            // Start timeline IMMEDIATELY while panel is invisible but still active
            if (timelineDirector != null && completionTimeline != null)
            {
                // Check if timeline is already playing
                if (timelineDirector.state != PlayState.Playing && !timelineStartedForFinalTerminal)
                {
                    timelineStartedForFinalTerminal = true;
                    StartCoroutine(PlayCompletionTimelineWithEffects());
                }
            }
            
            if (feedbackText != null)
            {
                feedbackText.text = "✓ All Terminals Activated! System Online...";
                feedbackText.color = Color.green;
            }
        }
        else
        {
            if (feedbackText != null)
            {
                feedbackText.text = "✓ Code Corrected! Terminal Activating...";
                feedbackText.color = Color.green;
            }
        }
        
        // Continue with normal completion sequence
        Invoke(nameof(CompletePuzzle), 1.5f);
    }
    
    private void CompletePuzzle()
    {
        // Check if this will be the final terminal before activating it
        bool willBeAllCompleted = true;
        int completedCount = 0;
        
        for (int i = 0; i < totalTerminals; i++)
        {
            if (i < puzzlesSolved.Count && puzzlesSolved[i])
            {
                completedCount++;
            }
            else
            {
                willBeAllCompleted = false;
            }
        }
        
        if (willBeAllCompleted)
        {
            // This is the final terminal - timeline already started in OnPuzzleSolved, just activate terminal
            if (currentPowerTerminal != null)
            {
                currentPowerTerminal.OnMinigameComplete();
            }
        }
        else
        {
            // Regular terminal completion - normal close
            CloseMinigame();
            
            // Activate the terminal
            if (currentPowerTerminal != null)
            {
                currentPowerTerminal.OnMinigameComplete();
            }
            
            CheckAllTerminalsCompleted();
        }
    }
    
    private void OnPuzzleFailed()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "✗ Code Error! Check the hint and try again.";
            feedbackText.color = Color.red;
        }
        
        Invoke(nameof(ClearFeedback), 2f);
    }
    
    private void ClearFeedback()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "";
        }
    }
    
    /// <summary>
    /// Make the panel invisible by setting alpha to 0 and clearing text, but keep it active for timeline execution
    /// </summary>
    private void MakePanelInvisible()
    {
        if (minigamePanel != null)
        {
            // Set panel alpha to 0
            CanvasGroup canvasGroup = minigamePanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = minigamePanel.AddComponent<CanvasGroup>();
            }
            
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        // Clear all text fields
        if (feedbackText != null)
            feedbackText.text = "";
            
        if (hintText != null)
            hintText.text = "";
            
        if (puzzleNameText != null)
            puzzleNameText.text = "";
            
        if (codeInputField != null)
            codeInputField.text = "";
        
        // Re-enable player controls immediately since panel is invisible
        EnablePlayerControls();
    }
    
    private void ShowAlreadySolvedMessage()
    {
        if (feedbackText != null)
        {
            feedbackText.text = "Terminal already activated!";
            feedbackText.color = Color.yellow;
        }
    }
    
    private void CheckAllTerminalsCompleted()
    {
        bool allSolved = true;
        for (int i = 0; i < totalTerminals; i++)
        {
            if (i >= puzzlesSolved.Count || !puzzlesSolved[i])
            {
                allSolved = false;
                break;
            }
        }
        
        if (allSolved)
        {
            TriggerAllCompletionEffects();
        }
    }
    
    private void TriggerAllCompletionEffects()
    {
        // Find and trigger all TilemapChanger scripts to change tiles to colored versions
        TilemapChanger[] tilemapChangers = FindObjectsByType<TilemapChanger>(FindObjectsSortMode.None);
        
        if (tilemapChangers.Length > 0)
        {
            foreach (TilemapChanger tilemapChanger in tilemapChangers)
            {
                if (tilemapChanger != null)
                {
                    tilemapChanger.ChangeTilesToColored();
                }
            }
        }
        
        // Control global lighting
        ControlGlobalLighting();
        
        // Activate background if assigned
        if (backgroundToActivate != null)
        {
            backgroundToActivate.SetActive(true);
        }
        
        // Disable fog objects if assigned
        if (fogObject1 != null)
        {
            fogObject1.SetActive(false);
        }
        
        if (fogObject2 != null)
        {
            fogObject2.SetActive(false);
        }
    }

    private void ControlGlobalLighting()
    {
        // Set global light intensity to 1 (full brightness)
        if (globalLight == null)
        {
            // Try to find a global light automatically
            UnityEngine.Rendering.Universal.Light2D[] allLights = FindObjectsByType<UnityEngine.Rendering.Universal.Light2D>(FindObjectsSortMode.None);
            foreach (var light in allLights)
            {
                if (light.lightType == UnityEngine.Rendering.Universal.Light2D.LightType.Global)
                {
                    globalLight = light;
                    break;
                }
            }
        }
        
        if (globalLight != null)
        {
            globalLight.intensity = 1f;
        }
        
        // Disable the specified spotlight
        if (spotlightToDisable == null)
        {
            // Try to find a spotlight to disable automatically (look for one with "disable" in name)
            UnityEngine.Rendering.Universal.Light2D[] allLights = FindObjectsByType<UnityEngine.Rendering.Universal.Light2D>(FindObjectsSortMode.None);
            foreach (var light in allLights)
            {
                if (light.name.ToLower().Contains("disable") || light.name.ToLower().Contains("main") || light.name.ToLower().Contains("darkness"))
                {
                    spotlightToDisable = light;
                    break;
                }
            }
        }
        
        if (spotlightToDisable != null)
        {
            spotlightToDisable.enabled = false;
        }
    }
    
    public void CloseMinigame()
    {
        if (minigamePanel != null)
        {
            minigamePanel.SetActive(false);
        }
        
        // Reset timeline flag for next play session
        timelineStartedForFinalTerminal = false;
        
        // Show UI again
        if (currentPowerTerminal != null)
        {
            currentPowerTerminal.CloseMinigamePanel();
        }
        
        EnablePlayerControls();
    }
    
    // Static method to get completion status
    public static bool AreAllTerminalsCompleted()
    {
        for (int i = 0; i < totalTerminals; i++)
        {
            if (i >= puzzlesSolved.Count || !puzzlesSolved[i])
            {
                return false;
            }
        }
        return true;
    }
    
    // Static method to get individual terminal status
    public static bool IsTerminalCompleted(int terminalID)
    {
        return terminalID >= 0 && terminalID < puzzlesSolved.Count && puzzlesSolved[terminalID];
    }
    
    // Debug method to reset all puzzles
    [ContextMenu("Reset All Puzzles")]
    public void ResetAllPuzzles()
    {
        for (int i = 0; i < puzzlesSolved.Count; i++)
        {
            puzzlesSolved[i] = false;
        }
    }
    
    private void DisablePlayerControls()
    {
        isMinigameActive = true;
        
        // Find and disable player controller
        if (playerController == null)
        {
            playerController = FindFirstObjectByType<PlayerController>();
        }
        
        if (playerController != null)
        {
            // Disable player input
            var inputEnabledField = typeof(PlayerController).GetField("InputEnabled", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (inputEnabledField != null)
            {
                inputEnabledField.SetValue(playerController, false);
            }
        }
        
        // Disable all terminal interactions
        if (allTerminals == null)
        {
            allTerminals = FindObjectsByType<PowerTerminal>(FindObjectsSortMode.None);
        }
        
        foreach (PowerTerminal terminal in allTerminals)
        {
            if (terminal != null)
            {
                terminal.SetInteractionEnabled(false);
            }
        }
    }
    
    private void EnablePlayerControls()
    {
        isMinigameActive = false;
        
        // Re-enable player controller
        if (playerController != null)
        {
            var inputEnabledField = typeof(PlayerController).GetField("InputEnabled", 
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (inputEnabledField != null)
            {
                inputEnabledField.SetValue(playerController, true);
            }
        }
        
        // Re-enable all terminal interactions
        if (allTerminals != null)
        {
            foreach (PowerTerminal terminal in allTerminals)
            {
                if (terminal != null)
                {
                    terminal.SetInteractionEnabled(true);
                }
            }
        }
    }
    
    public bool IsMinigameActive()
    {
        return isMinigameActive;
    }
    
    /// <summary>
    /// Coroutine to play timeline immediately while spreading completion effects across frames to avoid lag
    /// </summary>
    private System.Collections.IEnumerator PlayCompletionTimelineWithEffects()
    {
        // Activate the transition image IMMEDIATELY
        if (transitionImage != null)
        {
            transitionImage.SetActive(true);
            
            // Use Invoke instead of coroutines - more reliable for simple delays
            Invoke(nameof(DeactivateTransitionImage), 1.5f);
        }
        
        // Start the timeline IMMEDIATELY (no delay)
        if (timelineDirector != null && completionTimeline != null)
        {
            timelineDirector.Play(completionTimeline);
        }
        
        // Wait one frame to let timeline start
        yield return null;
        
        // Now trigger completion effects spread across multiple frames to avoid lag
        
        // Frame 1: Change tiles (this is the most expensive operation)
        TilemapChanger[] tilemapChangers = FindObjectsByType<TilemapChanger>(FindObjectsSortMode.None);
        if (tilemapChangers.Length > 0)
        {
            foreach (TilemapChanger tilemapChanger in tilemapChangers)
            {
                if (tilemapChanger != null)
                {
                    tilemapChanger.ChangeTilesToColored();
                }
            }
        }
        
        yield return null; // Wait one frame
        
        // Frame 2: Control lighting
        ControlGlobalLighting();
        
        yield return null; // Wait one frame
        
        // Frame 3: Activate background and disable fog
        if (backgroundToActivate != null)
        {
            backgroundToActivate.SetActive(true);
        }
        
        if (fogObject1 != null)
        {
            fogObject1.SetActive(false);
        }
        
        if (fogObject2 != null)
        {
            fogObject2.SetActive(false);
        }
        
        // Calculate remaining timeline duration
        float totalDuration = (float)completionTimeline.duration;
        
        // Wait for a shorter time - we don't need the full timeline for the transition effect
        float transitionDuration = Mathf.Min(totalDuration, 2f); // Max 2 seconds or timeline duration
        
        if (transitionDuration > 0)
        {
            yield return new WaitForSeconds(transitionDuration);
        }
        else
        {
            // Fallback: wait 1.5 seconds if timeline duration is invalid
            yield return new WaitForSeconds(1.5f);
        }
        
        // Deactivate the transition image
        if (transitionImage != null)
        {
            transitionImage.SetActive(false);
        }
        
        // Additional backup - use Invoke as well
        Invoke(nameof(DeactivateTransitionImage), 0.1f);
        
        // Now close the minigame
        CloseMinigame();
    }
    
    /// <summary>
    /// Deactivate transition image - called via Invoke for reliability
    /// </summary>
    private void DeactivateTransitionImage()
    {
        if (transitionImage != null && transitionImage.activeInHierarchy)
        {
            transitionImage.SetActive(false);
        }
    }
    
    /// <summary>
    /// Backup method to ensure transition image gets deactivated even if main timeline completion fails
    /// </summary>
    private System.Collections.IEnumerator DeactivateTransitionImageAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (transitionImage != null && transitionImage.activeInHierarchy)
        {
            transitionImage.SetActive(false);
        }
    }
}
