using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class DialogueNode
{
    public string characterName;
    public string dialogueText;
    public List<DialogueChoice> choices;
}

[System.Serializable]
public class DialogueChoice
{
    public string choiceText;
    public int nextNodeIndex;
}

public class DialogueManager : MonoBehaviour
{
    public Text dialogueText;
    public Text characterNameText;
    public GameObject choicesContainer;
    public Button choiceButtonPrefab;
    public GameObject DialogueTextContainer;
    public GameObject xButton;
    
    [Header("Typewriter Settings")]
    [Tooltip("Speed of the typewriter effect (characters per second)")]
    public float typewriterSpeed = 30f;
    
    [Tooltip("Allow clicking to skip typewriter animation")]
    public bool allowSkip = true;

    private List<DialogueNode> dialogueNodes;
    private int currentNodeIndex = 0;
    private Coroutine typewriterCoroutine;
    private bool isTyping = false;
    private string currentFullText = "";
    private bool skipRequested = false;

    private void Update()
    {
        // Allow skipping typewriter effect with mouse click or space/enter
        if (allowSkip && isTyping && (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Return)))
        {
            skipRequested = true;
        }
    }

    public void StartDialogue(List<DialogueNode> nodes)
    {
        dialogueNodes = nodes;
        currentNodeIndex = 0;
        DisplayNode();
    }

    private void DisplayNode()
    {
        if (currentNodeIndex < 0 || currentNodeIndex >= dialogueNodes.Count) return;
        
        DialogueNode node = dialogueNodes[currentNodeIndex];
        characterNameText.text = node.characterName;
        
        // Stop any existing typewriter coroutine
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
        }
        
        // Clear choices until typing is complete
        foreach (Transform child in choicesContainer.transform)
        {
            Destroy(child.gameObject);
        }
        
        // Start typewriter effect
        currentFullText = node.dialogueText;
        skipRequested = false;
        typewriterCoroutine = StartCoroutine(TypewriterEffect(node));
    }
    
    private IEnumerator TypewriterEffect(DialogueNode node)
    {
        isTyping = true;
        dialogueText.text = "";
        
        for (int i = 0; i <= currentFullText.Length; i++)
        {
            if (skipRequested)
            {
                dialogueText.text = currentFullText;
                break;
            }
            
            dialogueText.text = currentFullText.Substring(0, i);
            yield return new WaitForSeconds(1f / typewriterSpeed);
        }
        
        isTyping = false;
        skipRequested = false;
        
        // Now create the choice buttons after typing is complete
        foreach (var choice in node.choices)
        {
            Button choiceButton = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            choiceButton.GetComponentInChildren<Text>().text = choice.choiceText;
            choiceButton.onClick.AddListener(() => SelectChoice(choice.nextNodeIndex));
        }

        if (node.choices.Count == 0)
        {
            StartCoroutine(HideDialogueAfterDelay(5f));
        }
    }

    public void SelectChoice(int nextNodeIndex)
    {
        if (nextNodeIndex >= 0 && nextNodeIndex < dialogueNodes.Count)
        {
            currentNodeIndex = nextNodeIndex;
            DisplayNode();
        }
        else
        {
            EndDialogue();
        }
    }

    public void EndDialogue()
    {
        // Stop typewriter coroutine if running
        if (typewriterCoroutine != null)
        {
            StopCoroutine(typewriterCoroutine);
            typewriterCoroutine = null;
        }
        
        isTyping = false;
        dialogueText.text = "";
        characterNameText.text = "";
        choicesContainer.SetActive(false);

        if (DialogueTextContainer != null)
        {
            DialogueTextContainer.SetActive(false);
        }
        
        if (xButton != null)
        {
            xButton.SetActive(false);
        }
    }

    private IEnumerator HideDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndDialogue();
    }
}