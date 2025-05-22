using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHARACTER
{
    public class Character_Sprite : Character
    {
        public Character_Sprite(string name, CharacterConfigData configData) : base(name, configData)
        {
            Debug.Log("Character_Sprite constructor called with name: " + name);
        }
    }
}