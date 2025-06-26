using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    private const string SFX_ROOT_NAME = "SFX";
    private const string SFX_NAME_FORMAT = "SFX - [{0}]";
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

    public Dictionary<int, AudioChannel> channelList = new Dictionary<int, AudioChannel>();

    private Transform sfxRoot;

    public AudioSource PlaySoundEffect(string filePath, AudioMixerGroup mixer = null, float volume = 1, float pitch = 1, bool loop = false)
    {
        AudioClip clip = Resources.Load<AudioClip>(filePath);
        if (clip == null)
        {
            Debug.LogError($"AudioManager: Could not find audio clip at path {filePath}");
            return null;
        }

        return PlaySoundEffect(clip, mixer, volume, pitch, loop);
    }

    public AudioSource PlaySoundEffect(AudioClip clip, AudioMixerGroup mixer = null, float volume = 1, float pitch = 1, bool loop = false)
    {
        AudioSource sfxSource = new GameObject(string.Format(SFX_NAME_FORMAT, clip.name)).AddComponent<AudioSource>();
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
        AudioSource[] audioSources = sfxRoot.GetComponentsInChildren<AudioSource>();
        foreach (AudioSource source in audioSources)
        {
            if (source.clip.name.ToLower() == soundName)
            {
                source.Stop();
                Destroy(source.gameObject);
                return;
            }
        }
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

    private AudioChannel TryGetChannel(int channelIndex, bool createIfNotExist)
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
}
