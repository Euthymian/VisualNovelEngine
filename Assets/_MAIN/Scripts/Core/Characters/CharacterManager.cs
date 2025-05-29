using DIALOGUE;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHARACTER
{

    // CharacterManager will manage (store, retrieve by name, ...) all characters in the game

    public class CharacterManager : MonoBehaviour
    {
        public class CHARACTER_INFO
        {
            public string name; 
            public string castingName;
            /*
            When create a character, we call CreateCharacter() method with a character name. The name we pass in can simply be a name of the character
            or a name with casting name like "CharacterName as CastingName". 
            The reason of having casting name is that each character in CharacterConfigSO is unique, once we created a chacracter with an assigned name,
            we cant create another character with the same name, exept for Generic character (such as guard).
            We can have multiple guards share the same Generic character prefab, but we need to assign a casting name for each guard.
            */
            public CharacterConfigData characterConfigData = null;
            public GameObject prefab;
        }

        public static CharacterManager Instance { get; private set; }

        private const string CHARACTER_CAST_ID = " as ";
        
        private const string CHARACTER_NAME_ID = "<character_name>";
        public string characterRootPathFormat => $"Characters/{CHARACTER_NAME_ID}";
        public string characterPrefabNameFormat => $"Character - [{CHARACTER_NAME_ID}]";
        public string characterPrefabPathFormat => $"{characterRootPathFormat}/{characterPrefabNameFormat}";

        public Dictionary<string, Character> characterDictionary = new Dictionary<string, Character>();

        [SerializeField] private RectTransform characterPanel;
        public RectTransform CharacterPanel => characterPanel;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
            }
        }

        public CharacterConfigData GetCharacterConfigData(string characterName)
        {
            return DialogueSystem.Instance.dialogueSystemConfigSO.characterConfigSO.GetCharacterConfigData(characterName);
        }

        public Character CreateCharacter(string characterName)
        {
            if (characterDictionary.ContainsKey(characterName))
            {
                Debug.LogWarning($"Character {characterName} already exists.");
                return null;
            }

            CHARACTER_INFO info = GetCharacterInfo(characterName);

            Character character = CreateCharacterFromCHARACTER_INFO(info);

            characterDictionary.Add(characterName.ToLower(), character);

            return character;
        }

        private CHARACTER_INFO GetCharacterInfo(string characterName)
        {
            CHARACTER_INFO res = new CHARACTER_INFO();

            string[] nameData = characterName.Split(CHARACTER_CAST_ID, StringSplitOptions.RemoveEmptyEntries);

            res.name = nameData[0];
            res.castingName = nameData.Length > 1 ? nameData[1].Trim() : res.name;
            res.characterConfigData = GetCharacterConfigData(res.castingName); 
            res.prefab = GetPrefabForCharacter(res.castingName);

            return res;
        }

        private GameObject GetPrefabForCharacter(string characterName)
        {
            string prefabPath = FormatCharacterPath(characterPrefabPathFormat, characterName);
            return Resources.Load<GameObject>(prefabPath);
        }

        public string FormatCharacterPath(string path, string characterName)
        {
            return path.Replace(CHARACTER_NAME_ID, characterName);
        }

        private Character CreateCharacterFromCHARACTER_INFO(CHARACTER_INFO info)
        {
            CharacterConfigData configData = info.characterConfigData;
            switch (configData.characterType)
            {
                default:
                    return null;

                case Character.CharacterType.Text:
                    return new Character_Text(info.name, configData);
                     
                case Character.CharacterType.Sprite:
                case Character.CharacterType.SpriteSheet:
                    return new Character_Sprite(info.name, configData, info.prefab);

                case Character.CharacterType.Live2D:
                    return new Character_Live2D(info.name, configData, info.prefab);

                case Character.CharacterType.Model3D:
                    return new Character_Model3D(info.name, configData, info.prefab);
            }
        }
    
        public Character GetCharacter(string characterName, bool createIfDoentExist = false) 
        {
            if(characterDictionary.ContainsKey(characterName.ToLower()))
            {
                return characterDictionary[characterName.ToLower()];
            }
            else if (createIfDoentExist)
            {
                return CreateCharacter(characterName);
            }

            return null;
        }


    }
}