using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TESTING
{
    public class TestGeneral : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            TMP_Text text = GetComponent<TextMeshProUGUI>();
            Debug.Log(text.text);
            Debug.Log(text.textInfo.meshInfo[text.textInfo.characterInfo[0].materialReferenceIndex].colors32);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}