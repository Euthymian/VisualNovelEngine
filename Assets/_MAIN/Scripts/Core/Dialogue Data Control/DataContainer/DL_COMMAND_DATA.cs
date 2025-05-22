using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace DIALOGUE
{
    public class DL_COMMAND_DATA
    {
        private const char COMMAND_SEPARATOR = ',';
        private const char ARGUMENTS_SEPARATOR = ' ';
        private const char ARGUMENTS_CONTAINER = '(';
        private const string WAIT_COMMAND_ID = "[wait]";

        public List<Command> commands;

        public struct Command
        {
            public string name;
            public string[] args;
            public bool waitForCompletion;
        }

        public DL_COMMAND_DATA(string rawCommands)
        {
            commands = RipCommands(rawCommands);
        }

        private List<Command> RipCommands(string rawCommands)
        {
            List<Command> res = new List<Command>();
            string[] rawCommandsArray = rawCommands.Split(COMMAND_SEPARATOR, System.StringSplitOptions.RemoveEmptyEntries); // There is seperator but no command following it: Command1(), 

            foreach (string each in rawCommandsArray)
            {
                Command command = new Command();
                int indexOfArgumentsContainer = each.IndexOf(ARGUMENTS_CONTAINER);
                command.name = each.Substring(0, indexOfArgumentsContainer).Trim();

                if (command.name.ToLower().StartsWith(WAIT_COMMAND_ID))
                {
                    command.waitForCompletion = true;
                    command.name = command.name.Substring(WAIT_COMMAND_ID.Length);
                }
                else
                    command.waitForCompletion = false;

                command.args = RipArgs(each.Substring(indexOfArgumentsContainer + 1, each.Length - (indexOfArgumentsContainer + 1) - 1)); // -1 because of the close brackets
                res.Add(command);
            }

            return res;
        }

        private string[] RipArgs(string argsRawString)
        {
            List<string> argsList = new List<string>();

            /* EXPLAIN:
            Iterate through the string, if we find a quote, we toggle the inQuote variable.
            If we find a space and we are not in a quote, we add the current argument to the list and clear it.
            If not both case above, we add the character to the current argument.

            We can declare an empty string here then add characters to it, but in the background, everytime we concatenate a string, 
            it creates a new string and copies the old one to the new one. We don't want that, so we use StringBuilder.
            StringBuilder is a mutable string, meaning we can change it without creating a new one.
            */

            StringBuilder currentArg = new StringBuilder();

            bool inQuote = false;

            for (int i = 0; i < argsRawString.Length; i++)
            {
                if (argsRawString[i] == '"')
                {
                    inQuote = !inQuote;
                    continue;
                }

                if (!inQuote && argsRawString[i] == ARGUMENTS_SEPARATOR)
                {
                    argsList.Add(currentArg.ToString());
                    currentArg.Clear();
                    continue;
                }

                currentArg.Append(argsRawString[i]);
            }
            if (currentArg.Length > 0)
                argsList.Add(currentArg.ToString());

            return argsList.ToArray();
        }
    }
}