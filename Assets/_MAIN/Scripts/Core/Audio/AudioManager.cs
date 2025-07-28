using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    // Before we can access to AudioMixer, we need to expose the parameters in the AudioMixer on the Unity Editor manually.
    public const string EXPOSED_PARAM_MUSIC_VOLUME = "MusicVolume";
    public const string EXPOSED_PARAM_SFX_VOLUME = "SFXVolume";
    public const string EXPOSED_PARAM_VOICES_VOLUME = "VoicesVolume";
    public const float MUTED_VOLUME = -80f; // This is the lowest volume we can set in AudioMixer, which is equivalent to 0% volume in AudioSource.

    private const string SFX_ROOT_NAME = "SFX";
    public static readonly char[] SFX_NAME_FORMAT_CONTAINER = new char[] { '[', ']' };
    private static string SFX_NAME_FORMAT = $"SFX - {SFX_NAME_FORMAT_CONTAINER[0]}" + "{0}" + $"{SFX_NAME_FORMAT_CONTAINER[1]}";
    public const float TRACK_TRANSITION_DEFAULT_SPEED = 1;

    public static AudioManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            transform.SetParent(null); // DontDestroyOnLoad requires the object to not be a child of another object.
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
            return;
        }

        sfxRoot = new GameObject(SFX_ROOT_NAME).transform;
        sfxRoot.SetParent(transform);
    }

    public AudioMixerGroup musicMixer;
    public AudioMixerGroup sfxMixer;
    public AudioMixerGroup voicesMixer;

    // This curve will control volume of track in AudioMixer. We need to create this
    // because the way Mixer controls volume (by Decibels) is different from how Source controls volume (by linear scale).
    // Min decibel value is -80, which is the lowest volume we can set in AudioMixer.
    public AnimationCurve audioFallOffCurve;

    public Dictionary<int, AudioChannel> channelList = new Dictionary<int, AudioChannel>();

    private Transform sfxRoot;

    public AudioSource[] allSFXSources => sfxRoot.GetComponentsInChildren<AudioSource>();

    public AudioSource PlaySoundEffect(string filePath, AudioMixerGroup mixer = null, float volume = 1, float pitch = 1, bool loop = false)
    {
        AudioClip clip = Resources.Load<AudioClip>(filePath);
        if (clip == null)
        {
            Debug.LogError($"AudioManager: Could not find audio clip at path {filePath}");
            return null;
        }

        return PlaySoundEffect(clip, mixer, volume, pitch, loop, filePath);
    }

    public AudioSource PlaySoundEffect(AudioClip clip, AudioMixerGroup mixer = null, float volume = 1, float pitch = 1, bool loop = false, string filePath = "")
    {
        string fileName = clip.name;
        if (filePath != string.Empty)
            fileName = filePath;

        AudioSource sfxSource = new GameObject(string.Format(SFX_NAME_FORMAT, fileName)).AddComponent<AudioSource>();
        sfxSource.transform.SetParent(sfxRoot);
        sfxSource.transform.position = sfxRoot.position; // Center the source

        sfxSource.clip = clip;

        if (mixer == null)
            mixer = sfxMixer;
        sfxSource.outputAudioMixerGroup = mixer;
        sfxSource.volume = volume;
        sfxSource.pitch = pitch;
        sfxSource.loop = loop;
        sfxSource.spatialBlend = 0f; // 2D sound

        sfxSource.Play();

        if (!loop)
            Destroy(sfxSource.gameObject, (clip.length / pitch) + 1); // Destroy the source after the clip finishes playing (based on pitch and delay 1s)

        return sfxSource;
    }

    public AudioSource PlayVoice(string filePath, float volume = 1, float pitch = 1, bool loop = false)
    {
        return PlaySoundEffect(filePath, voicesMixer, volume, pitch, loop);
    }

    public AudioSource PlayVoice(AudioClip clip, float volume = 1, float pitch = 1, bool loop = false)
    {
        return PlaySoundEffect(clip, voicesMixer, volume, pitch, loop);
    }

    public void StopSoundEffect(AudioClip clip) => StopSoundEffect(clip.name);

    public void StopSoundEffect(string soundName)
    {
        soundName = soundName.ToLower();
        foreach (AudioSource source in allSFXSources)
        {
            if (source.clip.name.ToLower() == soundName)
            {
                source.Stop();
                Destroy(source.gameObject);
                return;
            }
        }
    }

    public bool IsPlayingSoundEffect(string soundName)
    {
        soundName = soundName.ToLower();
        foreach (AudioSource source in allSFXSources)
        {
            if (source.clip.name.ToLower() == soundName && source.isPlaying)
            {
                return true;
            }
        }
        return false;
    }

    public AudioTrack PlayTrack(string filePath, int channelIndex = 0, bool loop = false, float startVolume = 0, float cappedVolume = 1, float pitch = 1)
    {
        AudioClip clip = Resources.Load<AudioClip>(filePath);
        if (clip == null)
        {
            Debug.LogError($"AudioManager: Could not find audio clip at path {filePath}");
            return null;
        }

        return PlayTrack(clip, channelIndex, loop, startVolume, cappedVolume, pitch, filePath);
    }

    public AudioTrack PlayTrack(AudioClip clip, int channelIndex = 0, bool loop = false, float startVolume = 0, float cappedVolume = 1, float pitch = 1, string filePath = "")
    {
        AudioChannel channel = TryGetChannel(channelIndex, true);
        AudioTrack track = channel.PlayTrack(clip, loop, startVolume, cappedVolume, pitch, filePath);
        return track;
    }

    public void StopTrack(int channelIndex)
    {
        AudioChannel channel = TryGetChannel(channelIndex, false);

        channel.StopTrack();
    }

    public void StopTrack(string trackName)
    {
        trackName = trackName.ToLower();
        foreach (var channel in channelList.Values)
        {
            if (channel.activeTrack != null && channel.activeTrack.trackName.ToLower() == trackName)
            {
                channel.StopTrack();
                return;
            }
        }
    }

    public void StopAllTracks()
    {
        foreach(AudioChannel channel in channelList.Values)
        {
            channel.StopTrack();
        }
    }

    public void StopAllSoundEffects()
    {
        foreach (var item in allSFXSources)
        {
            Destroy(item.gameObject);
        }
    }

    public AudioChannel TryGetChannel(int channelIndex, bool createIfNotExist)
    {
        if (channelList.TryGetValue(channelIndex, out AudioChannel channel))
        {
            return channel;
        }
        else if (createIfNotExist)
        {
            channel = new AudioChannel(channelIndex);
            channelList.Add(channelIndex, channel);
            return channel;
        }

        return null;
    }

    public void SetMusicVolume(float volume, bool muted)
    {
        volume = muted ? MUTED_VOLUME : audioFallOffCurve.Evaluate(volume);
        musicMixer.audioMixer.SetFloat(EXPOSED_PARAM_MUSIC_VOLUME, volume);
    }

    public void SetSFXVolume(float volume, bool muted)
    {
        volume = muted ? MUTED_VOLUME : audioFallOffCurve.Evaluate(volume);
        sfxMixer.audioMixer.SetFloat(EXPOSED_PARAM_SFX_VOLUME, volume);
    }

    public void SetVoicesVolume(float volume, bool muted)
    {
        volume = muted ? MUTED_VOLUME : audioFallOffCurve.Evaluate(volume);
        voicesMixer.audioMixer.SetFloat(EXPOSED_PARAM_VOICES_VOLUME, volume);
    }
}
