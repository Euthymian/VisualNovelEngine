using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DIALOGUE
{

    //Handle visibility of the speaker name and other logics in the dialogue system

    [System.Serializable]
    public class SpeakerNameContainer
    {
        [SerializeField] private GameObject root;
        [field:SerializeField] public TextMeshProUGUI nameText { get; private set; } // This property is editable in the inspector but privately assignable

        public void SetNameColor(Color color) => nameText.color = color;
        public void SetNameFont(TMP_FontAsset font) => nameText.font = font;
        public void SetNameFontSize(float fontSize) => nameText.fontSize = fontSize;

        public void Hide()
        {
            root.SetActive(false);
        }

        public void Show(string name = "")
        {
            root.SetActive(true);

            if (name != string.Empty)
            {
                nameText.text = name;
            }
        }
    }
}