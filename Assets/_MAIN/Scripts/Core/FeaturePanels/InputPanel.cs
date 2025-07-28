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

        cgc.alpha = 0f;
        cgc.SetInteractableState(false);

        inputField.onValueChanged.AddListener(OnInputValueChanged);
        acceptButton.onClick.AddListener(OnAcceptInput);
        acceptButton.gameObject.SetActive(false);
    }

    public void Show(string title)
    {
        titleText.text = title;
        inputField.text = string.Empty;
        cgc.SetInteractableState(true);
        cgc.Show();
        isWaitingOnUserInput = true;
    }

    public void Hide()
    {
        cgc.Hide();
        cgc.SetInteractableState(false);
        isWaitingOnUserInput = false;
    }

    public void OnAcceptInput()
    {
        if (inputField.text == string.Empty)
            return;

        // Censor the input if needed
        string inputText = inputField.text;
        if (CensorManager.Censor(ref inputText))
        {
            UIConfirmationMenu.Instance.Show(
                "Bro, what did you just type?", 
                new UIConfirmationMenu.ConfirmationButton("Again", () => inputField.text = "")
            );
        }
        else
        {
            lastInput = inputField.text;
            Hide();
        }
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
