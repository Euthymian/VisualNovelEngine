using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;

namespace COMMAND
{

    // CMD means CommandDatabase 

    public abstract class CMD_Extension
    {
        public static void Extend(CommandDatabase commandDatabase) { }
    }
}