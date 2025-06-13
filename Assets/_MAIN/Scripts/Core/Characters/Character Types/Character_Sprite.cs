using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace CHARACTER
{
    public class Character_Sprite : Character
    {
        public override bool isVisible
        {
            get { return isShowing || rootCanvasGroup.alpha > 0; }
            set { rootCanvasGroup.alpha = value ? 1 : 0; }
        }

        private const string SPRITE_RENDERERS_PARENT_NAME = "Renderers";
        private const string DEFAULT_SPRITESHEET_NAME = "Default";
        private const char SPRITESHEET_SPRITE_SEPERATOR = '-';

        private CanvasGroup rootCanvasGroup;
        public List<CharacterSpriteLayer> layersList = new List<CharacterSpriteLayer>();

        private string spritesFolderPath = "";

        public Character_Sprite(string name, CharacterConfigData configData, GameObject prefab, string rootAssetFolder) : base(name, configData, prefab)
        {
            Debug.Log("Character_Sprite constructor called with name: " + name);

            rootCanvasGroup = root.GetComponent<CanvasGroup>();
            rootCanvasGroup.alpha = showOnStart ? 1 : 0;

            spritesFolderPath = rootAssetFolder + "/Images";
            //Debug.Log($"{name} folder path: {spritesFolderPath}");

            GetLayers();
        }

        private void GetLayers()
        {
            Transform rendererRoot = animator.transform.Find(SPRITE_RENDERERS_PARENT_NAME);

            if (rendererRoot == null)
                return;

            for (int i = 0; i < rendererRoot.childCount; i++)
            {
                Transform child = rendererRoot.GetChild(i);

                Image renderderImage = child.GetComponentInChildren<Image>();
                if (renderderImage != null)
                {
                    CharacterSpriteLayer layer = new CharacterSpriteLayer(renderderImage, i);
                    layersList.Add(layer);
                    child.name = $"Layer: {i}";
                }
            }
        }

        public void SetSprite(Sprite sprite, int layer = 0)
        {
            layersList[layer].SetSprite(sprite);
        }

        public Sprite GetSprite(string spriteName)
        {
            if (configData.characterType == CharacterType.SpriteSheet)
            {
                string[] spriteSheetData = spriteName.Split(SPRITESHEET_SPRITE_SEPERATOR, StringSplitOptions.RemoveEmptyEntries);

                if (spriteSheetData.Length == 2)
                {
                    // Specified texture (spritesheet) name and sprite name
                    string textureName = spriteSheetData[0].Trim();
                    spriteName = spriteSheetData[1].Trim();

                    Sprite[] sprites = Resources.LoadAll<Sprite>($"{spritesFolderPath}/{textureName}");

                    if (sprites.Length == 0)
                        Debug.LogError($"No sprites found in spritesheet <{textureName}> at path <{spritesFolderPath}>");

                    return Array.Find(sprites, sprite => sprite.name == spriteName);
                }
                else
                {
                    // Didnt specify texture (spritesheet) name, we will use the default spritesheet
                    Sprite[] defaultSprites = Resources.LoadAll<Sprite>($"{spritesFolderPath}/{DEFAULT_SPRITESHEET_NAME}");

                    if (defaultSprites.Length == 0)
                        Debug.LogError($"No sprites found in spritesheet <{DEFAULT_SPRITESHEET_NAME}> at path <{spritesFolderPath}>");

                    return Array.Find(defaultSprites, sprite => sprite.name == spriteName);
                }
            }
            else
            {
                return Resources.Load<Sprite>($"{spritesFolderPath}/{spriteName}");
            }
        }

        public Coroutine TransitionSprite(Sprite sprite, int layer = 0, float speed = 1f)
        {
            CharacterSpriteLayer spriteLayer = layersList[layer];

            return spriteLayer.TransitioningSprite(sprite, speed);
        }

        public override IEnumerator ShowingOrHiding(bool show)
        {
            float targetAlpha = show ? 1.0f : 0.0f;

            while (rootCanvasGroup.alpha != targetAlpha)
            {
                rootCanvasGroup.alpha = Mathf.MoveTowards(rootCanvasGroup.alpha, targetAlpha, 3f * Time.deltaTime);
                yield return null;
            }

            co_hiding = null;
            co_showing = null;
        }

        public override void SetColor(Color color)
        {
            base.SetColor(color);
            color = displayColor;

            foreach (CharacterSpriteLayer layer in layersList)
            {
                layer.StopTransitioningColor();
                layer.SetColor(color);
            }
        }

        public override IEnumerator TransitioningColor(float speed)
        {
            foreach (CharacterSpriteLayer layer in layersList)
                layer.TransitioningColor(displayColor, speed);

            yield return null;

            while (layersList.Any(layer => layer.isTransitioningColor))
            {
                yield return null;
            }

            co_transitioningColor = null; // Reset the coroutine reference when done
        }

        public override IEnumerator Highlighting(float speedMultiplier)
        {
            Color targetColor = displayColor;

            foreach (CharacterSpriteLayer layer in layersList)
                layer.TransitioningColor(targetColor, speedMultiplier);

            yield return null;

            while (layersList.Any(layer => layer.isTransitioningColor))
            {
                yield return null;
            }

            co_highlighting = null;
        }

        public override IEnumerator Flipping(bool faceLeftNow, float speedMultiplier, bool immediate)
        {
            foreach (CharacterSpriteLayer layer in layersList)
            {
                if (faceLeftNow)
                    layer.FaceLeft(speedMultiplier, immediate);
                else
                    layer.FaceRight(speedMultiplier, immediate);
            }

            yield return null;

            while (layersList.Any(layer => layer.isFlipping))
                yield return null;

            co_flipping = null;
        }

    }
}