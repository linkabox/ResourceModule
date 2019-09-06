using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

public static class GameResEditor
{
    [MenuItem("GameRes/Gen RawConfigData")]
    public static void GenRawConfigData()
    {
        ResourceModule.RawDataGenerator.GenRawConfigData(true);
    }

    [MenuItem("GameRes/OpenGameMain")]
    public static void OpenGameMain()
    {
        string path = PlayerPrefs.GetString("GameMainScene", "Assets/Scenes/GameLauncher.unity");
        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    }

    [MenuItem("GameRes/OpenUITest")]
    public static void OpenUITest()
    {
        string path = PlayerPrefs.GetString("UITestScene", "Assets/Scenes/UITest.unity");
        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    }


    [MenuItem("GameRes/OpenArenaTest")]
    public static void OpenArenaTest()
    {
        string path = PlayerPrefs.GetString("ArenaTestScene", "Assets/Scenes/ArenaTest3D.unity");
        EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
    }

    [MenuItem("GameRes/Update CustomChar")]
    public static void UpdateCustomChar()
    {
        string content = File.ReadAllText("Assets/BundleResources/Configs/lang.csv");
        HashSet<char> charSet = new HashSet<char>(content.ToCharArray());

        var bytes = System.Text.Encoding.UTF8.GetBytes(charSet.ToArray().OrderBy(c => c).ToArray());
        File.WriteAllBytes("Assets/GameRes/Fonts/custom_char.txt", bytes);
        Debug.Log("Update CustomChar:" + charSet.Count);
    }

    [MenuItem("GameRes/Fix FontRef")]
    public static void FixUITextFontRefSelections()
    {
        foreach (var gameObject in Selection.gameObjects)
        {
            FixUITextFontRef(gameObject);
        }
        AssetDatabase.SaveAssets();
    }

    public static void FixUITextFontRef(GameObject go)
    {
        var lbls = go.GetComponentsInChildren<Text>(true);
        var defaultFont = AssetDatabase.LoadMainAssetAtPath("Assets/GameRes/Fonts/my_font.ttf") as Font;

        foreach (var text in lbls)
        {
            if (text != null && text.font != defaultFont)
            {
                text.font = defaultFont;
                Debug.LogError("Replace:" + text.name, go);
            }
        }
        EditorUtility.SetDirty(go);
    }
}