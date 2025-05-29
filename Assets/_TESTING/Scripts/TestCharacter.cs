using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CHARACTER;

public class TestCharacter : MonoBehaviour
{
    private Character CreateCharacter(string name) => CharacterManager.Instance.CreateCharacter(name);

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(TestCharSay());
        //StartCoroutine(TestCreateChar());
        StartCoroutine(TestCreateCharCasting());
    }

    IEnumerator TestCreateCharCasting()
    {
        Character guard0 = CreateCharacter("Guard0 as Generic");
        Character fs2 = CreateCharacter("Female Student 2");
        Character rae = CreateCharacter("Raelin");
        Character kyo = CreateCharacter("KyoyaAkase");

        guard0.SetPosition(Vector2.zero);
        kyo.SetPosition(Vector2.one);
        fs2.SetPosition(new Vector2(0.5f, 0.5f));
        rae.SetPosition(new Vector2(2f, 0));

        yield return guard0.Show();
        fs2.Show();
        rae.Show();
        kyo.Show();

        yield return guard0.MoveToPosition(Vector2.one, smooth:true);
        yield return guard0.MoveToPosition(Vector2.zero, smooth: true);

        yield return null;
    }

    IEnumerator TestCreateChar()
    {
        Character char1 = CreateCharacter("Generic");

        yield return char1.Hide();
        yield return char1.Show();
        yield return char1.Say("\"STOPPPPP!\"");
    }

    IEnumerator TestCharSay()
    {
        Character char0 = CreateCharacter("Elen");
        Character char1 = CreateCharacter("ben");
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
