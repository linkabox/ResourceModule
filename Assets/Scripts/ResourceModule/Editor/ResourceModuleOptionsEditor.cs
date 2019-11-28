#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KEngineUtils.cs
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
using UnityEditor;
using UnityEngine;

namespace ResourceModule.Editor
{
	public class ResourceModuleOptionsEditor : EditorWindow
	{
		private static ResourceModuleOptionsEditor Instance;

		[MenuItem("ResourceModule/ResourceModule Options %#t")]
		private static void Init()
		{
			if (Instance == null)
			{
				Instance = ResourceModuleOptionsEditor.GetWindow<ResourceModuleOptionsEditor>(true, "ResourceModule Options");
				Instance.minSize = new Vector2(440, 285);
				Instance.Show();
			}
			else
			{
				Instance.Close();
				Instance = null;
			}
		}

		private readonly GUIStyle _headerStyle = new GUIStyle();
		private bool _isEditorMode;
		private bool _loadPackedLuaCode;
		private float _loadDelay;

		private void OnEnable()
		{
			_isEditorMode = PlayerPrefs.GetInt(ResourceModuleConfig.IsEdiotrMode, 1) != 0;
			_loadPackedLuaCode = PlayerPrefs.GetInt(ResourceModuleConfig.LoadPackedLuaCode, 0) != 0;
			_loadDelay = PlayerPrefs.GetFloat(ResourceModuleConfig.EditorModeLoadDelay, 0.25f);

			_headerStyle.fontSize = 22;
			_headerStyle.normal.textColor = Color.white;
		}

		private void OnGUI()
		{
			EditorGUILayout.LabelField("== ResourceModule ==");
			_isEditorMode = EditorGUILayout.Toggle("IsEdiotrMode:", _isEditorMode);
			EditorGUILayout.LabelField("开启:直接加载工程目录资源\n关闭:加载AssetBundle资源", GUILayout.Height(32f));
			EditorGUILayout.Space();

			_loadPackedLuaCode = EditorGUILayout.Toggle("Load Packed LuaCode:", _loadPackedLuaCode);
			EditorGUILayout.LabelField("EditorMode下开启只加载LuaPackedCode目录下的代码\nAssetBundle模式下为了方便调试可以关闭，Lua代码还是加载LuaCode中的代码", GUILayout.Height(32f));
			EditorGUILayout.Space();

			_loadDelay = EditorGUILayout.FloatField("Load Delay:", _loadDelay);
			EditorGUILayout.LabelField("EditorMode下模拟加载延迟");
			EditorGUILayout.Space();

			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if (GUILayout.Button("保存配置", "LargeButton", GUILayout.Height(50f)))
			{
				PlayerPrefs.SetInt(ResourceModuleConfig.IsEdiotrMode, _isEditorMode ? 1 : 0);
				PlayerPrefs.SetInt(ResourceModuleConfig.LoadPackedLuaCode, _loadPackedLuaCode ? 1 : 0);
				PlayerPrefs.SetFloat(ResourceModuleConfig.EditorModeLoadDelay, _loadDelay);
				Debug.Log("Save ResourceModuleConfig!");
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}