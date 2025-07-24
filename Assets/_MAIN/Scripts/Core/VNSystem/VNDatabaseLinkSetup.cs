using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VISUALNOVEL
{
    public class VNDatabaseLinkSetup : MonoBehaviour
    {
        public void SetupExternalLink()
        {
            VariableStore.CreateVariable("VN.mainCharName", "", () => VNGameSave.activeFile.playerName, value => VNGameSave.activeFile.playerName = value);
        }
    }
}