#if UNITY_EDITOR
using DIALOGUE;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TESTING
{
    public class TestGeneral : MonoBehaviour
    {
        [SerializeField] TextAsset file;

        private void Start()
        {
            LoadFile();
        }

        private void LoadFile()
        {
            List<string> lines = FileManager.ReadTextAsset(file);
            Conversation conversation = new Conversation(lines, file: file.name);
            DialogueSystem.Instance.Say(conversation);

        }
    }
}
#endif