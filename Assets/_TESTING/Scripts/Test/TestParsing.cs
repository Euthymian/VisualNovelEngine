using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TESTING
{
    public class TestParsing : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            TestParse();
        }

        void TestParse()
        {
            List<string> lines = FileManager.ReadTextAsset("test", false);
            foreach (string line in lines)
            {
                DIALOGUE.DIALOGUE_LINE parsedLine = DIALOGUE.DialogueParser.Parse(line);
            }
        }
    }
}