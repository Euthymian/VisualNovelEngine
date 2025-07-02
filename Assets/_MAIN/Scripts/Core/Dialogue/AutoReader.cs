using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace DIALOGUE
{
    public class AutoReader : MonoBehaviour
    {
        private const int DEFAULT_CHARACTERS_READ_PER_SECOND = 18;
        private const float READ_TIME_PADDING = 0.5f;
        private const float MINIMUM_READ_TIME = 1f;
        private const float MAXIMUM_READ_TIME = 99f;
        private const string STATUS_TEXT_AUTO = "Auto";
        private const string STATUS_TEXT_SKIP = "Skip";

        private ConversationManager conversationManager;
        private TextArchitect textArchitect => conversationManager.textArchitect;

        public bool skip { get; set; } = false;
        public float speed { get; set; } = 1f;

        private Coroutine co_running = null;
        public bool isOn => co_running != null;

        [SerializeField] private TextMeshProUGUI statusText;

        public void Initialize(ConversationManager conversationManager)
        {
            this.conversationManager = conversationManager;
            statusText.text = string.Empty;
        }

        public void Enable()
        {
            if (isOn)
                return;

            co_running = StartCoroutine(AutoRead());
        }

        public void Disable()
        {
            if (!isOn)
                return;

            StopCoroutine(co_running);
            skip = false;
            co_running = null;
            statusText.text = string.Empty;
        }

        private IEnumerator AutoRead()
        {
            if (!conversationManager.isRunning)
            {
                Disable();
                yield break;
            }

            if (!textArchitect.isBuilding && textArchitect.currentText != string.Empty)
                DialogueSystem.Instance.OnSystemPrompt_Next();

            while (conversationManager.isRunning)
            {
                //Read and wait
                if (!skip)
                {
                    // If no text is generated, wait for it a frame
                    while (!textArchitect.isBuilding && !conversationManager.isWaitingForAutoTimer)
                        yield return null;

                    float timeStart = Time.time;

                    while (textArchitect.isBuilding ||  conversationManager.isWaitingForAutoTimer)
                        yield return null;

                    float timeToRead = Mathf.Clamp(((float)textArchitect.tmpro.textInfo.characterCount / DEFAULT_CHARACTERS_READ_PER_SECOND), MINIMUM_READ_TIME, MAXIMUM_READ_TIME);
                    timeToRead = Mathf.Clamp((timeToRead - (Time.time - timeStart)), MINIMUM_READ_TIME, MAXIMUM_READ_TIME);
                    timeToRead = (timeToRead/speed) + READ_TIME_PADDING;

                    yield return new WaitForSeconds(timeToRead);
                }
                else
                {
                    textArchitect.ForceComplete();
                    yield return new WaitForSeconds(0.05f);
                }

                DialogueSystem.Instance.OnSystemPrompt_Next();
            }

            Disable();
        }

        public void Toggle_Auto()
        {
            bool prevSkip = skip;
            skip = false;

            if (prevSkip)
                Enable();
            else
            {
                if(!isOn)
                    Enable();
                else
                    Disable();
            }

            if(isOn)
                statusText.text = STATUS_TEXT_AUTO;
        }

        public void Toggle_Skip()
        {
            bool prevSkip = skip;
            skip = true;

            if (!prevSkip)
                Enable();
            else
            {
                if (!isOn)
                    Enable();
                else
                    Disable();
            }

            if (isOn)
                statusText.text = STATUS_TEXT_SKIP;
        }
    }
}