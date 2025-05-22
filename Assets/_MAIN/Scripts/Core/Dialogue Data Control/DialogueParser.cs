using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

// Parsing system convert raw text data into DIALOGUE_LINE

namespace DIALOGUE
{
    // Formula of a line from file: Speaker "Dialogue" Command("arg1", "arg2")

    public class DialogueParser
    {
        //private const string commandRegexPattern = "\\w*[^\\s]\\(";
        private const string COMMAND_REGEX_PATTERN = @"[\w\[\]]*[^\s]\("; // @ makes character string literal
        // \w* looks for any word character (alphanumeric & underscore) 0 or more times
        // but [^\\s] will only match if the next character is not a whitespace character
        // \( looks for a literal '(' character

        public static DIALOGUE_LINE Parse(string rawLine)
        {
            //Debug.Log($"Parsing line: '{rawLine}'");

            (string speaker, string dialogue, string commands) = RipContent(rawLine);

            //Debug.Log($"Speaker: <{speaker}>\nDialogue: <{dialogue}>\nCommands: <{commands}>");

            return new DIALOGUE_LINE(speaker, dialogue, commands);
        }

        private static (string, string, string) RipContent(string rawLine)
        {
            string speaker = "";
            string dialogue = "";
            string commands = "";

            int startDialogueIndex = -1;
            int endDialogueIndex = -1;
            bool hasEscape = false;

            for (int i = 0; i < rawLine.Length; i++)
            {
                char cur = rawLine[i];

                if (cur == '\\') hasEscape = !hasEscape;
                else if (cur == '\"' && !hasEscape)
                {
                    if (startDialogueIndex == -1)
                        startDialogueIndex = i;
                    else if (endDialogueIndex == -1)
                        endDialogueIndex = i;
                }
                else hasEscape = false;
            }

            //Debug.Log(rawLine.Substring(startDialogueIndex + 1, endDialogueIndex - startDialogueIndex - 1));

            // Regex command 
            Regex regex = new Regex(COMMAND_REGEX_PATTERN);

            /*
            Check for MatchCollection is because we are looking for command( pattern. but what if we have this pattern in the dialogue?
            Example: Alex "command("Hello World")"
            we want to ignore this case and only get the command pattern that fit the dialogue line convention
            */

            MatchCollection matches = regex.Matches(rawLine);
            int startCommandIndex = -1;
            foreach (Match match in matches)
            {
                if (match.Index < startDialogueIndex || match.Index > endDialogueIndex)
                {
                    startCommandIndex = match.Index;
                    break;
                }
            }

            if (startDialogueIndex == -1 && startCommandIndex != -1)
                return (rawLine.Substring(0, startCommandIndex).Trim(), "", rawLine.Substring(startCommandIndex).Trim()); //Removes all leading and trailing white-space characters from the current string.

            //There is a case that argumenmts we passed ,to command has dialogue pattern: Command("Hello \"World\"")
            //Handle this case by checking index value
            if (startDialogueIndex != -1 && endDialogueIndex != -1 && (startCommandIndex == -1 || startCommandIndex > endDialogueIndex))
            {
                //Actual dialogue
                speaker = rawLine.Substring(0, startDialogueIndex).Trim();
                dialogue = rawLine.Substring(startDialogueIndex + 1, endDialogueIndex - startDialogueIndex - 1).Replace("\\\"", "\"");
                if (startCommandIndex != -1)
                    commands = rawLine.Substring(startCommandIndex).Trim();
            }
            else if (startCommandIndex != -1 && startDialogueIndex > startCommandIndex)
            {
                //Dialogue is argument of command
                speaker = rawLine.Substring(0, startCommandIndex).Trim();
                commands = rawLine.Substring(startCommandIndex).Trim();
            }
            else
            {
                speaker = rawLine;
            }


            return (speaker, dialogue, commands);
        }
    }
}