using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestChoicePanel : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(Test1());
    }

    string ArrayToString(string[] arr)
    {
        string res = "";
        foreach(string s in arr)
        {
            res += s + "\n";
        }
        return res;
    }

    IEnumerator Test1()
    {
        ChoicePanel choicePanel = ChoicePanel.Instance;

        string[] choices = new string[] { "Choice 1", "Once upon a time, ...", "The society that separates its scholars from its warriors will have its thinking done by cowards and its fighting by fools", "Iam the storm that is approching." };
        ChoicePanel.Instance.Show("What is your choice?", choices);

        yield return new WaitUntil(() => !choicePanel.isWaitingOnUserChoice);

        Debug.Log($"Question is <{choicePanel.lastDecision.question}> \nOptions are:\n{ArrayToString(choicePanel.lastDecision.choices)}\nAnswer is {choicePanel.lastDecision.choices[choicePanel.lastDecision.answerIndex]}");

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
