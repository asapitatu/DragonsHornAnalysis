using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

#if UNITY_EDITOR
public class MyPathImportPostprocessor : AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string str in importedAssets)
        {
            if (str.IndexOf("/OurPath.csv") != -1)
            {
                TextAsset data = AssetDatabase.LoadAssetAtPath<TextAsset>(str);
                string assetfile = str.Replace(".csv", ".asset");
                MyPathInput asset = AssetDatabase.LoadAssetAtPath<MyPathInput>(assetfile);
                if (asset == null)
                {
                    asset = new MyPathInput();
                    AssetDatabase.CreateAsset(asset, assetfile);
                }

                MyRawPathInput rawData = new MyRawPathInput(data.text);
                asset.Parse(rawData);

                EditorUtility.SetDirty(asset);
                AssetDatabase.SaveAssets();
#if DEBUG_LOG || UNITY_EDITOR
                Debug.Log("Reimported Asset: " + str);
#endif
            }
        }
    }
}
#endif