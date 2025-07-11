
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Playing and managing tracks
public class AudioChannel 
{
    private const string TRACK_CONTAINER_NAME_FORMAT = "Channel - [{0}]";

    public int ChannelIndex { get; private set; }

    public Transform trackContainer { get; private set; } = null;
    private List<AudioTrack> trackList = new List<AudioTrack>();
    public AudioTrack activeTrack { get; private set; } = null;

    private Coroutine co_volumeLeveling = null;
    public bool isVolumeLeveling => co_volumeLeveling != null;

    public AudioChannel(int channelIndex)
    {
        ChannelIndex = channelIndex;

        trackContainer = new GameObject(string.Format(TRACK_CONTAINER_NAME_FORMAT, channelIndex)).transform;
        trackContainer.SetParent(AudioManager.Instance.transform);
    }

    public AudioTrack PlayTrack(AudioClip clip, bool loop, float startVolume, float cappedVolume, float pitch, string filePath)
    {
        if(TryGetTrack(clip.name, out AudioTrack existingTrack))
        {
            if(existingTrack.loop != loop || existingTrack.cappedVolume != cappedVolume || existingTrack.source.pitch != pitch)
            {
                existingTrack.source.loop = loop;
                existingTrack.cappedVolume = cappedVolume;
                existingTrack.source.pitch = pitch;
            }
            if (!existingTrack.isPlaying)
                existingTrack.Play();

            SetAsActiveTrack(existingTrack);

            return existingTrack;
        }

        AudioTrack track = new AudioTrack(clip, loop, startVolume, cappedVolume, pitch, this, AudioManager.Instance.musicMixer);
        track.Play();   

        SetAsActiveTrack(track);

        return track;
    }

    public bool TryGetTrack(string trackName, out AudioTrack track)
    {
        trackName = trackName.ToLower();
        foreach (var t in trackList)
        {
            if (t.trackName.ToLower() == trackName)
            {
                track = t;
                return true;
            }
        }

        track = null;
        return false;
    }

    private void SetAsActiveTrack(AudioTrack track)
    {
        if(!trackList.Contains(track))
            trackList.Add(track);
        activeTrack = track;

        TryStartVolumeLeveling();   
    }

    private void TryStartVolumeLeveling()
    {
        if(isVolumeLeveling)
            AudioManager.Instance.StopCoroutine(co_volumeLeveling);

        co_volumeLeveling = AudioManager.Instance.StartCoroutine(VolumeLeveling());
    }

    private IEnumerator VolumeLeveling()
    {
        while ((activeTrack != null && (trackList.Count > 1 || activeTrack.volume != activeTrack.cappedVolume)) || (activeTrack == null && trackList.Count > 0))
        {
            for(int i = trackList.Count -1; i>= 0; i--)
            {
                AudioTrack track = trackList[i];

                float targetVolume = activeTrack == track ? track.cappedVolume : 0f;

                if (track.volume == targetVolume)
                    continue;

                track.volume = Mathf.MoveTowards(track.volume, targetVolume, Time.deltaTime * AudioManager.TRACK_TRANSITION_DEFAULT_SPEED);

                if(track != activeTrack && track.volume == 0)
                    DestroyTrack(track);
            }

            yield return null;
        }

        co_volumeLeveling = null;
    }

    private void DestroyTrack(AudioTrack track)
    {
        if(trackList.Contains(track))
            trackList.Remove(track);

        Object.Destroy(track.root);
    }

    public void StopTrack()
    {
        if (activeTrack == null)
            return;

        activeTrack = null;

        TryStartVolumeLeveling();
    }
}
