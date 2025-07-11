using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestConversationQueue : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Test1());
    }

    IEnumerator Test1()
    {
        List<string> lines = new List<string>
        {
            "\"Hello, how are you?\"",
            "\"I'm fine, thank you!\"",
            "\"What about you?\"",
            "\"I'm doing great, thanks for asking!\""
        };

        yield return DialogueSystem.Instance.Say(lines);

        DialogueSystem.Instance.Hide();
    }

    // Update is called once per frame
    void Update()
    {
        List<string> lines;
        Conversation conversation = null;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            lines = new List<string>
            {
                "\"This is the first line of the conversation which come after thanks for asking.\"",
                "\"And this is the second line.\"",
                "\"Finally, we reach the third line.\""
            };
            conversation = new Conversation(lines);
            DialogueSystem.Instance.conversationManager.Enqueue(conversation);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            lines = new List<string>
            {
                "\"HIGH PRIORITY CONVERSATION START.\"",
                "\"SECOND LINE.\"",
                "\"THIRD LINE.\""
            };
            conversation = new Conversation(lines);
            DialogueSystem.Instance.conversationManager.EnqueuePriority(conversation);
        }
    }
}
