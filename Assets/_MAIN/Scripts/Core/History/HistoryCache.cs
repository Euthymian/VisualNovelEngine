using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Video;

namespace HISTORY
{
    public class HistoryCache 
    {
        public static Dictionary<string, (object asset, int staleIndex)> loadedAssets = new Dictionary<string, (object asset, int staleIndex)>();
    
        public static T TryLoadObject<T>(string key)
        {
            object resource = null;

            if(loadedAssets.ContainsKey(key))
                resource = loadedAssets[key].asset;
            else 
            {
                resource = Resources.Load(key);
                if (resource != null)
                {
                    loadedAssets[key] = (resource, 0);
                }
            }

            if(resource != null)
            {
                if(resource is T)
                    return (T)resource;
                else
                    Debug.LogWarning($"Resource at key '{key}' is not of type {typeof(T)} but of type {resource.GetType()}");
            }

            Debug.LogWarning($"Resource at key '{key}' not found or is null.");
            return default(T);
        }

        public static TMP_FontAsset LoadFont(string key) => TryLoadObject<TMP_FontAsset>(key);
        public static AudioClip LoadAudioClip(string key) => TryLoadObject<AudioClip>(key);
        public static Texture2D LoadTexture(string key) => TryLoadObject<Texture2D>(key);
        public static VideoClip LoadVideoClip(string key) => TryLoadObject<VideoClip>(key);
    }
}