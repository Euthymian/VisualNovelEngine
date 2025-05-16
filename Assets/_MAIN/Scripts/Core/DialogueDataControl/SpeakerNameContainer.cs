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
        [SerializeField] private TextMeshProUGUI nameText;

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