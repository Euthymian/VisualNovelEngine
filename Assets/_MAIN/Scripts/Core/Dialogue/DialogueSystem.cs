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
        public ConversationManager conversationManager { get; private set; }
        public TextArchitect textArchitect;
        private AutoReader autoReader;

        [SerializeField] private CanvasGroup mainCanvasCG;
        private CanvasGroupController canvasGroupController;

        public delegate void DialogueSystemEvent();
        public event DialogueSystemEvent onUserPrompt_Next;

        //everytime dialogue is cleared (have a new line), this event will be invoked
        public event DialogueSystemEvent onClear;

        public DialogueContinuePrompt dialogueContinuePrompt;

        public void OnUserPrompt_Next()
        {
            onUserPrompt_Next?.Invoke();

            if (autoReader != null && autoReader.isOn)
                autoReader.Disable(); // Disable auto reader when user prompt next, so that user can take control of the dialogue flow
        }

        public void OnSystemPrompt_Next()
        {
            onUserPrompt_Next?.Invoke();
        }

        public void OnSystemPrompt_Clear()
        {
            onClear?.Invoke();
        }

        public void OnStartViewingHistory()
        {
            dialogueContinuePrompt.Hide();

            conversationManager.allowUserPrompt = false;

            autoReader.allowToggle = false;
            if(autoReader.isOn)
                autoReader.Disable();
        }

        public void OnStopViewingHistory()
        {
            dialogueContinuePrompt.Show();
            autoReader.allowToggle = true;
            conversationManager.allowUserPrompt = true;
        }

        public void ApplySpeakerDataToDialogueContainer(string speakerName)
        {
            Character character = CharacterManager.Instance.GetCharacter(speakerName);

            CharacterConfigData characterConfigData = character != null ? character.configData : CharacterManager.Instance.GetCharacterConfigData(speakerName);
            // When create character, we already called DialogueSystem.Instance.dialogueSystemConfigSO.characterConfigSO.GetCharacterConfigData(characterName);
            // but we still need to call it here beacuase there is a case that we have a character but we dont want to create it,
            // we just want to use the character config data directly

            ApplySpeakerDataToDialogueContainer(characterConfigData);
        }

        public void ApplySpeakerDataToDialogueContainer(CharacterConfigData configData)
        {
            dialogueContainer.SetDialogueFont(configData.dialogueFont);
            dialogueContainer.SetDialogueColor(configData.dialogueColor);
            dialogueContainer.SetDialogueFontSize(configData.dialogueFontSize * dialogueSystemConfigSO.dialogueFontScale);
            dialogueContainer.speakerNameContainer.SetNameFont(configData.nameFont);
            dialogueContainer.speakerNameContainer.SetNameColor(configData.nameColor);
            dialogueContainer.speakerNameContainer.SetNameFontSize(configData.nameFontSize * dialogueSystemConfigSO.nameFontScale);
        }

        // Reason of making 2 ApplySpeakerDataToDialogueContainer methods is if we have direct reference to characterConfigData, we can use it directly
        // else we need to search for the characterConfigData by name

        public void ShowSpeakerName(string speakerName = "")
        {
            if(speakerName.ToLower() != "narrator")
                dialogueContainer.speakerNameContainer.Show(speakerName);
            else
            {
                dialogueContainer.speakerNameContainer.Hide();
                dialogueContainer.speakerNameContainer.nameText.text = "";
            }
        }


        public bool isConversationRunning => conversationManager.isRunning;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                //DontDestroyOnLoad(gameObject);
                Initialize();
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
            conversationManager = new ConversationManager(textArchitect);
            canvasGroupController = new CanvasGroupController(this, mainCanvasCG);
            dialogueContainer.Initialize();

            if(TryGetComponent(out autoReader))
                autoReader.Initialize(conversationManager);
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

        public Coroutine Say(List<string> lines, string filePath = "")
        {
            Conversation conversation = new Conversation(lines, file: filePath);
            return conversationManager.StartConversation(conversation);
        }
        
        public Coroutine Say(Conversation conversation)
        {
            return conversationManager.StartConversation(conversation);
        }

        public Coroutine Show(float speedMultiplier = 1, bool immediate = false) => canvasGroupController.Show(speedMultiplier, immediate);
        
        public Coroutine Hide(float speedMultiplier = 1, bool immediate = false) => canvasGroupController.Hide(speedMultiplier, immediate);

        public bool isVisible => canvasGroupController.isVisible;
    }
}