using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

using static DIALOGUE.LogicalLines.LogicalLineUtils.Encapsulation;

namespace DIALOGUE.LogicalLines
{
    public class LL_Choice : ILogicalLine
    {
        public string keyword => "choice";
        private const char CHOICE_IDENTIFIER = '-';

        private struct Choice
        {
            public string title;
            public List<string> resultLines;
        }

        private bool IsChoiceStart(string line) => line.Trim().StartsWith(CHOICE_IDENTIFIER);

        private void AddLineToResults(string line, ref Choice choice, ref int encapsulationLevel)
        {
            line.Trim();

            if (IsEncapslationStart(line))
            {
                if (encapsulationLevel > 0)
                    choice.resultLines.Add(line);
                encapsulationLevel++;
                return;
            }

            if (IsEncapslationEnd(line))
            {
                encapsulationLevel--;
                if (encapsulationLevel > 0)
                    choice.resultLines.Add(line);
                return;
            }

            choice.resultLines.Add(line);
        }

        private List<Choice> GetChoiceFromData(EncapsulatedData rawChoiceData)
        {
            List<Choice> choices = new List<Choice>();
            int encapsulationLevel = 0;

            Choice choice = new Choice { title = "", resultLines = new List<string>() };

            bool isFirstChoice = true;
            foreach (string line in rawChoiceData.lines.Skip(1)) //Skip 1 is because first line is title line of the choice
            {
                if (IsChoiceStart(line) && encapsulationLevel == 1)
                {
                    if (!isFirstChoice)
                    {
                        choices.Add(choice);
                        choice = new Choice { title = "", resultLines = new List<string>() };
                    }

                    isFirstChoice = false;

                    choice.title = line.Trim().Substring(1); // Remove the leading '-' 
                    continue;
                }

                AddLineToResults(line, ref choice, ref encapsulationLevel);
            }

            if (!choices.Contains(choice))
                choices.Add(choice);

            return choices;
        }

        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            Conversation currentConversation = DialogueSystem.Instance.conversationManager.conversation;
            int currentProgress = DialogueSystem.Instance.conversationManager.conversationProgress;

            EncapsulatedData data = RipEncapsulationData(currentConversation, currentProgress, ripHeaderAndEncapsulators: true); // We want to rip header which containing choice title

            List<Choice> choiceList = GetChoiceFromData(data);

            string title = line.dialogueData.rawData;
            //string title = data.lines[0].Trim(); 
            string[] choiceTitles = choiceList.Select(c => c.title).ToArray();

            ChoicePanel.Instance.Show(title, choiceTitles);

            while(ChoicePanel.Instance.isWaitingOnUserChoice)
                yield return null;
            
            Choice selectedChoice = choiceList[ChoicePanel.Instance.lastDecision.answerIndex];

            Conversation conversationFromChoice = new Conversation(selectedChoice.resultLines);
            DialogueSystem.Instance.conversationManager.conversation.SetProgress(data.endIndex); //+1?
            DialogueSystem.Instance.conversationManager.EnqueuePriority(conversationFromChoice);
        }

        public bool Matches(DIALOGUE_LINE line)
        {
            return (line.hasSpeaker && line.speakerData.speakerName.ToLower() == keyword);
        }
    }
}