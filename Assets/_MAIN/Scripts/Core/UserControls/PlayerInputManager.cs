using HISTORY;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private List<(InputAction action, Action<InputAction.CallbackContext> command)> actions = new List<(InputAction, Action<InputAction.CallbackContext>)>();

    // Call Awake is because if use Start, no actions will be assigned because OnEnable is called before Start.
    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        InitializeActions();
    }

    private void InitializeActions()
    {
        actions.Add((playerInput.actions["Next"], OnNext));
        actions.Add((playerInput.actions["HistoryBack"], OnHistoryBack));
        actions.Add((playerInput.actions["HistoryForward"], OnHistoryForward));
        actions.Add((playerInput.actions["HistoryLogs"], OnHistoryLogToggle));
    }

    private void OnEnable()
    {
        foreach(var each in actions)
        {
            each.action.performed += each.command;
        }
    }

    private void OnDisable()
    {
        foreach (var each in actions)
        {
            each.action.performed -= each.command;
        }
    }

    public void OnNext(InputAction.CallbackContext c)
    {
        DIALOGUE.DialogueSystem.Instance.OnUserPrompt_Next();
    }

    public void OnHistoryBack(InputAction.CallbackContext c)
    {
        HistoryManager.Instance.GoBack();
    }

    public void OnHistoryForward(InputAction.CallbackContext c)
    {
        HistoryManager.Instance.GoForward();
    }

    public void OnHistoryLogToggle(InputAction.CallbackContext c)
    {
        HistoryLogManager logManager = HistoryManager.Instance.historyLogManager;

        if (logManager.isOpen)
        {
            logManager.Close();
        }
        else
        {
            logManager.Open();
        }
    }
}
