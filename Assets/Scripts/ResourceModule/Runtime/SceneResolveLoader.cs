
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ResourceModule
{
	/// <summary>
	/// 根据不同加载模式加载对应资源
	/// </summary>
	public class SceneResolveLoader : AbstractResourceLoader
	{
		public delegate void OnUnloadScene();

		private AssetBundleLoader _bundleLoader;
		private string _scenePath;

		public override bool IsUnique
		{
			get { return true; }
		}

		public static SceneResolveLoader Load(string sceneName, string package = null,
			LoadSceneMode loadSceneMode = LoadSceneMode.Single, LoaderDelgate onFinish = null,
			LoaderMode loaderMode = LoaderMode.Async)
		{
			return AutoNew<SceneResolveLoader>(sceneName, onFinish, false, loaderMode, package, loadSceneMode);
		}

		protected override void Init(string url, params object[] args)
		{
			_scenePath = ResourceModuleConfig.BundleResoucesDir + "/" + url;
			var loaderMode = (LoaderMode)args[0];
			string package = (string)args[1];
			LoadSceneMode loadSceneMode = (LoadSceneMode)args[2];

			base.Init(url, args);
			ResManager.Instance.StartCoroutine(_LoadCoroutine(url, loaderMode, package, loadSceneMode));
		}

		private IEnumerator _LoadCoroutine(string path, LoaderMode loaderMode, string package,
			LoadSceneMode loadSceneMode)
		{
			object getAsset = null;
			if (ResManager.IsEdiotrMode && Application.isEditor)
			{
#if UNITY_EDITOR
				var allScenes = UnityEditor.EditorBuildSettings.scenes;
				UnityEditor.EditorBuildSettingsScene buildScene = null;
				foreach (var scene in allScenes)
				{
					if (scene.path == _scenePath)
					{
						buildScene = scene;
						break;
					}
				}

				if (buildScene == null || !buildScene.enabled)
				{
					Debug.LogError("EditorMode must add scene to build settings and active:" + path);
					OnFinish(null);
					yield break;
				}

				//编辑器状态下模拟异步加载延迟等待指定秒数
				if (loaderMode == LoaderMode.Async)
				{
					yield return new WaitForSecondsRealtime(ResManager.EditorModeLoadDelay);
				}
#else
                Debug.LogErrorFormat("`IsEditorLoadAsset` is Unity Editor only");
#endif
			}
			else
			{
				DateTime beginTime = DateTime.Now;
				string bundlePath = string.IsNullOrEmpty(package) ? path : package;
				_bundleLoader =
					AssetBundleLoader.Load(bundlePath + ResourceModuleConfig.AssetBundleExt, null, loaderMode);

				while (!_bundleLoader.IsCompleted)
				{
					if (IsReadyDisposed) // 中途释放
					{
						_bundleLoader.Release();
						OnFinish(null);
						yield break;
					}

					this.Progress = _bundleLoader.Progress / 2f;
					yield return null;
				}

				if (!_bundleLoader.IsSuccess || !_bundleLoader.Bundle.isStreamedSceneAssetBundle)
				{
					Debug.LogErrorFormat("[AssetFileLoader]Load BundleLoader Failed(Error) when Finished: {0}", path);
					_bundleLoader.Release();
					OnFinish(null);
					yield break;
				}

				ResManager.LogLoadTime("AssetFileBridge", loaderMode, path, beginTime);
			}

			this.Progress = 0.5f;
			if (loaderMode == LoaderMode.Async)
			{
				var asyncOp = SceneManager.LoadSceneAsync(_scenePath, loadSceneMode);
				while (!asyncOp.isDone)
				{
					this.Progress = 0.5f + asyncOp.progress / 2f;
					yield return null;
				}
			}
			else
			{
				SceneManager.LoadScene(_scenePath, loadSceneMode);
			}

			this.Progress = 1f;
			getAsset = true;

			OnFinish(getAsset);
		}

		public void UnloadScene(OnUnloadScene onFinish = null, LoaderMode loaderMode = LoaderMode.Async)
		{
			if (loaderMode == LoaderMode.Sync)
			{
				this.ReleaseImmediate();
				if (onFinish != null) onFinish();
			}
			else
			{
				ResManager.Instance.StartCoroutine(_UnloadScene(onFinish));
			}
		}

		private IEnumerator _UnloadScene(OnUnloadScene onFinish)
		{
			var asynOp = SceneManager.UnloadSceneAsync(this._scenePath);
			yield return asynOp;

			this.Release();

			if (onFinish != null) onFinish();
		}

		protected override void DoDispose()
		{
			if (_bundleLoader != null)
				_bundleLoader.Release();

			base.DoDispose();
		}
	}

}
