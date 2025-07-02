using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DIALOGUE.LogicalLines
{
    public interface ILogicalLine 
    {
        public string keyword { get; }
        public bool Matches(DIALOGUE_LINE line);
        public IEnumerator Execute(DIALOGUE_LINE line);
    }
}