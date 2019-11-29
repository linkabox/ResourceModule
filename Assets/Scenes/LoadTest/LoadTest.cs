using System.Collections;
using System.Collections.Generic;
using ResourceModule;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadTest : MonoBehaviour
{
	public LoadSceneMode LoadSceneMode;
	public Transform canvas;
	// Use this for initialization
	void Start()
	{
		DontDestroyOnLoad(this.gameObject);
	}

	void OnGUI()
	{
		if (GUILayout.Button("Load Assets"))
		{
			var loader = ResManager.LoadAsset("UIPrefabs/Lobby.prefab", "uiprefabs");
			var go = Object.Instantiate(loader.MainAsset) as GameObject;
			go.transform.SetParent(canvas, false);
			go.transform.localPosition = Vector3.zero;
			go.transform.localScale = Vector3.one;

		}

		if (GUILayout.Button("Load Scene Sync"))
		{
			var loader = ResManager.LoadScene("Scenes/BattleScene.unity", null, LoadSceneMode, (
				(ok, resultObject) =>
				{
					Debug.LogError("LoadScene Sync Callback:" + ok);
				}));

		}

        if (GUILayout.Button("Load Scene ASync"))
		{
			var loader = ResManager.LoadSceneAsync("Scenes/BattleScene.unity", null, LoadSceneMode.Additive, (
				(ok, resultObject) =>
				{
					Debug.LogError("LoadScene ASync Callback:" + ok);
				}));

		}

		if (GUILayout.Button("Unload Scene ASync"))
		{
			ResManager.UnloadSceneAsync("Scenes/BattleScene.unity", (
				() =>
				{
					Debug.LogError("Unload Scene Finish");
				}));

		}

	}

	// Update is called once per frame
	void Update()
	{

	}
}
