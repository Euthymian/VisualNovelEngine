using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System;
using UnityEngine;
using UnityEngine.Events;
using CHARACTER;
using JetBrains.Annotations;

namespace COMMAND
{
    // This class is responsible for managing the command database, loading all commands from the CMD_Extension classes and executing them.

    public class CommandManager : MonoBehaviour
    {
        private const char SUB_COMMAND_IDENTIFIER = '.';
        public const string DATABASE_CHARACTER_BASE = "characters";
        public const string DATABASE_CHARACTER_SPRITE = "characters_sprite";
        public const string DATABASE_CHARACTER_LIVE2D = "characters_live2D";
        public const string DATABASE_CHARACTER_MODEL3D = "characters_model3D";

        public static CommandManager Instance { get; private set; }

        private List<CommandProcess> activeProcesses = new List<CommandProcess>();
        // We only need to keep track the last process in the list becuase 
        // if the last process is a [wait] process, no process will be added until this one finish. we can prompt to force stop it 
        // if not, we do nothing with it 
        private CommandProcess topProcess => activeProcesses.Last();

        private CommandDatabase commandDatabase;
        private Dictionary<string, CommandDatabase> subDatabases = new Dictionary<string, CommandDatabase>();

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
                 // Get all types that inherit from CMD_Extension in the assembly
                Type[] extensionTypes = assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(CMD_Extension))).ToArray(); 

                foreach (Type extension in extensionTypes)
                {
                    MethodInfo extendMethod = extension.GetMethod("Extend");
                    extendMethod?.Invoke(null, new object[] { commandDatabase });
                    // the first parameter is null because it specifies which object to invoke the method on, and we don't need to specify an object since it's a static method
                }
            }
        }

        public CoroutineWrapper Execute(string commandName, params string[] args)
        {
            if (commandName.Contains(SUB_COMMAND_IDENTIFIER))
                return ExecuteSubCommand(commandName, args);

            Delegate command = commandDatabase.GetCommand(commandName);

            if (command == null)
                return null;

            return StartProcess(commandName, command, args);
        }

        private CoroutineWrapper ExecuteSubCommand(string commandName, string[] args)
        {
            string[] commandNameParts = commandName.Split(SUB_COMMAND_IDENTIFIER);
            // in most common cases, there are only 2 parts.
            // First is subDatabase name such as Raelin, Kyo, Camera,...
            // Second is the command such as Move, Hide, ...
            // but there are also some cases where there are more than 2 parts, such as Cam.Current.SetPos, ....
            string subDatabaseName = string.Join(SUB_COMMAND_IDENTIFIER, commandNameParts.Take(commandNameParts.Length - 1)); // Join everything except the last part
            string subCommandName = commandNameParts.Last();

            if (subDatabases.ContainsKey(subDatabaseName))
            {
                Delegate command = subDatabases[subDatabaseName].GetCommand(subCommandName);
                if(command != null)
                {
                    return StartProcess(commandName, command, args);
                }
                else
                {
                    Debug.LogError($"Sub-command '{subCommandName}' does not exist in sub-database '{subDatabaseName}'.");
                    return null;
                }
            }

            if (CharacterManager.Instance.HasCharacter(subDatabaseName))
            {
                List<string> newArgs = new List<string>(args);
                newArgs.Insert(0, subDatabaseName); 

                args = newArgs.ToArray();

                return ExecuteCharacterCommand(subCommandName, args);
            }

            Debug.LogError($"Sub-database '{subDatabaseName}' does not exist.");
            return null;
        }

        private CoroutineWrapper ExecuteCharacterCommand(string commandName, params string[] args)
        {
            Delegate command = null;
            CommandDatabase db = subDatabases[DATABASE_CHARACTER_BASE];

            if (db.HasCommand(commandName))
            {
                command = db.GetCommand(commandName);
                return StartProcess(commandName, command, args);
            }

            CharacterConfigData configData = CharacterManager.Instance.GetCharacterConfigData(args[0]);
            switch (configData.characterType)
            {
                case Character.CharacterType.Sprite:
                case Character.CharacterType.SpriteSheet:
                    db = subDatabases[DATABASE_CHARACTER_SPRITE];
                    break;
                case Character.CharacterType.Live2D:
                    db = subDatabases[DATABASE_CHARACTER_LIVE2D];
                    break;
                case Character.CharacterType.Model3D:
                    db = subDatabases[DATABASE_CHARACTER_MODEL3D];
                    break;
                default:
                    Debug.LogError($"Character type {configData.characterType} is not supported.");
                    return null;
            }

            command = db.GetCommand(commandName);
            if (command == null)
            {
                Debug.LogError($"CommandManager was able to execute command {commandName} on character {args[0]}. The character name or command may be invalid!");
                return null;
            }
            return StartProcess(commandName, command, args);
        }

        private CoroutineWrapper StartProcess(string commandName, Delegate command, string[] args)
        {
            System.Guid processID = System.Guid.NewGuid();
            CommandProcess cmd = new CommandProcess(processID, commandName, command, null, args, null);
            activeProcesses.Add(cmd);

            Coroutine co = StartCoroutine(RunningProcess(cmd));

            cmd.runningProcess = new CoroutineWrapper(this, co);

            return cmd.runningProcess;
        }

        public void StopCurrentProcess()
        {
            if (topProcess != null)
                KillProcess(topProcess);
        }

        public void StopAllProcesses()
        {
            foreach (var cmd in activeProcesses)
            {
                if(cmd.runningProcess != null && !cmd.runningProcess.IsDone)
                    cmd.runningProcess.Stop();

                cmd.onTerminateAction?.Invoke();
            }

            activeProcesses.Clear();
        }

        public void StopLatestProcesses(int numberOfLastestProcesses)
        {
            if (numberOfLastestProcesses > activeProcesses.Count)
                return;

            int finishIndex = activeProcesses.Count - numberOfLastestProcesses;
            for (int i = activeProcesses.Count - 1;i >= finishIndex; i--)
            {
                CommandProcess cmd = activeProcesses[i];

                if (cmd.runningProcess != null && !cmd.runningProcess.IsDone)
                    cmd.runningProcess.Stop();
                cmd.onTerminateAction?.Invoke();

                activeProcesses.RemoveAt(i);
            }
        }

        public void KillProcess(CommandProcess cmd)
        {
            activeProcesses.Remove(cmd);
        
            if(cmd.runningProcess != null && !cmd.runningProcess.IsDone)
                cmd.runningProcess.Stop();

            cmd.onTerminateAction?.Invoke();
        }

        private IEnumerator RunningProcess(CommandProcess cmd)
        {
            yield return WaitingForProcessToComplete(cmd.command, cmd.args);
            
            KillProcess(cmd);
        }

        private IEnumerator WaitingForProcessToComplete(Delegate command, string[] args)
        {
            if (command is Action) 
                command.DynamicInvoke();
            else if (command is Action<string>) 
                command.DynamicInvoke(args.Length == 0 ? string.Empty : args[0]);
            else if (command is Action<string[]>) 
                command.DynamicInvoke((object)args);

            else if (command is Func<IEnumerator>)
                yield return ((Func<IEnumerator>)command)();
            else if (command is Func<string, IEnumerator>)
                yield return ((Func<string, IEnumerator>)command)(args.Length == 0 ? string.Empty : args[0]);
            else if (command is Func<string[], IEnumerator>)
                yield return ((Func<string[], IEnumerator>)command).Invoke(args);
        }

        public void AddTerminationActionToCurrentProcess(UnityAction action)
        {
            if (topProcess == null)
                return;

            topProcess.onTerminateAction = new UnityEvent();
            topProcess.onTerminateAction.AddListener(action);
        }

        public CommandDatabase CreateSubDatabase(string name)
        {
            name = name.ToLower();

            if(subDatabases.TryGetValue(name, out CommandDatabase bd))
            {
                Debug.LogWarning($"Sub-database {name} already exists.");
                return bd;
            }

            CommandDatabase newDb = new CommandDatabase();
            subDatabases.Add(name, newDb);

            return newDb;
        }
    }
}