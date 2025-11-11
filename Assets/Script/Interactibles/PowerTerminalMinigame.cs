using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
    
    private CodePuzzle currentPuzzle;
    [Header("Player Control")]
    private PlayerController playerController;
    private PowerTerminal[] allTerminals;
    
    private bool isMinigameActive = false;
    private bool isInitialized = false;
    
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
        // Mark the terminal as solved
        if (currentTerminalID < puzzlesSolved.Count)
        {
            puzzlesSolved[currentTerminalID] = true;
        }
        
        if (feedbackText != null)
        {
            feedbackText.text = "✓ Code Corrected! Terminal Activating...";
            feedbackText.color = Color.green;
        }
        
        // Close minigame panel and activate terminal
        Invoke(nameof(CompletePuzzle), 1.5f);
    }
    
    private void CompletePuzzle()
    {
        CloseMinigame();
        
        // Activate the terminal
        if (currentPowerTerminal != null)
        {
            currentPowerTerminal.OnMinigameComplete();
        }
        
        CheckAllTerminalsCompleted();
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
            OnAllTerminalsCompleted();
        }
    }
    
    private void OnAllTerminalsCompleted()
    {
        Debug.Log("🎉 ALL TERMINALS ACTIVATED! OBJECTIVE COMPLETED!");
        
        // Find and trigger all TilemapChanger scripts to change tiles to colored versions
        TilemapChanger[] tilemapChangers = FindObjectsByType<TilemapChanger>(FindObjectsSortMode.None);
        
        if (tilemapChangers.Length > 0)
        {
            Debug.Log($"Found {tilemapChangers.Length} TilemapChanger(s). Changing tiles to colored versions...");
            
            foreach (TilemapChanger tilemapChanger in tilemapChangers)
            {
                if (tilemapChanger != null)
                {
                    tilemapChanger.ChangeTilesToColored();
                }
            }
        }
        else
        {
            Debug.Log("No TilemapChanger scripts found in the scene.");
        }
    }
    
    public void CloseMinigame()
    {
        if (minigamePanel != null)
        {
            minigamePanel.SetActive(false);
        }
        
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
        Debug.Log("All puzzles reset!");
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
}
