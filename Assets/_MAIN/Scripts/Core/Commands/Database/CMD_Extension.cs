using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COMMAND
{

    // CMD means CommandDatabase 

    public abstract class CMD_Extension
    {
        public static void Extend(CommandDatabase commandDatabase) { }

        public static CommandParameters ConvertDataToParameters(string[] data, int startingIndex = 0) => new CommandParameters(data, startingIndex);
    }
}