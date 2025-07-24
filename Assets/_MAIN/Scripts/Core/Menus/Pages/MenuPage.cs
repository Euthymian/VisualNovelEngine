using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuPage : MonoBehaviour
{
    private const string OPEN_TRIGGER = "Open";
    private const string CLOSE_TRIGGER = "Close";

    [SerializeField] private Animator anim;

    public virtual void Open()
    {
        anim.SetTrigger(OPEN_TRIGGER);
    }

    public virtual void Close()
    {
        anim.SetTrigger(CLOSE_TRIGGER);
    }
}
