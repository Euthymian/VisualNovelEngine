using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace DIALOGUE.LogicalLines
{
    public class LL_Choice : ILogicalLine
    {
        public string keyword => "choice";
        private const char ENCAPSULATION_START = '{';
        private const char ENCAPSULATION_END = '}';
        private const char CHOICE_IDENTIFIER = '-';

        private struct RawChoiceData
        {
            public List<string> lines;
            public int endIndex;
        }

        private struct Choice
        {
            public string title;
            public List<string> resultLines;
        }

        private RawChoiceData RipChoiceData()
        {
            Conversation currentConversation = DialogueSystem.Instance.conversationManager.conversation;
            int currentProgress = DialogueSystem.Instance.conversationManager.conversationProgress;
            int encapsulationLevel = 0;
            RawChoiceData data = new RawChoiceData { lines = new List<string>(), endIndex = 0 };

            for (int i = currentProgress; i < currentConversation.Count; i++)
            {
                string line = currentConversation.GetLines()[i];
                data.lines.Add(line);

                if (IsEncapslationStart(line))
                {
                    encapsulationLevel++;
                    continue;
                }

                if (IsEncapslationEnd(line))
                {
                    encapsulationLevel--;
                    if (encapsulationLevel == 0)
                    {
                        data.endIndex = i;
                        break;
                    }
                }
            }

            return data;
        }

        private bool IsEncapslationStart(string line) => line.Trim().StartsWith(ENCAPSULATION_START);
        private bool IsEncapslationEnd(string line) => line.Trim().StartsWith(ENCAPSULATION_END);
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
                if (encapsulationLevel == 0)
                    choice.resultLines.Add(line);
                return;
            }

            choice.resultLines.Add(line);
        }

        private List<Choice> GetChoiceFromData(RawChoiceData rawChoiceData)
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
            RawChoiceData rawChoiceData = RipChoiceData();

            List<Choice> choiceList = GetChoiceFromData(rawChoiceData);

            string title = line.dialogueData.rawData;
            string[] choiceTitles = choiceList.Select(c => c.title).ToArray();

            ChoicePanel.Instance.Show(title, choiceTitles);

            while(ChoicePanel.Instance.isWaitingOnUserChoice)
                yield return null;
            
            Choice selectedChoice = choiceList[ChoicePanel.Instance.lastDecision.answerIndex];

            Conversation conversationFromChoice = new Conversation(selectedChoice.resultLines);
            DialogueSystem.Instance.conversationManager.conversation.SetProgress(rawChoiceData.endIndex); //+1?
            DialogueSystem.Instance.conversationManager.EnqueuePriority(conversationFromChoice);
        }

        public bool Matches(DIALOGUE_LINE line)
        {
            return (line.hasSpeaker && line.speakerData.speakerName.ToLower() == keyword);
        }
    }
}