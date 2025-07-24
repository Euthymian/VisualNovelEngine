using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHARACTER
{
    public class Character_Text : Character
    {
        public Character_Text(string castingName, string name, CharacterConfigData configData) : base(castingName, name, configData)
        {
            Debug.Log("Character_Text constructor called with name: " + name);
        }
    }
}