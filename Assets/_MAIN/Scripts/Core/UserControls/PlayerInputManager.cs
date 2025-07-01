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
        actions.Add((playerInput.actions["Next"], PromptSpeedUpNext));
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

    public void PromptSpeedUpNext(InputAction.CallbackContext c)
    {
        DIALOGUE.DialogueSystem.Instance.OnUserPrompt_Next();
    }
}
