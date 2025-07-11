using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TESTING
{
    public class TestTextArchitect : MonoBehaviour
    { 
        public TextArchitect.BuildMethod bm = TextArchitect.BuildMethod.instant;
        TextArchitect textArchitect;
        string[] lines = new string[5]
        {
            "This is a test line 1",
            "This is a test line 2",
            "This is a test line 3",
            "This is a test line 4",
            "This is a test line 5"
        };

        private void Start()
        {
            textArchitect = new TextArchitect(DIALOGUE.DialogueSystem.Instance.dialogueContainer.dialogueText);
            textArchitect.buildMethod = TextArchitect.BuildMethod.fade;
        }

        string longLine = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed do eiusmod tempor incididunt ut labore et dolore magna aliqua. Ut enim ad minim veniam, quis nostrud exercitation ullamco laboris nisi ut aliquip ex ea commodo consequat. Duis aute irure dolor in reprehenderit in voluptate velit esse cillum dolore eu fugiat nulla pariatur. Excepteur sint occaecat cupidatat non proident, sunt in culpa qui officia deserunt mollit anim id est laborum.";

        private void Update()
        {
            if(bm != textArchitect.buildMethod)
            {
                textArchitect.buildMethod = bm;
                textArchitect.Stop();
            }

            if (Input.GetKeyDown(KeyCode.S))
                textArchitect.Stop();

            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (!textArchitect.isBuilding) textArchitect.Build(longLine);
                else
                {
                    if(!textArchitect.hurryUp) textArchitect.hurryUp = true;
                    else textArchitect.ForceComplete();
                }
            }
            else if (Input.GetKeyDown(KeyCode.A))
            {
                textArchitect.Append(longLine);
            }
        }
    }
}