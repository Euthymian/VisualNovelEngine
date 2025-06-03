using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CHARACTER
{
    // Contains all data and functions availabe to layer composing a sprite char.
    public class CharacterSpriteLayer
    {
        private CharacterManager characterManager => CharacterManager.Instance;

        public int layer { get; private set; } = 0; // identifies layer number sorting depth within layer stack
        public Image renderer { get; private set; } = null; // the current active image renderer
        public CanvasGroup rendererCanvasGroup => renderer.GetComponent<CanvasGroup>();

        private Coroutine co_transitioningSprite = null;
        public bool isTransitioningSprite => co_transitioningSprite != null;

        private Coroutine co_incrementingAlpha = null;
        public bool isIncrementingAlpha => co_incrementingAlpha != null;

        private Coroutine co_transitioningColor = null;
        public bool isTransitioningColor => co_transitioningColor != null;

        private const float DEFAULT_TRANSITION_SPEED = 3f;
        private float transitionSpeedMultiplier = 1f;

        // This is a list of all renderers that being faded out -> Thats why we just need the CanvasGroup component
        public List<CanvasGroup> oldRenderers = new List<CanvasGroup>();

        public CharacterSpriteLayer(Image defaultRenderer, int layer)
        {
            this.layer = layer;
            this.renderer = defaultRenderer;
        }

        public void SetSprite(Sprite sprite)
        {
            renderer.sprite = sprite;
        }

        public Coroutine TransitioningSprite(Sprite newSprite, float speed = 0.5f)
        {
            if (newSprite == renderer.sprite)
                return null;

            if (isTransitioningSprite)
                characterManager.StopCoroutine(co_transitioningSprite);

            co_transitioningSprite = characterManager.StartCoroutine(TransitioningSpriteCoroutine(newSprite, speed));
            return co_transitioningSprite;
        }

        private IEnumerator TransitioningSpriteCoroutine(Sprite newSprite, float speedMultiplier)
        {
            transitionSpeedMultiplier = speedMultiplier;

            Image newRenderer = CreateRenderer(renderer.transform.parent);
            newRenderer.sprite = newSprite;

            yield return TryStartIncrementingAlpha();

            co_transitioningSprite = null; // Reset the coroutine reference
        }

        private Image CreateRenderer(Transform parent)
        {
            Image newRenderer = UnityEngine.Object.Instantiate(renderer, parent);
            oldRenderers.Add(rendererCanvasGroup);

            newRenderer.name = renderer.name;
            renderer = newRenderer;
            renderer.gameObject.SetActive(true);
            rendererCanvasGroup.alpha = 0f;

            return newRenderer;
        }

        private Coroutine TryStartIncrementingAlpha()
        {
            if (isIncrementingAlpha)
                return co_incrementingAlpha;

            co_incrementingAlpha = characterManager.StartCoroutine(IncrementingAlpha());

            return co_incrementingAlpha;
        }

        private IEnumerator IncrementingAlpha()
        {
            while (rendererCanvasGroup.alpha < 1f || oldRenderers.Any(oldCanvasGroup => oldCanvasGroup.alpha > 0))
            {
                float fadeSpeed = Time.deltaTime * DEFAULT_TRANSITION_SPEED * transitionSpeedMultiplier;

                rendererCanvasGroup.alpha = Mathf.MoveTowards(rendererCanvasGroup.alpha, 1f, fadeSpeed);

                // Reason of iterating backwards is to avoid modifying the list while iterating
                // If we iterate forwards, we will skip some elements when removing current one
                for (int i = oldRenderers.Count - 1; i >= 0; i--)
                {
                    CanvasGroup currentCG = oldRenderers[i];
                    currentCG.alpha = Mathf.MoveTowards(currentCG.alpha, 0f, fadeSpeed);

                    if (currentCG.alpha <= 0f)
                    {
                        oldRenderers.RemoveAt(i); // Remove the destroyed renderer from the list
                        UnityEngine.Object.Destroy(currentCG.gameObject);
                    }
                }

                yield return null;
            }

            co_incrementingAlpha = null; // Reset the coroutine reference
        }

        public void SetColor(Color color)
        {
            renderer.color = color;

            foreach (CanvasGroup oldRendererCG in oldRenderers)
            {
                oldRendererCG.GetComponent<Image>().color = color;
            }
        }

        public Coroutine TransitioningColor(Color color, float speed)
        {
            if (isTransitioningColor)
                characterManager.StopCoroutine(co_transitioningColor);

            co_transitioningColor = characterManager.StartCoroutine(TransitioningColorCoroutine(color, speed));
            return co_transitioningColor;
        }

        private IEnumerator TransitioningColorCoroutine(Color color, float speedMultiplier)
        {
            Color oldColor = renderer.color;

            List<Image> oldImages = new List<Image>();

            foreach (CanvasGroup oldRendererCG in oldRenderers)
            {
                oldImages.Add(oldRendererCG.GetComponent<Image>());
            }

            float colorPercent = 0;
            while(colorPercent < 1f)
            {
                colorPercent += DEFAULT_TRANSITION_SPEED * speedMultiplier * Time.deltaTime;

                renderer.color = Color.Lerp(oldColor, color, colorPercent);
                foreach (Image item in oldImages)
                {
                    item.color = renderer.color;
                }

                yield return null;
            }

            co_transitioningColor = null; // Reset the coroutine reference
        }

        public void StopTransitioningColor()
        {
            if (!isTransitioningColor)
                return;

            characterManager.StopCoroutine(co_transitioningColor);
            co_transitioningColor = null;
        }

    }
}