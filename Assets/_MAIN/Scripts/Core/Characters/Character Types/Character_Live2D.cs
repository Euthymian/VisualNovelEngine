using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHARACTER
{
    public class Character_Live2D : Character
    {
        public Character_Live2D(string name, CharacterConfigData configData) : base(name, configData)
        {
            Debug.Log("Character_Live2D constructor called with name: " + name);
        }
    }
}