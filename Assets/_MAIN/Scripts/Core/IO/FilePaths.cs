using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Centralized file paths for the game

public class FilePaths
{
    // in Editor, Application.dataPath is Assets/
    // in Build, Application.dataPath will be the path where data is stored
    public static readonly string root = $"{Application.dataPath}/gameData/";
}
