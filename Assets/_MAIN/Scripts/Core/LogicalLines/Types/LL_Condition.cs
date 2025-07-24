using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static DIALOGUE.LogicalLines.LogicalLineUtils.Encapsulation;
using static DIALOGUE.LogicalLines.LogicalLineUtils.Expression;
using static DIALOGUE.LogicalLines.LogicalLineUtils.Conditions;

namespace DIALOGUE.LogicalLines
{
    public class LL_Condition : ILogicalLine
    {
        private const string ELSE = "else";
        public string keyword => "if";
        private readonly string[] CONTAINERS = new string[] { "(", ")" };

        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            string rawCondition = ExtractCondition(line.rawData.Trim());

            bool conditionResult = EvaluateCondition(rawCondition);

            Conversation currentConversation = DialogueSystem.Instance.conversationManager.conversation;
            int currentProgress = DialogueSystem.Instance.conversationManager.conversationProgress;

            EncapsulatedData ifData = RipEncapsulationData(currentConversation, currentProgress, ripHeaderAndEncapsulators: false, parentStartingIndex: currentConversation.fileStartIndex);
            EncapsulatedData elseData = new EncapsulatedData();

            int nextPossibleElseIndex = ifData.endIndex + 1 - currentConversation.fileStartIndex;
            if (nextPossibleElseIndex < currentConversation.Count)
            {
                string nextLine = currentConversation.GetLines()[nextPossibleElseIndex].Trim();
                if(nextLine.StartsWith(ELSE))
                {
                    elseData = RipEncapsulationData(currentConversation, nextPossibleElseIndex, ripHeaderAndEncapsulators: false, parentStartingIndex: currentConversation.fileStartIndex);
                }
            }

            currentConversation.SetProgress(elseData.isNull ? currentProgress + ifData.endIndex - ifData.startIndex : currentProgress + elseData.endIndex - ifData.startIndex);

            EncapsulatedData selectedData = conditionResult ? ifData : elseData;
            if(!selectedData.isNull && selectedData.lines.Count > 0)
            {
                Conversation newConversation = new Conversation(selectedData.lines, file: currentConversation.file, fileStartIndex: selectedData.startIndex, fileEndIndex: selectedData.endIndex);
                DialogueSystem.Instance.conversationManager.EnqueuePriority(newConversation);
            }

            yield return null;
        }

        public bool Matches(DIALOGUE_LINE line)
        {
            return line.rawData.Trim().StartsWith(keyword);
        }

        private string ExtractCondition(string line)
        {
            int startIndex = line.IndexOf(CONTAINERS[0]) + 1;
            int endIndex = line.IndexOf(CONTAINERS[1]) - 1;

            return line.Substring(startIndex, endIndex - startIndex + 1).Trim();
        }
    }
}