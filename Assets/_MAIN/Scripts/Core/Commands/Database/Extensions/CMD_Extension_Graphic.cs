using GRAPHIC;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Video;

namespace COMMAND
{
    public class CMD_Extension_Graphic : CMD_Extension
    {
        private static string[] PARAM_PANEL = new string[] { "-panel", "-p" };
        private static string[] PARAM_LAYER = new string[] { "-layer", "-l" };
        private static string[] PARAM_GRAPHIC = new string[] { "-media", "-m" };
        private static string[] PARAM_SPEED = new string[] { "-speed", "-spd" };
        private static string[] PARAM_IMMEDIATE = new string[] { "-immediate", "-i" };
        private static string[] PARAM_BLENDTEXTURE = new string[] { "-blendtex", "-b" };
        private static string[] PARAM_USEVIDEOAUDIO = new string[] { "-audio", "-au" };

        new public static void Extend(CommandDatabase database)
        {
            database.AddCommand("setlayermedia", new System.Func<string[], IEnumerator>(SetLayerMedia));
            database.AddCommand("clearlayermedia", new System.Func<string[], IEnumerator>(ClearLayerMedia));
        }

        private static IEnumerator SetLayerMedia(string[] data)
        {
            // Params available:
            string panelName = "";
            int layer = 0;
            string mediaName = "";
            float transitionSpeed = 0;
            bool immediate = false;
            string blendTextureName = "";
            bool useAudio = false;
        
            string pathToGraphic = "";
            Object graphicObject = null;
            Texture blendTexture = null;

            CommandParameters parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_PANEL, out panelName);
            GraphicPanel panel = GraphicPanelManager.Instance.GetPanel(panelName);
            if(panel == null)
            {
                Debug.LogError($"Panel with name '{panelName}' not found.");
                yield break;
            }

            parameters.TryGetValue(PARAM_LAYER, out layer, defaultValue:0);
            GraphicLayer graphicLayer = panel.GetLayer(layer, true);

            parameters.TryGetValue(PARAM_GRAPHIC, out mediaName);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            parameters.TryGetValue(PARAM_SPEED, out transitionSpeed, defaultValue: 1f);

            parameters.TryGetValue(PARAM_BLENDTEXTURE, out blendTextureName);

            parameters.TryGetValue(PARAM_USEVIDEOAUDIO, out useAudio, defaultValue: false);

            pathToGraphic = FilePaths.GetPathToResource(FilePaths.resources_bgImages, mediaName);
            graphicObject = Resources.Load<Texture>(pathToGraphic);
            if (graphicObject == null)
            {
                pathToGraphic = FilePaths.GetPathToResource(FilePaths.resources_bgVideos, mediaName);
                graphicObject = Resources.Load<VideoClip>(pathToGraphic);
            }
            if (graphicObject == null)
            {
                Debug.LogError($"Graphic object '{mediaName}' not found in Resources");
                yield break;
            }

            if (!immediate && !string.IsNullOrEmpty(blendTextureName))
                blendTexture = Resources.Load<Texture>(FilePaths.GetPathToResource(FilePaths.resources_transitionEffects, blendTextureName));

            if (graphicObject is Texture)
            {
                if(!immediate)
                    CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { graphicLayer.SetTexture(graphicObject as Texture, filePath: pathToGraphic, immediate: true); });

                yield return graphicLayer.SetTexture(graphicObject as Texture, transitionSpeed, blendTexture, pathToGraphic, immediate);
            }
            else
            {
                if (!immediate)
                    CommandManager.Instance.AddTerminationActionToCurrentProcess(() => { graphicLayer.SetVideo(graphicObject as VideoClip, filePath: pathToGraphic, immediate: true); });

                yield return graphicLayer.SetVideo(graphicObject as VideoClip, transitionSpeed, useAudio, blendTexture, pathToGraphic, immediate);
            }

        }

        private static IEnumerator ClearLayerMedia(string[] data)
        {
            string panelName = "";
            int layer = 0;
            float transitionSpeed = 0;
            bool immediate = false;
            string blendTextureName = "";

            Texture blendTexture = null;

            CommandParameters parameters = ConvertDataToParameters(data);

            parameters.TryGetValue(PARAM_PANEL, out panelName);
            GraphicPanel panel = GraphicPanelManager.Instance.GetPanel(panelName);
            if (panel == null)
            {
                Debug.LogError($"Panel with name '{panelName}' not found.");
                yield break;
            }

            parameters.TryGetValue(PARAM_LAYER, out layer, defaultValue: -1);

            parameters.TryGetValue(PARAM_SPEED, out transitionSpeed, defaultValue: 1f);

            parameters.TryGetValue(PARAM_IMMEDIATE, out immediate, defaultValue: false);

            parameters.TryGetValue(PARAM_BLENDTEXTURE, out blendTextureName);
            if(!immediate && !string.IsNullOrEmpty(blendTextureName))
                blendTexture = Resources.Load<Texture>(FilePaths.resources_transitionEffects + blendTextureName);

            if(layer == -1)
                panel.Clear(transitionSpeed, blendTexture, immediate);
            else
            {
                GraphicLayer graphicLayer = panel.GetLayer(layer, false);
                if(graphicLayer == null)
                {
                    Debug.LogError($"Layer {layer} not found in panel '{panelName}'.");
                    yield break;
                }
                
                graphicLayer.Clear(transitionSpeed, blendTexture, immediate);
            }
        }
    }
}