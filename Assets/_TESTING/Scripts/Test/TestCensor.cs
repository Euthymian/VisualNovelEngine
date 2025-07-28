#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCensor : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Check("this line has badword1");
        Check("this line has no badword");
        Check("this line is $Tink1n9");
        Check("but this line is a$Tink1n9 but not hard block");
        Check("this line has alotOfTofu");
    }

    // Update is called once per frame
    void Check(string line)
    {
        if(CensorManager.Censor(ref line))
        {
            Debug.Log($"<color=red>Censored: {line}</color>");
        }
        else
        {
            Debug.Log($"<color=green>Not Censored: {line}</color>");
        }
    }
}
#endif