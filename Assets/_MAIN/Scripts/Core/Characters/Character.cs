using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CHARACTER
{

    // Base class for all characters which all types of characters will inherit from

    public abstract class Character 
    {
        public string name = "";
        public string displayName = "";
        public RectTransform root = null;
        public CharacterConfigData configData;

        public Character(string name, CharacterConfigData configData)
        {
            this.name = name;
            displayName = name;
            this.configData = configData;
        }

        // This Say function will be used for a case when we create a simple temporary character and doesnt actually have a dialogue file to
        // load by DialogueSystem. The Say method belongs to character class itself make it easier to call

        public Coroutine Say(string dialouge) => Say(new List<string>() { dialouge });

        public Coroutine Say(List<string> dialouges)
        {
            DialogueSystem.Instance.ShowSpeakerName(displayName);
            UpdateTextCustomizationOnScreen();
            return DialogueSystem.Instance.Say(dialouges);
        }

        public void UpdateTextCustomizationOnScreen()
        {
            DialogueSystem.Instance.ApplySpeakerDataToDialogueContainer(configData);
        }

        public void SetNameColor(Color color) => configData.nameColor = color;
        public void SetDialogueColor(Color color) => configData.dialogueColor = color;
        public void SetNameFont(TMP_FontAsset font) => configData.nameFont = font;
        public void SetDialogueFont(TMP_FontAsset font) => configData.dialogueFont = font;
        public void ResetCharacterDefaultConfig() => configData = CharacterManager.Instance.GetCharacterConfigData(name);

        public enum CharacterType
        {
            Text,
            Sprite,
            SpriteSheet,
            Live2D,
            Model3D
        }
    }
}