using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TESTING
{
    public class TestVariableStore : MonoBehaviour
    {
        public float ex_float = 0.0f;
        public int ex_int = 0;
        public string ex_string = "test";
        public bool ex_bool = false;

        // Start is called before the first frame update
        void Start()
        {
            VariableStore.CreateDatabase("d1");
            VariableStore.CreateDatabase("d2");
            VariableStore.CreateDatabase("d3");

            VariableStore.CreateVariable("num0", 1);
            VariableStore.CreateVariable("d1.bool1", true);
            VariableStore.CreateVariable("d1.num1", 4);
            VariableStore.CreateVariable("d2.string2", "three");
            VariableStore.CreateVariable("d3.float3", 4.0f);
            VariableStore.CreateVariable("string0", "gojo");

            VariableStore.CreateDatabase("d_links");

            VariableStore.CreateVariable("d_links.float", ex_float, () => ex_float, (v) => ex_float = v);
            VariableStore.CreateVariable("d_links.int", ex_int, () => ex_int, (v) => ex_int = v);
            VariableStore.CreateVariable("d_links.string", ex_string, () => ex_string, (v) => ex_string = v);
            VariableStore.CreateVariable("d_links.bool", ex_bool, () => ex_bool, (v) => ex_bool = v);

            VariableStore.PrintAllDatabases();

            VariableStore.PrintAllVariables();
        }

        // Update is called once per frame
        void Update()
        {
            if(Input.GetKeyDown(KeyCode.N))
                VariableStore.PrintAllVariables();

            //#region TEST INTERNAL VARIABLES
            //if (Input.GetKeyDown(KeyCode.A))
            //{
            //    VariableStore.TryGetValue("d1.num1", out object x);
            //    VariableStore.TrySetValue("d1.num1", (int)x + 6);
            //}

            //if (Input.GetKeyDown(KeyCode.S))
            //{
            //    VariableStore.TryGetValue("d2.string2", out object y);
            //    VariableStore.TrySetValue("d2.string2", (string)y + " four");
            //}

            //if (Input.GetKeyDown(KeyCode.D))
            //{
            //    VariableStore.TryGetValue("d3.float3", out object z);
            //    VariableStore.TryGetValue("num0", out object y);
            //    VariableStore.TryGetValue("d1.num1", out object x);
            //    Debug.Log("num0 + d3.float3 + d1.num1= " + ((int)y + (float)z + (int)x));
            //}

            //if (Input.GetKeyDown(KeyCode.F))
            //{
            //    VariableStore.TryGetValue("d1.bool1", out object t);
            //    if((bool)t)
            //        Debug.Log("d1.bool1 is true");
            //}
            //#endregion

            //#region TEST EXTERNAL VARIABLES
            //if(Input.GetKeyDown(KeyCode.Q))
            //{
            //    VariableStore.TryGetValue("d_links.float", out object x);
            //    VariableStore.TrySetValue("d_links.float", (float)x + 0.33f);
            //}

            //if (Input.GetKeyDown(KeyCode.W))
            //{
            //    VariableStore.TryGetValue("d_links.int", out object x);
            //    VariableStore.TrySetValue("d_links.int", (int)x + 1);
            //}

            //if (Input.GetKeyDown(KeyCode.E))
            //{
            //    VariableStore.TryGetValue("d_links.string", out object x);
            //    VariableStore.TrySetValue("d_links.string", (string)x + " test");
            //}

            //if (Input.GetKeyDown(KeyCode.R))
            //{
            //    VariableStore.TryGetValue("d_links.bool", out object x);
            //    VariableStore.TrySetValue("d_links.bool", !(bool)x);
            //}
            //#endregion

            //if (Input.GetKeyDown(KeyCode.H))
            //{
            //    VariableStore.RemoveVariable("num0");
            //    VariableStore.RemoveVariable("d_links.int");
            //    VariableStore.RemoveVariable("d1.bool1");
            //    VariableStore.RemoveVariable("d2.string2");
            //}

            //if (Input.GetKeyDown(KeyCode.J))
            //{
            //    VariableStore.RemoveAllVariables();
            //}
        }
    }
}