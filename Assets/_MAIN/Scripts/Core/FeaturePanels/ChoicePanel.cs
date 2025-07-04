using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ChoicePanel : MonoBehaviour
{
    private const float BUTTON_MIN_WIDTH = 50;
    private const float BUTTON_MAX_WIDTH = 1000;
    private const float BUTTON_WIDTH_PADDING = 25;
    private const float BUTTON_HEIGHT_PER_LINE = 50;
    private const float BUTTON_HEIGHT_PADDING = 20;

    public class ChoicePanelDecision
    {
        public string question = string.Empty;
        public int answerIndex = -1;
        public string[] choices = new string[0];
        public ChoicePanelDecision(string question, string[] choices)
        {
            this.question = question;
            this.choices = choices;
            answerIndex = -1;
        }
    }

    private struct ChoiceButton
    {
        public Button button;
        public TextMeshProUGUI title;
        public LayoutElement layoutElement;
    }

    public static ChoicePanel Instance { get; private set; }

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private GameObject choiceButtonPrefab;
    [SerializeField] private VerticalLayoutGroup buttonLayoutGroup;

    private CanvasGroupController cgc;
    private List<ChoiceButton> buttonList = new List<ChoiceButton>();

    public bool isWaitingOnUserChoice { get; private set; } = false;

    public ChoicePanelDecision lastDecision { get; private set; } = null;

    private void Awake()
    {
        Instance = this;

        cgc = new CanvasGroupController(this, canvasGroup);

        cgc.alpha = 0f;
        cgc.SetInteractableState(false);
    }

    private void Start()
    {
        
    }

    public void Show(string question, string[] choices)
    {
        lastDecision = new ChoicePanelDecision(question, choices);

        isWaitingOnUserChoice = true;

        cgc.SetInteractableState(true);
        cgc.Show();

        titleText.text = question;
        StartCoroutine(GenerateChoices(choices));
    }

    private IEnumerator GenerateChoices(string[] choices)
    {
        float maxWidth = 0f;

        // To optimize performance, we reuse existing buttons if they are available (and alter data), create new ones only if needed
        for (int i= 0; i < choices.Length; i++)
        {
            ChoiceButton choiceButton;
            if (i < buttonList.Count)
            {
                choiceButton = buttonList[i];
            }
            else
            {
                GameObject newButtonObject = Instantiate(choiceButtonPrefab, buttonLayoutGroup.transform);
                newButtonObject.SetActive(true);
                
                Button newButton = newButtonObject.GetComponent<Button>();
                TextMeshProUGUI newTitle = newButtonObject.GetComponentInChildren<TextMeshProUGUI>();
                LayoutElement newLayoutElement = newButtonObject.GetComponent<LayoutElement>();

                choiceButton = new ChoiceButton
                {
                    button = newButton,
                    title = newTitle,
                    layoutElement = newLayoutElement
                };

                buttonList.Add(choiceButton); 
            }

            choiceButton.button.onClick.RemoveAllListeners();
            int buttonIndex = i;
            choiceButton.button.onClick.AddListener(() => AcceptAnswer(buttonIndex));
            /*
            Cant pass the index i directly to the listener because it will always be the last value of i (= choices.Length) when the listener is called
                choiceButton.button.onClick.AddListener(() => AcceptAnswer(i));
            => cache the value of i in a local variable buttonIndex
            */
            choiceButton.title.text = choices[i];

            float buttonWidth = Mathf.Clamp(BUTTON_WIDTH_PADDING + choiceButton.title.preferredWidth, BUTTON_MIN_WIDTH, BUTTON_MAX_WIDTH);
            maxWidth = Mathf.Max(maxWidth, buttonWidth);
        }

        foreach (var button in buttonList)
        {
            button.layoutElement.preferredWidth = maxWidth;
        }

        for(int i=0; i<buttonList.Count; i++)
        {
            bool show = i< choices.Length;
            buttonList[i].button.gameObject.SetActive(show);
        }

        // Reason for coroutine and wait before update button height is because we want the height is responsive to number of text lines
        // but we only get proper number of text lines after the UI has been updated
        // So we wait for the end of frame to ensure that the text has been rendered and the line count is accurate
        yield return new WaitForEndOfFrame();

        foreach(var button in buttonList)
        {
            int lines = button.title.textInfo.lineCount;
            button.layoutElement.preferredHeight = BUTTON_HEIGHT_PER_LINE * lines + BUTTON_HEIGHT_PADDING;
        }

    }

    public void Hide()
    {
        cgc.SetInteractableState(false);
        cgc.Hide();
    }

    private void AcceptAnswer(int index)
    {
        if(index < 0 || index >= lastDecision.choices.Length)
            return;
        lastDecision.answerIndex = index;
        isWaitingOnUserChoice = false;
        Hide();
    }
}
