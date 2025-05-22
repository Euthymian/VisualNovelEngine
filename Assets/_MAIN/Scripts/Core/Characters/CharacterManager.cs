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
            public CharacterConfigData characterConfigData = null;
        }

        public static CharacterManager Instance { get; private set; }

        public Dictionary<string, Character> characterDictionary = new Dictionary<string, Character>();

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

            res.name = characterName;
            res.characterConfigData = DialogueSystem.Instance.dialogueSystemConfigSO.characterConfigSO.GetCharacterConfigData(characterName); 

            return res;
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
                    return new Character_Sprite(info.name, configData);

                case Character.CharacterType.Live2D:
                    return new Character_Live2D(info.name, configData);

                case Character.CharacterType.Model3D:
                    return new Character_Model3D(info.name, configData);
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