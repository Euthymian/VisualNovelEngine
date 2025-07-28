using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HISTORY
{
    [System.Serializable]
    public class AudioTrackData 
    {
        public int channel = 0;
        public string trackName;
        public string trackPath;
        public float trackVolume;
        public float trackPitch;
        public bool loop;

        public AudioTrackData(AudioChannel channel)
        {
            this.channel = channel.ChannelIndex;

            if (channel.activeTrack == null)
                return;

            AudioTrack track = channel.activeTrack;
            trackName = track.trackName;
            trackPath = track.path;
            trackVolume = track.cappedVolume;
            trackPitch = track.pitch;
            loop = track.loop;
        }

        public static List<AudioTrackData> Capture()
        {
            List<AudioTrackData> audioDataList = new List<AudioTrackData>();
            foreach (var channel in AudioManager.Instance.channelList)
            {
                if(channel.Value.activeTrack == null)
                    continue;

                audioDataList.Add(new AudioTrackData(channel.Value));
            }
            return audioDataList;
        }

        public static void Apply(List<AudioTrackData> data)
        {
            // cache all channels which created in this history state
            List<int> cache = new List<int>();

            foreach (var channelData in data)
            {
                AudioChannel channel = AudioManager.Instance.TryGetChannel(channelData.channel, createIfNotExist: true);
                if (channel.activeTrack == null || channelData.trackName != channel.activeTrack.trackName)
                {
                    AudioClip clip = HistoryCache.LoadAudioClip(channelData.trackPath);
                    if (clip != null)
                    {
                        channel.StopTrack(immediate: true);
                        channel.PlayTrack(clip, channelData.loop, channelData.trackVolume, channelData.trackVolume, channelData.trackPitch, channelData.trackPath);
                    }
                    else
                    {
                        Debug.LogWarning($"History State: Could not find audio clip at path {channelData.trackPath}");
                    }
                }

                cache.Add(channelData.channel);
            }

            // if there are channels that are not in the cache, stop their active tracks
            foreach (var channel in AudioManager.Instance.channelList)
            {
                if (!cache.Contains(channel.Key))
                {
                    channel.Value.StopTrack(immediate: true);
                }
            }
        }
    }
}