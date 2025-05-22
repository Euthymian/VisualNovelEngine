using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CHARACTER
{

    // Data container defining configurations params
    [System.Serializable]
    public class CharacterConfigData
    {
        public string name;
        public string alias; // This is an alias for the character name in case the name is too long

        public Character.CharacterType characterType;

        public Color nameColor;
        public Color dialogueColor;

        public TMP_FontAsset nameFont;
        public TMP_FontAsset dialogueFont;

        public CharacterConfigData Copy()
        {
            CharacterConfigData copy = new CharacterConfigData();
            copy.name = name;
            copy.alias = alias;
            copy.characterType = characterType;
            copy.nameFont = nameFont;
            copy.dialogueFont = dialogueFont;

            // We cant directly copy the colors will reference original value when we do copy.nameColor = nameColor;
            // So we need to create a new color with the same values
            copy.nameColor = new Color(nameColor.r, nameColor.g, nameColor.b, nameColor.a);
            copy.dialogueColor = new Color(dialogueColor.r, dialogueColor.g, dialogueColor.b, dialogueColor.a);
            return copy;
        }

        private static Color defaultColor => DialogueSystem.Instance.dialogueSystemConfigSO.defaultTextColor;
        private static TMP_FontAsset defaultFont => DialogueSystem.Instance.dialogueSystemConfigSO.defaultFont;

        public static CharacterConfigData Default
        {
            get
            {
                CharacterConfigData defaultConfig = new CharacterConfigData();
                defaultConfig.name = "Default";
                defaultConfig.alias = "Default";
                defaultConfig.characterType = Character.CharacterType.Text;
                defaultConfig.nameColor = defaultColor;
                defaultConfig.dialogueColor = defaultColor;
                defaultConfig.nameFont = defaultFont;
                defaultConfig.dialogueFont = defaultFont;
                return defaultConfig;
            }
        }
    }
}