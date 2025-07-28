using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VISUALNOVEL;

public class MainMenu : MonoBehaviour
{
    public const string MAIN_MENU_SCENE = "MainMenu";

    public static MainMenu Instance { get; private set; }
    private void Awake()
    {
        Instance = this;
    }

    public AudioClip menuMusic;
    public CanvasGroup mainCG;
    private CanvasGroupController cgc;

    private void Start()
    {
        cgc = new CanvasGroupController(this, mainCG);
        AudioManager.Instance.StopAllSoundEffects();
        AudioManager.Instance.StopAllTracks();
        AudioManager.Instance.PlayTrack(menuMusic, channelIndex:0, startVolume: 1);
    }

    private void StartNewGame()
    {
        VNGameSave.activeFile = new VNGameSave();
        StartCoroutine(StartingGame());
    }

    public void Click_StartNewGame()
    {
        UIConfirmationMenu.Instance.Show("Are you sure you want to start a new game?",
            new UIConfirmationMenu.ConfirmationButton("Yes", StartNewGame, true),
            new UIConfirmationMenu.ConfirmationButton("No", null, true));
    }

    public void LoadGame(VNGameSave file)
    {
        VNGameSave.activeFile = file;
        StartCoroutine(StartingGame());
    }

    private IEnumerator StartingGame()
    {
        cgc.Hide(0.3f);
        AudioManager.Instance.StopTrack(0);

        while(cgc.isVisible)
            yield return null;

        VN_Configuration.activeConfig.Save();
        SceneManager.LoadScene("VisualNovel");
    }
}
