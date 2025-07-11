using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

namespace GRAPHIC
{
    public class GraphicObject
    {
        private const string NAME_FORMAT = "Graphic - [{0}]";
        private const string DEFAULT_UI_MATERIAL = "Default UI Material";
        private const string MATERIAL_PATH = "Materials/layerTransitionMaterial";
        private const string MATERIAL_FIELD_COLOR = "_Color";
        private const string MATERIAL_FIELD_MAINTEX = "_MainTex";
        private const string MATERIAL_FIELD_BLENDTEX = "_BlendTex";
        private const string MATERIAL_FIELD_BLEND = "_Blend";
        private const string MATERIAL_FIELD_ALPHA = "_Alpha";

        private GraphicLayer layer;

        public string graphicName = "";
        public RawImage renderer;

        public bool isVideo { get { return videoPlayer != null; } }
        public VideoPlayer videoPlayer = null;
        public AudioSource audioSource = null;

        public string graphicPath = "";

        public Coroutine co_fadingIn = null;
        public Coroutine co_fadingOut = null;

        GraphicPanelManager panelManager => GraphicPanelManager.Instance;

        public GraphicObject(GraphicLayer layer, string graphicPath, Texture texture, bool immediate)
        {
            this.layer = layer;
            this.graphicPath = graphicPath;
            GameObject ob = new GameObject();

            ob.transform.SetParent(layer.panel);
            renderer = ob.AddComponent<RawImage>();

            graphicName = texture.name;

            InitGraphic(immediate);

            renderer.name = string.Format(NAME_FORMAT, graphicName);
            renderer.material.SetTexture(MATERIAL_FIELD_MAINTEX, texture);
        }

        public GraphicObject(GraphicLayer layer, string graphicPath, VideoClip video, bool useAudio, bool immediate)
        {
            this.layer = layer;
            this.graphicPath = graphicPath;
            GameObject ob = new GameObject();

            ob.transform.SetParent(layer.panel);
            renderer = ob.AddComponent<RawImage>();

            graphicName = video.name;

            InitGraphic(immediate);

            renderer.name = string.Format(NAME_FORMAT, graphicName);
            // To play video, we need to create a render texture and assign it to the RawImage.
            RenderTexture renderTexture = new RenderTexture(Mathf.RoundToInt(video.width), Mathf.RoundToInt(video.height), 0);
            renderer.material.SetTexture(MATERIAL_FIELD_MAINTEX, renderTexture);

            videoPlayer = renderer.AddComponent<VideoPlayer>();
            videoPlayer.playOnAwake = true;
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = video;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.targetTexture = renderTexture;
            videoPlayer.isLooping = true;

            videoPlayer.audioOutputMode = VideoAudioOutputMode.AudioSource;
            audioSource = videoPlayer.AddComponent<AudioSource>();
            audioSource.volume = immediate ? 1 : 0;

            if (!useAudio)
                audioSource.mute = true;

            videoPlayer.SetTargetAudioSource(0, audioSource);

            videoPlayer.frame = 0; // Start at the first frame
            videoPlayer.Prepare();
            videoPlayer.Play();

            // For some reason, without this, audioSource doenst sync with what set above (if not using audio).
            videoPlayer.enabled = false;
            videoPlayer.enabled = true;
        }

        private void InitGraphic(bool immediate)
        {
            renderer.transform.localPosition = Vector3.zero;
            renderer.transform.localScale = Vector3.one;

            RectTransform rect = renderer.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.one;

            renderer.material = GetTransitionMaterial();

            renderer.material.SetFloat(MATERIAL_FIELD_BLEND, immediate ? 1 : 0);
            renderer.material.SetFloat(MATERIAL_FIELD_ALPHA, immediate ? 1 : 0);
        }

        private Material GetTransitionMaterial()
        {
            Material mat = Resources.Load<Material>(MATERIAL_PATH);
            
            if (mat != null)
                return new Material(mat);

            return null;
        }

        public Coroutine FadeIn(float speed = 1, Texture blendingTexture = null)
        {
            if (co_fadingOut != null)
                panelManager.StopCoroutine(co_fadingOut);

            if (co_fadingIn != null)
                panelManager.StopCoroutine(co_fadingIn);

            co_fadingIn = panelManager.StartCoroutine(Fading(1f, speed, blendingTexture));
            return co_fadingIn;
        }

        public Coroutine FadeOut(float speed = 1, Texture blendingTexture = null)
        {
            if (co_fadingIn != null)
                panelManager.StopCoroutine(co_fadingIn);

            if (co_fadingOut != null)
                panelManager.StopCoroutine(co_fadingOut);

            co_fadingOut = panelManager.StartCoroutine(Fading(0f, speed, blendingTexture));
            return co_fadingOut;
        }

        private IEnumerator Fading(float target, float speed, Texture blendingTexture = null)
        {
            bool isBlending = blendingTexture != null;
            bool fadingIn = target > 0f;

            // get the transition material at the start of fade, so we can use it for blending.
            if (renderer.material.name == DEFAULT_UI_MATERIAL)
            {
                Texture tex = renderer.material.GetTexture(MATERIAL_FIELD_MAINTEX);
                renderer.material = GetTransitionMaterial();
                renderer.material.SetTexture(MATERIAL_FIELD_MAINTEX, tex);
            }

            renderer.material.SetTexture(MATERIAL_FIELD_BLENDTEX, blendingTexture);
            renderer.material.SetFloat(MATERIAL_FIELD_ALPHA, isBlending ? 1 : fadingIn ? 0 : 1);
            renderer.material.SetFloat(MATERIAL_FIELD_BLEND, isBlending ? fadingIn ? 0 : 1 : 1);

            string opacityField = isBlending ? MATERIAL_FIELD_BLEND : MATERIAL_FIELD_ALPHA;

            while (renderer.material.GetFloat(opacityField) != target)
            {
                float opacity = Mathf.MoveTowards(renderer.material.GetFloat(opacityField), target, speed * Time.deltaTime);
                renderer.material.SetFloat(opacityField, opacity);

                if (isVideo)
                    audioSource.volume = opacity;

                yield return null;
            }

            co_fadingIn = null;
            co_fadingOut = null;

            if (target == 0)
                Destroy();
            else
            {
                OnDestroyOthersAfterFadeIn();
                // After fade in, set material to default material so the RawImage will be affected by canvas group alpha.
                // This is needed becuase when fade out the whole main canvas by CanvasGroupController, every layer will be faded out smoothly,
                // except graphic layers, which using custom material.
                renderer.texture = renderer.material.GetTexture(MATERIAL_FIELD_MAINTEX);
                renderer.material = null;
            }
        }

        private void OnDestroyOthersAfterFadeIn()
        {
            layer.ClearOldGraphics();
        }

        public void Destroy()
        {
            if (layer.currentGraphic != null && layer.currentGraphic.renderer == renderer)
                layer.currentGraphic = null;

            if(layer.oldGraphicObjectList.Contains(this))
                layer.oldGraphicObjectList.Remove(this);

            Object.Destroy(renderer.gameObject);
        }
    }
}