using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Centralized file paths for the game

public class FilePaths
{
    // in Editor, Application.dataPath is Assets/
    // in Build, Application.dataPath will be the path where data is stored
    public static readonly string root = $"{Application.dataPath}/gameData/";

    //Resources paths
    public static readonly string resources_graphics = "Graphics/";
    public static readonly string resources_bgImages = $"{resources_graphics}BG Images/";
    public static readonly string resources_bgVideos = $"{resources_graphics}BG Videos/";
    public static readonly string resources_transitionEffects = $"{resources_graphics}Transition Effects/";
}
