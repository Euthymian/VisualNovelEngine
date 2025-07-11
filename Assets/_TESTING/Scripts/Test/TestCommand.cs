using COMMAND;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class TestCommand : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        //StartCoroutine(Running());
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Debug.Log("Right Arrow Pressed");
            CommandManager.Instance.Execute("MoveCharDemo", "Right");
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Debug.Log("Left Arrow Pressed");
            CommandManager.Instance.Execute("MoveCharDemo", "Left");
        }
    }


    IEnumerator Running()
    {
        yield return CommandManager.Instance.Execute("print_1p", "This is custom message as single argument.");
        yield return CommandManager.Instance.Execute("print_0p");
        yield return CommandManager.Instance.Execute("print_mp", "Line1", "Line2", "Line3", "Line4");

        yield return CommandManager.Instance.Execute("lambda_0p");
        yield return CommandManager.Instance.Execute("lambda_1p", "Haha");
        yield return CommandManager.Instance.Execute("lambda_mp", "Line1", "Line2", "Line3", "Line4");

        yield return CommandManager.Instance.Execute("coroutine_0p");
        yield return CommandManager.Instance.Execute("coroutine_1p", "Co This is a test");
        yield return CommandManager.Instance.Execute("coroutine_mp", "Co Line1", "Co Line2", "Co Line3", "Co Line4");
    }
}
