using CHARACTER;
using TMPro;
using UnityEngine;

namespace DIALOGUE
{
    [CreateAssetMenu(fileName = "Dialogue System Config SO", menuName = "Dialogue System/Dialogue System Config SO")]
    public class DialogueSystemConfigSO : ScriptableObject
    {
        public CharacterConfigSO characterConfigSO;

        public TMP_FontAsset defaultFont;
        public Color defaultTextColor = Color.white;

        public float defaultNameFontSize = 40;
        public float defaultDialogueFontSize = 30f;
        public float dialogueFontScale = 1;
        public float nameFontScale = 1;
    }
}