using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HISTORY
{
    [System.Serializable]
    public class AudioSFXData 
    {
        public string filePath;
        public float volume, pitch;
        // Absent the loop property, as the only SFXs are recorded have loop set to true.

        public static List<AudioSFXData> Capture()
        {
            List<AudioSFXData> dataList = new List<AudioSFXData>();
            AudioSource[] sfx = AudioManager.Instance.allSFXSources;
            foreach(var source in sfx)
            {
                if (!source.loop)
                    continue;

                AudioSFXData data = new AudioSFXData();
                data.volume = source.volume;
                data.pitch = source.pitch;

                string sourceObjectName = source.gameObject.name;
                //SFX_NANME_FORMAT_CONTAINER is a char array with two elements: '[' and ']'.
                //Split the name by array of 2 chars, we will get 3 parts: before the first '[', between the two '[', and after the last ']'. 
                string resourcePath = sourceObjectName.Split(AudioManager.SFX_NAME_FORMAT_CONTAINER)[1];
                data.filePath = resourcePath;

                dataList.Add(data);
            }

            return dataList;
        }

        public static void Apply(List<AudioSFXData> data)
        {
            AudioManager.Instance.StopAllSoundEffects();
            foreach (var sfxData in data)
            {
                AudioManager.Instance.PlaySoundEffect(sfxData.filePath, volume: sfxData.volume, pitch: sfxData.pitch, loop: true);
            }
        }
    }
}