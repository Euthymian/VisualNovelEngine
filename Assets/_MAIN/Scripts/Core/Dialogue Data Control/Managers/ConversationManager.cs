using COMMAND;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;

namespace DIALOGUE
{
    // Handles logic to run dialogue on screen 1 line a time
    public class ConversationManager
    {     
        private TextArchitect textArchitect = null;

        private Coroutine process = null;
        public bool isRunning => process != null;

        private bool userPrompted = false;

        public ConversationManager(TextArchitect textArchitect)
        {
            this.textArchitect = textArchitect;
            DialogueSystem.Instance.onUserPrompt_SpeedUp += DialogueSystem_onUserPrompt_SpeedUp;
        }

        private void DialogueSystem_onUserPrompt_SpeedUp()
        {
            userPrompted = true;
        }

        public Coroutine StartConversation(List<string> conversation)
        {
            StopConversation();

            process = DialogueSystem.Instance.StartCoroutine(RunningConversation(conversation));

            return process;
        }

        public void StopConversation()
        {

            if (!isRunning) return;

            DialogueSystem.Instance.StopCoroutine(process);
            process = null;
        }

        IEnumerator RunningConversation(List<string> conversation)
        {
            for (int i = 0; i < conversation.Count; i++)
            {
                if (string.IsNullOrEmpty(conversation[i])) continue;

                DIALOGUE_LINE line = DialogueParser.Parse(conversation[i]);
                //Debug.Log(line.dialogueData);

                if (line.hasDialogue)
                    yield return HandleDialogueRun(line);

                if (line.hasCommands)
                    yield return HandleCommandRun(line);

                if (line.hasDialogue) // Only wait if there is a dialogue line to build, if only commands, skip this part
                    // wait for user prompt to continue
                    yield return WaitForUserInput();
            }
        }

        IEnumerator HandleDialogueRun(DIALOGUE_LINE line)
        {
            if(line.hasSpeaker)
                DialogueSystem.Instance.ShowSpeakerName(line.speakerData.displayName);

            // current dialogue line finishes building here
            yield return BuildSegmentLines(line.dialogueData);
        }

        IEnumerator BuildSegmentLines(DL_DIALOGUE_DATA dialogueData)
        {
            for(int i = 0; i < dialogueData.segments.Count; i++)
            {
                DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment = dialogueData.segments[i];

                yield return WaitForSegmentSignalToBeTrigger(segment);

                yield return BuildDialogue(segment.dialogue, segment.append);
            }
        }

        IEnumerator WaitForSegmentSignalToBeTrigger(DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment)
        {
            switch (segment.startSignal)
            {
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.NONE:
                    break;
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.C:
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.A:
                    yield return WaitForUserInput();
                    break;
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WC:
                case DL_DIALOGUE_DATA.DIALOGUE_SEGMENT.StartSignal.WA:
                    yield return new WaitForSeconds(segment.signalDelay);
                    break;
            }
        }

        IEnumerator BuildDialogue(string dialogue, bool append)
        {
            if (!append)
                textArchitect.Build(dialogue);
            else
                textArchitect.Append(dialogue);

            while (textArchitect.isBuilding)
            {
                if (userPrompted)
                {
                    if(!textArchitect.hurryUp) textArchitect.hurryUp = true;
                    else textArchitect.ForceComplete();

                    userPrompted = false;
                }
                yield return null;
            }
        }

        IEnumerator HandleCommandRun(DIALOGUE_LINE line)
        {
            List<DL_COMMAND_DATA.Command> commands = line.commandsData.commands;

            foreach(DL_COMMAND_DATA.Command command in commands)
            {
                if(command.waitForCompletion)
                {
                    yield return CommandManager.Instance.Execute(command.name, command.args);
                }
                else CommandManager.Instance.Execute(command.name, command.args);
            }

            yield return null;
        }

        IEnumerator WaitForUserInput()
        {
            while(!userPrompted)
                yield return null;

            userPrompted = false; 
        }
    }
}