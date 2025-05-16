using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Control dialogue and conversations

namespace DIALOGUE
{
    public class DialogueSystem : MonoBehaviour
    {
        // if there are more than 1 line, the file content will be considered as a conversation even has only 1 speaker

        public static DialogueSystem Instance { get; private set; }

        public DialogueContainer dialogueContainer = new DialogueContainer();
        public ConversationManager conversationManager;
        public TextArchitect textArchitect;

        public delegate void DialogueSystemEvent();
        public event DialogueSystemEvent onUserPrompt_SpeedUp;

        public void OnUserPrompt_SpeedUp()
        {
            onUserPrompt_SpeedUp?.Invoke();
        }

        public void ShowSpeakerName(string speakerName = "")
        {
            if(speakerName.ToLower() != "narrator")
                dialogueContainer.speakerNameContainer.Show(speakerName);
            else
                dialogueContainer.speakerNameContainer.Hide();
        }


        public bool isConversationRunning => conversationManager.isRunning;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                //DontDestroyOnLoad(gameObject);
                Initialize();

                conversationManager = new ConversationManager(textArchitect);
            }
            else
            {
                DestroyImmediate(gameObject);
            }
        }

        bool initialized = false;
        private void Initialize()
        {
            if (initialized) return;

            initialized = true;
            textArchitect = new TextArchitect(dialogueContainer.dialogueText);
        }

        public void Say(string speaker, string dialogue)
        {
            List<string> conversation = new List<string>()
            {
                $"{speaker} \"{dialogue}\""
            };
            Say(conversation);
        }

        public void Say(List<string> conversation)
        {
            conversationManager.StartConversation(conversation);
        }
    }
}