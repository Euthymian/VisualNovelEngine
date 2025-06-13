using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CHARACTER
{
    public class Character_Model3D : Character
    {
        private const string RENDER_GROUP_PREFAB_ID = "RenderGroup - [{0}]";
        private const string CHARACTER_RENDER_TEXTURE_NAME = "RenderTexture";
        private const int CHARACTER_STACKING_DEPTH = 15;
        private const float EXPRESSION_TRANSITION_SPEED = 100;
        private const float DEFAULT_TRANSITION_SPEED = 3f;
        private const float DEFAULT_FACING_DIRECTION_VALUE = 25f;

        GameObject renderGroup;
        private Camera modelCamera;
        private Transform modelContainer, model;
        private Animator modelAnimator;
        private SkinnedMeshRenderer eyesController;
        private SkinnedMeshRenderer headController;
        private SkinnedMeshRenderer lashesController;

        private RawImage mainCanvasRenderer;
        private CanvasGroup rootCanvasGroup => root.GetComponent<CanvasGroup>();
        private CanvasGroup mainCanvasRendererCanvasGroup => mainCanvasRenderer.GetComponent<CanvasGroup>();

        private Dictionary<string, Coroutine> expressionCoroutineDict = new Dictionary<string, Coroutine>();

        private struct OldRenderer
        {
            public CanvasGroup oldCanvasGroup;
            public RawImage oldMainCanvasRenderer;
            public GameObject oldRenderGroup;

            public OldRenderer(CanvasGroup oldCanvasGroup, RawImage oldMainCanvasRenderer, GameObject oldRenderGroup)
            {
                this.oldCanvasGroup = oldCanvasGroup;
                this.oldMainCanvasRenderer = oldMainCanvasRenderer;
                this.oldRenderGroup = oldRenderGroup;
            }
        }
        private List<OldRenderer> oldRenderers = new List<OldRenderer>();

        public override bool isVisible 
        { 
            get => isShowing || rootCanvasGroup.alpha > 0; 
            set => rootCanvasGroup.alpha = value ? 1 : 0; 
        }

        private Coroutine co_fadingOutOldRenderers = null;
        private bool isFadingOutOldRenderers => co_fadingOutOldRenderers != null;
        private float oldRenderersFadeOutSpeedMultiplier = DEFAULT_TRANSITION_SPEED;

        public Character_Model3D(string name, CharacterConfigData configData, GameObject prefab, string rootAssetFolder) : base(name, configData, prefab)
        {
            Debug.Log("Character_Model3D constructor called with name: " + name);

            string pathToRenderGroup = rootAssetFolder + '/' + string.Format(RENDER_GROUP_PREFAB_ID, configData.name);
            GameObject renderGroupPrefab = Resources.Load<GameObject>(pathToRenderGroup);
            renderGroup = Object.Instantiate(renderGroupPrefab, characterManager.CharacterPanelModel3D);
            renderGroup.name = string.Format(RENDER_GROUP_PREFAB_ID, configData.name);
            renderGroup.transform.localPosition += Vector3.down * CHARACTER_STACKING_DEPTH * characterManager.GetCurrentNumberOfCharactersByType(CharacterType.Model3D);

            modelCamera = renderGroup.GetComponentInChildren<Camera>();
            modelContainer = modelCamera.transform.GetChild(0);
            model = modelContainer.GetChild(0);
            modelAnimator = model.GetComponent<Animator>();

            SkinnedMeshRenderer[] headSkinnedMeshRendererList = model.GetChild(0).GetComponentsInChildren<SkinnedMeshRenderer>();
            eyesController = headSkinnedMeshRendererList[1];
            headController = headSkinnedMeshRendererList[2];
            lashesController = headSkinnedMeshRendererList[3];

            mainCanvasRenderer = animator.GetComponentInChildren<RawImage>();
            RenderTexture renderTexture = Resources.Load<RenderTexture>(rootAssetFolder + '/' + CHARACTER_RENDER_TEXTURE_NAME);
            RenderTexture newTexture = new RenderTexture(renderTexture);
            mainCanvasRenderer.texture = newTexture;
            modelCamera.targetTexture = newTexture;

            rootCanvasGroup.alpha = showOnStart ? 1 : 0;
        }

        public void SetMotion(string motionName)
        {
            modelAnimator.Play(motionName);
        }

        public Coroutine SetExpression(string expressionName, float percent = 100, float speedMultiplier = 1, bool immediate = false)
        {
            if (eyesController == null || headController == null || lashesController == null)
            {
                Debug.Log($"Some BlendShapes of ${displayName} dont exist");
                return null;
            }

            if (expressionCoroutineDict.ContainsKey(expressionName))
            {
                characterManager.StopCoroutine(expressionCoroutineDict[expressionName]);
                expressionCoroutineDict.Remove(expressionName);
            }

            percent = percent / 100f;
            Coroutine expressionCoroutine = characterManager.StartCoroutine(ExpressionCoroutine(expressionName, percent, speedMultiplier, immediate));
            expressionCoroutineDict[expressionName] = expressionCoroutine;

            return expressionCoroutine;
        }

        private IEnumerator ExpressionCoroutine(string expressionName, float percent, float speedMultiplier, bool immediate)
        {
            Model3D_ExpressionsSO.Expression expression = characterManager.model3DExpressionsSO.GetExpressionByName(expressionName);

            if (immediate)
            {
                foreach (Model3D_ExpressionsSO.EyesBlendShape_Weight eyeBlendShape in expression.eyeBlendShapes)
                    eyesController.SetBlendShapeWeight(eyesController.sharedMesh.GetBlendShapeIndex(eyeBlendShape.eyesBlendShapeName), eyeBlendShape.weight * percent);

                foreach (Model3D_ExpressionsSO.HeadBlendShape_Weight headBlendShape in expression.headBlendShapes)
                    headController.SetBlendShapeWeight(headController.sharedMesh.GetBlendShapeIndex(headBlendShape.headBlendShapeName), headBlendShape.weight * percent);

                foreach (Model3D_ExpressionsSO.LashesBlendShape_Weight lashesBlendShape in expression.lashesBlendShapes)
                    lashesController.SetBlendShapeWeight(lashesController.sharedMesh.GetBlendShapeIndex(lashesBlendShape.lashesBlendShapeName), lashesBlendShape.weight * percent);
            }
            else
            {
                while (!IsCompleteExpressionTransition(expression, percent))
                {

                    foreach (Model3D_ExpressionsSO.EyesBlendShape_Weight eyesBlendShape in expression.eyeBlendShapes)
                    {
                        int blendShapeIndex = eyesController.sharedMesh.GetBlendShapeIndex(eyesBlendShape.eyesBlendShapeName);

                        float currentWeight = eyesController.GetBlendShapeWeight(blendShapeIndex);

                        currentWeight = Mathf.MoveTowards(currentWeight, Mathf.Round(eyesBlendShape.weight * percent), EXPRESSION_TRANSITION_SPEED * speedMultiplier * Time.deltaTime);
                        eyesController.SetBlendShapeWeight(blendShapeIndex, currentWeight);
                    }

                    foreach (Model3D_ExpressionsSO.HeadBlendShape_Weight headBlendShape in expression.headBlendShapes)
                    {
                        int blendShapeIndex = headController.sharedMesh.GetBlendShapeIndex(headBlendShape.headBlendShapeName);

                        float currentWeight = headController.GetBlendShapeWeight(blendShapeIndex);

                        currentWeight = Mathf.MoveTowards(currentWeight, Mathf.Round(headBlendShape.weight * percent), EXPRESSION_TRANSITION_SPEED * speedMultiplier * Time.deltaTime);
                        headController.SetBlendShapeWeight(blendShapeIndex, currentWeight);
                    }

                    foreach (Model3D_ExpressionsSO.LashesBlendShape_Weight lashesBlendShape in expression.lashesBlendShapes)
                    {
                        int blendShapeIndex = lashesController.sharedMesh.GetBlendShapeIndex(lashesBlendShape.lashesBlendShapeName);

                        float currentWeight = lashesController.GetBlendShapeWeight(blendShapeIndex);

                        currentWeight = Mathf.MoveTowards(currentWeight, Mathf.Round(lashesBlendShape.weight * percent), EXPRESSION_TRANSITION_SPEED * speedMultiplier * Time.deltaTime); 
                        lashesController.SetBlendShapeWeight(blendShapeIndex, currentWeight);
                    }

                    yield return null;
                }
            }

            expressionCoroutineDict.Remove(expressionName);
        }


        private bool IsCompleteExpressionTransition(Model3D_ExpressionsSO.Expression expression, float percent)
        {
            foreach (Model3D_ExpressionsSO.EyesBlendShape_Weight eyesBlendShape in expression.eyeBlendShapes)
            {
                if (eyesController.GetBlendShapeWeight(eyesController.sharedMesh.GetBlendShapeIndex(eyesBlendShape.eyesBlendShapeName)) != Mathf.Round(eyesBlendShape.weight * percent))
                    return false;
            }

            foreach (Model3D_ExpressionsSO.HeadBlendShape_Weight headBlendShape in expression.headBlendShapes)
            {
                if (headController.GetBlendShapeWeight(headController.sharedMesh.GetBlendShapeIndex(headBlendShape.headBlendShapeName)) != Mathf.Round(headBlendShape.weight * percent))
                    return false;
            }

            foreach (Model3D_ExpressionsSO.LashesBlendShape_Weight lashesBlendShape in expression.lashesBlendShapes)
            {
                if (lashesController.GetBlendShapeWeight(lashesController.sharedMesh.GetBlendShapeIndex(lashesBlendShape.lashesBlendShapeName)) != Mathf.Round(lashesBlendShape.weight * percent))
                    return false;
            }

            return true;
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

            mainCanvasRenderer.color = color;

            foreach (OldRenderer oldRenderer in oldRenderers)
                oldRenderer.oldMainCanvasRenderer.color = color;
        }

        public override IEnumerator TransitioningColor(float speedMultiplier)
        {
            yield return TransitionColorCoroutine(speedMultiplier);

            co_transitioningColor = null;
        }

        public override IEnumerator Highlighting(float speedMultiplier)
        {
            if(!isTransitioningColor)
                yield return TransitionColorCoroutine(speedMultiplier);

            co_highlighting = null;
        }

        private IEnumerator TransitionColorCoroutine(float speedMultiplier)
        {
            Color oldColor = mainCanvasRenderer.color;

            float colorPercent = 0f;
            while (colorPercent <= 1f)
            {
                colorPercent += Time.deltaTime * speedMultiplier * DEFAULT_TRANSITION_SPEED;

                Color currentLerpedColor = Color.Lerp(oldColor, displayColor, colorPercent);
                mainCanvasRenderer.color = currentLerpedColor;
                foreach (OldRenderer oldRenderer in oldRenderers)
                {
                    oldRenderer.oldMainCanvasRenderer.color = currentLerpedColor;
                }

                yield return null;
            }
        }

        private void CreateNewCharacterInstance()
        {
            oldRenderers.Add(new OldRenderer(
                mainCanvasRendererCanvasGroup,
                mainCanvasRenderer,
                renderGroup
            ));

            renderGroup = Object.Instantiate(renderGroup, renderGroup.transform.parent);
            renderGroup.name = string.Format(RENDER_GROUP_PREFAB_ID, configData.name);
            for(int i=0;i<oldRenderers.Count; i++)
            {
                oldRenderers[i].oldRenderGroup.transform.localPosition = Vector3.zero +  Vector3.right * CHARACTER_STACKING_DEPTH * i;
            }
            renderGroup.transform.localPosition = Vector3.zero + Vector3.right * CHARACTER_STACKING_DEPTH * oldRenderers.Count;

            modelCamera = renderGroup.GetComponentInChildren<Camera>();
            modelContainer = modelCamera.transform.GetChild(0);
            model = modelContainer.GetChild(0);
            modelAnimator = model.GetComponent<Animator>();

            SkinnedMeshRenderer[] headSkinnedMeshRendererList = model.GetChild(0).GetComponentsInChildren<SkinnedMeshRenderer>();
            eyesController = headSkinnedMeshRendererList[1];
            headController = headSkinnedMeshRendererList[2];
            lashesController = headSkinnedMeshRendererList[3];

            string mainCanvasRendererName = mainCanvasRenderer.name;
            Texture oldMainCanvasRendererTexture = mainCanvasRenderer.texture;

            mainCanvasRenderer = Object.Instantiate(mainCanvasRenderer.gameObject, mainCanvasRenderer.transform.parent).GetComponent<RawImage>();
            mainCanvasRenderer.name = mainCanvasRendererName;
            mainCanvasRendererCanvasGroup.alpha = 0;

            RenderTexture newTexture = new RenderTexture(oldMainCanvasRendererTexture as RenderTexture);
            mainCanvasRenderer.texture = newTexture;
            modelCamera.targetTexture = newTexture;
        }

        private IEnumerator FadingOutOldRenderer()
        {
            while(oldRenderers.Any(o => o.oldCanvasGroup.alpha > 0))
            {
                float speed = Time.deltaTime * oldRenderersFadeOutSpeedMultiplier * DEFAULT_TRANSITION_SPEED;
                foreach (OldRenderer or in oldRenderers)
                {
                    or.oldCanvasGroup.alpha = Mathf.MoveTowards(or.oldCanvasGroup.alpha, 0, speed);
                }

                yield return null;
            }

            foreach (OldRenderer or in oldRenderers)
            {
                Object.Destroy(or.oldRenderGroup);
                Object.Destroy(or.oldCanvasGroup.gameObject);
            }
            oldRenderers.Clear();

            co_fadingOutOldRenderers = null;
        }

        public override IEnumerator Flipping(bool facingLeftNow, float speedMultiplier, bool immediate)
        {
            Vector3 facingAngle = new Vector3(0, (facingLeftNow ? DEFAULT_FACING_DIRECTION_VALUE : -DEFAULT_FACING_DIRECTION_VALUE), 0);

            if (immediate)
            {
                modelContainer.localEulerAngles = facingAngle;
            }
            else
            {
                CreateNewCharacterInstance();
                modelContainer.localEulerAngles = facingAngle;

                oldRenderersFadeOutSpeedMultiplier = speedMultiplier;
                if(!isFadingOutOldRenderers)
                    co_fadingOutOldRenderers = characterManager.StartCoroutine(FadingOutOldRenderer());

                CanvasGroup newMainCanvasRendererCanvasGroup = mainCanvasRendererCanvasGroup;
                while(newMainCanvasRendererCanvasGroup.alpha != 1)
                {
                    float speed = Time.deltaTime * speedMultiplier * DEFAULT_TRANSITION_SPEED;
                    newMainCanvasRendererCanvasGroup.alpha = Mathf.MoveTowards(newMainCanvasRendererCanvasGroup.alpha, 1, speed);
                    yield return null;
                }

            }

            co_flipping = null; 
        }
    }
}