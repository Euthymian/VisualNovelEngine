using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CHARACTER;

public class TestCharacter : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //Character char1 = CharacterManager.Instance.CreateCharacter("Stella");
        //Character char2 = CharacterManager.Instance.CreateCharacter("ben");
        //Character char3 = CharacterManager.Instance.CreateCharacter("bfuiewghf");
        StartCoroutine(TestCharSay());
    }

    IEnumerator TestCharSay()
    {
        Character char0 = CharacterManager.Instance.CreateCharacter("Elen");
        Character char1 = CharacterManager.Instance.CreateCharacter("ben");
        List<string> lines = new List<string>()
        {
            "\"Hello, how are you?\"",
            "\"My name is Elen.\"",
            "\"What is your name?\""
        };
        yield return char0.Say(lines);
        char0.SetNameColor(Color.red);
        yield return char1.Say("\"I am Adam.{wc 1} Nice to meet you.\"");
        char1.SetNameColor(Color.green);
        yield return char0.Say("\"Oh {wa 4} What a surprise! {wc 3} You are my enemy then.\"");
        yield return char1.Say("\"I am not your enemy! {wc 2} I am your friend!\"");
        yield return char0.Say("\"SO WHY DO YOU KILL MY PARENT?? {wc 6} ANWSER ADAM!\"");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
