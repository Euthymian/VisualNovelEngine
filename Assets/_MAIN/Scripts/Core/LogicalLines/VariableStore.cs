using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VariableStore
{
    private const string DEFAULT_DATABASE_NAME = "default";
    public const char DATABASE_VARIABLE_SEPARATOR = '.';
    public static readonly string REGEX_VARIABLE_IDS = @"[!]?\$[a-zA-Z0-9_.]+";
    public const char VARIABLE_ID = '$';

    public class Database
    {
        public string name;
        public Dictionary<string, Variable> variables = new Dictionary<string, Variable>();

        public Database(string name = DEFAULT_DATABASE_NAME)
        {
            this.name = name;
            variables = new Dictionary<string, Variable>();
        }
    }

    public abstract class Variable
    {
        public abstract object Get();
        public abstract void Set(object value);
    }

    public class Variable<T> : Variable
    {
        private T value;

        // The only way we can link this variable to an external variable is through commands or getter, setter
        // That is why we have this 2 fields, they will store the getter and setter functions from the outside
        private Func<T> getter;
        private Action<T> setter;

        // If getter and setter are not provided, we can use the value field directly
        // => If getter and setter are null, this is an internal variable; else, it is an external variable
        public Variable(T defaultValue = default, Func<T> getter = null, Action<T> setter = null)
        {
            value = defaultValue;

            if (getter == null)
                this.getter = () => value;
            else
                this.getter = getter;

            if (setter == null)
                this.setter = (newValue) => value = newValue;
            else
                this.setter = setter;
        }

        public override object Get() => getter();

        public override void Set(object value) => setter((T)value);
    }

    public static Dictionary<string, Database> databases = new Dictionary<string, Database>() { { DEFAULT_DATABASE_NAME, new Database(DEFAULT_DATABASE_NAME) } };
    private static Database defaultDatabase => databases[DEFAULT_DATABASE_NAME];

    public static bool CreateDatabase(string name)
    {
        if (databases.ContainsKey(name))
        {
            Debug.LogError($"Database with name '{name}' already exists.");
            return false;
        }

        databases[name] = new Database(name);
        return true;
    }

    public static Database GetDatabase(string name)
    {
        if (name == string.Empty)
        {
            return defaultDatabase;
        }

        if (!databases.ContainsKey(name))
            CreateDatabase(name);

        return databases[name];
    }

    private static (string[], Database, string) ExtractInfo(string name)
    {
        string[] parts = name.Split(DATABASE_VARIABLE_SEPARATOR);
        Database db = parts.Length > 1 ? GetDatabase(parts[0]) : GetDatabase(DEFAULT_DATABASE_NAME);
        string variableName = parts.Length > 1 ? parts[1] : parts[0];
        return (parts, db, variableName);
    }

    public static bool CreateVariable<T>(string name, T defaultValue = default, Func<T> getter = null, Action<T> setter = null)
    {
        (string[] parts, Database db, string variableName) = ExtractInfo(name);

        if (db.variables.ContainsKey(variableName))
        {
            Debug.LogError("Variablealready exists in");
            return false;
        }

        db.variables[variableName] = new Variable<T>(defaultValue, getter, setter);

        return true;
    }

    public static bool HasVariable(string name)
    {
        string[] parts = name.Split(DATABASE_VARIABLE_SEPARATOR);
        Database db = parts.Length > 1 ? GetDatabase(parts[0]) : GetDatabase(DEFAULT_DATABASE_NAME);
        string variableName = parts.Length > 1 ? parts[1] : parts[0];

        return db.variables.ContainsKey(variableName);
    }

    public static bool TryGetValue(string name, out object variable)
    {
        (string[] parts, Database db, string variableName) = ExtractInfo(name);

        if (!db.variables.ContainsKey(variableName))
        {
            variable = null;
            return false;
        }

        variable = db.variables[variableName].Get();
        return true;
    }

    public static bool TrySetValue<T>(string name, T value)
    {
        (string[] parts, Database db, string variableName) = ExtractInfo(name);

        if (!db.variables.ContainsKey(variableName))
        {
            return false;
        }

        db.variables[variableName].Set(value);
        return true;
    }

    public static void RemoveVariable(string name)
    {
        (string[] parts, Database db, string variableName) = ExtractInfo(name);
        if (!db.variables.ContainsKey(variableName))
        {
            Debug.LogError($"Variable '{variableName}' does not exist in database '{db.name}'.");
            return;
        }
        db.variables.Remove(variableName);
    }

    public static void RemoveAllVariables()
    {
        databases.Clear();
        databases[DEFAULT_DATABASE_NAME] = new Database(DEFAULT_DATABASE_NAME);
    }

    public static void PrintAllDatabases()
    {
        string result = "Databases:\n";
        foreach (var db in databases)
        {
            result += $"- <color=#FFB145>{db.Key}</color>\n";
        }
        Debug.Log(result);
    }

    private static void PrintAllDatabaseVariables(Database db)
    {
        string res = $"Variable of <color=#FFB145>{db.name}</color>:\n";
        foreach (var variable in db.variables)
        {
            res += $"- <color=#FFFF00>{variable.Key}</color>: <color=#00FF00>{variable.Value.Get()}</color>\n";
        }
        Debug.Log(res);
    }

    public static void PrintAllVariables(Database db = null)
    {
        if(db != null)
            PrintAllDatabaseVariables(db);
        else
        {
            foreach(var each in databases)
            {
                PrintAllDatabaseVariables(each.Value);
            }
                
        }

    }
}
