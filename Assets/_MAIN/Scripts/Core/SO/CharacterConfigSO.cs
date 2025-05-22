using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace CHARACTER
{
    [CreateAssetMenu(fileName = "Character Config SO", menuName = "Dialogue System/Character Config SO")]
    public class CharacterConfigSO : ScriptableObject
    {
        public CharacterConfigData[] characterConfigDatas;

        public CharacterConfigData GetCharacterConfigData(string characterName)
        {
            characterName = characterName.ToLower();
            foreach (var characterConfigData in characterConfigDatas)
            {
                if (string.Equals(characterName,characterConfigData.name.ToLower()) || string.Equals(characterName, characterConfigData.alias.ToLower()))
                // using string.Equals because it is more efficient than string == string
                {
                    /*
                    we shouldnt return the characterConfigData directly by return characterConfigData; because we dont want to return the 
                    value/variable directly defined in the scriptable object

                    Reason: Persistence of SO data
                    When we export the game, the scriptable object data will be reset everytime we reset the game
                    When we make changes to SO in Unity Editor, the changes will stay even if we close the Editor, because we editted to SO original data
                    -> Return a copy that preserving the original data
                    */
                    return characterConfigData.Copy();
                }
            }

            return CharacterConfigData.Default;
        }
    }
}