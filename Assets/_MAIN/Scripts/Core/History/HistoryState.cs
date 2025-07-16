using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HISTORY
{
    [System.Serializable]
    public class HistoryState
    {
        public DialogueData dialogueData;
        public List<CharacterData> characterData;
        public List<GraphicData> graphicData;
        public List<AudioData> audioData;

        public static HistoryState Capture()
        {
            HistoryState state = new HistoryState();
            state.dialogueData = DialogueData.Capture();
            state.characterData = CharacterData.Capture();
            state.graphicData = GraphicData.Capture();
            state.audioData = AudioData.Capture();
            return state;
        }

        public void Load()
        {
            DialogueData.Apply(dialogueData);
            CharacterData.Apply(characterData);
            AudioData.Apply(audioData);
            GraphicData.Apply(graphicData);
        }
    }
}