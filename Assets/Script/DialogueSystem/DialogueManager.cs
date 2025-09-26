using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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
    public TextMeshProUGUI dialogueText;
    public TextMeshProUGUI characterNameText;
    public GameObject choicesContainer;
    public Button choiceButtonPrefab;
    public GameObject panel1;
    public GameObject panel2;

    private List<DialogueNode> dialogueNodes;
    private int currentNodeIndex = 0;

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
        dialogueText.text = node.dialogueText;
        
        foreach (Transform child in choicesContainer.transform)
        {
            Destroy(child.gameObject);
        }

        foreach (var choice in node.choices)
        {
            Button choiceButton = Instantiate(choiceButtonPrefab, choicesContainer.transform);
            choiceButton.GetComponentInChildren<TextMeshProUGUI>().text = choice.choiceText;
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

    private void EndDialogue()
    {
        dialogueText.text = "";
        characterNameText.text = "";
        choicesContainer.SetActive(false);

        if (panel1 != null)
        {
            panel1.SetActive(false);
        }

        if (panel2 != null)
        {
            panel2.SetActive(false);
        }
    }

    private IEnumerator HideDialogueAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        EndDialogue();
    }
}