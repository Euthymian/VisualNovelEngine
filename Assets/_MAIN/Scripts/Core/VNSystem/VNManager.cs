using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VISUALNOVEL
{
    public class VNManager : MonoBehaviour
    {
        public static VNManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;

            VNDatabaseLinkSetup linkSetup = GetComponent<VNDatabaseLinkSetup>();
            linkSetup.SetupExternalLink();
        }

        public void LoadFile(string filePath)
        {
            List<string> lines = new List<string>();
            TextAsset file = Resources.Load<TextAsset>(filePath);

            try
            {
                lines = FileManager.ReadTextAsset(file);
            }
            catch
            {
                Debug.LogError($"Failed to load file: {filePath}. Not exist in the Resources folder.");
                return;
            }

            DialogueSystem.Instance.Say(lines, filePath);
        }
    }
}