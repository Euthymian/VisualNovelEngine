using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonBehaviours : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private static ButtonBehaviours selectedButton = null;

    public Animator anim;

    // IPointerEnterHandler and IPointerExitHandler are used to detect when mouse entered and exited button area
    public void OnPointerExit(PointerEventData eventData)
    {
        // Exit the button area
        anim.Play("Exit");
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Enter the button area
        if(selectedButton != null && selectedButton != this)
        {
            selectedButton.OnPointerExit(null);
        }

        anim.Play("Enter");
        selectedButton = this;
    }
}
