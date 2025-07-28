using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class ConfigMenu : MenuPage
{
    public static ConfigMenu Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    [System.Serializable]
    public class UI_ITEMS
    {
        private static Color SELECTED_BUTTON_COLOR = new Color(1, 0.6f, 0, 1);
        private static Color SELECTED_TEXT_BUTTON_COLOR = new Color(1, 1, 0, 1);
        private static Color UNSELECTED_BUTTON_COLOR = new Color(1, 1, 1, 1);
        private static Color UNSELECTED_TEXT_BUTTON_COLOR = new Color(0.25f, 0.25f, 0.25f, 1);
        public static Color musicOnColor = new Color(1, 0.65f, 0, 1);
        public static Color musicOffColor = new Color(0.5f, 0.5f, 0.5f, 1);

        [Header("General")]
        public Button fullscreen;
        public Button windowed;
        public TMP_Dropdown resolution;
        public Button continueSkipAfterChoice, stopSkipAfterChoice;
        public Slider textSpeed, autoReadSpeed;

        [Header("Audio")]
        public Slider music;
        // Reason for this is becuase if we do music.fillRect.GetComponent<Image>(), it may not work (NullReference Exception) if the Slider is disabled on start
        public Image musicFill; 
        public Slider sfx;
        public Image sfxFill;
        public Slider voices;
        public Image voicesFill;
        public Sprite volumeOnSprite;
        public Sprite volumeOffSprite;
        public Image musicMute, sfxMute, voicesMute;

        public void SetButtonColors(Button A, Button B, bool selectedA)
        {
            A.GetComponent<Image>().color = selectedA ? SELECTED_BUTTON_COLOR : UNSELECTED_BUTTON_COLOR;
            B.GetComponent<Image>().color = selectedA ? UNSELECTED_BUTTON_COLOR : SELECTED_BUTTON_COLOR;

            A.GetComponentInChildren<TextMeshProUGUI>().color = selectedA ? SELECTED_TEXT_BUTTON_COLOR : UNSELECTED_TEXT_BUTTON_COLOR;
            B.GetComponentInChildren<TextMeshProUGUI>().color = selectedA ? UNSELECTED_TEXT_BUTTON_COLOR : SELECTED_TEXT_BUTTON_COLOR;
        }
    }

    [SerializeField] private GameObject[] panels;
    private GameObject activePanel;

    public UI_ITEMS UI;


    private void Start()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].SetActive(i == 0);
        }

        activePanel = panels[0];

        SetAvailableResolutions();

        LoadConfigFromFile();
    }

    private void SetAvailableResolutions()
    {
        // Get all resolutions available on the screen
        Resolution[] resolutions = Screen.resolutions;

        List<string> options = new List<string>();

        for(int i= resolutions.Length - 1; i >= 0; i--)
        {
            if (!options.Contains($"{resolutions[i].width}x{resolutions[i].height}"))
            {
                options.Add($"{resolutions[i].width}x{resolutions[i].height}");
            }
        }

        UI.resolution.ClearOptions();
        UI.resolution.AddOptions(options);
    }

    private void LoadConfigFromFile()
    {
        if (File.Exists(VN_Configuration.filePath))
            VN_Configuration.activeConfig = FileManager.Load<VN_Configuration>(VN_Configuration.filePath, VN_Configuration.ENCRYPT);
        else
            VN_Configuration.activeConfig = new VN_Configuration();

        VN_Configuration.activeConfig.Load();
    }

    private void OnApplicationQuit()
    {
        VN_Configuration.activeConfig.Save();
        VN_Configuration.activeConfig = null; // When we export the unity app, when we close, system will clear all static infor including this one
        // -> set to null before application quit to avoid memory leak
    }

    public void OpenPanel(string panelName)
    {
        GameObject panel = panels.First(p => p.name.ToLower() == panelName.ToLower());

        if (panel == null)
        {
            Debug.LogError($"Panel with name {panelName} not found in ConfigMenu.");
            return;
        }

        if (activePanel != null && activePanel != panel)
            activePanel.SetActive(false);

        panel.SetActive(true);
        activePanel = panel;

    }

    // UI callable methods
    public void SetDisplayToFullScreen(bool fullscreen)
    {
        Screen.fullScreen = fullscreen;
        UI.SetButtonColors(UI.fullscreen, UI.windowed, fullscreen);
    }

    public void SetDisplayResolution()
    {
        string res = UI.resolution.captionText.text;
        string[] parts = res.Split('x');

        if(int.TryParse(parts[0], out int width) && int.TryParse(parts[1], out int height))
        {
            Screen.SetResolution(width, height, Screen.fullScreen);
            VN_Configuration.activeConfig.display_resolution = res;
        }
        else
        {
            Debug.LogError($"Invalid resolution format: {res}");
        } 
    }

    public void SetContinueSkipAfterChoice(bool continueSkipping)
    {
        VN_Configuration.activeConfig.continueSkipAfterChoice = continueSkipping;
        UI.SetButtonColors(UI.continueSkipAfterChoice, UI.stopSkipAfterChoice, continueSkipping);
    }

    public void SetTextSpeed()
    {
        VN_Configuration.activeConfig.dialougeTextSpeed = UI.textSpeed.value;

        if(DialogueSystem.Instance != null)
            DialogueSystem.Instance.conversationManager.textArchitect.speed = VN_Configuration.activeConfig.dialougeTextSpeed;
    }

    public void SetAutoReadSpeed()
    {
        VN_Configuration.activeConfig.dialougeAutoReadSpeed = UI.autoReadSpeed.value;

        if (DialogueSystem.Instance != null)
            DialogueSystem.Instance.autoReader.speed = VN_Configuration.activeConfig.dialougeAutoReadSpeed;
    }

    public void SetMusicVolume()
    {
        VN_Configuration.activeConfig.musicVolume = UI.music.value;
        
        AudioManager.Instance.SetMusicVolume(VN_Configuration.activeConfig.musicVolume, VN_Configuration.activeConfig.musicMuted);

        UI.musicFill.color = VN_Configuration.activeConfig.musicMuted ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;
    }

    public void SetSFXVolume()
    {
        VN_Configuration.activeConfig.sfxVolume = UI.sfx.value;

        AudioManager.Instance.SetMusicVolume(VN_Configuration.activeConfig.sfxVolume, VN_Configuration.activeConfig.sfxMuted);

        UI.sfxFill.color = VN_Configuration.activeConfig.sfxMuted ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;
    }

    public void SetVoiceVolume()
    {
        VN_Configuration.activeConfig.voicesVolume = UI.voices.value;

        AudioManager.Instance.SetMusicVolume(VN_Configuration.activeConfig.voicesVolume, VN_Configuration.activeConfig.voicesMuted);

        UI.voicesFill.color = VN_Configuration.activeConfig.voicesMuted ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;
    }

    public void SetMusicMuted()
    {
        VN_Configuration.activeConfig.musicMuted = !VN_Configuration.activeConfig.musicMuted;
        UI.musicFill.color = VN_Configuration.activeConfig.musicMuted ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;
        UI.musicMute.sprite = VN_Configuration.activeConfig.musicMuted ? UI.volumeOffSprite : UI.volumeOnSprite;

        AudioManager.Instance.SetMusicVolume(VN_Configuration.activeConfig.musicVolume, VN_Configuration.activeConfig.musicMuted);
    }

    public void SetSFXMuted()
    {
        VN_Configuration.activeConfig.sfxMuted = !VN_Configuration.activeConfig.sfxMuted;
        UI.sfxFill.color = VN_Configuration.activeConfig.sfxMuted ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;
        UI.sfxMute.sprite = VN_Configuration.activeConfig.sfxMuted ? UI.volumeOffSprite : UI.volumeOnSprite;

        AudioManager.Instance.SetMusicVolume(VN_Configuration.activeConfig.sfxVolume, VN_Configuration.activeConfig.sfxMuted);
    }

    public void SetVoicesMuted()
    {
        VN_Configuration.activeConfig.voicesMuted = !VN_Configuration.activeConfig.voicesMuted;
        UI.voicesFill.color = VN_Configuration.activeConfig.voicesMuted ? UI_ITEMS.musicOffColor : UI_ITEMS.musicOnColor;
        UI.voicesMute.sprite = VN_Configuration.activeConfig.voicesMuted ? UI.volumeOffSprite : UI.volumeOnSprite;

        AudioManager.Instance.SetMusicVolume(VN_Configuration.activeConfig.voicesVolume, VN_Configuration.activeConfig.voicesMuted);
    }
}
