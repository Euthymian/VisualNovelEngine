using CHARACTER;
using COMMAND;
using DIALOGUE.LogicalLines;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using UnityEngine.UIElements;

namespace DIALOGUE
{
    // Handles logic to run dialogue on screen 1 line a time
    public class ConversationManager
    {
        public TextArchitect textArchitect = null;

        private Coroutine process = null;
        public bool isRunning => process != null;

        private bool userPrompted = false;
        private List<CoroutineWrapper> currentLineCommandList = new List<CoroutineWrapper>();

        private TagManager tagManager;
        private LogicalLineManager logicalLineManager;

        public Conversation conversation => (conversationQueue.IsEmpty() ? null : conversationQueue.top);
        public int conversationProgress => (conversationQueue.IsEmpty() ? -1 : conversationQueue.top.GetProgress());
        private ConversationQueue conversationQueue;

        public ConversationManager(TextArchitect textArchitect)
        {
            this.textArchitect = textArchitect;
            DialogueSystem.Instance.onUserPrompt_Next += DialogueSystem_onUserPrompt_Next;

            tagManager = new TagManager();
            logicalLineManager = new LogicalLineManager();

            conversationQueue = new ConversationQueue();
        }

        public void Enqueue(Conversation conversation) => conversationQueue.Enqueue(conversation);
        public void EnqueuePriority(Conversation conversation) => conversationQueue.EnqueuePriority(conversation);

        private void DialogueSystem_onUserPrompt_Next()
        {
            userPrompted = true;
        }

        public Coroutine StartConversation(Conversation conversation)
        {
            StopConversation();

            Enqueue(conversation);

            process = DialogueSystem.Instance.StartCoroutine(RunningConversation());

            return process;
        }

        public void StopConversation()
        {

            if (!isRunning) return;

            DialogueSystem.Instance.StopCoroutine(process);
            process = null;
        }

        IEnumerator RunningConversation()
        {
            while (!conversationQueue.IsEmpty())
            {
                // Cache a reference to conversationQueue.top because the top may change during the process (add new priority conversation to the queue)
                Conversation currentConversation = conversation;

                if (currentConversation.HasReachedEnd())
                {
                    conversationQueue.Dequeue();
                    continue;
                }

                //Debug.Log($"Processing \n{conversation.GetRawLines()}\n at line {conversation.GetProgress()}\nQueue has {conversationQueue.conversationQueue.Count} conversations");
                string rawLine = currentConversation.CurrentLine();

                if (string.IsNullOrEmpty(rawLine))
                {
                    TryAdvanceConversation(currentConversation);
                    continue;
                }

                DIALOGUE_LINE line = DialogueParser.Parse(rawLine);
                //Debug.Log(line.dialogueData);

                if (logicalLineManager.TryGetLogic(line, out Coroutine logic))
                {
                    yield return logic;
                }
                else
                {
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

                TryAdvanceConversation(currentConversation);
            }

            process = null;
        }

        private void TryAdvanceConversation(Conversation conversation)
        {
            conversation.IncrementProgress();
            //Debug.Log($"Try incrementing conversation {conversation.GetRawLines()}");
            //Debug.Log($"Top queue before dequeue: {conversationQueue.top.GetRawLines()}");

            /*
            Reason for this logic:

            conversation.IncrementProgress();
            if (conversation.HasReachedEnd())
            {
                conversationQueue.Dequeue();
            }

            When reached to the end of current top conversation, before prompt to call TryAdvanceConversation(currentConversation), 
            we add new priority conversation to the queue.
            Then prompt so TryAdvanceConversation(currentConversation) will IncrementProgress() of old top queue (before add new priority conversation).
            Then check if the old top queue has reached end, if so, dequeue. But the conversation we are dequeuing is new priority conversation, not the old top queue.
            -> after dequeue, the old top queue is still there, but current process is now out of range
            -> That why this logic will help prevent the issue.

            There is another case that trigger bug:
            
            char1 "hey"
            choice "1+1=?"
            {
                -1
                char1 "so stupid"
                -2
                char1 "is it needed to be answer?"
            } <- file end here

            when conversationManager extracts the choice, it waits until player makes choice. after the choice is made, the progress will move to } then 
            EnqueuePriority() then TryAdvanceConversation(currentConversation) will be called and it will trigger the bug becuase } is end of the file.
            */



            //if (conversation.HasReachedEnd())
            //{
            //    Conversation tempTopQueue = conversationQueue.top;
            //    bool popTopTemporarily = false;
            //    if (conversation != tempTopQueue)
            //    {
            //        popTopTemporarily = true;
            //        conversationQueue.Dequeue();
            //    }

            //    conversationQueue.Dequeue();

            //    if (popTopTemporarily)
            //    {
            //        conversationQueue.EnqueuePriority(tempTopQueue);
            //    }
            //}


            // the approach works fine but we need to EnqueuePriority again -> may cause performance issue if the queue is large
            // -> instead, we do nothing then check in the conversation loop if the current top has reached end, remove and continue (line 78 -> 82)
            if (conversation != conversationQueue.top)
                return;

            if (conversation.HasReachedEnd())
                conversationQueue.Dequeue();

            //Debug.Log($"Top queue after dequeue: {conversationQueue.top.GetRawLines()}");
        }

        IEnumerator HandleDialogueRun(DIALOGUE_LINE line)
        {
            if (line.hasSpeaker)
            {
                HandleSpeakerLogic(line.speakerData);
            }

            if (!DialogueSystem.Instance.dialogueContainer.isVisible)
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

            DialogueSystem.Instance.ShowSpeakerName(tagManager.Inject(speakerData.displayName));
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
            for (int i = 0; i < dialogueData.segmentList.Count; i++)
            {
                DL_DIALOGUE_DATA.DIALOGUE_SEGMENT segment = dialogueData.segmentList[i];

                yield return WaitForSegmentSignalToBeTrigger(segment);

                yield return BuildDialogue(segment.dialogue, segment.append);
                //Debug.Log($"build <{segment.dialogue}>complete");
            }
        }

        public bool isWaitingForAutoTimer { get; private set; } = false;
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
                    isWaitingForAutoTimer = true;
                    yield return new WaitForSeconds(segment.signalDelay);
                    isWaitingForAutoTimer = false;
                    break;
            }
        }

        IEnumerator BuildDialogue(string dialogue, bool append)
        {
            dialogue = tagManager.Inject(dialogue);

            if (!append)
                textArchitect.Build(dialogue);
            else
                textArchitect.Append(dialogue);

            while (textArchitect.isBuilding)
            {
                if (userPrompted)
                {
                    if (!textArchitect.hurryUp) textArchitect.hurryUp = true;
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
                foreach (DL_COMMAND_DATA.Command command in commands)
                {
                    CoroutineWrapper coroutineWrapper = CommandManager.Instance.Execute(command.name, command.args);
                    currentLineCommandList.Add(coroutineWrapper);
                }

                while (currentLineCommandList.Any(c => !c.IsDone))
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