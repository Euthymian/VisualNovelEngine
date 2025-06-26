using CHARACTER;
using GRAPHIC;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestAudio : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        //StartCoroutine(TestSFX());
        StartCoroutine(TestChannelTrack());
    }

    IEnumerator TestSFX()
    {
        Character_Sprite rae = CharacterManager.Instance.CreateCharacter("Raelin") as Character_Sprite;
        rae.Show();

        yield return new WaitForSeconds(2f);

        AudioManager.Instance.PlaySoundEffect("Audio/SFX/RadioStatic", loop:true);

        yield return rae.Say("\"Im gonna stop the radio, its too annoying!\"");

        AudioManager.Instance.PlayVoice("Audio/Voices/wakeup");
        yield return new WaitForSeconds(1f);
        AudioManager.Instance.StopSoundEffect("RadioStatic");

        rae.Say("\"There, much better!\"");
    }

    IEnumerator TestChannelTrack()
    {
        Character_Sprite rae = CharacterManager.Instance.CreateCharacter("Raelin") as Character_Sprite;
        rae.Show();

        GraphicPanelManager.Instance.GetPanel("Background").GetLayer(0, true).SetTexture("Graphics/BG Images/BG Fox_bedroom_evening");
        AudioManager.Instance.PlayTrack("Audio/Ambience/RainyMood", loop: true, cappedVolume: 0.134f);
        AudioManager.Instance.PlayTrack("Audio/Music/Calm", channelIndex:1, loop: true, startVolume: 0.2f, cappedVolume: 0.7f);
        yield return new WaitForSeconds(2f);
        AudioManager.Instance.PlayTrack("Audio/Music/Calm2", channelIndex: 1, loop: true, startVolume: 0.2f, cappedVolume: 0.5f);

        yield return rae.Say("\"I love the rain, it makes me feel so calm and relaxed.\"");
        AudioManager.Instance.StopTrack(0);
        rae.Say("\"Oh, the rain stopped.\"");
    }
}
