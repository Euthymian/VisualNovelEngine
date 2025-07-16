using CHARACTER;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HISTORY
{
    [System.Serializable]
    public class CharacterData 
    {
        [System.Serializable]
        public class SpriteData
        {
            public List<LayerData> layers;

            [System.Serializable]
            public class LayerData
            {
                public string spriteName;
                public Color color;
            }
        }

        [System.Serializable]
        public class Live2DData
        {
            public string expression;
            public string motion;
        }

        [System.Serializable]
        public class Model3DData
        {
            public Vector3 pos;
            public Quaternion rotation;
        }

        [System.Serializable]
        public class MotionData
        {
            public List<MotionParamater> parameters = new List<MotionParamater>();

            [System.Serializable]
            public class MotionParamater
            {
                public string name;
                public string type;
                public string value;
            }
        }

        public string characterName;
        public string displayName;
        public bool enabled;
        public Color color;
        public int priority;
        public bool isHighlighted;
        public bool isFacingLeft;
        public Vector2 position;
        public CharacterConfigCache characterConfigCache;

        public string generalMotionJSON;

        // This is for all other specific data for each character type (Sprite, 3D Model, Live2D)
        public string dataJSON;

        [System.Serializable]
        public class CharacterConfigCache
        {
            public string name;
            public string alias; 

            public Character.CharacterType characterType;

            public Color nameColor;
            public Color dialogueColor;

            public string nameFont;
            public string dialogueFont;

            public float nameFontSize;
            public float dialogueFontSize;

            public CharacterConfigCache(CharacterConfigData reference)
            {
                name = reference.name;
                alias = reference.alias;
                characterType = reference.characterType;
                nameColor = reference.nameColor;
                dialogueColor = reference.dialogueColor;
                nameFont = FilePaths.resources_font + reference.nameFont.name;
                dialogueFont = FilePaths.resources_font + reference.dialogueFont.name;
                nameFontSize = reference.nameFontSize;
                dialogueFontSize = reference.dialogueFontSize;
            }
        }

        public static List<CharacterData> Capture()
        {
            List<CharacterData> characterDatas = new List<CharacterData>();

            foreach(var character in CharacterManager.Instance.allCharacters)
            {
                if(!character.isVisible)
                    continue;

                CharacterData charData = new CharacterData();
                charData.characterName = character.name;
                charData.displayName = character.displayName;
                charData.enabled = character.isVisible;
                charData.color = character.color;
                charData.priority = character.priority;
                charData.isHighlighted = character.highlighted;
                charData.isFacingLeft = character.isFacingLeft;
                charData.position = character.targetPos;
                charData.characterConfigCache = new CharacterConfigCache(character.configData);
                charData.generalMotionJSON = GetMotionData(character);

                switch (character.configData.characterType)
                {
                    case Character.CharacterType.Sprite:
                    case Character.CharacterType.SpriteSheet:
                        SpriteData spriteData = new SpriteData();
                        spriteData.layers = new List<SpriteData.LayerData>();

                        Character_Sprite sc = character as Character_Sprite;

                        foreach (var layer in sc.layersList)
                        {
                            SpriteData.LayerData layerData = new SpriteData.LayerData();
                            layerData.spriteName = layer.renderer.sprite.name;
                            layerData.color = layer.renderer.color;
                            spriteData.layers.Add(layerData);
                        }

                        charData.dataJSON = JsonUtility.ToJson(spriteData);
                        break;

                    case Character.CharacterType.Live2D:
                        Live2DData live2DData = new Live2DData();

                        Character_Live2D lc = character as Character_Live2D;

                        live2DData.expression = lc.activeExpression;
                        live2DData.motion = lc.activeMotion;

                        charData.dataJSON = JsonUtility.ToJson(live2DData);
                        break;

                    case Character.CharacterType.Model3D:
                        Model3DData model3DData = new Model3DData();

                        Character_Model3D mc = character as Character_Model3D;

                        model3DData.pos = mc.model.position;
                        model3DData.rotation = mc.model.rotation;

                        charData.dataJSON = JsonUtility.ToJson(model3DData);
                        break;
                }

                characterDatas.Add(charData);
            }

            return characterDatas;
        }

        public static void Apply(List<CharacterData> data)
        {
            // cache all characters which created in this history state
            List<string> cache = new List<string>();

            foreach (var characterData in data)
            {
                Character character = CharacterManager.Instance.GetCharacter(characterData.characterName, createIfDoentExist: true);

                character.displayName = characterData.displayName;

                character.isVisible = characterData.enabled;
                character.SetColor(characterData.color);
                if (characterData.isHighlighted)
                    character.Highlight(immediate: true);
                else
                    character.UnHighlight(immediate: true);

                character.SetPriority(characterData.priority);

                if (characterData.isFacingLeft) 
                    character.FaceLeft(immediate: true);
                else
                    character.FaceRight(immediate: true);

                character.SetPosition(characterData.position);

                MotionData motionData = JsonUtility.FromJson<MotionData>(characterData.generalMotionJSON);
                ApplyMotionData(character, motionData);

                // For now, we dont need to use CharacterConfigCache yet, becuase DialogueData will apply any changes in font, textSize, ...

                switch (character.configData.characterType)
                {
                    case Character.CharacterType.Sprite:
                    case Character.CharacterType.SpriteSheet:
                        SpriteData spriteData = JsonUtility.FromJson<SpriteData>(characterData.dataJSON);
                        Character_Sprite sc = character as Character_Sprite;

                        for (int i = 0; i < spriteData.layers.Count; i++)
                        {
                            SpriteData.LayerData layer = spriteData.layers[i];

                            if (sc.layersList[i].renderer.sprite != null && sc.layersList[i].renderer.sprite.name != layer.spriteName)
                            {
                                Sprite sprite = sc.GetSprite(layer.spriteName);
                                if (sprite != null)
                                    sc.SetSprite(sprite, i);
                                else
                                    Debug.LogWarning($"History State: Sprite '{layer.spriteName}' not found for character '{character.name}' in layer {i}.");
                            }
                        }

                        break;

                    case Character.CharacterType.Live2D:
                        Live2DData live2DData = JsonUtility.FromJson<Live2DData>(characterData.dataJSON);
                        Character_Live2D lc = character as Character_Live2D;

                        if(lc.activeExpression != live2DData.expression)
                            lc.SetExpression(live2DData.expression);

                        if (lc.activeMotion != live2DData.motion)
                            lc.SetAnimation(live2DData.motion);

                        break;

                    case Character.CharacterType.Model3D:
                        Model3DData model3DData = JsonUtility.FromJson<Model3DData>(characterData.dataJSON);
                        Character_Model3D mc = character as Character_Model3D;

                        mc.model.position = model3DData.pos;
                        mc.model.rotation = model3DData.rotation;
                        
                        break;
                }

                cache.Add(character.name);
            }

            // remove all characters which not in this history state
            foreach (var character in CharacterManager.Instance.allCharacters)
            {
                if (!cache.Contains(character.name))
                   character.isVisible = false;
            }
        }

        private static string GetMotionData(Character character)
        {
            Animator animator = character.animator;
            MotionData data = new MotionData();

            foreach(var param in animator.parameters)
            {
                if(param.type == AnimatorControllerParameterType.Trigger)
                {
                    // Triggers are 1 time events
                    continue;
                }

                MotionData.MotionParamater mData = new MotionData.MotionParamater { name = param.name };

                switch (param.type)
                {
                    case AnimatorControllerParameterType.Bool:
                        mData.type = "Bool";
                        mData.value = animator.GetBool(param.name).ToString();
                        break;
                    case AnimatorControllerParameterType.Float:
                        mData.type = "Float";
                        mData.value = animator.GetFloat(param.name).ToString();
                        break;
                    case AnimatorControllerParameterType.Int:
                        mData.type = "Int";
                        mData.value = animator.GetInteger(param.name).ToString();
                        break;
                }

                data.parameters.Add(mData);
            }

            return JsonUtility.ToJson(data);
        }

        private static void ApplyMotionData(Character character, MotionData data)
        {
                Animator anim = character.animator;

            foreach(var param in data.parameters)
            {
                switch (param.type)
                {
                    case "Bool":
                        anim.SetBool(param.name, bool.Parse(param.value));
                        break;
                    case "Float":
                        anim.SetFloat(param.name, float.Parse(param.value));
                        break;
                    case "Int":
                        anim.SetInteger(param.name, int.Parse(param.value));
                        break;
                    default:
                        Debug.LogWarning($"Unknown motion parameter type: {param.type} for parameter: {param.name}");
                        break;
                }
            }

            anim.SetTrigger(Character.ANIMATION_TRIGGER_TRIGGER_REFRESH_ID);
        }
    }
}