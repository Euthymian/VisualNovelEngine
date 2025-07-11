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

            EncapsulatedData ifData = RipEncapsulationData(currentConversation, currentProgress, ripHeaderAndEncapsulators: false);
            EncapsulatedData elseData = new EncapsulatedData();

            if(ifData.endIndex + 1 < currentConversation.Count)
            {
                string nextLine = currentConversation.GetLines()[ifData.endIndex + 1].Trim();
                if(nextLine.StartsWith(ELSE))
                {
                    elseData = RipEncapsulationData(currentConversation, ifData.endIndex + 1, ripHeaderAndEncapsulators: false);
                    ifData.endIndex = elseData.endIndex;
                }
            }

            currentConversation.SetProgress(ifData.endIndex);

            EncapsulatedData selectedData = conditionResult ? ifData : elseData;
            if(!selectedData.isNull && selectedData.lines.Count > 0)
            {
                Conversation newConversation = new Conversation(selectedData.lines);
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