using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Storage container for dialogue lines that has been parsed and ripped from the text file

namespace DIALOGUE
{
    public class DIALOGUE_LINE 
    {
        public string rawData { get; private set; } = string.Empty;

        public DL_SPEAKER_DATA speakerData;
        public DL_DIALOGUE_DATA dialogueData;
        public DL_COMMAND_DATA commandsData;

        public bool hasDialogue => dialogueData != null;
        public bool hasCommands => commandsData != null;
        public bool hasSpeaker => speakerData != null;

        public bool hasWaitlineKeyword = false; //wait for all commands on this line to finish before continuing

        public DIALOGUE_LINE(string rawLine, string speaker, string dialogue, string commands)
        {
            rawData = rawLine;

            speakerData = string.IsNullOrWhiteSpace(speaker) ? null : new DL_SPEAKER_DATA(speaker);
            dialogueData = string.IsNullOrWhiteSpace(dialogue) ? null : new DL_DIALOGUE_DATA(dialogue);
            commandsData = string.IsNullOrWhiteSpace(commands) ? null : new DL_COMMAND_DATA(commands); ;

            if(commandsData != null && commandsData.hasWaitlineKeyword)
                hasWaitlineKeyword = true;
        }
    }
}