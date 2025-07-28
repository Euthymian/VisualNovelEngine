using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HISTORY
{
    [RequireComponent(typeof(HistoryNavigation))]
    [RequireComponent(typeof(HistoryLogManager))]
    public class HistoryManager : MonoBehaviour
    {
        public int HISTORY_CACHE_LIMIT = 10;
        public static HistoryManager Instance { get; private set; }

        public List<HistoryState> historyStateList = new List<HistoryState>();

        public HistoryNavigation historyNavigation;
        public HistoryLogManager historyLogManager { get; private set; }

        public bool isViewingHistory => historyNavigation.isViewingHistory;

        private void Awake()
        {
            Instance = this;
            historyNavigation = GetComponent<HistoryNavigation>();
            historyLogManager = GetComponent<HistoryLogManager>();
        }

        private void Start()
        {
            DialogueSystem.Instance.onClear += LogCurrentState; 
        }

        public void LogCurrentState()
        {
            HistoryState currentState = HistoryState.Capture();
            historyStateList.Add(currentState);
            historyLogManager.AddLog(currentState);

            if (historyStateList.Count > HISTORY_CACHE_LIMIT)
                historyStateList.RemoveAt(0);
        }

        public void LoadState(HistoryState state)
        {
            state.Load();
        }

        public void GoForward() => historyNavigation.GoForward();
        public void GoBack() => historyNavigation.GoBack();
    }
}