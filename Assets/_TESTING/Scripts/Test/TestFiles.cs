using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TESTING
{
    public class TestFiles : MonoBehaviour
    {
        private string fileName = "test.txt";
        private string fileAssetName = "test"; // Working with asset doesnt require .txt extension. test.txt is in Resources folder
        [SerializeField] private TextAsset textAsset; // Pass the text asset in the inspector

        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine(Run());
        }

        IEnumerator Run()
        {
            List<string> lines = FileManager.ReadTextFile(fileName, false);
            List<string> lines2 = FileManager.ReadTextAsset(fileAssetName, false);
            List<string> lines3 = FileManager.ReadTextAsset(textAsset, false);

            //foreach (string line in lines)
            //{
            //    Debug.Log(line);
            //}

            yield return null;
        }
    }
}