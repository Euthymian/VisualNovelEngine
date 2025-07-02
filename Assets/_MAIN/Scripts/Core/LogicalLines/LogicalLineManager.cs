using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace DIALOGUE.LogicalLines
{
    public class LogicalLineManager
    {
        private List<ILogicalLine> logicalLineList = new List<ILogicalLine>();


        public LogicalLineManager()
        {
            LoadLogicalLines();
        }

        private void LoadLogicalLines()
        {
            Assembly assembly = Assembly.GetExecutingAssembly();
            Type[] lineTypes = assembly.GetTypes().Where(t => typeof(ILogicalLine).IsAssignableFrom(t) && !t.IsInterface).ToArray();

            foreach(var each in lineTypes)
            {
                ILogicalLine line = (ILogicalLine)Activator.CreateInstance(each);
                logicalLineList.Add(line);
            }
        }

        public bool TryGetLogic(DIALOGUE_LINE line, out Coroutine logic)
        {
            foreach(ILogicalLine each in logicalLineList)
            {
                if (each.Matches(line))
                {
                    logic = DialogueSystem.Instance.StartCoroutine(each.Execute(line));
                    return true;
                }
            }

            logic = null;
            return false;
        }
    }
}