using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VISUALNOVEL
{
    public class VNManager : MonoBehaviour
    {
        public static VNManager Instance { get; private set; }

        [SerializeField] private VisualNovelSO visualNovelSO;

        public Camera mainCamera;

        private void Awake()
        {
            Instance = this;

            VNDatabaseLinkSetup linkSetup = GetComponent<VNDatabaseLinkSetup>();
            linkSetup.SetupExternalLink();

            if(VNGameSave.activeFile == null)
                VNGameSave.activeFile = new VNGameSave();
        }

        private void Start()
        {
            LoadGame();
        }

        private void LoadGame()
        {
            if (VNGameSave.activeFile.newGame)
            {
                // Load the first chapter file
                List<string> lines = FileManager.ReadTextAsset(visualNovelSO.firstChapter);
                Conversation startConversation = new Conversation(lines, file:visualNovelSO.pathToFirstChapter);
                DialogueSystem.Instance.Say(startConversation);
            }
            else
            {
                VNGameSave.activeFile.Activate();
            }
        }
    }
}