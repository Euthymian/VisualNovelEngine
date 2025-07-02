using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InputPanel : MonoBehaviour
{
    public static InputPanel Instance { get; private set; } = null;

    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Button acceptButton;

    private CanvasGroupController cgc;

    public string lastInput { get; private set; } = string.Empty;

    public bool isWaitingOnUserInput { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        cgc = new CanvasGroupController(this, canvasGroup);

        canvasGroup.alpha = 0f;
        SetCanvasState(false);

        inputField.onValueChanged.AddListener(OnInputValueChanged);
        acceptButton.onClick.AddListener(OnAcceptInput);
        acceptButton.gameObject.SetActive(false);
    }

    public void Show(string title)
    {
        titleText.text = title;
        inputField.text = string.Empty;
        SetCanvasState(true);
        cgc.Show();
        isWaitingOnUserInput = true;
    }

    public void Hide()
    {
        cgc.Hide();
        SetCanvasState(false);
        isWaitingOnUserInput = false;
    }

    public void OnAcceptInput()
    {
        if(inputField.text == string.Empty)
            return;

        lastInput = inputField.text;
        Hide();
    }

    private void SetCanvasState(bool active)
    {
        canvasGroup.interactable = active;
        canvasGroup.blocksRaycasts = active;
    }

    public void OnInputValueChanged(string c)
    {
        acceptButton.gameObject.SetActive(InputFieldHasText());
    }

    private bool InputFieldHasText()
    {
        return !string.IsNullOrEmpty(inputField.text);
    }
}
