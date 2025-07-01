using DIALOGUE;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COMMAND
{
    public class CMD_Extension_General : CMD_Extension
    {
        private static string[] PARAM_IMMEDIATE => new string[] { "-i", "-immediate" };
        private static string[] PARAM_SPEED => new string[] { "-spd", "-speed" };

        new public static void Extend(CommandDatabase cmdDatabase)
        {
            cmdDatabase.AddCommand("wait", new Func<string, IEnumerator>(Wait));
            cmdDatabase.AddCommand("terminateallcommands", new Action(TerminateAllCommands));
            cmdDatabase.AddCommand("showdb", new Func<string[], IEnumerator>(ShowDialogueBox));
            cmdDatabase.AddCommand("hidedb", new Func<string[], IEnumerator>(HideDialogueBox));
            cmdDatabase.AddCommand("showui", new Func<string[], IEnumerator>(ShowDialogueSystem));
            cmdDatabase.AddCommand("hideui", new Func<string[], IEnumerator>(HideDialogueSystem));
        }

        private static IEnumerator Wait(string data)
        {
            if(float.TryParse(data, out float time))
                yield return new WaitForSeconds(time);
        }

        public static void TerminateAllCommands()
        {
            CommandManager.Instance.StopAllProcesses();
        }

        private static IEnumerator ShowDialogueBox(string[] data)
        {
            float speedMultiplier = 1f;
            bool immediate = false;

            CommandParameters parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, false);
            parameters.TryGetValue(PARAM_SPEED, out speedMultiplier, 1f);

            CommandManager.Instance.AddTerminationActionToCurrentProcess(() =>
            {
                DialogueSystem.Instance.dialogueContainer.Show(speedMultiplier, immediate:true);
            });
            yield return DialogueSystem.Instance.dialogueContainer.Show(speedMultiplier, immediate);
        }
        
        private static IEnumerator HideDialogueBox(string[] data)
        {
            float speedMultiplier = 1f;
            bool immediate = false;

            CommandParameters parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, false);
            parameters.TryGetValue(PARAM_SPEED, out speedMultiplier, 1f);

            CommandManager.Instance.AddTerminationActionToCurrentProcess(() =>
            {
                Debug.Log("terminate Hiding Dialogue Box");
                DialogueSystem.Instance.dialogueContainer.Hide(speedMultiplier, immediate: true);
            });
            yield return DialogueSystem.Instance.dialogueContainer.Hide(speedMultiplier, immediate);
        }

        private static IEnumerator ShowDialogueSystem(string[] data)
        {
            float speedMultiplier = 1f;
            bool immediate = false;

            CommandParameters parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, false);
            parameters.TryGetValue(PARAM_SPEED, out speedMultiplier, 1f);

            CommandManager.Instance.AddTerminationActionToCurrentProcess(() =>
            {
                DialogueSystem.Instance.Show(speedMultiplier, immediate);
            });
            yield return DialogueSystem.Instance.Show(speedMultiplier, immediate: true);
        }
        
        private static IEnumerator HideDialogueSystem(string[] data)
        {
            float speedMultiplier = 1f;
            bool immediate = false;

            CommandParameters parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, false);
            parameters.TryGetValue(PARAM_SPEED, out speedMultiplier, 1f);

            CommandManager.Instance.AddTerminationActionToCurrentProcess(() =>
            {
                DialogueSystem.Instance.Hide(speedMultiplier, immediate);
            });
            yield return DialogueSystem.Instance.Hide(speedMultiplier, immediate: true);
        }
    }
}