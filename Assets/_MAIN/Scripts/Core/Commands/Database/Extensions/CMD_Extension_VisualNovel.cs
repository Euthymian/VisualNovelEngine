using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COMMAND
{
    public class CMD_Extension_VisualNovel : CMD_Extension
    {
        new public static void Extend(CommandDatabase cmdDatabase)
        {
            cmdDatabase.AddCommand("setplayername", new Action<string>(SetPlayerNameVariable));
        }

        private static void SetPlayerNameVariable(string data)
        {
            VISUALNOVEL.VNGameSave.activeFile.playerName = data;
        }
    }
}