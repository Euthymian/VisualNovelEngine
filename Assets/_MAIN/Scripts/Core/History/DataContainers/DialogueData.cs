using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HISTORY
{
    [System.Serializable]
    public class DialogueData
    {
        public string currentDialogue = "";
        public string currentSpeaker = "";

        public string dialogueFont;
        public Color dialogueColor;
        public float dialogueSize;

        public string speakerNameFont;
        public Color speakerNameColor;
        public float speakerNameSize;

        public static DialogueData Capture()
        {
            DialogueData data = new DialogueData();

            DialogueSystem dialogueSystem = DialogueSystem.Instance;
            TextMeshProUGUI dialogueText = dialogueSystem.dialogueContainer.dialogueText;
            TextMeshProUGUI speakerNameText = dialogueSystem.dialogueContainer.speakerNameContainer.nameText;

            data.currentDialogue = dialogueText.text;
            data.currentSpeaker = speakerNameText.text;

            data.dialogueFont = FilePaths.resources_font + dialogueText.font.name;
            data.dialogueColor = dialogueText.color;
            data.dialogueSize = dialogueText.fontSize;

            data.speakerNameFont = FilePaths.resources_font + speakerNameText.font.name;
            data.speakerNameColor = speakerNameText.color;
            data.speakerNameSize = speakerNameText.fontSize;

            return data;
        }

        public static void Apply(DialogueData data)
        {
            DialogueSystem dialogueSystem = DialogueSystem.Instance;
            TextMeshProUGUI dialogueText = dialogueSystem.dialogueContainer.dialogueText;
            TextMeshProUGUI speakerNameText = dialogueSystem.dialogueContainer.speakerNameContainer.nameText;

            //dialogueText.text = data.currentDialogue;
            dialogueSystem.conversationManager.textArchitect.SetText(data.currentDialogue);
            dialogueText.color = data.dialogueColor;
            dialogueText.fontSize = data.dialogueSize;
            
            speakerNameText.text = data.currentSpeaker;
            if (speakerNameText.text != string.Empty)
                dialogueSystem.dialogueContainer.speakerNameContainer.Show();
            else
                dialogueSystem.dialogueContainer.speakerNameContainer.Hide();
            speakerNameText.color = data.speakerNameColor;
            speakerNameText.fontSize = data.speakerNameSize;
            
            if(data.dialogueFont != dialogueText.font.name)
            {
                TMP_FontAsset font = HistoryCache.LoadFont(data.dialogueFont);
                if(font != null)
                    dialogueText.font = font;
                else
                    Debug.LogWarning($"History State: Could not load dialogue font '{data.dialogueFont}'");
            }

            if (data.speakerNameFont != speakerNameText.font.name)
            {
                TMP_FontAsset font = HistoryCache.LoadFont(data.dialogueFont);
                if (font != null)
                    speakerNameText.font = font;
                else
                    Debug.LogWarning($"History State: Could not load speaker name font '{data.speakerNameFont}'");
            }

            dialogueText.maxVisibleCharacters = data.currentDialogue.Length;
            dialogueText.ForceMeshUpdate();
        }
    }
}