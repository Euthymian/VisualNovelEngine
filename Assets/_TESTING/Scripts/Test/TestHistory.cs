using GRAPHIC;
using HISTORY;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestHistory : MonoBehaviour
{
    //public DialogueData dialogueData;
    //public List<AudioData> audioDatas;
    //public List<GraphicData> graphicDatas;
    //public List<CharacterData> characterDatas;

    public HistoryState historyState = new HistoryState();

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.H))
            historyState = HistoryState.Capture();

        if(Input.GetKeyDown(KeyCode.L))
            historyState.Load();

        //dialogueData = DialogueData.Capture();
        //audioDatas = AudioData.Capture();
        //graphicDatas = GraphicData.Capture();
        //characterDatas = CharacterData.Capture();
    }
}
