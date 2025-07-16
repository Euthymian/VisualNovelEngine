using DIALOGUE;
using HISTORY;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HistoryNavigation : MonoBehaviour
{
    public int progress = 0;

    [SerializeField] private TextMeshProUGUI statusText;

    HistoryManager historyManager => HistoryManager.Instance;
    List<HistoryState> historyStateList => historyManager.historyStateList;

    //When we decide to go back to history, we cant return to where we are now because we havent captured yet, so we cache the current state
    HistoryState cacheState = null;
    private bool isOnCacheState = false;

    public bool isViewingHistory = false;

    public bool canNavigate => !DialogueSystem.Instance.conversationManager.isOnLogicalLine;

    public void GoForward()
    {
        if (!isViewingHistory || !canNavigate)
            return;

        HistoryState state = null;
        
        if(progress < historyStateList.Count - 1)
        {
            progress++;
            state = historyStateList[progress];
        }
        else
        {
            isOnCacheState = true;
            state = cacheState;
        }

        state.Load();

        if (isOnCacheState)
        {
            isViewingHistory = false;
            isOnCacheState = false;
            cacheState = null;
            DialogueSystem.Instance.onUserPrompt_Next -= GoForward;
            DialogueSystem.Instance.OnStopViewingHistory();
            statusText.text = "";
        }
        else
            UpdateStatusText();
    }

    public void GoBack()
    {
        if (historyStateList.Count == 0 || (progress == 0 && isViewingHistory) || !canNavigate)
            return;

        //if has valid state to go back, go back 1 state, else start the lastest state
        progress = isViewingHistory ? progress - 1 : historyStateList.Count - 1;

        if (!isViewingHistory)
        {
            isViewingHistory = true;
            isOnCacheState = false;
            cacheState = HistoryState.Capture();

            DialogueSystem.Instance.onUserPrompt_Next += GoForward;
            DialogueSystem.Instance.OnStartViewingHistory();
        }

        HistoryState state = historyStateList[progress];
        state.Load();
        UpdateStatusText();
    }

    private void UpdateStatusText()
    {
        statusText.text = $"{progress + 1}/{historyStateList.Count}";
    }
}
