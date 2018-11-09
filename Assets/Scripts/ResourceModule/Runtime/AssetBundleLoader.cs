#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KAssetBundleLoader.cs
// Date:     2015/12/03
// Author:  Kelly
// Email: 23110388@qq.com
// Github: https://github.com/mr-kelly/KEngine
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 3.0 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library.

#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using ResourceModule;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ResourceModule
{
	/// <summary>
	/// 加载模式，同步或异步
	/// </summary>
	public enum LoaderMode
	{
		Async,
		Sync,
	}

	// 調用WWWLoader
	public class AssetBundleLoader : AbstractResourceLoader
	{
		public delegate void OnLoadBundle(bool isOk, AssetBundle ab);

		public AssetBundle Bundle
		{
			get { return ResultObject as AssetBundle; }
		}

		/// <summary>
		/// 依赖的AssetBundleLoader
		/// </summary>
		private AssetBundleLoader[] _depLoaders;

		/// <summary>
		/// AssetBundle加载方式
		/// </summary>
		private LoaderMode _loaderMode;

		public static AssetBundleLoader Load(string url, OnLoadBundle callback = null,
			LoaderMode loaderMode = LoaderMode.Async)
		{
			url = url.ToLower();
			LoaderDelgate newCallback = null;
			if (callback != null)
			{
				newCallback = (isOk, obj) => callback(isOk, obj as AssetBundle);
			}
			var newLoader = AutoNew<AssetBundleLoader>(url, newCallback, false, loaderMode);
			return newLoader;
		}

		internal static AssetBundle LoadBundle(string url)
		{
			url = ResManager.BundlePathRoot + url;
			string fullUrl;
			var getResPathType = ResManager.GetBundleFullPath(url, out fullUrl);
			//Log.Info("LoadBundle Path:{0}", fullUrl);
			if (getResPathType == ResPathType.Invalid)
			{
				if (Debug.isDebugBuild)
					Debug.LogErrorFormat("[AssetBundleLoader]Error Path: {0}", url);
				return null;
			}

			return AssetBundle.LoadFromFile(fullUrl);
		}

		private static AssetBundleCreateRequest LoadBundleAsync(string url)
		{
			url = ResManager.BundlePathRoot + url;
			string fullUrl;
			var getResPathType = ResManager.GetBundleFullPath(url, out fullUrl);
			//Log.Info("LoadBundleAsync Path:{0}", fullUrl);
			if (getResPathType == ResPathType.Invalid)
			{
				if (Debug.isDebugBuild)
					Debug.LogErrorFormat("[AssetBundleLoader]Error Path: {0}", url);
				return null;
			}

			return AssetBundle.LoadFromFileAsync(fullUrl);
		}


		protected override void Init(string url, params object[] args)
		{
			ResManager.PreLoadManifest();
			base.Init(url);

			_loaderMode = (LoaderMode)args[0];

			ResManager.LogRequest("AssetBundle", url);
			ResManager.Instance.StartCoroutine(_LoadCoroutine(url));
		}

		private IEnumerator _LoadCoroutine(string url)
		{
			//先加载依赖的Bundle文件
			var deps = ResManager.BundleManifest.GetAllDependencies(url);
			if (deps.Length > 0)
			{
				_depLoaders = new AssetBundleLoader[deps.Length];
				for (var i = 0; i < deps.Length; i++)
				{
					var dep = deps[i];
					_depLoaders[i] = AssetBundleLoader.Load(dep, null, _loaderMode);
				}

				foreach (var loader in _depLoaders)
				{
					while (!loader.IsCompleted)
					{
						yield return null;
					}
				}
			}

			AssetBundle assetBundle;
			if (_loaderMode == LoaderMode.Sync)
			{
				assetBundle = LoadBundle(url);
			}
			else
			{
				var loadRequest = LoadBundleAsync(url);
				while (!loadRequest.isDone)
				{
					Progress = loadRequest.progress;
					yield return null;
				}

				assetBundle = loadRequest.assetBundle;
			}

			Progress = 1f;
			if (assetBundle == null)
			{
				Debug.LogErrorFormat("[AssetBundleLoader]Error Load AssetBundle: {0}", url);
			}

			OnFinish(assetBundle);

			//Array.Clear(cloneBytes, 0, cloneBytes.Length);  // 手工释放内存

			//GC.Collect(0);// 手工释放内存
		}

		protected override void DoDispose()
		{
			if (_depLoaders != null)
			{
				foreach (var depLoader in _depLoaders)
				{
					depLoader.Release();
				}
			}
			_depLoaders = null;

			var assetBundle = this.Bundle;
			if (assetBundle != null)
			{
				assetBundle.Unload(false);
			}

			base.DoDispose();
		}

		public override void Release()
		{
			base.Release();
		}
	}

}
