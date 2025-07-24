using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

namespace DIALOGUE.LogicalLines
{
    //Make static class is because we just need to add using DIALOGUE.LogicalLines.LogicalLineUtils.Encapsulation; to use all its fields and methods directly
    //See LL_Choice, LL_Operator and LL_Condition for examples of usage
    public static class LogicalLineUtils
    {
        public static class Encapsulation
        {
            public struct EncapsulatedData
            {
                public bool isNull => lines == null;
                public List<string> lines;
                public int startIndex;
                public int endIndex;
            }

            private const char ENCAPSULATION_START = '{';
            private const char ENCAPSULATION_END = '}';

            public static bool IsEncapslationStart(string line) => line.Trim().StartsWith(ENCAPSULATION_START);
            public static bool IsEncapslationEnd(string line) => line.Trim().StartsWith(ENCAPSULATION_END); 
            
            public static EncapsulatedData RipEncapsulationData(Conversation conversation, int startIndex, bool ripHeaderAndEncapsulators = false, int parentStartingIndex = 0)
            {
                int encapsulationLevel = 0;
                EncapsulatedData data = new EncapsulatedData { lines = new List<string>(), startIndex = startIndex + parentStartingIndex, endIndex = 0 };
                bool firstLine = true;

                for (int i = startIndex; i < conversation.Count; i++)
                {
                    string line = conversation.GetLines()[i];

                    //if(ripHeaderAndEncapsulators || (encapsulationLevel > 0 && !IsEncapslationEnd(line))) // -> need fix
                    if(ripHeaderAndEncapsulators || !firstLine) 
                        data.lines.Add(line);
                    
                    if(firstLine) firstLine = false; 

                    if (IsEncapslationStart(line))
                    {
                        encapsulationLevel++;
                        continue;
                    }

                    if (IsEncapslationEnd(line))
                    {
                        encapsulationLevel--;
                        if (encapsulationLevel == 0)
                        {
                            data.endIndex = i + parentStartingIndex;
                            break;
                        }
                    }
                }

                return data;
            }
        }

        public static class Expression
        {
            public static readonly string REGEX_ARITHMATIC = @"([-+*/=]=?)";
            public static readonly string REGEX_OPERATOR_LINE = @"^\$\w+\s*(=|\+=|-=|\*=|/=|)\s*";
            public static HashSet<string> OPERATORS = new HashSet<string>
            {
                "+", "-", "*", "/", "=", "+=", "-=", "*=", "/="
            };
            
            public static object CalculateValue(string[] parts)
            {
                List<string> operandStrings = new List<string>();
                List<string> operatorStrings = new List<string>();
                List<object> operands = new List<object>();

                for(int i=0;i<parts.Length; i++)
                {
                    string part = parts[i].Trim();

                    if(part == string.Empty)
                        continue;

                    if(OPERATORS.Contains(part))
                        operatorStrings.Add(part);
                    else
                        operandStrings.Add(part);
                }

                foreach (string operandString in operandStrings)
                {
                    operands.Add(ExtractValue(operandString));
                }

                CalculateValue_MultiplicationAndDivision(operatorStrings, operands);
                CalculateValue_AdditionAndSubtraction(operatorStrings, operands);

                return operands[0];
            }

            private static void CalculateValue_MultiplicationAndDivision(List<string> operatorStrings, List<object> operands)
            {
                for(int i=0;i<operatorStrings.Count ; i++)
                {
                    string currentOperator = operatorStrings[i];

                    if(currentOperator == "*" || currentOperator == "/")
                    {
                        double leftOperand = Convert.ToDouble(operands[i]);
                        double rightOperand = Convert.ToDouble(operands[i + 1]);

                        if(currentOperator == "*")
                            operands[i] = leftOperand * rightOperand;
                        else
                        {
                            if(rightOperand == 0)
                            {
                                Debug.LogError("Division by zero is not allowed.");
                                return;
                            }
                            operands[i] = leftOperand / rightOperand;
                        }

                        operands.RemoveAt(i + 1);
                        operatorStrings.RemoveAt(i);
                        i--; // Adjust index after removal
                    }
                }
            }

            private static void CalculateValue_AdditionAndSubtraction(List<string> operatorStrings, List<object> operands)
            {
                for (int i = 0; i < operatorStrings.Count; i++)
                {
                    string currentOperator = operatorStrings[i];

                    if (currentOperator == "+" || currentOperator == "-")
                    {
                        double leftOperand = Convert.ToDouble(operands[i]);
                        double rightOperand = Convert.ToDouble(operands[i + 1]);

                        if (currentOperator == "+")
                            operands[i] = leftOperand + rightOperand;
                        else
                            operands[i] = leftOperand - rightOperand;

                        operands.RemoveAt(i + 1);
                        operatorStrings.RemoveAt(i);
                        i--; // Adjust index after removal
                    }
                }
            }

            private static object ExtractValue(string value)
            {
                bool negate = false;
                if (value.StartsWith('!'))
                {
                    negate = true;
                    value = value.Substring(1).Trim();
                }

