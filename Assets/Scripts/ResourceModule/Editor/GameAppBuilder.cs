using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEditor.Build;
using Debug = UnityEngine.Debug;

public class GameAppBuilder
{
    [MenuItem("ResourceModule/BuildGame")]
    public static void BuildGame()
    {
        BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions();
        var buildScenes = new List<string>();
        var allScenes = EditorBuildSettings.scenes;
        foreach (var scene in allScenes)
        {
            if (scene.enabled)
            {
                var importer = AssetImporter.GetAtPath(scene.path);
                if (importer != null && string.IsNullOrEmpty(importer.assetBundleName))
                {
                    buildScenes.Add(scene.path);
                }
            }
        }

        buildPlayerOptions.scenes = buildScenes.ToArray();
        string dir = EditorUtility.OpenFolderPanel("BuildApp", "", "");
        if (string.IsNullOrEmpty(dir))
        {
            Debug.LogError("选中Build目录为空");
            return;
        }

        Debug.Log("BuildGameApp:" + dir);
        string appName = "";
        if (EditorUserBuildSettings.activeBuildTarget == BuildTarget.Android)
        {
            appName = "game_" + DateTime.Now.ToString("yyyy_MM_dd-HH_mm_ss") + ".apk";
            buildPlayerOptions.locationPathName = dir + "/" + appName;
        }
        else
        {
            buildPlayerOptions.locationPathName = dir;
        }
        buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
        buildPlayerOptions.options = BuildOptions.None;
        BuildPipeline.BuildPlayer(buildPlayerOptions);

        Process.Start(dir);
    }
}
