using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

// Playing music within channel.
public class AudioTrack 
{
    private const string TRACK_NAME_FORMAT = "Track - [{0}]";

    public string trackName { get; private set; }
    private AudioChannel channel;
    private AudioSource source;
    public bool loop => source.loop;
    public float cappedVolume { get; private set; }
    public bool isPlaying => source.isPlaying;

    public GameObject root => source.gameObject;

    public float volume
    {
        get => source.volume;
        set => source.volume = value;
    }

    public AudioTrack(AudioClip clip, bool loop, float startVolume, float cappedVolume, float pitch, AudioChannel channel, AudioMixerGroup mixer)
    {
        trackName = clip.name;
        this.channel = channel;
        this.cappedVolume = cappedVolume;

        source = CreateSource();
        source.clip = clip;
        source.loop = loop;
        source.volume = startVolume;
        source.pitch = pitch;

        source.outputAudioMixerGroup = mixer;
    }

    private AudioSource CreateSource()
    {
        GameObject ob = new GameObject(string.Format(TRACK_NAME_FORMAT, trackName));
        ob.transform.SetParent(channel.trackContainer);
        AudioSource audioSource = ob.AddComponent<AudioSource>();

        return audioSource;
    }

    public void Play()
    {
        if (source.isPlaying)
            return;
        source.Play();
    }

    public void Stop()
    {
        if (!source.isPlaying)
            return;
        source.Stop();
    }
}
