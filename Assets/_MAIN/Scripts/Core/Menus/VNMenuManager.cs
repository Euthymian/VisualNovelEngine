using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VNMenuManager : MonoBehaviour
{
    public static VNMenuManager Instance { get; private set; }

    [SerializeField] private CanvasGroup rootCanvasGroup;
    [SerializeField] private MenuPage[] pageList;

    private bool isOpen = false;

    private MenuPage activePage = null;

    private CanvasGroupController cgc;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        cgc = new CanvasGroupController(this, rootCanvasGroup);
    }

    private void OpenRoot()
    {
        cgc.Show();
        cgc.SetInteractableState(true);
        isOpen = true;
    }

    public void CloseRoot()
    {
        cgc.Hide();
        cgc.SetInteractableState(false);
        isOpen = false;
    }

    public MenuPage GetPageByType(MenuPage.PageType type)
    {
        return pageList.FirstOrDefault(page => page.pageType == type);
    }

    private void OpenPage(MenuPage page)
    {
        if (page == null)
            return;

        if (activePage != null && activePage != page)
            activePage.Close();

        page.Open();
        activePage = page;

        if (!isOpen)
            OpenRoot();
    }

    public void OpenSavePage()
    {
        var page = GetPageByType(MenuPage.PageType.SaveAndLoad);
        var slm = page.anim.GetComponentInParent<SaveAndLoadMenu>();
        slm.currentFunction = SaveAndLoadMenu.MenuFunction.Save;

        OpenPage(page);
    }

    public void OpenLoadPage()
    {
        var page = GetPageByType(MenuPage.PageType.SaveAndLoad);
        var slm = page.anim.GetComponentInParent<SaveAndLoadMenu>();
        slm.currentFunction = SaveAndLoadMenu.MenuFunction.Load;

        OpenPage(page);
    }

    public void OpenConfigPage()
    {
        var page = GetPageByType(MenuPage.PageType.Config);

        OpenPage(page);
    }

    public void OpenHelpPage()
    {
        var page = GetPageByType(MenuPage.PageType.Help);

        OpenPage(page);
    }
}
