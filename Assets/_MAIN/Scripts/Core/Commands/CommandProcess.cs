using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

// Cache all processes/commands running at one time in the background 
// Think about Linux processes, they have their own ID. By monitoring process ID - PID, we can do actions on that process (kill)

namespace COMMAND
{
    public class CommandProcess
    {
        public Guid ID;
        public string processName;
        public Delegate command;
        public CoroutineWrapper runningProcess;
        public string[] args;

        public UnityEvent onTerminateAction;

        public CommandProcess(Guid ID, string processName, Delegate command, CoroutineWrapper runningProcess, string[] args, UnityEvent onTerminateAction = null)
        {
            this.ID = ID;
            this.processName = processName;
            this.command = command;
            this.args = args;
            this.runningProcess = runningProcess;
            this.onTerminateAction = onTerminateAction;
        }
    }
}