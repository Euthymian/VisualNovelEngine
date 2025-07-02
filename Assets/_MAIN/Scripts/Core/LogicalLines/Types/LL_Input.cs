using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE.LogicalLines
{
    // Ctrl + . to auto-complete Interface
    public class LL_Input : ILogicalLine
    {
        public string keyword => "input";

        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            string title = line.dialogueData.rawData;
            InputPanel.Instance.Show(title);

            while (InputPanel.Instance.isWaitingOnUserInput)
                yield return null;
        }

        public bool Matches(DIALOGUE_LINE line)
        {
            return (line.hasSpeaker && line.speakerData.speakerName.ToLower() == keyword);
        }
    }
}