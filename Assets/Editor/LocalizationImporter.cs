using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LocalizationImporter : AssetPostprocessor
{
    private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
        string[] movedFromAssetPaths)
    {
        foreach (string importedAsset in importedAssets)
        {
            if (importedAsset.Contains("lang.csv"))
            {
                UILocalizeBaseEditor.LoadCsvFromEditor();
                return;
            }
        }
    }
}
