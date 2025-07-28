#if UNITY_EDITOR
using CHARACTER;
using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using VISUALNOVEL;

public class TestDialogueFiles : MonoBehaviour
{
    [SerializeField] private TextAsset textAsset;

    void Start()
    {
        StartConversation();
    }

    void StartConversation()
    {
        string fullPath = AssetDatabase.GetAssetPath(textAsset);
        int resourceIndex = fullPath.IndexOf("Resources/");
        string relativePath = fullPath.Substring(resourceIndex + "Resources/".Length);
        string filePath = Path.ChangeExtension(relativePath, null);

        LoadFile(filePath);

        #region OldTest
        //List<string> lines = FileManager.ReadTextAsset(textAsset);
        ////List<string> lines = FileManager.ReadTextAsset("Dialogue Files/test1", false);

        //foreach (string line in lines)
        //{
        //    DIALOGUE.DIALOGUE_LINE parsedLine = DIALOGUE.DialogueParser.Parse(line);

        //    Debug.Log($"Segmenting line: <{line}>");

        //    Debug.Log($"Get speaker cast data");
        //    if (parsedLine.speakerData != null)
        //    {
        //        Debug.Log($"Name: {parsedLine.speakerData.speakerName}\nAs: {parsedLine.speakerData.speakerCastName}\nAt pos: {parsedLine.speakerData.castPos.x}:{parsedLine.speakerData.castPos.y}");
        //        Debug.Log("------ EXPRESSIONS ------");
        //        foreach (var expression in parsedLine.speakerData.CastExpressions)
        //        {
        //            Debug.Log($"Layer: {expression.layer} Expression: {expression.expression}");
        //        }
        //    }
        //    else Debug.Log("No speaker data found in this line.");

        //    Debug.Log($"Get dialogue data from");
        //    if (parsedLine.dialogueData != null)
        //    {
        //        for (int i = 0; i < parsedLine.dialogueData.segmentList.Count; i++)
        //        {
        //            Debug.Log($"Segment {i}: <{parsedLine.dialogueData.segmentList[i].startSignal}:{parsedLine.dialogueData.segmentList[i].signalDelay}> -- <{parsedLine.dialogueData.segmentList[i].dialogue}> ");
        //        }
        //    }
        //    else Debug.Log("No dialogue data found in this line.");

        //    Debug.Log($"Get commands data from");
        //    if (parsedLine.commandsData != null)
        //    {
        //        foreach (DIALOGUE.DL_COMMAND_DATA.Command command in parsedLine.commandsData.commands)
        //        {
        //            Debug.Log($"------ Command: {command.name}");
        //            Debug.Log($"Arguments: {string.Join(", ", command.args)}");
        //            Debug.Log($"-------------------------");
        //        }
        //    }
        //    else Debug.Log("No commands data found in this line.");
        //}

        //DIALOGUE.DialogueSystem.Instance.Say(lines);
        #endregion
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.DownArrow))
            DialogueSystem.Instance.dialogueContainer.Hide();
        else if (Input.GetKeyDown(KeyCode.UpArrow))
            DialogueSystem.Instance.dialogueContainer.Show();
    }



    public void LoadFile(string filePath)
    {
        List<string> lines = new List<string>();
        TextAsset file = Resources.Load<TextAsset>(filePath);

        try
        {
            lines = FileManager.ReadTextAsset(file);
        }
        catch
        {
            Debug.LogError($"Failed to load file: {filePath}. Not exist in the Resources folder.");
            return;
        }

        DialogueSystem.Instance.Say(lines, filePath);
    }
}
#endif