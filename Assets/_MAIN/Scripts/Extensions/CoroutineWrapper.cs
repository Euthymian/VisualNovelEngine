using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CoroutineWrapper
{
    private MonoBehaviour owner;
    private Coroutine coroutine;
    private Coroutine[] coroutines;

    public bool IsDone = false;

    public CoroutineWrapper(MonoBehaviour owner, Coroutine coroutine)
    {
        this.owner = owner;
        this.coroutine = coroutine;
    }

    public CoroutineWrapper(MonoBehaviour owner, Coroutine[] coroutines)
    {
        this.owner = owner;
        this.coroutines = coroutines;
    }

    public void Stop()
    {
        owner.StopCoroutine(coroutine);
        IsDone = true;
    }
}
