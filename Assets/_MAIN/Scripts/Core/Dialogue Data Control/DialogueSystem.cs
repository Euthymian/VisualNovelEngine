using CHARACTER;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Control dialogue and conversations

namespace DIALOGUE
{
    public class DialogueSystem : MonoBehaviour
    {
        // if there are more than 1 line, the file content will be considered as a conversation even has only 1 speaker

        [SerializeField] private DialogueSystemConfigSO _dialogueSystemConfigSO;
        public DialogueSystemConfigSO dialogueSystemConfigSO => _dialogueSystemConfigSO;

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

        public void ApplySpeakerDataToDialogueContainer(string speakerName)
        {
            Character character = CharacterManager.Instance.GetCharacter(speakerName);
            CharacterConfigData characterConfigData = character != null ? character.configData : CharacterConfigData.Default;
            // CharacterConfigData characterConfigData = character != null ? character.configData : CharacterManager.Instance.GetCharacterConfigData(speakerName);
            // This is in video version. When create character, we already called DialogueSystem.Instance.dialogueSystemConfigSO.characterConfigSO.GetCharacterConfigData(characterName);
            // -> its not necessary to call it again 

            ApplySpeakerDataToDialogueContainer(characterConfigData);
        }

        public void ApplySpeakerDataToDialogueContainer(CharacterConfigData configData)
        {
            dialogueContainer.SetDialogueFont(configData.dialogueFont);
            dialogueContainer.SetDialogueColor(configData.dialogueColor);
            dialogueContainer.speakerNameContainer.SetNameFont(configData.nameFont);
            dialogueContainer.speakerNameContainer.SetNameColor(configData.nameColor);
        }

        // Reason of making 2 ApplySpeakerDataToDialogueContainer methods is if we have direct reference to characterConfigData, we can use it directly
        // else we need to search for the characterConfigData by name

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

        // Return Coroutine in Say method is good practice because anytime we want to add conversation, we dont need to manually check if
        // there any coroutine is running or not, we just need to yield return the coroutine 

        public Coroutine Say(string speaker, string dialogue)
        {
            List<string> conversation = new List<string>()
            {
                $"{speaker} \"{dialogue}\""
            };
            return Say(conversation);
        }

        public Coroutine Say(List<string> conversation)
        {
            return conversationManager.StartConversation(conversation);
        }
    }
}