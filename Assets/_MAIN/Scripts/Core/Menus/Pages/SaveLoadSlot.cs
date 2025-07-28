using HISTORY;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VISUALNOVEL;

public class SaveLoadSlot : MonoBehaviour
{
    public GameObject root;
    public RawImage previewImage;
    public TextMeshProUGUI infoText;
    public Button deleteButton;
    public Button loadButton;
    public Button saveButton;

    [HideInInspector] public int fileNum = 0;
    [HideInInspector] public string filePath = string.Empty;

    public void PopulateDetails(SaveAndLoadMenu.MenuFunction function)
    {
        if (File.Exists(filePath))
        {
            VNGameSave file = VNGameSave.Load(filePath);
            PopulateDetailsFromFile(function, file);
        }
        else
            PopulateDetailsFromFile(function, null); // Empty slot
    }

    private void PopulateDetailsFromFile(SaveAndLoadMenu.MenuFunction func, VNGameSave file)
    {
        if (file == null)
        {
            infoText.text = $"{fileNum}. Empty Slot";
            deleteButton.gameObject.SetActive(false);
            loadButton.gameObject.SetActive(false);
            saveButton.gameObject.SetActive(func == SaveAndLoadMenu.MenuFunction.Save);
            previewImage.texture = SaveAndLoadMenu.Instance.emptyFileImage;
        }
        else
        {
            infoText.text = $"{fileNum}. {file.playerName} - {file.timeStamp}";
            deleteButton.gameObject.SetActive(true);
            loadButton.gameObject.SetActive(func == SaveAndLoadMenu.MenuFunction.Load);
            saveButton.gameObject.SetActive(func == SaveAndLoadMenu.MenuFunction.Save);

            byte[] screenshotData = File.ReadAllBytes(file.screenshotPath);
            Texture2D screenshot = new Texture2D(1, 1);
            ImageConversion.LoadImage(screenshot, screenshotData);
            previewImage.texture = screenshot;
        }
    }

    public void Delete()
    {
        UIConfirmationMenu.Instance.Show(
            // Title
            "Are you sure you want to delete this save file? (<i>This cant be undone</i>)",
            // Choice 1
            new UIConfirmationMenu.ConfirmationButton("Yes", () =>
                {
                    UIConfirmationMenu.Instance.Show("Confirm Deletion",
                        new UIConfirmationMenu.ConfirmationButton("Delete", OnConfirmedDelete, true),
                        new UIConfirmationMenu.ConfirmationButton("Cancel", null, true));
                }, 
            autoCloseOnQuit: false),
            // Choice 2
            new UIConfirmationMenu.ConfirmationButton("No", null, true));
    }

    private void OnConfirmedDelete()
    {
        File.Delete(filePath);
        PopulateDetails(SaveAndLoadMenu.Instance.currentFunction);
    }

    public void Load()
    {
        VNGameSave file = VNGameSave.Load(filePath, false);
        SaveAndLoadMenu.Instance.Close(closeAllMenus: true);

        if (SceneManager.GetActiveScene().name == MainMenu.MAIN_MENU_SCENE)
        {
            MainMenu.Instance.LoadGame(file);
        }
        else
        {
            file.Activate();
        }
    }

    public void Save()
    {
        if (HistoryManager.Instance.isViewingHistory) 
        {
            UIConfirmationMenu.Instance.Show("Save while viewing history?", new UIConfirmationMenu.ConfirmationButton("Go Back", null));
            return;
        }

        var activeSave = VNGameSave.activeFile;
        activeSave.slotNumber = fileNum;
        activeSave.Save();

        PopulateDetailsFromFile(SaveAndLoadMenu.Instance.currentFunction, activeSave);
    }
}
