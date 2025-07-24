using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VISUALNOVEL;

public class TestVNGameSave : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        VNGameSave.activeFile = new VNGameSave();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            VNGameSave.activeFile.Save();
        }
        else if (Input.GetKeyDown(KeyCode.L))
        {
            try
            {
                VNGameSave.Load($"{FilePaths.gameSaves}1{VNGameSave.FILE_TYPE}", activateOnLoad: true);
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Failed to load JSON VNGameSave: {e.ToString()}");
            }
        }
    }
}
