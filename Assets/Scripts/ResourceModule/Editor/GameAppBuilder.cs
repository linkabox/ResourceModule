using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

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
		string path = EditorUtility.OpenFilePanel("BuildApp", "", "");
		buildPlayerOptions.locationPathName = path;
		buildPlayerOptions.target = EditorUserBuildSettings.activeBuildTarget;
		buildPlayerOptions.options = BuildOptions.None;
		BuildPipeline.BuildPlayer(buildPlayerOptions);
	}
}
