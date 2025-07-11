using CHARACTER;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInputPanel : MonoBehaviour
{
    public InputPanel inputPanel;

    void Start()
    {
        StartCoroutine(Test1());
    }

    IEnumerator Test1()
    {
        Character celeste = CharacterManager.Instance.CreateCharacter("Celeste");

        yield return celeste.Show();

        yield return celeste.Say("\"What is your name?\"");

        inputPanel.Show("Enter your name");
        while(inputPanel.isWaitingOnUserInput)
            yield return null;

        yield return celeste.Say($"\"Nice to meet you, {inputPanel.lastInput}.\"");
    }
}
