using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestDialogueFiles : MonoBehaviour
{
    void Start()
    {
        StartConversation();
    }

    void StartConversation()
    {
        List<string> lines = FileManager.ReadTextAsset("test1", false);

        foreach (string line in lines)
        {
            DIALOGUE.DIALOGUE_LINE parsedLine = DIALOGUE.DialogueParser.Parse(line);
            //Debug.Log($"Segmenting line: <{line}>");

            //for (int i = 0; i < parsedLine.dialogue.segments.Count; i++)
            //{
            //    Debug.Log($"Segment {i}: <{parsedLine.dialogue.segments[i].startSignal}:{parsedLine.dialogue.segments[i].signalDelay}> -- <{parsedLine.dialogue.segments[i].dialogue}> ");
            //}

            //Debug.Log($"Get speaker cast data from: {line}");
            //Debug.Log($"Name: {parsedLine.speaker.speakerName}\nAs: {parsedLine.speaker.speakerCastName}\nAt pos: {parsedLine.speaker.castPos.x}:{parsedLine.speaker.castPos.y}");
            //Debug.Log("------ EXPRESSIONS ------");
            //foreach (var expression in parsedLine.speaker.CastExpressions)
            //{
            //    Debug.Log($"Layer: {expression.layer} Expression: {expression.expression}");
            //}

            //Debug.Log($"Get commands data from: {line}");
            //if (parsedLine.commandsData == null)
            //    continue;
            //foreach(DL_COMMAND_DATA.Command command in parsedLine.commandsData.commands)
            //{
            //    Debug.Log($"------ Command: {command.name}");
            //    Debug.Log($"Arguments: {string.Join(", ", command.args)}");
            //    Debug.Log($"-------------------------");
            //}
        }

        DIALOGUE.DialogueSystem.Instance.Say(lines);
    }
}
