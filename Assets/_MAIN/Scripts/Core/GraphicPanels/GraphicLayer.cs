using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

namespace GRAPHIC
{
    public class GraphicLayer
    {
        public const string LAYER_OBJECT_NAME_FORMAT = "Layer: {0}";
        public int layerDepth = 0;
        public Transform panel;

        public GraphicObject currentGraphic = null;
        public List<GraphicObject> oldGraphicObjectList = new List<GraphicObject>();

        public Coroutine SetTexture(string filePath, float transitionSpeed = 1, Texture blendingTexture = null, bool immediate = false)
        {
            Texture texture = Resources.Load<Texture>(filePath);

            if (texture == null)
            {
                Debug.LogError($"Texture not found at path: {filePath}");
                return null;
            }

            return SetTexture(texture, transitionSpeed, blendingTexture, filePath, immediate);
        }

        public Coroutine SetTexture(Texture texture, float transitionSpeed = 1, Texture blendingTexture = null, string filePath = "", bool immediate = false)
        {
            return CreateGraphic(texture, transitionSpeed, filePath, blendingTexture:blendingTexture, immediate:immediate);
        }

        public Coroutine SetVideo(string filePath, float transitionSpeed = 1, bool useAudio = true, Texture blendingTexture = null, bool immediate = false)
        {
            VideoClip video = Resources.Load<VideoClip>(filePath);

            if (video == null)
            {
                Debug.LogError($"Video not found at path: {filePath}");
                return null;
            }

            return SetVideo(video, transitionSpeed, useAudio, blendingTexture, filePath, immediate);
        }

        public Coroutine SetVideo(VideoClip videoClip, float transitionSpeed = 1, bool useAudio = true, Texture blendingTexture = null, string filePath = "", bool immediate = false)
        {
            return CreateGraphic(videoClip, transitionSpeed, filePath, useAudio, blendingTexture, immediate);
        }

        private Coroutine CreateGraphic<T>(T graphicData, float transitionSpeed, string filePath, bool useAudioForVideo = true, Texture blendingTexture = null, bool immediate = false)
        {
            GraphicObject newGraphic = null;

            if (graphicData is Texture)
                newGraphic = new GraphicObject(this, filePath, graphicData as Texture, immediate);
            else if (graphicData is VideoClip)
                newGraphic = new GraphicObject(this, filePath, graphicData as VideoClip, useAudioForVideo, immediate);

            if (currentGraphic != null && !oldGraphicObjectList.Contains(currentGraphic))
                oldGraphicObjectList.Add(currentGraphic);
            currentGraphic = newGraphic;

            if(!immediate)
                return currentGraphic.FadeIn(transitionSpeed, blendingTexture);

            ClearOldGraphics();
            return null;
        }

        public void ClearOldGraphics()
        {
            foreach (GraphicObject graphic in oldGraphicObjectList)
            {
                if(graphic.renderer != null)
                    Object.Destroy(graphic.renderer.gameObject);
            }

            oldGraphicObjectList.Clear();
        }

        public void Clear(float transitionSpeed = 1, Texture blendTexure = null, bool immediate = false)
        {
            if (currentGraphic != null)
            {
                if(!immediate)
                    currentGraphic.FadeOut(transitionSpeed, blendTexure);
                else
                    currentGraphic.Destroy();
            }

            foreach (GraphicObject graphic in oldGraphicObjectList)
            {
                if(!immediate)
                    graphic.FadeOut(transitionSpeed, blendTexure);
                else
                    graphic.Destroy();
            }
            oldGraphicObjectList.Clear();
        }
    }
}