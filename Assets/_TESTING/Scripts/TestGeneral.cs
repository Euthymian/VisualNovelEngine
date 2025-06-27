using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace TESTING
{
    public class TestGeneral : MonoBehaviour
    {
        Coroutine _co1  = null;
        void Start()
        {
            StartCoroutine(co2());
        }

        Coroutine co1Wrapper()
        {
            if (_co1 == null)
            {
                _co1 = StartCoroutine(co1());
            }
            return _co1;
        }

        IEnumerator co1()
        {
            int c = 0;
            while (true)
            {
                Debug.Log("Coroutine 1 is running");
                c++;
                yield return new WaitForSeconds(1f);
                if(c == 4)
                {
                    Debug.Log("c reached 4");
                    _co1 = null;
                    break;
                }
            }
        }

        IEnumerator co2()
        {
            Debug.Log("Starting Coroutine 2");
            yield return co1Wrapper();
            Debug.Log("Coroutine 2 has finished running");
            if(_co1 != null)
            {
                Debug.Log("Coroutine 1 is still running");
            }
            else
            {
                Debug.Log("Coroutine 1 has been stopped");
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.Space))
            {
                Debug.Log("space down");
                StopCoroutine(_co1);
                if (_co1 != null)
                {
                    Debug.Log("Coroutine 1 is still running _co1: " + _co1.ToString());
                }
                else
                {
                    Debug.Log("Coroutine 1 has been stopped");
                }
            }
        }
    }
}