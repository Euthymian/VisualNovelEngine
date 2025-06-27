using CHARACTER;
using COMMAND;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        private List<CoroutineWrapper> currentLineCommandList = new List<CoroutineWrapper>();

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
                {
                    // wait for user prompt to continue
                    yield return WaitForUserInput();

                    CommandManager.Instance.StopAllProcesses();
                }   
            }
        }

        IEnumerator HandleDialogueRun(DIALOGUE_LINE line)
        {
            if (line.hasSpeaker)
            {
                HandleSpeakerLogic(line.speakerData);
            }

            if(!DialogueSystem.Instance.dialogueContainer.isVisible)
                yield return DialogueSystem.Instance.dialogueContainer.Show();

            // current dialogue line finishes building here
            yield return BuildSegmentLines(line.dialogueData);
        }

        private void HandleSpeakerLogic(DL_SPEAKER_DATA speakerData)
        {
            bool characterMustBeCreated = speakerData.makeCharacterEnter || speakerData.hasCastingPosition || speakerData.hasCastingExpressions;

            Character character = CharacterManager.Instance.GetCharacter(speakerData.speakerName, createIfDoentExist: characterMustBeCreated);

            if (speakerData.makeCharacterEnter && !character.isVisible && !character.isShowing)
                    character.Show();

            DialogueSystem.Instance.ShowSpeakerName(speakerData.displayName);
            DialogueSystem.Instance.ApplySpeakerDataToDialogueContainer(speakerData.speakerName);

            if (speakerData.hasCastingPosition)
            {
                character.SetPosition(speakerData.castPos);
                //character.MoveToPosition(speakerData.castPos);
            }

            if (speakerData.hasCastingExpressions)
            {
                foreach (var ce in speakerData.CastExpressions)
                {
                    character.OnReceiveCastingExpression(ce.layer, ce.expression);
                }
            }

        }

        IEnumerator BuildSegmentLines(DL_DIALOGUE_DATA dialogueData)
        {
            for(int i = 0; i < dialogueData.segmentList.Count; i++)
            {
                DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment = dialogueData.segmentList[i];

                yield return WaitForSegmentSignalToBeTrigger(segment);

                yield return BuildDialogue(segment.dialogue, segment.append);
                //Debug.Log($"build <{segment.dialogue}>complete");
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

            if (!line.hasWaitlineKeyword)
            {
                foreach (DL_COMMAND_DATA.Command command in commands)
                {
                    if (command.waitForCompletion || command.name.Contains("wait"))
                    {
                        CoroutineWrapper coroutineWrapper = CommandManager.Instance.Execute(command.name, command.args);
                        coroutineWrapper.needWait = true;
                        while (!coroutineWrapper.IsDone)
                        {
                            if (userPrompted)
                            {
                                CommandManager.Instance.StopCurrentProcess();
                                userPrompted = false;
                            }
                            yield return null;
                        }
                    }
                    // By default, coroutineWrapper.needWait of commands that dont have [wait] or [waitline] will be set to false
                    else CommandManager.Instance.Execute(command.name, command.args); 
                }
            }
            else
            {
                foreach(DL_COMMAND_DATA.Command command in commands)
                {
                    CoroutineWrapper coroutineWrapper = CommandManager.Instance.Execute(command.name, command.args);
                    currentLineCommandList.Add(coroutineWrapper);
                }

                while(currentLineCommandList.Any(c => !c.IsDone))
                {
                    if (userPrompted)
                    {
                        int count = currentLineCommandList.Count(c => !c.IsDone);
                        //List<CoroutineWrapper> undoneCoroutineWrappers = currentLineCommandList.Where(c => !c.IsDone).ToList();
                        CommandManager.Instance.StopLatestProcesses(count);
                        userPrompted = false;
                    }
                    yield return null;
                }

                currentLineCommandList.Clear();
            }

            yield return null;
        }

        IEnumerator WaitForUserInput()
        {
            DialogueSystem.Instance.dialogueContinuePrompt.Show();

            while (!userPrompted)
                yield return null;

            DialogueSystem.Instance.dialogueContinuePrompt.Hide();
            
            userPrompted = false; 
        }
    }
}