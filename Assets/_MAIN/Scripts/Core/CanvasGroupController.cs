using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasGroupController
{
    private const float DEFAULT_FADE_SPEED = 3f;

    private MonoBehaviour owner;
    private CanvasGroup rootCanvasGroup;

    private Coroutine co_showing = null;
    private Coroutine co_hiding = null;
    public bool isShowing => co_showing != null;
    public bool isHiding => co_hiding != null;
    public bool isFading => isHiding || isShowing;

    public bool isVisible => co_showing != null || rootCanvasGroup.alpha > 0f;

    public CanvasGroupController(MonoBehaviour owner, CanvasGroup rootCanvasGroup)
    {
        this.owner = owner;
        this.rootCanvasGroup = rootCanvasGroup;
    }

    public float alpha
    {
        get => rootCanvasGroup.alpha;
        set => rootCanvasGroup.alpha = value;
    }

    public Coroutine Show(float speedMultiplier = 1, bool immediate = false)
    {
        if (isShowing)
        {
            owner.StopCoroutine(co_showing);
            co_showing = null;
        }
        else if (isHiding)
        {
            owner.StopCoroutine(co_hiding);
            co_hiding = null;
        }

        co_showing = owner.StartCoroutine(Fading(1f, speedMultiplier, immediate));
        return co_showing;
    }

    public Coroutine Hide(float speedMultiplier = 1, bool immediate = false)
    {
        if (isHiding)
        {
            owner.StopCoroutine(co_hiding);
            co_hiding = null;
        }
        else if (isShowing)
        {
            owner.StopCoroutine(co_showing);
            co_showing = null;
        }

        co_hiding = owner.StartCoroutine(Fading(0f, speedMultiplier, immediate));
        return co_hiding;
    }

    private IEnumerator Fading(float alpha, float speedMultiplier, bool immediate)
    {
        CanvasGroup cg = rootCanvasGroup;

        if (immediate)
            cg.alpha = alpha;

        while (cg.alpha != alpha)
        {
            cg.alpha = Mathf.MoveTowards(cg.alpha, alpha, DEFAULT_FADE_SPEED * Time.deltaTime * speedMultiplier);
            yield return null;
        }

        co_hiding = null;
        co_showing = null;
    }

    public void SetInteractableState(bool active)
    {
        rootCanvasGroup.interactable = active;
        rootCanvasGroup.blocksRaycasts = active;
    }
}
