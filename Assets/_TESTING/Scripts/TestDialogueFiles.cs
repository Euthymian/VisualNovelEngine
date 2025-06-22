using CHARACTER;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDialogueFiles : MonoBehaviour
{
    [SerializeField] private TextAsset textAsset;

    void Start()
    {
        StartConversation();
    }

    void StartConversation()
    {
        //Character_Sprite raelin = CharacterManager.Instance.CreateCharacter("Raelin") as Character_Sprite;
        //Character_Sprite kyo = CharacterManager.Instance.CreateCharacter("KyoyaAkase") as Character_Sprite;
        //List<string> lines = FileManager.ReadTextAsset("Dialogue Files/test1", false);
        List<string> lines = FileManager.ReadTextAsset(textAsset, false);

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
        //        for (int i = 0; i < parsedLine.dialogueData.segments.Count; i++)
        //        {
        //            Debug.Log($"Segment {i}: <{parsedLine.dialogueData.segments[i].startSignal}:{parsedLine.dialogueData.segments[i].signalDelay}> -- <{parsedLine.dialogueData.segments[i].dialogue}> ");
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

        DIALOGUE.DialogueSystem.Instance.Say(lines);
    }
}
