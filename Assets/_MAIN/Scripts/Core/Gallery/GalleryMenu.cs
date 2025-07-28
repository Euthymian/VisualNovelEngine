using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class GalleryMenu : MonoBehaviour
{
    private const int PAGE_BUTTONS_LIMIT = 2;

    private int maxPages = 0;
    private int currentPage = 0;

    [SerializeField] private CanvasGroup rootCG;
    private CanvasGroupController root_cgc;

    [SerializeField] private Texture[] galleryImages;

    [SerializeField] private Button[] galleryPreviewButtons;
    private int previewPerPage => galleryPreviewButtons.Length;
    [SerializeField] private Button backButton;
    [SerializeField] private Button panelSelectionButtonPrefab;
    [SerializeField] private Button nextButton;

    [SerializeField] private CanvasGroup previewCG;
    private CanvasGroupController preview_cgc;

    [SerializeField] private Button previewButton;

    private bool initialized = false;

    private void Start()
    {
        root_cgc = new CanvasGroupController(this, rootCG);
        preview_cgc = new CanvasGroupController(this, previewCG);

        GalleryConfig.Load();

        GetAllGalleryImages();

        //foreach(Texture e in galleryImages)
        //    GalleryConfig.UnlockImage(e.name);
    }

    public void Open()
    {
        if (!initialized)
        {
            initialized = true;
            Initialized();
        }

        root_cgc.Show();
        root_cgc.SetInteractableState(true);
    }

    private void Initialized()
    {
        ConstructNavBar();
        LoadPage(1);
    }

    private void LoadPage(int pageNum)
    {
        int startIndex = (pageNum - 1) * previewPerPage;
        for (int i = 0; i < previewPerPage; i++)
        {
            int index = i + startIndex;
            Button btn = galleryPreviewButtons[i];

            btn.onClick.RemoveAllListeners();

            if (index >= galleryImages.Length)
            {
                btn.transform.parent.gameObject.SetActive(false);
                continue;
            }

            btn.transform.parent.gameObject.SetActive(true);
            RawImage renderer = btn.targetGraphic as RawImage;
            Texture previewImage = galleryImages[index];

            if (GalleryConfig.IsImageUnlocked(previewImage.name))
            {
                renderer.color = Color.white;
                renderer.texture = previewImage;

                btn.onClick.AddListener(() => ShowPreviewImage(previewImage));  
            }
            else
            {
                renderer.color = Color.black;
                renderer.texture = null;
            }
        }

        currentPage = pageNum;
    }

    private void ShowPreviewImage(Texture image)
    {
        RawImage renderer = previewButton.targetGraphic as RawImage;
        renderer.texture = image;
        preview_cgc.Show();
        preview_cgc.SetInteractableState(true);
    }

    public void HidePreviewImage()
    {
        preview_cgc.Hide();
        preview_cgc.SetInteractableState(false);
    }

    private void ConstructNavBar()
    {
        // Our system will look at Gallery folder to see how many images we have
        int totalImages = galleryImages.Length;

        maxPages = (int)Mathf.Ceil((float)totalImages / previewPerPage);
        int pageLimit = Mathf.Min(maxPages, PAGE_BUTTONS_LIMIT);

        for (int i = 1; i <= pageLimit; i++)
        {
            GameObject newButtonOb = Instantiate(panelSelectionButtonPrefab.gameObject, panelSelectionButtonPrefab.transform.parent);
            newButtonOb.SetActive(true);
            newButtonOb.name = i.ToString();

            Button newBtn = newButtonOb.GetComponent<Button>();
            TextMeshProUGUI btnText = newButtonOb.GetComponentInChildren<TextMeshProUGUI>();
            btnText.text = i.ToString();

            int page = i; // Capture the current value of i for the lambda
            newBtn.onClick.AddListener(() => LoadPage(page));
        }

        backButton.gameObject.SetActive(pageLimit < maxPages);
        nextButton.gameObject.SetActive(pageLimit < maxPages);

        nextButton.transform.SetAsLastSibling();
    }

    private void GetAllGalleryImages()
    {
        galleryImages = Resources.LoadAll<Texture>(FilePaths.resources_gallery);
    }

    public void Close()
    {
        root_cgc.Hide();
        root_cgc.SetInteractableState(false);
    }

    public void OnNextPage()
    {
        if(currentPage < maxPages)
            LoadPage(currentPage + 1);
    }

    public void OnBackPage()
    {
        if (currentPage > 1)
            LoadPage(currentPage - 1);
    }
}
