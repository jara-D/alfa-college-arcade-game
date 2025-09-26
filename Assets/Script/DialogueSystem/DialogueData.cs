using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Dialogue Data", menuName = "Dialogue System/Dialogue Data")]
public class DialogueData : ScriptableObject
{
    [Header("Dialogue Information")]
    public string dialogueName = "New Dialogue";
    
    [TextArea(2, 4)]
    public string description = "Description of this dialogue";
    
    [Header("Dialogue Nodes")]
    public List<DialogueNode> dialogueNodes = new List<DialogueNode>();
}