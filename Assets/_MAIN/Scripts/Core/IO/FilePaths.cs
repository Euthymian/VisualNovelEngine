using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Centralized file paths for the game

public class FilePaths
{
    private const string HOME_DIR_SYMBOL = "~"; 

    // in Editor, Application.dataPath is Assets/
    // in Build, Application.dataPath will be the path where data is stored
    public static readonly string root = $"{Application.dataPath}/gameData/";

    public static readonly string gameSaves = $"{runtimePath}SaveFiles/";

    //Resources paths
    public static readonly string resources_font = "Fonts/";

    public static readonly string resources_graphics = "Graphics/";
    public static readonly string resources_bgImages = $"{resources_graphics}BG Images/";
    public static readonly string resources_bgVideos = $"{resources_graphics}BG Videos/";
    public static readonly string resources_transitionEffects = $"{resources_graphics}Transition Effects/";

    public static readonly string resources_audio = "Audio/";
    public static readonly string resources_sfx = $"{resources_audio}SFX/";
    public static readonly string resources_voices = $"{resources_audio}Voices/";
    public static readonly string resources_music = $"{resources_audio}Music/";
    public static readonly string resources_ambience = $"{resources_audio}Ambience/";
    
    public static readonly string resources_dialogue = $"Dialogue Files/";

    public static string GetPathToResource(string defaultPath, string resourceName)
    {
        if (resourceName.StartsWith(HOME_DIR_SYMBOL))
            return resourceName.Substring(HOME_DIR_SYMBOL.Length);

        return defaultPath + resourceName;
    }

    public static string runtimePath
    {
        get
        {
            #if UNITY_EDITOR
                return "Assets/appData/";
            #else
                return Application.persistentDataPath + "/appData/";
            #endif
        }
    }
}
