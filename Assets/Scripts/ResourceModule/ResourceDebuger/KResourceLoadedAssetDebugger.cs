﻿#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KResourceLoadedAssetDebugger.cs
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

#if UNITY_EDITOR
using ResourceModule;
using UnityEngine;

namespace ResourceModule
{

	/// <summary>
	/// 对XXXLoader的结果Asset进行Debug显示
	/// </summary>
	public class KResoourceLoadedAssetDebugger : MonoBehaviour
	{
		public string MemorySize;
		public UnityEngine.Object mainAsset;
		public Object[] allAssets;
		private const string bigType = "LoadedAssetDebugger";
		public string Type;
		private bool IsRemoveFromParent = false;

		public static KResoourceLoadedAssetDebugger Create(string type, string url, object rawObj)
		{
			var mainAsset = rawObj as UnityEngine.Object;
			UnityEngine.Object[] allAssets = null;
			long totalSize = 0;
			if (mainAsset != null)
			{
				totalSize = UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(mainAsset);
			}
			else
			{
				allAssets = rawObj as UnityEngine.Object[];
				type = allAssets[0].GetType().Name;

				foreach (var asset in allAssets)
				{
					totalSize += UnityEngine.Profiling.Profiler.GetRuntimeMemorySizeLong(asset);
				}
			}

			var newHelpGameObject = new GameObject(string.Format("LoadedObject-{0}-{1}", type, url));
			KDebuggerObjectTool.SetParent(bigType, type, newHelpGameObject);

			var newHelp = newHelpGameObject.AddComponent<KResoourceLoadedAssetDebugger>();
			newHelp.Type = type;
			newHelp.mainAsset = mainAsset;
			newHelp.allAssets = allAssets;
			newHelp.MemorySize = string.Format("{0:F5}KB", totalSize / 1024f);

			return newHelp;
		}

		private void Update()
		{
			if (mainAsset == null && allAssets == null && !IsRemoveFromParent)
			{
				KDebuggerObjectTool.RemoveFromParent(bigType, Type, gameObject);
				IsRemoveFromParent = true;
			}
		}
	}
}
#endif