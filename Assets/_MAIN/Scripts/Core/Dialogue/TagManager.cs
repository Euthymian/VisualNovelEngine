using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text.RegularExpressions;
using System;
using System.Linq;
using VISUALNOVEL;

namespace DIALOGUE
{
    public class TagManager
    {
        private static readonly Dictionary<string, Func<string>> tags = new Dictionary<string, Func<string>>()
        {
            { "<mainChar>", () => VNGameSave.activeFile.playerName },
            { "<time>", () => DateTime.Now.ToString("hh:mm tt") },
            { "<playerLevel>", () => "15" },
            { "<tempVal1>", () => "42" },
            { "<input>", () => InputPanel.Instance.lastInput }
        };
        private static readonly Regex tagRegex = new Regex(@"<\w+>");

        public static string Inject(string text, bool injectTags = true, bool injectVariables = true)
        {
            if(injectTags)
                text = InjectTags(text);

            if (injectVariables)
                text = InjectVariable(text);

            return text;
        }

        private static string InjectTags(string value)
        {
            if (tagRegex.IsMatch(value))
            {
                foreach (Match match in tagRegex.Matches(value))
                {
                    if (tags.TryGetValue(match.Value, out var tagValueFunc))
                    {
                        value = value.Replace(match.Value, tagValueFunc());
                    }
                }
            }

            return value;
        }

        private static string InjectVariable(string value)
        {
            //Debug.Log(value);
            MatchCollection matches = Regex.Matches(value, VariableStore.REGEX_VARIABLE_IDS);
            List<Match> matchesList = matches.Cast<Match>().ToList();

            // Reverse the list to avoid index issues when remove old string then insert another with different length
            for (int i = matchesList.Count - 1; i >= 0; i--)
            {
                Match match = matchesList[i];
                string variableName = match.Value.TrimStart(VariableStore.VARIABLE_ID, '!');
                bool negate = match.Value.StartsWith('!');

                //If in our dialogue, our variable is last word of a sentence, it will end with a dot (.) - narrator "Hi $name."
                //Without this checking, the Variable store will treat '$name.' as variable with 'name' is database and '' is variable name
                bool endWithIllegalChar = variableName.EndsWith(VariableStore.DATABASE_VARIABLE_SEPARATOR);
                if (endWithIllegalChar)
                {
                    variableName = variableName.Substring(0, variableName.Length - 1);
                }

                if (!VariableStore.TryGetValue(variableName, out object variableValue))
                {
                    Debug.LogError($"Variable '{variableName}' does not exist.");
                    continue;
                }

                if(negate && variableValue is bool boolValue)
                {
                    variableValue = !boolValue;
                }

                int lengthToBeRemoved = match.Index + match.Length > value.Length ? value.Length - match.Index : match.Length;
                if (endWithIllegalChar)
                    lengthToBeRemoved -= 1;

                value = value.Remove(match.Index, lengthToBeRemoved);
                value = value.Insert(match.Index, variableValue.ToString());
            }

            return value;
        }
    }
}