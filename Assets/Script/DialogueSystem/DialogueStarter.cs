using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueStarter : MonoBehaviour
{
    public DialogueManager dialogueManager;
    public GameObject panel1;
    public GameObject panel2;
    public GameObject choicesContainer;
    public float interactionRange = 2f;
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
        List<DialogueNode> dialogueNodes = new List<DialogueNode>
        {
            // Node 0
            new DialogueNode
            {
                characterName = "John Business",
                dialogueText = "Hello good sir, quite the climb huh?",
                choices = new List<DialogueChoice>
                {
                    new DialogueChoice { choiceText = "Indeed, what are you doing here?", nextNodeIndex = 2 },
                    new DialogueChoice { choiceText = "JOHN BUSINESS?!?!?!!?!?", nextNodeIndex = 3 }
                }
            },

            // Node 1 (ending node)
            new DialogueNode
            {
                characterName = "John Business",
                dialogueText = "Business!",
                choices = new List<DialogueChoice>()
            },
            
            // Node 2
            new DialogueNode
            {
                characterName = "John Business",
                dialogueText = "Business of course!",
                choices = new List<DialogueChoice>
                {
                    new DialogueChoice { choiceText = "Fair enough", nextNodeIndex = 1 }
                }
            },

            // Node 3
            new DialogueNode
            {
                characterName = "John Business",
                dialogueText = "'Tis i, John Business! I am here to conduct business!",
                choices = new List<DialogueChoice>
                {
                    new DialogueChoice { choiceText = "Such as?", nextNodeIndex = 2 },
                    new DialogueChoice { choiceText = "HELL YEA", nextNodeIndex = 1 }
                }
            },
        };

        dialogueManager.StartDialogue(dialogueNodes);
        ActivatePanels();
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

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}