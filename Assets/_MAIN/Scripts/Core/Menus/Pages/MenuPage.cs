using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class MenuPage : MonoBehaviour
{
    public enum PageType { SaveAndLoad, Config, Help }
    public PageType pageType;

    private const string OPEN_TRIGGER = "Open";
    private const string CLOSE_TRIGGER = "Close";

    public Animator anim;

    public virtual void Open()
    {
        anim.SetTrigger(OPEN_TRIGGER);
    }

    public virtual void Close(bool closeAllMenus =false)
    {
        anim.SetTrigger(CLOSE_TRIGGER);

        if (closeAllMenus)
        {
            VNMenuManager.Instance.CloseRoot();
        }
    }
}
