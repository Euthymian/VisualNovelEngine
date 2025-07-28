using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "VisualNovelSO", menuName = "VNSystem/VisualNovelSO")]
public class VisualNovelSO : ScriptableObject
{
    public TextAsset firstChapter;
    public string pathToFirstChapter => $"Dialogue Files/{firstChapter.name}";
}
