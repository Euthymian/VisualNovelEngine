using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIConfirmationMenu : MonoBehaviour
{
    public struct ConfirmationButton
    {
        public string title;
        public System.Action action;
        public bool autoCloseOnQuit;

        public ConfirmationButton(string title, System.Action action, bool autoCloseOnQuit = true)
        {
            this.title = title;
            this.action = action;
            this.autoCloseOnQuit = autoCloseOnQuit;
        }
    }

    public static UIConfirmationMenu Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    [SerializeField] private Animator anim;
    [SerializeField] private TextMeshProUGUI title;
    [SerializeField] private LayoutGroup choiceLayoutGroup;
    [SerializeField] private GameObject choiceButtonPrefab;

    private GameObject[] activeObjects = new GameObject[0];

    public void Show(string title, params ConfirmationButton[] options)
    {
        if (options.Length == 0)
        {
            Debug.LogError("No options provided for confirmation menu.");
            return;
        }

        CreateOptionButtons(options);

        this.title.text = title;
        anim.Play("Enter");
    }

    public void Hide()
    {
        anim.Play("Exit");
    }

    private void CreateOptionButtons(ConfirmationButton[] options)
    {
        foreach (GameObject obj in activeObjects)
        {
            Destroy(obj);
        }

        activeObjects = new GameObject[options.Length];

        for (int i = 0; i < options.Length; i++)
        {
            ConfirmationButton option = options[i];
            GameObject buttonObj = Instantiate(choiceButtonPrefab, choiceLayoutGroup.transform);
            buttonObj.SetActive(true);

            Button button = buttonObj.GetComponent<Button>();
            TextMeshProUGUI buttonText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = option.title;

            if (option.action != null)
                button.onClick.AddListener(() => option.action.Invoke());
            if (option.autoCloseOnQuit)
                button.onClick.AddListener(() => Hide());
            
            activeObjects[i] = buttonObj;
        }
    }
}
