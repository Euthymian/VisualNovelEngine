using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class ClearHideFlagsTool : EditorWindow
{
    [SerializeField]
    private List<GameObject> targetPrefabs = new List<GameObject>();

    [MenuItem("Tools/Live2D/Clear HideFlags from Prefabs")]
    public static void ShowWindow()
    {
        GetWindow<ClearHideFlagsTool>("Clear HideFlags");
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Drag your prefabs here:", EditorStyles.boldLabel);
        SerializedObject so = new SerializedObject(this);
        SerializedProperty listProperty = so.FindProperty("targetPrefabs");
        EditorGUILayout.PropertyField(listProperty, true);
        so.ApplyModifiedProperties();

        GUILayout.Space(10);

        if (GUILayout.Button("Clear HideFlags"))
        {
            ClearHideFlags();
        }
    }

    private void ClearHideFlags()
    {
        int totalCleared = 0;

        foreach (var prefab in targetPrefabs)
        {
            if (prefab == null) continue;

            string path = AssetDatabase.GetAssetPath(prefab);
            if (string.IsNullOrEmpty(path)) continue;

            Object[] allAssets = AssetDatabase.LoadAllAssetsAtPath(path);

            foreach (var obj in allAssets)
            {
                if (obj == null) continue;

                if (obj.hideFlags != HideFlags.None)
                {
                    obj.hideFlags = HideFlags.None;
                    EditorUtility.SetDirty(obj);
                    Debug.Log($"Cleared hideFlags on: {obj.name} ({path})");
                    totalCleared++;
                }
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Done! Cleared hideFlags on {totalCleared} object(s).");
    }
}
