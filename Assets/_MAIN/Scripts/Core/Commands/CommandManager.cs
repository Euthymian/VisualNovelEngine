using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine;

namespace COMMAND
{

    // This class is responsible for managing the command database, loading all commands from the CMD_Extension classes and executing them.

    public class CommandManager : MonoBehaviour
    {
        public static CommandManager Instance { get; private set; }

        private static Coroutine process = null;
        public static bool isRunningProcess => process != null;

        private CommandDatabase commandDatabase;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            else
            {
                Instance = this;
                commandDatabase = new CommandDatabase();

                // The CommandManager will look for all classes that inherit from CMD_Extension inside current Assembly then call the Extend method to populate the command database

                Assembly assembly = Assembly.GetExecutingAssembly(); // Get the current assembly
                Type[] extensionTypes = assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(CMD_Extension))).ToArray(); // Get all types that inherit from CMD_Extension in the assembly

                foreach (Type extension in extensionTypes)
                {
                    MethodInfo extendMethod = extension.GetMethod("Extend");
                    extendMethod?.Invoke(null, new object[] { commandDatabase });
                    // the first parameter is null because it specifies which object to invoke the method on, and we don't need to specify an object since it's a static method


                }
            }
        }

        public Coroutine Execute(string commandName, params string[] args)
        {
            Delegate command = commandDatabase.GetCommand(commandName);

            if (command == null)
                return null;

            return StartProcess(commandName, command, args);
        }

        private Coroutine StartProcess(string commandName, Delegate command, string[] args)
        {
            StopCurrentProcess();

            process = StartCoroutine(RunningProcess(command, args));
            return process;
        }

        private void StopCurrentProcess()
        {
            if (process != null)
            {
                StopCoroutine(process);
            }
            process = null;
        }

        private IEnumerator RunningProcess(Delegate command, string[] args)
        {
            yield return WaitingForProcessToComplete(command, args);
            process = null;
        }

        private IEnumerator WaitingForProcessToComplete(Delegate command, string[] args)
        {
            if (command is Action) command.DynamicInvoke();
            else if (command is Action<string>) command.DynamicInvoke(args[0]);
            else if (command is Action<string[]>) command.DynamicInvoke((object)args);

            else if (command is Func<IEnumerator>)
                yield return ((Func<IEnumerator>)command)();
            else if (command is Func<string, IEnumerator>)
                yield return ((Func<string, IEnumerator>)command)(args[0]);
            else if (command is Func<string[], IEnumerator>)
                yield return ((Func<string[], IEnumerator>)command).Invoke(args);
        }
    }
}