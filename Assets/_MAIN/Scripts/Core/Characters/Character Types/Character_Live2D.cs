using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Rendering;
using Live2D.Cubism.Framework.Expression;
using System.Linq;

namespace CHARACTER
{
    public class Character_Live2D : Character
    {
        private const float DEFAULT_TRANSITION_SPEED = 3f;
        private const int CHARACTER_SORTING_DEPTH_UNIT = 250;

        private CubismRenderController cubismRenderController;
        private CubismExpressionController cubismExpressionController;
        private Animator motionAnimator;

        private List<CubismRenderController> oldCubismRenderControllerList = new List<CubismRenderController>();

        public override bool isVisible 
        { 
            get => isShowing || cubismRenderController.Opacity > 0;
            set => cubismRenderController.Opacity = value ? 1 : 0; 
        }

        public Character_Live2D(string name, CharacterConfigData configData, GameObject prefab, string rootAssetFolder) : base(name, configData, prefab)
        {
            Debug.Log("Character_Live2D constructor called with name: " + name);

            motionAnimator = animator.transform.GetChild(0).GetComponentInChildren<Animator>();
            /*
            GetComponentInChildren<Animator>() will also find the component on the parent containing the children
            -> if we get the motionAnimator by
                    motionAnimator = animator.GetComponentInChildren<Animator>();
            it will return the animator on the parent, not the one on the child.
            */
            cubismExpressionController = motionAnimator.GetComponent<CubismExpressionController>();
            cubismRenderController = motionAnimator.GetComponent<CubismRenderController>();
            cubismRenderController.Opacity = showOnStart ? 1 : 0;
        }

        public void SetAnimation(string animationName)
        {
            motionAnimator.Play(animationName);
        }

        public void SetExpression(int expressionIndex)
        {
            cubismExpressionController.CurrentExpressionIndex = expressionIndex;
        }

        public void SetExpression(string expressionName)
        {
            cubismExpressionController.CurrentExpressionIndex = GetExpressionIndexByName(expressionName);
        }

        private int GetExpressionIndexByName(string expressionName)
        {
            expressionName = expressionName.ToLower();
            for (int i = 0; i < cubismExpressionController.ExpressionsList.CubismExpressionObjects.Length; i++)
            {
                CubismExpressionData exprData = cubismExpressionController.ExpressionsList.CubismExpressionObjects[i];
                if (exprData.name.Split('.')[0].ToLower() == expressionName)
                    return i;
            }
            return -1;
        }

        public override IEnumerator ShowingOrHiding(bool show)
        {
            float targetOpacity = show ? 1f : 0f;

            while(cubismRenderController.Opacity != targetOpacity)
            {
                cubismRenderController.Opacity = Mathf.MoveTowards(cubismRenderController.Opacity, targetOpacity, DEFAULT_TRANSITION_SPEED * Time.deltaTime);
                yield return null;
            }

            co_showing = null;
            co_hiding = null;
        }

        public override void SetColor(Color color)
        {
            base.SetColor(color);
            color = displayColor;

            foreach(CubismRenderer cr in cubismRenderController.Renderers)
            {
                cr.Color = color;
            }
        }

        public override IEnumerator TransitioningColor(float speed)
        {
            yield return TransitioningColorLive2D(speed);

            co_transitioningColor = null;
        }

        private IEnumerator TransitioningColorLive2D(float speed)
        {
            CubismRenderer[] crs = cubismRenderController.Renderers;
            Color startColor = crs[0].Color;

            float colorPercent = 0f;
            while (colorPercent <= 1f)
            {
                //colorPercent = Mathf.Clamp01(colorPercent + DEFAULT_TRANSITION_SPEED * speed * Time.deltaTime);
                colorPercent = colorPercent + DEFAULT_TRANSITION_SPEED * speed * Time.deltaTime;
                Color currentLerpedColor = Color.Lerp(startColor, displayColor, colorPercent);

                foreach (CubismRenderer cr in cubismRenderController.Renderers)
                {
                    cr.Color = currentLerpedColor;
                }

                yield return null;
            }
        }

        public override IEnumerator Highlighting(float speedMultiplier)
        {
            if(!isTransitioningColor)
                yield return TransitioningColorLive2D(speedMultiplier);

            co_highlighting = null;
        }

        public override IEnumerator Flipping(bool facingLeftNow, float speedMultiplier, bool immediate)
        {
            GameObject newLive2dCharacter = CreateNewCharacterController();
            float xScale = newLive2dCharacter.transform.localScale.x;
            newLive2dCharacter.transform.localScale = new Vector3(facingLeftNow ? xScale : -xScale, newLive2dCharacter.transform.localScale.y, newLive2dCharacter.transform.localScale.z);
            cubismRenderController.Opacity = 0;
            
            float transitionSpeed = DEFAULT_TRANSITION_SPEED * speedMultiplier * Time.deltaTime;

            while(cubismRenderController.Opacity < 1 || oldCubismRenderControllerList.Any(r => r.Opacity < 0))
            {
                cubismRenderController.Opacity = Mathf.MoveTowards(cubismRenderController.Opacity, 1f, transitionSpeed);
                foreach (CubismRenderController oldController in oldCubismRenderControllerList)
                {
                    oldController.Opacity = Mathf.MoveTowards(oldController.Opacity, 0f, transitionSpeed);
                }

                yield return null;
            }

            foreach (CubismRenderController oldController in oldCubismRenderControllerList)
            {
                Object.Destroy(oldController.gameObject);
            }
            oldCubismRenderControllerList.Clear();

            co_flipping = null;
        }

        private GameObject CreateNewCharacterController()
        {
            oldCubismRenderControllerList.Add(cubismRenderController);

            GameObject newLive2dCharacter = Object.Instantiate(cubismRenderController.gameObject, cubismRenderController.transform.parent);
            newLive2dCharacter.name = name;
            cubismExpressionController = newLive2dCharacter.GetComponent<CubismExpressionController>();
            cubismRenderController = newLive2dCharacter.GetComponent<CubismRenderController>();
            motionAnimator = newLive2dCharacter.GetComponent<Animator>();

            return newLive2dCharacter;
        }

        public override void OnSort(int sortingIndex)
        {
            cubismRenderController.SortingOrder = sortingIndex * CHARACTER_SORTING_DEPTH_UNIT;
        }
    }
}