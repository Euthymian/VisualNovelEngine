using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
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
        if(file == null)
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
        File.Delete(filePath);
        PopulateDetails(SaveAndLoadMenu.Instance.currentFunction);
    }

    public void Load()
    {
        VNGameSave file = VNGameSave.Load(filePath, true);
        SaveAndLoadMenu.Instance.Close(closeAllMenus: true);

    }

    public void Save()
    {
        var activeSave = VNGameSave.activeFile;
        activeSave.slotNumber = fileNum;
        activeSave.Save();

        PopulateDetailsFromFile(SaveAndLoadMenu.Instance.currentFunction, activeSave);
    }
}
