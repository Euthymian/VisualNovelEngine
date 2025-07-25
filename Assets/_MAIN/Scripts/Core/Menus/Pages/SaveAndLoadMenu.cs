using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VISUALNOVEL;

public class SaveAndLoadMenu : MenuPage
{
    public static SaveAndLoadMenu Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    public const int MAX_FILES = 24;
    private string savePath = FilePaths.gameSaves;
    private int currentPage = 1;
    private bool loadedFilesFirstTime = false;

    public enum MenuFunction{ Save, Load }

    public MenuFunction currentFunction = MenuFunction.Save;

    public SaveLoadSlot[] saveSlots; //Default to 6
    public int slotsPerPage => saveSlots.Length;

    public Texture emptyFileImage;

    public override void Open()
    {
        base.Open();

        if (!loadedFilesFirstTime)
        {
            // Remove loadedFilesFirstTime ?
            PopulateSaveSlots(currentPage);
        }
    }

    public void PopulateSaveSlots(int pageNum)
    {
        int startFile = (pageNum - 1) * slotsPerPage + 1;
        int endFile = startFile + slotsPerPage - 1;

        for(int i = 0; i < slotsPerPage; i++)
        {
            int fileNum = startFile + i;
            SaveLoadSlot slot = saveSlots[i];

            if(fileNum <= MAX_FILES)
            {
                slot.root.SetActive(true);
                string filePath = $"{FilePaths.gameSaves}{fileNum}{VNGameSave.FILE_TYPE}";
                slot.fileNum = fileNum;
                slot.filePath = filePath;
                slot.PopulateDetails(currentFunction);
            }
            else
            {
                slot.root.SetActive(false);
            }
        }
    }
}
