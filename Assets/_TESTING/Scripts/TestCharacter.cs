using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using CHARACTER;
using UnityEngine.Experimental.AI;

public class TestCharacter : MonoBehaviour
{
    private Character CreateCharacter(string name) => CharacterManager.Instance.CreateCharacter(name);

    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(TestCharSay());
        //StartCoroutine(TestCreateChar());
        //StartCoroutine(TestCharActions());
        StartCoroutine(TestCharColor());
        //StartCoroutine(TestPriority());
    }

    IEnumerator TestPriority()
    {
        Character_Sprite rae = CreateCharacter("Raelin") as Character_Sprite;
        Character_Sprite kyo = CreateCharacter("KyoyaAkase") as Character_Sprite;
        Character_Sprite guard = CreateCharacter("Guard as Generic") as Character_Sprite;
        Character_Sprite redGuard = CreateCharacter("Red Guard as Generic") as Character_Sprite;

        redGuard.SetColor(Color.red);

        rae.SetPosition(new Vector2(0.3f, 0f));
        kyo.SetPosition(new Vector2(0.4f, 0.2f));
        guard.SetPosition(new Vector2(0.5f, 0.3f));
        redGuard.SetPosition(new Vector2(0.6f, 0.5f));

        rae.Show();
        kyo.Show();
        guard.Show();
        redGuard.Show();

        rae.SetPriority(1);
        guard.SetPriority(2);
        kyo.SetPriority(34);
        redGuard.SetPriority(100);

        yield return new WaitForSeconds(3);

        CharacterManager.Instance.SortCharacters(new string[] { "Raelin", "KyoyaAkase" });

        Debug.Log($"Rae: {rae.priority}\nKyo: {kyo.priority}\nGuard: {guard.priority}\nRed Guard: {redGuard.priority}");

        yield return new WaitForSeconds(3);


        CharacterManager.Instance.SortCharacters(new string[] { "KyoyaAkase", "Guard", "Red Guard", "Raelin" });
        Debug.Log($"Rae: {rae.priority}\nKyo: {kyo.priority}\nGuard: {guard.priority}\nRed Guard: {redGuard.priority}");
    }

    IEnumerator TestCharColor()
    {
        Character_Sprite rae = CreateCharacter("Raelin") as Character_Sprite;
        Character_Sprite kyo = CreateCharacter("KyoyaAkase") as Character_Sprite;

        rae.SetPosition(new Vector2(0.3f, 0f));
        kyo.SetPosition(new Vector2(0.7f, 0f));


        rae.Show();
        kyo.Show();

        kyo.UnHighlight();
        yield return rae.Say("\"Hello, I am Raelin.\"");

        yield return rae.Flip(0.5f);
        yield return new WaitForSeconds(0.5f);
        yield return rae.FaceLeft(immediate:true);
        yield return new WaitForSeconds(0.5f);
        yield return rae.FaceRight(immediate: true);
        kyo.Highlight();
        rae.UnHighlight();
        yield return kyo.Say("\"Hello, I am Kyoya.\"");

        rae.Highlight();
        kyo.UnHighlight();
        yield return rae.Say("\"I am a student at the academy.\"");
    
        kyo.Highlight();
        rae.UnHighlight();
        yield return kyo.Say("\"OK, but ...{wc 2} Whos ask?\"");

        rae.Highlight();
        rae.Animate("Hop");
        rae.TransitionSprite(rae.GetSprite("Raelin-A_Shock"), layer: 1);
        yield return rae.Say("\"....\"");
        rae.TransitionSprite(rae.GetSprite("Raelin-A_Scared"), layer: 1);
        rae.Animate("Shiver", true);
        yield return rae.Say("\"Aaaaaaa.....\"");
    }

    IEnumerator TestCharActions()
    {
        Character_Sprite guard0 = CreateCharacter("Guard0 as Generic") as Character_Sprite;
        Character_Sprite fs2 = CreateCharacter("Female Student 2") as Character_Sprite;
        Character_Sprite rae = CreateCharacter("Raelin") as Character_Sprite;
        Character_Sprite kyo = CreateCharacter("KyoyaAkase") as Character_Sprite;
        Character char1 = CreateCharacter("Elen");

        //guard0.SetPosition(Vector2.zero);
        //guard0.SetSprite(guard0.GetSprite("Monk"), 0);
        //yield return guard0.Show();
        //yield return new WaitForSeconds(1f);
        //guard0.TransitionSprite(guard0.GetSprite("Dad"));
        //yield return guard0.MoveToPosition(Vector2.one, smooth:true);
        //yield return guard0.MoveToPosition(Vector2.zero, smooth: true);


        kyo.Show();
        kyo.SetPosition(Vector2.one);
        yield return new WaitForSeconds(1f);
        kyo.TransitionSprite(kyo.GetSprite("p2"), 0);
        yield return kyo.TransitionSprite(kyo.GetSprite("p2e3"), 1);
        kyo.layersList[1].SetColor(Color.red);
        yield return new WaitForSeconds(1f);
        yield return kyo.MoveToPosition(Vector2.zero, smooth: true);
        //yield return TestKyoChangeExpression(kyo);


        fs2.SetPosition(new Vector2(0.5f, 0.5f));
        //fs2.Show();


        rae.Show();
        rae.SetPosition(new Vector2(1f, 0));
        yield return new WaitForSeconds(1f);
        rae.TransitionSprite(rae.GetSprite("Raelin-B2"), layer:0);
        yield return rae.TransitionSprite(rae.GetSprite("Raelin-B_Blush"), layer:1);

        yield return new WaitForSeconds(1f);

        yield return rae.TransitionSprite(rae.GetSprite("Raelin-B_Stern"), layer:1);

        rae.MoveToPosition(new Vector2(0.3f, 0f), smooth: true);
        kyo.MoveToPosition(new Vector2(0.7f, 0f), smooth: true);

        

        //NOTE: use yield to setup sequence of actions, without yield, actions will be executed in parallel 

        //Debug.Log(rae.isVisible);




        yield return null;
    }
    
    IEnumerator TestKyoChangeExpression(Character_Sprite kyo)
    {
        yield return new WaitForSeconds(1f);
        kyo.SetSprite(kyo.GetSprite("p2"), 0);
        kyo.SetSprite(kyo.GetSprite("p2e3"), 1);
        yield return new WaitForSeconds(1f);
        kyo.SetSprite(kyo.GetSprite("p2e2"), 1);
    } 

    IEnumerator TestCreateChar()
    {
        Character char1 = CreateCharacter("Generic");

        yield return char1.Hide();

        Debug.Log(char1.isVisible);
        yield return new WaitForSeconds(2f);

        yield return char1.Show();

        Debug.Log(char1.isVisible);


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
