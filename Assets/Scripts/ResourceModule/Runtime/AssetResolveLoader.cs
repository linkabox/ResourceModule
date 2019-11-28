#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
//
// Filename: AssetFileLoader.cs
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
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ResourceModule
{
    /// <summary>
    /// 根据不同加载模式加载对应资源
    /// </summary>
    public class AssetResolveLoader : AbstractResourceLoader
    {
        public delegate void OnLoadAsset(bool isOk, Object resultObj);
        public delegate void OnLoadAssetList(bool isOk, Object[] allAssets);

        public Object MainAsset
        {
            get { return ResultObject as Object; }
        }

        public Object[] AllAssets
        {
            get { return ResultObject as Object[]; }
        }

        private AssetBundleLoader _bundleLoader;

        public static AssetResolveLoader Load(string path, string package, Type type, OnLoadAsset onFinish = null, LoaderMode loaderMode = LoaderMode.Async)
        {
            LoaderDelgate realcallback = null;
            if (onFinish != null)
            {
                realcallback = (isOk, obj) => onFinish(isOk, obj as Object);
            }

            return AutoNew<AssetResolveLoader>(path, realcallback, false, loaderMode, package, type, false);
        }

        public static AssetResolveLoader LoadAll(string path, string package, Type type, OnLoadAssetList onFinish = null, LoaderMode loaderMode = LoaderMode.Async)
        {
            LoaderDelgate realcallback = null;
            if (onFinish != null)
            {
                realcallback = (isOk, obj) => onFinish(isOk, obj as Object[]);
            }

            return AutoNew<AssetResolveLoader>(path, realcallback, false, loaderMode, package, type, true);
        }

        protected override void Init(string url, params object[] args)
        {
            var loaderMode = (LoaderMode)args[0];
            string package = (string)args[1];
            Type type = (Type)args[2];
            bool loadAll = (bool)args[3];

            base.Init(url, args);
            ResManager.Instance.StartCoroutine(_LoadCoroutine(url, loaderMode, package, type, loadAll));
        }

        private IEnumerator _LoadCoroutine(string path, LoaderMode loaderMode, string package, Type type, bool loadAll)
        {
            if (type == null)
            {
                type = typeof(UnityEngine.Object);
            }
            object getAsset = null;

            if (ResManager.IsEdiotrMode && Application.isEditor)
            {
#if UNITY_EDITOR
                string projPath = ResourceModuleConfig.BundleResoucesDir + "/" + path;
                if (!string.IsNullOrEmpty(package))
                {
                    string bundleName = UnityEditor.AssetDatabase.GetImplicitAssetBundleName(projPath);
                    if (bundleName != package + ResourceModuleConfig.AssetBundleExt)
                    {
                        Debug.LogErrorFormat("Asset is BundleName set error: {0} {1}\n{2}", package, bundleName, projPath);
                    }
                }

                if (loadAll)
                    getAsset = UnityEditor.AssetDatabase.LoadAllAssetRepresentationsAtPath(projPath);
                else
                    getAsset = UnityEditor.AssetDatabase.LoadAssetAtPath(projPath, type);

                if (getAsset == null)
                {
                    Debug.LogErrorFormat("Asset is NULL(from {0} Folder): {1}", ResourceModuleConfig.BundleResoucesDir, path);
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
                _bundleLoader = AssetBundleLoader.Load(bundlePath + ResourceModuleConfig.AssetBundleExt, null, loaderMode);

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

                if (!_bundleLoader.IsSuccess)
                {
                    Debug.LogErrorFormat("[AssetFileLoader]Load BundleLoader Failed(Error) when Finished: {0}", path);
                    _bundleLoader.Release();
                    OnFinish(null);
                    yield break;
                }

                this.Progress = 0.5f;
                var assetBundle = _bundleLoader.Bundle;
                if (loadAll)
                {
                    if (loaderMode == LoaderMode.Sync)
                    {
                        getAsset = assetBundle.LoadAllAssets(type);
                    }
                    else
                    {
                        var request = assetBundle.LoadAllAssetsAsync(type);
                        while (!request.isDone)
                        {
                            this.Progress = 0.5f + request.progress / 2f;
                            yield return null;
                        }
                        getAsset = request.allAssets;
                    }
                }
                else
                {
                    var assetPath = ResourceModuleConfig.BundleResoucesDir + "/" + path;
                    if (loaderMode == LoaderMode.Sync)
                    {
                        getAsset = assetBundle.LoadAsset(assetPath, type);
                    }
                    else
                    {
                        var request = assetBundle.LoadAssetAsync(assetPath, type);
                        while (!request.isDone)
                        {
                            this.Progress = 0.5f + request.progress / 2f;
                            yield return null;
                        }
                        getAsset = request.asset;
                    }
                }

                ResManager.LogLoadTime("AssetFileBridge", loaderMode, path, beginTime);
                if (getAsset == null)
                {
                    Debug.LogErrorFormat("Asset is NULL: {0}", path);
                }
            }

#if UNITY_EDITOR
            if (getAsset != null)
            {
                KResoourceLoadedAssetDebugger.Create(getAsset.GetType().Name, Url, getAsset);
            }
#endif
            OnFinish(getAsset);
        }

        protected override void DoDispose()
        {
            if (_bundleLoader != null)
                _bundleLoader.Release();

            //var mainAsset = MainAsset;
            //if (mainAsset != null)
            //{
            //	Resources.UnloadAsset(mainAsset);
            //}

            //var allAssets = AllAssets;
            //if (allAssets != null)
            //{
            //	foreach (var asset in allAssets)
            //	{
            //		Resources.UnloadAsset(asset);
            //	}
            //}

            base.DoDispose();
        }
    }

}
