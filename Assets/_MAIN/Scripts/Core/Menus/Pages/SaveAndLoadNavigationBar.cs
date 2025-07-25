using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class SaveAndLoadNavigationBar : MonoBehaviour
{
    [SerializeField] private SaveAndLoadMenu menu;

    private bool initialized = false;

    [SerializeField] private GameObject buttonPrefab;
    [SerializeField] private Button backButton;
    [SerializeField] private Button nextButton;

    private const int MAX_PAGE_BUTTONS = 5;

    public int selectedPage { get; private set; } = 1;
    private int maxPages = 0;

    private void Start()
    {
        InitializeMenu();
    }

    private void InitializeMenu()
    {
        if(initialized)
            return;

        initialized = true;

        maxPages = Mathf.CeilToInt((float)SaveAndLoadMenu.MAX_FILES / menu.slotsPerPage);
        int pageButtonsLimit = MAX_PAGE_BUTTONS < maxPages ? MAX_PAGE_BUTTONS : maxPages;

        for(int i = 1; i <= pageButtonsLimit; i++)
        {
            GameObject ob = Instantiate(buttonPrefab, buttonPrefab.transform.parent);
            ob.SetActive(true);
            ob.name = i.ToString();

            Button button = ob.GetComponent<Button>();
            TextMeshProUGUI btnText = button.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = i.ToString();
            int iCopy = i; // Capture the current value of i for the listener, unless all buttons will tie to the last value of i.
            button.onClick.AddListener(() => SelectSaveFilePage(iCopy));
        }

        backButton.gameObject.SetActive(pageButtonsLimit < maxPages);
        nextButton.gameObject.SetActive(pageButtonsLimit < maxPages);

        nextButton.transform.SetAsLastSibling();
    }

    private void SelectSaveFilePage(int pageNum)
    {
        selectedPage = pageNum;
        menu.PopulateSaveSlots(selectedPage);
    }

    public void NextPage()
    {
        if (selectedPage < maxPages)
            SelectSaveFilePage(selectedPage + 1);
    }

    public void PreviousPage()
    {
        if (selectedPage > 1)
            SelectSaveFilePage(selectedPage - 1);
    }
}
