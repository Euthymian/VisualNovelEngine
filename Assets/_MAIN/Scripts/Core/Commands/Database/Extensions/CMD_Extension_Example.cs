using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COMMAND
{
    public class CMD_Extension_Example : CMD_Extension
    {
        new public static void Extend(CommandDatabase commandDatabase) // new keyword to hide the base class method
        {
            // Add regular commands
            commandDatabase.AddCommand("print_0p", new Action(PrintDefaultMessage));
            commandDatabase.AddCommand("print_1p", new Action<string>(PrintCustomMessage));
            commandDatabase.AddCommand("print_mp", new Action<string[]>(PrintLines));

            // Add lambda command
            commandDatabase.AddCommand("lambda_0p", new Action(() => Debug.Log("Lambda expression with no parameters.")));
            commandDatabase.AddCommand("lambda_1p", new Action<string>((message) => Debug.Log("Lambda expression with one parameter: " + message)));
            commandDatabase.AddCommand("lambda_mp", new Action<string[]>((lines) => { Debug.Log(string.Join(", ", lines)); }));

            // Add coroutine command
            commandDatabase.AddCommand("coroutine_0p", new Func<IEnumerator>(SimpleProcess));
            commandDatabase.AddCommand("coroutine_1p", new Func<string, IEnumerator>(Process1Arg));
            commandDatabase.AddCommand("coroutine_mp", new Func<string[], IEnumerator>(ProcessMArgs));

            // Move character demo
            commandDatabase.AddCommand("MoveCharDemo", new Func<string, IEnumerator>(MoveCharDemo));
        }

        private static void PrintDefaultMessage()
        {
            Debug.Log("Default message from CMD_Extension_Example");
        }

        private static void PrintCustomMessage(string message)
        {
            Debug.Log("Print custom message:" + message);
        }

        private static void PrintLines(string[] lines)
        {
            int i = 1;
            foreach (string line in lines)
            {
                Debug.Log(i + ". " + line);
                i++;
            }
        }

        private static IEnumerator SimpleProcess()
        {
            for (int i = 0; i < 5; i++)
            {
                Debug.Log("Simple process: " + i);
                yield return new WaitForSeconds(1f);
            }
        }

        private static IEnumerator Process1Arg(string data)
        {
            for (int i = 0; i < 5; i++)
            {
                Debug.Log("Process1Arg: " + i + " " + data);
                yield return new WaitForSeconds(1f);
            }
        }

        private static IEnumerator ProcessMArgs(string[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                Debug.Log("ProcessMArgs: " + i + " " + data[i]);
                yield return new WaitForSeconds(1f);
            }
        }

        private static IEnumerator MoveCharDemo(string dir)
        {
            bool leftDir = dir.ToLower() == "left";

            Transform character = GameObject.Find("DemoChar").transform;
            float speed = 15f;

            float targetX = leftDir ? -8f : 8f;
            float currentX = character.position.x;

            while (Mathf.Abs(currentX - targetX) > 0.1f)
            {
                currentX = Mathf.MoveTowards(currentX, targetX, speed * Time.deltaTime);
                character.position = new Vector3(currentX, character.position.y, character.position.z);
                yield return null;
            }

        }
    }
}