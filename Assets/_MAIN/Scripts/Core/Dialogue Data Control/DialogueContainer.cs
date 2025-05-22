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

        public void SetDialogueColor(Color color) => dialogueText.color = color;
        public void SetDialogueFont(TMP_FontAsset font) => dialogueText.font = font;
    }
}