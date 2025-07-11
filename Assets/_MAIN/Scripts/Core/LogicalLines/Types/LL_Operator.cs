using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

using static DIALOGUE.LogicalLines.LogicalLineUtils.Expression;

namespace DIALOGUE.LogicalLines
{
    public class LL_Operator : ILogicalLine
    {
        public string keyword => throw new System.NotImplementedException();

        public IEnumerator Execute(DIALOGUE_LINE line)
        {
            //Debug.Log($"Detected operator line: {line.rawData}");
            string trimmedLine = line.rawData.Trim();
            string[] parts = Regex.Split(trimmedLine, REGEX_ARITHMATIC);

            if (parts.Length < 3)
            {
                Debug.LogError($"Invalid operator line: {trimmedLine}");
                yield break;
            }

            string variable = parts[0].Trim().TrimStart(VariableStore.VARIABLE_ID);
            string op = parts[1].Trim();
            string[] remainParts = new string[parts.Length - 2];
            Array.Copy(parts, 2, remainParts, 0, parts.Length - 2);

            object value = CalculateValue(remainParts);

            if (value == null)
                yield break;

            ProcessOperator(variable, op, value);
        }

        private void ProcessOperator(string variable, string op, object value)
        {
            if (VariableStore.TryGetValue(variable, out var currentValue))
            {
                ProcessOperatorOnVariable(variable, op, value, currentValue);
            }
            else if (op == "=")
            {
                VariableStore.CreateVariable(variable, value);
            }
        }

        private void ProcessOperatorOnVariable(string variable, string op, object value, object currentValue)
        {
            switch (op)
            {
                case "=":
                    VariableStore.TrySetValue(variable, value);
                    break;
                case "+=":
                    VariableStore.TrySetValue(variable, ConcatenateOrAdd(currentValue, value));
                    break;
                case "-=":
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) - Convert.ToDouble(value));
                    break;
                case "*=":
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) * Convert.ToDouble(value));
                    break;
                case "/=":
                    VariableStore.TrySetValue(variable, Convert.ToDouble(currentValue) / Convert.ToDouble(value));
                    break;
                default:
                    Debug.LogError($"Unknown operator '{op}'");
                    break;
            }
        }

        private object ConcatenateOrAdd(object currentValue, object value)
        {
            if (value is string)
                return currentValue.ToString() + value;

            return Convert.ToDouble(currentValue) + Convert.ToDouble(value);
        }

        public bool Matches(DIALOGUE_LINE line)
        {
            Match match = Regex.Match(line.rawData.Trim(), REGEX_OPERATOR_LINE);
            return match.Success;
        }
    }
}