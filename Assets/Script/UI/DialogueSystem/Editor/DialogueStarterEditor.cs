using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

#if UNITY_EDITOR
[CustomEditor(typeof(DialogueStarter))]
public class DialogueStarterEditor : Editor
{
    private DialogueStarter dialogueStarter;
    private string[] dialogueNames;
    
    private void OnEnable()
    {
        dialogueStarter = (DialogueStarter)target;
        UpdateDialogueNames();
    }
    
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Dialogue Selection Helper", EditorStyles.boldLabel);
        
        if (dialogueStarter.availableDialogues != null && dialogueStarter.availableDialogues.Count > 0)
        {
            UpdateDialogueNames();
            
            // Create dropdown for dialogue selection
            EditorGUI.BeginChangeCheck();
            int newIndex = EditorGUILayout.Popup("Select Dialogue", dialogueStarter.selectedDialogueIndex, dialogueNames);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(dialogueStarter, "Change Selected Dialogue");
                dialogueStarter.selectedDialogueIndex = newIndex;
                EditorUtility.SetDirty(dialogueStarter);
            }
            
            // Display current dialogue info
            if (newIndex < dialogueStarter.availableDialogues.Count && dialogueStarter.availableDialogues[newIndex] != null)
            {
                var currentDialogue = dialogueStarter.availableDialogues[newIndex];
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField($"Selected: {currentDialogue.dialogueName}", EditorStyles.helpBox);
                
                if (!string.IsNullOrEmpty(currentDialogue.description))
                {
                    EditorGUILayout.LabelField("Description:", EditorStyles.miniLabel);
                    EditorGUILayout.LabelField(currentDialogue.description, EditorStyles.wordWrappedMiniLabel);
                }
                
                EditorGUILayout.LabelField($"Nodes: {currentDialogue.dialogueNodes.Count}", EditorStyles.miniLabel);
            }
            
            EditorGUILayout.Space(10);
            
            // Quick add buttons
            EditorGUILayout.LabelField("Quick Actions", EditorStyles.boldLabel);
            
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Create New Dialogue Asset"))
            {
                CreateNewDialogueAsset();
            }
            
            if (GUILayout.Button("Refresh List"))
            {
                UpdateDialogueNames();
                Repaint();
            }
            EditorGUILayout.EndHorizontal();
            
            // Runtime testing buttons (only show when playing)
            if (Application.isPlaying)
            {
                EditorGUILayout.Space(5);
                EditorGUILayout.LabelField("Runtime Testing", EditorStyles.boldLabel);
                
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Start Current Dialogue"))
                {
                    // Use reflection to call the private StartDialogue method
                    var method = typeof(DialogueStarter).GetMethod("StartDialogue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    method.Invoke(dialogueStarter, null);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No dialogue assets assigned. Add DialogueData assets to the 'Available Dialogues' list.", MessageType.Info);
            
            if (GUILayout.Button("Create New Dialogue Asset"))
            {
                CreateNewDialogueAsset();
            }
        }
    }
    
    private void UpdateDialogueNames()
    {
        if (dialogueStarter.availableDialogues != null)
        {
            dialogueNames = new string[dialogueStarter.availableDialogues.Count];
            for (int i = 0; i < dialogueStarter.availableDialogues.Count; i++)
            {
                if (dialogueStarter.availableDialogues[i] != null)
                {
                    dialogueNames[i] = $"{i}: {dialogueStarter.availableDialogues[i].dialogueName}";
                }
                else
                {
                    dialogueNames[i] = $"{i}: [Missing Asset]";
                }
            }
        }
        else
        {
            dialogueNames = new string[0];
        }
    }
    
    private void CreateNewDialogueAsset()
    {
        DialogueData newDialogue = ScriptableObject.CreateInstance<DialogueData>();
        
        string path = EditorUtility.SaveFilePanelInProject(
            "Create New Dialogue Asset",
            "NewDialogue",
            "asset",
            "Choose where to save the new dialogue asset");
            
        if (!string.IsNullOrEmpty(path))
        {
            AssetDatabase.CreateAsset(newDialogue, path);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            // Select the new asset
            EditorUtility.FocusProjectWindow();
            Selection.activeObject = newDialogue;
        }
    }
}
#endif