using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace VISUALNOVEL
{
    // Store dialogue file name, progress, start and end index ....
    [System.Serializable]
    public class VN_ConversationDataCompressed
    {
        public string fileName;
        public int startIndex, endIndex;
        public int progress;
    }
}