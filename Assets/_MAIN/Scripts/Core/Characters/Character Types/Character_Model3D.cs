using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CHARACTER
{
    public class Character_Model3D : Character
    {
        public Character_Model3D(string name, CharacterConfigData configData, GameObject prefab, string rootAssetFolder) : base(name, configData, prefab)
        {
            Debug.Log("Character_Model3D constructor called with name: " + name);
        }
    }
}