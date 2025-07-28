using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace COMMAND
{
    public class CMD_Extension_Audio : CMD_Extension
    {
        private static string[] PARAM_SFX = new string[] { "-s", "-sfx" };
        private static string[] PARAM_VOLUME = new string[] { "-v", "-vol", "-volume" };
        private static string[] PARAM_PITCH = new string[] { "-p", "-pitch" };
        private static string[] PARAM_LOOP = new string[] { "-l", "-loop" };

        private static string[] PARAM_CHANNEL = new string[] { "-c", "-channel" };
        private static string[] PARAM_STARTVOLUME = new string[] { "-sv", "-startVolume" };
        private static string[] PARAM_CAPPEDVOLUME = new string[] { "-cv", "-cappedVolume" };
        private static string[] PARAM_MUSIC = new string[] { "-m", "-music" };
        private static string[] PARAM_AMBIENCE = new string[] { "-a", "-ambience" };


        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("playsfx", new System.Action<string[]>(PlaySFX));
            database.AddCommand("stopsfx", new System.Action<string>(StopSFX));
            database.AddCommand("playvoice", new System.Action<string[]>(PlayVoice));
            database.AddCommand("stopvoice", new System.Action<string>(StopSFX));
            database.AddCommand("playmusic", new System.Action<string[]>(PlayMusic));
            database.AddCommand("playambience", new System.Action<string[]>(PlayAmbience));
            database.AddCommand("stopmusic", new System.Action<string>(StopMusic));
            database.AddCommand("stopambience", new System.Action<string>(StopAmbience));
        }

        private static void PlaySFX(string[] data)
        {
            string filePath;
            float volume, pitch;
            bool loop;

            CommandParameters parameters = ConvertDataToParameters(data);

            parameters.TryGetValue<string>(PARAM_SFX, out filePath);
            parameters.TryGetValue<float>(PARAM_VOLUME, out volume, 1f);
            parameters.TryGetValue<float>(PARAM_PITCH, out pitch, 1f);
            parameters.TryGetValue<bool>(PARAM_LOOP, out loop, false);

            string resourcePath = FilePaths.GetPathToResource(FilePaths.resources_sfx, filePath);
            AudioClip audioClip = Resources.Load<AudioClip>(resourcePath);

            if(audioClip == null)
            {
                Debug.LogError($"AudioClip not found at path: {FilePaths.GetPathToResource(FilePaths.resources_sfx, filePath)}");
                return;
            }

            AudioManager.Instance.PlaySoundEffect(audioClip, volume:volume, pitch: pitch, loop: loop, filePath: resourcePath);
        }

        private static void StopSFX(string data)
        {
            AudioManager.Instance.StopSoundEffect(data);
        }

        private static void PlayVoice(string[] data)
        {
            string filePath;
            float volume, pitch;
            bool loop;

            CommandParameters parameters = ConvertDataToParameters(data);

            parameters.TryGetValue<string>(PARAM_SFX, out filePath);
            parameters.TryGetValue<float>(PARAM_VOLUME, out volume, 1f);
            parameters.TryGetValue<float>(PARAM_PITCH, out pitch, 1f);
            parameters.TryGetValue<bool>(PARAM_LOOP, out loop, false);

            AudioClip audioClip = Resources.Load<AudioClip>(FilePaths.GetPathToResource(FilePaths.resources_voices, filePath));

            if (audioClip == null)
            {
                Debug.LogError($"AudioClip not found at path: {FilePaths.GetPathToResource(FilePaths.resources_voices, filePath)}");
                return;
            }

            AudioManager.Instance.PlayVoice(audioClip, volume: volume, pitch: pitch, loop: loop);
        }

        private static void PlayMusic(string[] data)
        {
            string filePath;
            int channel;

            CommandParameters parameters = ConvertDataToParameters(data);

            parameters.TryGetValue<string>(PARAM_MUSIC, out filePath);
            filePath = FilePaths.GetPathToResource(FilePaths.resources_music, filePath);

            parameters.TryGetValue<int>(PARAM_CHANNEL, out channel, 1);

            PlayTrack(filePath, channel, parameters);
        }

        private static void PlayAmbience(string[] data)
        {
            string filePath;
            int channel;

            CommandParameters parameters = ConvertDataToParameters(data);

            parameters.TryGetValue<string>(PARAM_AMBIENCE, out filePath);
            filePath = FilePaths.GetPathToResource(FilePaths.resources_ambience, filePath);

            parameters.TryGetValue<int>(PARAM_CHANNEL, out channel, 0);

            PlayTrack(filePath, channel, parameters);
        }

        private static void PlayTrack(string filePath, int channel, CommandParameters parameters)
        {
            bool loop;
            float startVolume, cappedVolume, pitch;

            parameters.TryGetValue<float>(PARAM_STARTVOLUME, out startVolume, 0f);
            parameters.TryGetValue<float>(PARAM_CAPPEDVOLUME, out cappedVolume, 1f);
            parameters.TryGetValue<float>(PARAM_PITCH, out pitch, 1f);
            parameters.TryGetValue<bool>(PARAM_LOOP, out loop, false);

            AudioClip audioClip = Resources.Load<AudioClip>(filePath);
            if (audioClip == null)
            {
                Debug.LogError($"AudioClip not found at path: {filePath}");
                return;
            }

            AudioManager.Instance.PlayTrack(audioClip, channel, loop, startVolume, cappedVolume, pitch, filePath);
        }

        private static void StopTrack(string data)
        {
            if(int.TryParse(data, out int channelIndex))
                AudioManager.Instance.StopTrack(channelIndex);
            else
                AudioManager.Instance.StopTrack(data);
        }

        private static void StopMusic(string data)
        {
            if (data == string.Empty)
                StopTrack("1");
            else
                StopTrack(data);
        }

        private static void StopAmbience(string data)
        {
            if (data == string.Empty)
                StopTrack("0");
            else
                StopTrack(data);
        }
    }
}