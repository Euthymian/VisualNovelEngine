using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHARACTER
{
    public class Character_Sprite : Character
    {
        private CanvasGroup rootCanvasGroup;

        public Character_Sprite(string name, CharacterConfigData configData, GameObject prefab) : base(name, configData, prefab)
        {
            Debug.Log("Character_Sprite constructor called with name: " + name);
            rootCanvasGroup = root.GetComponent<CanvasGroup>();
            rootCanvasGroup.alpha = 0;
        }

        public override IEnumerator ShowingOrHiding(bool show)
        {
            float targetAlpha = show ? 1.0f : 0.0f;

            while(rootCanvasGroup.alpha != targetAlpha)
            {
                rootCanvasGroup.alpha = Mathf.MoveTowards(rootCanvasGroup.alpha, targetAlpha, 3f * Time.deltaTime);
                yield return null;
            }

            co_hiding = null;
            co_showing = null;
        }
    }
}