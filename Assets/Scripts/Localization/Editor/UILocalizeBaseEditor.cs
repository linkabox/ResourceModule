using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.IO;

public class UILocalizeBaseEditor : Editor
{
#if UNITY_EDITOR
    public const string LangPath = "Assets/BundleResources/Configs/lang.csv";

    [UnityEditor.MenuItem("Localization/Reload csv")]
    public static void LoadCsvFromEditor()
    {
        var data = File.ReadAllBytes(LangPath);
        Localization.LoadCSV(data);
    }

    [UnityEditor.MenuItem("Localization/Open lang csv")]
    public static void OpenConfig()
    {
        Process.Start(Path.GetFullPath(LangPath));
    }

#endif

    public bool DrawLangPopup()
    {
        if (GUILayout.Button("Reload Csv"))
        {
            LoadCsvFromEditor();
        }

        if (Localization.Langs == null)
        {
            GUILayout.Label("未加载lang.csv");
            return false;
        }
        EditorGUILayout.PrefixLabel("Langs:");
        Localization.LangIndex = EditorGUILayout.Popup(Localization.LangIndex, Localization.Langs);
        EditorGUILayout.Space();
        return true;
    }
}