                if (value.StartsWith(VariableStore.VARIABLE_ID))
                {
                    string variableName = value.TrimStart(VariableStore.VARIABLE_ID);
                    if (!VariableStore.HasVariable(variableName))
                    {
                        Debug.LogError($"Variable '{variableName}' does not exist.");
                        return null;
                    }

                    VariableStore.TryGetValue(variableName, out object variableValue);

                    if (negate && variableValue is bool boolValue)
                        return !boolValue;
                    return variableValue;
                }
                // '\"' checking inside a string, like "\"Hello\""
                else if (value.StartsWith('\"') && value.EndsWith('\"'))
                {
                    // The value can be a string containing variables - $newDialogue += "IT'S $time ALREADY." 
                    value = TagManager.Inject(value, injectTags: true, injectVariables: true);
                    return value.Trim('"');
                }
                else
                {
                    if(int.TryParse(value, out int intValue))
                        return intValue;
                    else if (float.TryParse(value, out float floatValue))
                        return floatValue;
                    else if (bool.TryParse(value, out bool boolValue))
                        return negate ? !boolValue : boolValue;
                    else
                    {
                        value = TagManager.Inject(value, injectTags: true, injectVariables: true);
                        return value;
                    }
                }
            }
        }

        public static class Conditions
        {
            public static readonly string REGEX_CONDITION = @"(==|!=|<=|>=|<|>|&&|\|\|)";

            public static bool EvaluateCondition(string condition)
            {
                //This will turn variables and tags into their actual values
                condition = TagManager.Inject(condition, injectTags: true, injectVariables: true);


                string[] parts = Regex.Split(condition, REGEX_CONDITION).Select(p => p.Trim()).ToArray();

                // condition can be $name == "The Destroyer" but our name doesnt have "" -> remove "" from parts 
                for (int i = 0; i<parts.Length; i++)
                {
                    if (parts[i].StartsWith("\"") && parts[i].EndsWith("\""))
                        parts[i] = parts[i].Trim('"');
                }

                // if condition is checking only 1 bool, it isnt necessary to write $light == true, just simply $light or !$light
                if(parts.Length == 1)
                {
                    if (bool.TryParse(parts[0], out bool res))
                        return res;
                    else
                    {
                        Debug.LogError($"Cant parse condition: {condition}");
                        return false;
                    }
                }
                else if(parts.Length == 3)
                {
                    return EvaluateExpression(parts[0], parts[1], parts[2]);
                }
                else
                {
                    Debug.LogError($"Invalid condition format: {condition}");
                    return false;
                }
            }

            private delegate bool OperatorFunc<T>(T left, T right);

            private static Dictionary<string, OperatorFunc<bool>> boolOperators = new Dictionary<string, OperatorFunc<bool>>()
            {
                { "==", (left, right) => left == right },
                { "!=", (left, right) => left != right }, 
                { "&&", (left, right) => left && right },
                { "||", (left, right) => left || right }
            };

            private static Dictionary<string, OperatorFunc<float>> floatOperators = new Dictionary<string, OperatorFunc<float>>()
            {
                { "==", (left, right) => left == right },
                { "!=", (left, right) => left != right },
                { "<", (left, right) => left < right },
                { "<=", (left, right) => left <= right },
                { ">", (left, right) => left > right },
                { ">=", (left, right) => left >= right }
            };

            private static Dictionary<string, OperatorFunc<float>> intOperators = new Dictionary<string, OperatorFunc<float>>()
            {
                { "==", (left, right) => left == right },
                { "!=", (left, right) => left != right },
                { "<", (left, right) => left < right },
                { "<=", (left, right) => left <= right },
                { ">", (left, right) => left > right },
                { ">=", (left, right) => left >= right }
            };


            private static bool EvaluateExpression(string left, string op, string right)
            {
                if (bool.TryParse(left, out bool leftBool) && bool.TryParse(right, out bool rightBool))
                {
                    if(!boolOperators.ContainsKey(op))
                        Debug.LogError($"Unsupported boolean operator '{op}' in condition: {left} {op} {right}");
                    return boolOperators[op](leftBool, rightBool);
                }

                if (float.TryParse(left, out float leftFloat) && float.TryParse(right, out float rightFloat))
                {
                    if(!floatOperators.ContainsKey(op))
                        Debug.LogError($"Unsupported float operator '{op}' in condition: {left} {op} {right}");
                    return floatOperators[op](leftFloat, rightFloat);
                }

                if (int.TryParse(left, out int leftInt) && int.TryParse(right, out int rightInt))
                {
                    if (!intOperators.ContainsKey(op))
                        Debug.LogError($"Unsupported int operator '{op}' in condition: {left} {op} {right}");
                    return intOperators[op](leftInt, rightInt);
                }

                // else, the left and right are strings
                switch (op)
                {
                    case "==":
                        return left == right;
                    case "!=":
                        return left != right;
                    default:
                        throw new InvalidOperationException($"Unsupported operator '{op}'");
                }
            }
        }
    }
}