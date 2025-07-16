using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace HISTORY
{
    public class HistoryLogManager : MonoBehaviour
    {
        private const float LOG_STARTING_HEIGHT = 1;
        private const float LOG_HEIGHT_INCREMENT_PER_LINE = 1;
        private const float LOG_DEFAULT_HEIGHT = 1;
        private const float TEXT_DEFAULT_SCALE = 0.7f;

        private const string SPEAKER_NAME_TEXT = "SpeakerNameText";
        private const string DIALOGUE_TEXT = "DialogueText";

        private float logScaling = 1;
        private float textScaling => logScaling * 3;

        [SerializeField] private Animator anim;
        [SerializeField] private GameObject logPrefab;

        HistoryManager historyManager => HistoryManager.Instance;
        private List<HistoryLog> historyLogs = new List<HistoryLog>();

        public bool isOpen { get; private set; } = false;

        [SerializeField] private Slider logScaleSlider;

        public void Open()
        {
            if(isOpen) return;

            anim.Play("Open");

            isOpen = true;
        }
        
        public void Close()
        {
            if(!isOpen) return;

            anim.Play("Close");

            isOpen = false;
        }

        public void AddLog(HistoryState state)
        {
            if(historyLogs.Count >= historyManager.HISTORY_CACHE_LIMIT)
            {
                DestroyImmediate(historyLogs[0].container);
                historyLogs.RemoveAt(0);
            }

            CreateLog(state);
        }

        private void CreateLog(HistoryState state)
        {
            HistoryLog log = new HistoryLog();

            log.container = Instantiate(logPrefab, logPrefab.transform.parent);
            log.container.SetActive(true);

            log.speakerNameText = log.container.transform.Find(SPEAKER_NAME_TEXT).GetComponent<TextMeshProUGUI>();
            log.dialogueText = log.container.transform.Find(DIALOGUE_TEXT).GetComponent<TextMeshProUGUI>();

            if(state.dialogueData.currentSpeaker == string.Empty)
                log.speakerNameText.text = string.Empty;
            else
            {
                log.speakerNameText.text = state.dialogueData.currentSpeaker;
                log.speakerNameText.font = HistoryCache.LoadFont(state.dialogueData.speakerNameFont);
                log.speakerNameText.color = state.dialogueData.speakerNameColor;
                log.speakerFontSize = TEXT_DEFAULT_SCALE * state.dialogueData.speakerNameSize;
                log.speakerNameText.fontSize = log.speakerFontSize + textScaling;
            }

            log.dialogueText.text = state.dialogueData.currentDialogue;
            log.dialogueText.font = HistoryCache.LoadFont(state.dialogueData.dialogueFont);
            log.dialogueText.color = state.dialogueData.dialogueColor;
            log.dialogueFontSize = TEXT_DEFAULT_SCALE * state.dialogueData.dialogueSize;
            log.dialogueText.fontSize = log.dialogueFontSize + textScaling;

            FitLogToText(log);

            historyLogs.Add(log);
        }

        private void FitLogToText(HistoryLog log)
        {
            RectTransform rect = log.dialogueText.GetComponent<RectTransform>();
            ContentSizeFitter textCSF = log.dialogueText.GetComponent<ContentSizeFitter>();
            LayoutElement layout = log.container.GetComponent<LayoutElement>();

            textCSF.SetLayoutVertical();

            float height = rect.rect.height;
            float ratio = height / LOG_DEFAULT_HEIGHT;
            float extraScale = (LOG_HEIGHT_INCREMENT_PER_LINE * ratio) - LOG_HEIGHT_INCREMENT_PER_LINE; //Exclude the first line
            float scale = LOG_STARTING_HEIGHT + extraScale;

            layout.preferredHeight = scale * textScaling;

            layout.preferredHeight += 2 * logScaling; // Add some padding
        }

        public void SetLogScaling()
        {
            logScaling = logScaleSlider.value;

            foreach (HistoryLog log in historyLogs)
            {
                log.speakerNameText.fontSize = log.speakerFontSize + textScaling;
                log.dialogueText.fontSize = log.dialogueFontSize + textScaling;
                FitLogToText(log);
            }
        }

        public void ClearLogs()
        {
            foreach (HistoryLog log in historyLogs)
                DestroyImmediate(log.container);

            historyLogs.Clear();
        }
    }
}