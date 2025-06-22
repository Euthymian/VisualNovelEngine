using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COMMAND
{
    public class CMD_Extension_General : CMD_Extension
    {
        new public static void Extend(CommandDatabase cmdDatabase)
        {
            cmdDatabase.AddCommand("wait", new Func<string, IEnumerator>(Wait));
        }

        private static IEnumerator Wait(string data)
        {
            if(float.TryParse(data, out float time))
                yield return new WaitForSeconds(time);
        }
    }
}