using System;
using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Text;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;
using Object = UnityEngine.Object;

[InitializeOnLoad]
public class EditorExt
{
    #region Time
    [MenuItem("Time/Time reset", false, 0)]
    static void GameNormalTime()
    {
        UnityEngine.Time.timeScale = 1f;
    }

    [MenuItem("Time/Time 1.5", false, 0)]
    static void GameTimeSpeedUp()
    {
        UnityEngine.Time.timeScale = 1.5f;
    }

    [MenuItem("Time/Time 0.5", false, 0)]
    static void GameHalfTime()
    {
        UnityEngine.Time.timeScale = 0.5f;
    }

    [MenuItem("Time/Time 0.25", false, 0)]
    static void GameQuarterTime()
    {
        UnityEngine.Time.timeScale = 0.25f;
    }
    #endregion

    #region GameData
    [MenuItem("GameData/PrintAllDependencies", false, 0)]
    static void PrintAllDependencies()
    {
        PrintDependencies(Selection.activeObject, true);
    }

    [MenuItem("GameData/PrintDirectDependencies", false, 0)]
    static void PrintDirectDependencies()
    {
        PrintDependencies(Selection.activeObject, false);
    }

    public static void PrintDependencies(Object asset, bool recursive)
    {
        string assetPath = AssetDatabase.GetAssetPath(asset);
        var deps = AssetDatabase.GetDependencies(assetPath, recursive);
        var sb = new StringBuilder();
        sb.AppendLine((recursive ? "All" : "Direct") + " Dependencies:" + assetPath);
        foreach (string dep in deps)
        {
            sb.AppendLine(dep);
        }
        Debug.Log(sb.ToString());
    }

    [MenuItem("GameData/CleanUpPersistentData", false, 0)]
    static void CleanUpPersistentData()
    {
        try
        {
            Directory.Delete(Application.persistentDataPath, true);
            Debug.Log("CleanUpPersistentData Success!");
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message);
        }
    }

    [MenuItem("GameData/OpenPlayerPrefs", false, 0)]
    static void OpenPlayerPrefs()
    {
#if UNITY_EDITOR_WIN
        Process ps = new Process();
        ps.StartInfo.FileName = Application.dataPath + "/Editor/regjump.exe";
        ps.StartInfo.Arguments = string.Format(@"HKCU\Software\Unity\UnityEditor\{0}\{1}", PlayerSettings.companyName,
            PlayerSettings.productName);
        ps.StartInfo.UseShellExecute = true;
        ps.StartInfo.Verb = "runas";
        ps.Start();
#endif
    }

    [MenuItem("GameData/CleanUpPlayerPrefs", false, 0)]
    static void CleanUpPlayerPrefs()
    {
        PlayerPrefs.DeleteAll();
        Debug.Log("CleanUpPlayerPrefs Success!");
    }

    [MenuItem("GameData/persistentDataPath", false, 0)]
    static void PersistentDataPath()
    {
        System.Diagnostics.Process.Start(Application.persistentDataPath);
    }

    [MenuItem("GameData/temporaryCachePath", false, 0)]
    static void TemporaryCachePath()
    {
        System.Diagnostics.Process.Start(Application.temporaryCachePath);
    }

    #endregion

    #region Hierarchy

    [MenuItem("HierarchyExt/RenameChildNode", false, 0)]
    public static void RenameChildNode()
    {
        if (Selection.activeTransform == null) return;
        if (Selection.activeTransform.parent == null) return;

        string prefix = Selection.activeTransform.name;
        var parent = Selection.activeTransform.parent;
        for (int i = 0; i < parent.childCount; i++)
        {
            var t = parent.GetChild(i);
            t.name = prefix + "_" + (i + 1);
        }
    }

    [MenuItem("HierarchyExt/PrintNodePath", false, 0)]
    static void PrintNodePath()
    {
        if (Selection.gameObjects == null || Selection.gameObjects.Length != 2)
        {
            Debug.LogError("请选择两个节点，第一个为父节点，第二个为子节点");
            return;
        }

        Transform node1 = Selection.gameObjects[0].transform;
        Transform node2 = Selection.gameObjects[1].transform;
        if (node1.IsChildOf(node2))
        {
            Debug.LogError(AnimationUtility.CalculateTransformPath(node1, node2));
        }
        else
        {
            Debug.LogError(AnimationUtility.CalculateTransformPath(node2, node1));
        }

    }

    [MenuItem("HierarchyExt/PrintChildCount", false, 0)]
    static void PrintChildCount()
    {
        if (Selection.activeTransform == null)
        {
            return;
        }

        Debug.LogError("ChildCount:" + Selection.activeTransform.childCount);
    }

    [MenuItem("HierarchyExt/PrintChildCountRecursive", false, 0)]
    static void PrintChildCountRecursive()
    {
        if (Selection.activeTransform == null)
        {
            return;
        }

        int cnt = 0;
        FetchChild(Selection.activeTransform, ref cnt);
        Debug.LogError("ChildCount:" + cnt);
    }

    public static void FetchChild(Transform node, ref int count)
    {
        foreach (Transform child in node)
        {
            count += 1;
            FetchChild(child, ref count);
        }
    }

    [MenuItem("HierarchyExt/PrinteSelectionCount", false, 0)]
    static void PrinteSelectionCount()
    {
        if (Selection.objects == null)
        {
            return;
        }

        Debug.LogError("SelectionCount:" + Selection.objects.Length);
    }

    [MenuItem("HierarchyExt/PrintWorldPos", false, 0)]
    static void PrintWorldPos()
    {
        if (Selection.activeTransform == null)
        {
            return;
        }

        Debug.LogError("WorldPos:" + Selection.activeTransform.position);
    }

    [MenuItem("HierarchyExt/PrintWorldScale", false, 0)]
    static void PrintWorldScale()
    {
        if (Selection.activeTransform == null)
        {
            return;
        }

        Debug.LogError("WorldPos:" + Selection.activeTransform.lossyScale);
    }

    [MenuItem("GameObject/Selection/Sort Transform Up %#_-")]
    public static void SortHierarchyUp()
    {
        SortHierarchyBy(-1);
    }

    [MenuItem("GameObject/Selection/Sort Transform Down %#_=")]
    public static void SortHierarchyDown()
    {
        SortHierarchyBy(1);
    }

    static void SortHierarchyBy(int offset)
    {
        if (Selection.activeTransform != null)
        {
            int siblingIndex = Selection.activeTransform.GetSiblingIndex();
            if (siblingIndex == 0 && offset < 0)
            {
                Selection.activeTransform.SetAsLastSibling();
            }
            else
            {
                Selection.activeTransform.SetSiblingIndex(siblingIndex + offset);
                if (siblingIndex == Selection.activeTransform.GetSiblingIndex())
                {
                    Selection.activeTransform.SetAsFirstSibling();
                }
            }
        }
    }

    #endregion

    #region UI

    [MenuItem("GameObject/MakePixelPerfect &#p")]
    public static void MakePixelPerfect()
    {
        if (Selection.activeTransform != null)
        {
            MakePixelPerfect(Selection.activeTransform as RectTransform);
        }
    }

    public static void MakePixelPerfect(RectTransform rectTransform)
    {
        if (rectTransform == null) return;

        var pos = rectTransform.anchoredPosition;
        rectTransform.anchoredPosition = new Vector3(Mathf.RoundToInt(pos.x), Mathf.RoundToInt(pos.y));

        var image = rectTransform.GetComponent<Image>();
        if (image != null)
        {
            image.SetNativeSize();
        }
    }

    #endregion
}