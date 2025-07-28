using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VN_Configuration
{
    public static VN_Configuration activeConfig;

    public const bool ENCRYPT = false;

    public static string filePath => $"{FilePaths.root}vnconfig.cfg";

    //General Settings
    public bool display_fullscreen = true;
    public string display_resolution = "1920x1080";
    public bool continueSkipAfterChoice = false;
    public float dialougeTextSpeed = 1;
    public float dialougeAutoReadSpeed = 1;

    //Audio Settings
    public float musicVolume = 1;
    public float sfxVolume = 1;
    public float voicesVolume = 1;
    public bool musicMuted = false;
    public bool sfxMuted = false;
    public bool voicesMuted = false;

    //Other Settings
    public float historyLogScale = 1;

    public void Load()
    {
        var ui = ConfigMenu.Instance.UI;

        // Apply general settings
        ConfigMenu.Instance.SetDisplayToFullScreen(display_fullscreen);
        ui.SetButtonColors(ui.fullscreen, ui.windowed, display_fullscreen);

        int resIndex = 0;
        for(int i=0; i < ui.resolution.options.Count; i++)
        {
            string res = ui.resolution.options[i].text;
            if(res == display_resolution)
            {
                resIndex = i;
                break;
            }
        }
        ui.resolution.value = resIndex;

        ui.SetButtonColors(ui.continueSkipAfterChoice, ui.stopSkipAfterChoice, continueSkipAfterChoice);

        ui.textSpeed.value = dialougeTextSpeed;
        ui.autoReadSpeed.value = dialougeAutoReadSpeed;

        // Apply audio settings
        ui.music.value = musicVolume;
        ui.sfx.value = sfxVolume;
        ui.voices.value = voicesVolume;

        ui.musicMute.sprite = musicMuted ? ui.volumeOffSprite : ui.volumeOnSprite;
        ui.sfxMute.sprite = sfxMuted ? ui.volumeOffSprite : ui.volumeOnSprite;
        ui.voicesMute.sprite = voicesMuted ? ui.volumeOffSprite : ui.volumeOnSprite;
    }

    public void Save()
    {
        FileManager.Save(filePath, JsonUtility.ToJson(this), ENCRYPT);
    }
}
