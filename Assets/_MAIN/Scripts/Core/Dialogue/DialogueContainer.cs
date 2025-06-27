using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Rendering;

// Graphical display with all dialogue boxes

namespace DIALOGUE
{
    [System.Serializable]
    public class DialogueContainer
    {
        public GameObject rootContainer;
        public SpeakerNameContainer speakerNameContainer;
        public TextMeshProUGUI dialogueText;

        private CanvasGroupController canvasGroupController;

        bool initialized = false;

        public void SetDialogueColor(Color color) => dialogueText.color = color;
        public void SetDialogueFont(TMP_FontAsset font) => dialogueText.font = font;
        public void SetDialogueFontSize(float fontSize) => dialogueText.fontSize = fontSize;
    
        public void Initialize()
        {
            if(initialized)
                return;

            initialized = true;

            canvasGroupController = new CanvasGroupController(DialogueSystem.Instance, rootContainer.GetComponent<CanvasGroup>());
        }

        public bool isVisible => canvasGroupController.isVisible;
        public Coroutine Show(float speedMultiplier = 1, bool immediate = false) => canvasGroupController.Show(speedMultiplier, immediate);
        public Coroutine Hide(float speedMultiplier = 1, bool immediate = false) => canvasGroupController.Hide(speedMultiplier, immediate);
    }
}