using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// Graphical display with all dialogue boxes

namespace DIALOGUE
{
    [System.Serializable]
    public class DialogueContainer
    {
        public GameObject rootContainer;
        public SpeakerNameContainer speakerNameContainer;
        public TextMeshProUGUI dialogueText;
    }
}