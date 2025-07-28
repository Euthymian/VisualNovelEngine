using GRAPHIC;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace COMMAND
{
    public class CMD_Extension_Gallery : CMD_Extension
    {
        private static string[] PARAM_GRAPHIC = new string[] { "-media", "-m" };
        private static string[] PARAM_SPEED = new string[] { "-speed", "-spd" };
        private static string[] PARAM_IMMEDIATE = new string[] { "-immediate", "-i" };
        private static string[] PARAM_BLENDTEXTURE = new string[] { "-blendtex", "-b" };

        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("showgalleryimage", new Func<string[], IEnumerator>(ShowGalleryImage));
            database.AddCommand("hidegalleryimage", new Func<string[], IEnumerator>(HideGalleryImage));
        }

        public static IEnumerator HideGalleryImage(string[] data)
        {
            GraphicLayer graphicLayer = GraphicPanelManager.Instance.GetPanel("Cinematic").GetLayer(0, true);
            if(graphicLayer == null)
            {
                Debug.LogError("Cinematic panel or layer not found.");
                yield break;
            }

            float transitionSpeed = 0;
            bool immediate = false;
            string blendTextureName = "";
            Texture blendTexture = null;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            parameters.TryGetValue(PARAM_SPEED, out transitionSpeed, defaultValue: 1f);

            parameters.TryGetValue(PARAM_BLENDTEXTURE, out blendTextureName);

            if (!immediate && !string.IsNullOrEmpty(blendTextureName))
                blendTexture = Resources.Load<Texture>(FilePaths.GetPathToResource(FilePaths.resources_transitionEffects, blendTextureName));

            if(!immediate)
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { graphicLayer.Clear(immediate: true); });

            graphicLayer.Clear(transitionSpeed, blendTexture, immediate);

            if (graphicLayer.currentGraphic != null)
                yield return graphicLayer.currentGraphic.co_fadingOut;
        }

        public static IEnumerator ShowGalleryImage(string[] data)
        {
            string mediaName = "";
            float transitionSpeed = 0;
            bool immediate = false;
            string blendTextureName = "";
            Texture blendTexture = null;

            var parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_GRAPHIC, out mediaName);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            parameters.TryGetValue(PARAM_SPEED, out transitionSpeed, defaultValue: 1f);

            parameters.TryGetValue(PARAM_BLENDTEXTURE, out blendTextureName);

            string pathToGraphic = FilePaths.resources_gallery + mediaName;
            Texture graphic = Resources.Load<Texture>(pathToGraphic);

            if(graphic == null)
            {
                Debug.LogError($"Gallery image '{mediaName}' not found at path '{pathToGraphic}'.");
                yield break;
            }

            if (!immediate && !string.IsNullOrEmpty(blendTextureName))
                blendTexture = Resources.Load<Texture>(FilePaths.GetPathToResource(FilePaths.resources_transitionEffects, blendTextureName));

            GraphicLayer graphicLayer = GraphicPanelManager.Instance.GetPanel("Cinematic").GetLayer(0, true);

            if(!immediate)
                CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { graphicLayer.SetTexture(graphic, filePath: pathToGraphic, immediate: true); });

            GalleryConfig.UnlockImage(mediaName);

            yield return graphicLayer.SetTexture(graphic, transitionSpeed, filePath: pathToGraphic, blendingTexture: blendTexture, immediate: immediate);
        }
    }
}