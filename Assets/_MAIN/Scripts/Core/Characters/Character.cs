using DIALOGUE;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace CHARACTER
{

    // Base class for all characters which all types of characters will inherit from

    public abstract class Character 
    {
        public const float UNHIGHLIGHTED_DARKEN_STRENGTH = 0.65f;

        protected bool showOnStart = false; 
        public string name = "";
        public string displayName = "";
        public RectTransform root = null;
        public CharacterConfigData configData;
        public Animator animator;

        public Color color { get; protected set; } = Color.white;
        protected Color displayColor => highlighted ? highlightedColor : unhighlightedColor;
        protected Color highlightedColor => color;
        protected Color unhighlightedColor => new Color(color.r * UNHIGHLIGHTED_DARKEN_STRENGTH, color.g * UNHIGHLIGHTED_DARKEN_STRENGTH, color.b * UNHIGHLIGHTED_DARKEN_STRENGTH, color.a);
        public bool highlighted { get; protected set; } = true;

        protected CharacterManager characterManager => CharacterManager.Instance;

        protected Coroutine co_hiding, co_showing;
        public bool isShowing => co_showing != null;
        public bool isHiding => co_hiding != null;
        public virtual bool isVisible {get; set;}  // Always false for text characters

        protected Coroutine co_moving;
        public bool isMoving => co_moving != null;

        protected Coroutine co_changingColor;
        public bool isChangingColor => co_changingColor != null;

        public Coroutine co_highlighting;
        public bool isHighlighting => (highlighted && co_highlighting != null);
        public bool isUnHighlighting => (!highlighted && co_highlighting != null);

        public Character(string name, CharacterConfigData configData, GameObject prefab = null)
        {
            this.name = name;
            displayName = name;
            this.configData = configData;

            if(prefab != null)
            {
                GameObject ob = Object.Instantiate(prefab, CharacterManager.Instance.CharacterPanel);
                // after instantiating the prefab, by default, the name of the object will be the prefab name with "(Clone)" suffix.
                // We set the name of the object to the character prefab name so it is more readable and easier to find in the hierarchy.
                ob.name = characterManager.FormatCharacterPath(characterManager.characterPrefabNameFormat, name);
                ob.SetActive(true);
                root = ob.GetComponent<RectTransform>();
                animator = ob.GetComponentInChildren<Animator>();
            }
        }

        public virtual Coroutine Show()
        {
            if(isShowing) return co_showing;

            if (isHiding)
            {
                characterManager.StopCoroutine(co_hiding);
            }

            co_showing = characterManager.StartCoroutine(ShowingOrHiding(true));
            return co_showing;
        }

        public virtual Coroutine Hide()
        {
            if(isHiding) return co_hiding;

            if (isShowing)
            {
                characterManager.StopCoroutine(co_showing);
            }

            co_hiding = characterManager.StartCoroutine(ShowingOrHiding(false));
            return co_hiding;
        }

        public virtual IEnumerator ShowingOrHiding(bool show)
        {
            Debug.Log("Cant call IEnumerator ShowingOrHiding() on base character class");
            yield return null;
        }

        // This Say function will be used for a case when we create a simple temporary character and doesnt actually have a dialogue file to
        // load by DialogueSystem. The Say method belongs to character class itself make it easier to call
        public Coroutine Say(string dialouge) => Say(new List<string>() { dialouge });

        public Coroutine Say(List<string> dialouges)
        {
            DialogueSystem.Instance.ShowSpeakerName(displayName);
            UpdateTextCustomizationOnScreen();
            return DialogueSystem.Instance.Say(dialouges);
        }

        public void UpdateTextCustomizationOnScreen()
        {
            DialogueSystem.Instance.ApplySpeakerDataToDialogueContainer(configData);
        }

        public void SetNameColor(Color color) => configData.nameColor = color;
        public void SetDialogueColor(Color color) => configData.dialogueColor = color;
        public void SetNameFont(TMP_FontAsset font) => configData.nameFont = font;
        public void SetDialogueFont(TMP_FontAsset font) => configData.dialogueFont = font;
        public void ResetCharacterDefaultConfig() => configData = CharacterManager.Instance.GetCharacterConfigData(name);

        // This section handles the position of the character on the screen.
        // We set, move character with normalized values (0->1)
        public virtual void SetPosition(Vector2 pos)
        {
            if(root == null)
                return;

            (Vector2 minAnchorTarget, Vector2 maxAnchorTarget) = ConvertUITargetPositionToRelativeCharacterAnchorTargets(pos);

            root.anchorMin = minAnchorTarget;
            root.anchorMax = maxAnchorTarget;
        }

        public virtual Coroutine MoveToPosition(Vector2 pos, float speed = 2f, bool smooth = false)
        {
            if (root == null)
                return null;

            if (isMoving)
                characterManager.StopCoroutine(co_moving);

            co_moving = characterManager.StartCoroutine(MovingToPos(pos, speed, smooth));
            return co_moving;
        }

        private IEnumerator MovingToPos(Vector2 pos, float speed = 2f, bool smooth = false)
        {
            (Vector2 minAnchorTarget, Vector2 maxAnchorTarget) = ConvertUITargetPositionToRelativeCharacterAnchorTargets(pos);
            Vector2 padding = root.anchorMax - root.anchorMin;

            while(root.anchorMin != minAnchorTarget || root.anchorMax != maxAnchorTarget)
            {
                root.anchorMin = smooth ?
                    Vector2.Lerp(root.anchorMin, minAnchorTarget, Time.deltaTime * speed)
                  : Vector2.MoveTowards(root.anchorMin, minAnchorTarget, Time.deltaTime * speed * 0.35f);
                // Because MoveTowords is much faster than Lerp, we need to slow it down a bit by * 0.35f

                root.anchorMax = root.anchorMin + padding;

                // If we are moving smoothly, we can stop the coroutine when we are close enough to the target
                // because Lerp will micro increment values so it may take a while to reach the target
                if (smooth && Vector2.Distance(root.anchorMin, minAnchorTarget) <= 0.001f)
                {
                    root.anchorMin = minAnchorTarget;
                    root.anchorMax = maxAnchorTarget;
                    break;
                }

                yield return null;
            }

            co_moving = null; // Reset the coroutine reference when done
        }

        protected (Vector2, Vector2) ConvertUITargetPositionToRelativeCharacterAnchorTargets(Vector2 pos)
        {
            Vector2 padding = root.anchorMax - root.anchorMin;

            float maxX = 1f - padding.x;
            float maxY = 1f - padding.y;

            Vector2 minAnchorTarget = new Vector2(maxX * pos.x, maxY * pos.y);
            Vector2 maxAnchorTarget = minAnchorTarget + padding;

            return (minAnchorTarget, maxAnchorTarget);
        }

        public virtual void SetColor(Color color)
        {
            this.color = color;
        }

        public Coroutine TransitionColor(Color color, float speed = 1f)
        {
            this.color = color;

            if(isChangingColor)
                characterManager.StopCoroutine(co_changingColor);
            
            co_changingColor = characterManager.StartCoroutine(TransitioningColor(displayColor, speed));
            return co_changingColor;
        }

        public virtual IEnumerator TransitioningColor(Color color, float speed)
        {
            Debug.Log("Cant call IEnumerator TransitioningColor() on base character class");
            yield return null;
        }

        public Coroutine Highlight(float speed = 1f)
        {
            if(isHighlighting)
                return co_highlighting;

            if (isUnHighlighting)
                characterManager.StopCoroutine(co_highlighting);

            highlighted = true;
            co_highlighting = characterManager.StartCoroutine(Highlighting(highlighted, speed));
            return co_highlighting;
        }

        public Coroutine UnHighlight(float speed = 1f)
        {
            if (isUnHighlighting)
                return co_highlighting;

            if (isHighlighting)
                characterManager.StopCoroutine(co_highlighting);

            highlighted = false;
            co_highlighting = characterManager.StartCoroutine(Highlighting(highlighted, speed));
            return co_highlighting;
        }

        public virtual IEnumerator Highlighting(bool highlight, float speedMultiplier)
        {
            Debug.Log("Cant call IEnumerator Highlighting() on base character class");
            yield return null;
        }

        public enum CharacterType
        {
            Text,
            Sprite,
            SpriteSheet,
            Live2D,
            Model3D
        }
    }
}