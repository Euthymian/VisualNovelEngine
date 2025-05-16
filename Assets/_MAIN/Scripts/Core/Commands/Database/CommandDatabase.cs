using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COMMAND
{
    public class CommandDatabase
    {
        private Dictionary<string, Delegate> database = new Dictionary<string, Delegate>();

        public bool HasCommand(string command) => database.ContainsKey(command);

        public void AddCommand(string commandName, Delegate command)
        {
            if (!database.ContainsKey(commandName))
            {
                database.Add(commandName, command);
            }
            else
            {
                Debug.LogError($"Command {commandName} already exists in the database.");
            }
        }

        public Delegate GetCommand(string commandName)
        {
            if (database.ContainsKey(commandName))
            {
                return database[commandName];
            }
            else
            {
                Debug.LogError($"Command {commandName} does not exist in the database.");
                return null;
            }
        }
    }
}