#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: KDebuggerObjectTool.cs
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
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ResourceModule;
using UnityEngine;
using Object = UnityEngine.Object;

namespace ResourceModule
{
	/// <summary>
	/// 专门用于资源Debugger用到的父对象自动生成
	/// DebuggerObject - 用于管理虚拟对象（只用于显示调试信息的对象）
	/// </summary>
	public class KDebuggerObjectTool
	{
		private static readonly Dictionary<string, Transform> Parents = new Dictionary<string, Transform>();
		private static readonly Dictionary<string, int> Counts = new Dictionary<string, int>(); // 数量统计...
		private static Transform _root;
		private static Transform Root
		{
			get
			{
				if (_root == null)
				{
					var go = new GameObject("__ResourceModuleDebbuger__");
					Object.DontDestroyOnLoad(go);
					_root = go.transform;
				}

				return _root;
			}
		}

		private static string GetUri(string bigType, string smallType)
		{
			var uri = string.Format("{0}/{1}", bigType, smallType);
			return uri;
		}

		/// <summary>
		/// 设置某个物件，在指定调试组下
		/// </summary>
		/// <param name="bigType"></param>
		/// <param name="smallType"></param>
		/// <param name="go"></param>
		public static void SetParent(string bigType, string smallType, GameObject go)
		{
			var uri = GetUri(bigType, smallType);
			Transform theParent = GetParent(bigType, smallType);

			int typeCount;
			if (!Counts.TryGetValue(uri, out typeCount))
			{
				Counts[uri] = 0;
			}
			typeCount = ++Counts[uri];

			try
			{
				go.transform.SetParent(theParent);
			}
			catch (Exception e)
			{
				Debug.LogErrorFormat(string.Format("[SetParent]{0}->{1}->{2}", bigType, smallType, e.Message));
			}

			theParent.gameObject.name = GetNameWithCount(smallType, typeCount);
		}

		public static void RemoveFromParent(string bigType, string smallType, GameObject obj)
		{
			if (!KResourceLoaderDebugger.IsApplicationQuit)
			{
				if (obj != null)
					GameObject.Destroy(obj);

				string url = GetUri(bigType, smallType);
				if (Counts.ContainsKey(url))
				{
					var newCount = --Counts[url];
					var parent = GetParent(bigType, smallType);
					parent.name = GetNameWithCount(smallType, newCount);
				}
			}
		}

		/// <summary>
		/// 设置Parent名字,带有数量
		/// </summary>
		/// <param name="obj"></param>
		/// <param name="smallType"></param>
		/// <param name="count"></param>
		protected static string GetNameWithCount(string smallType, int count)
		{
			return string.Format("{0}({1})", smallType, count);
		}

		protected static Transform GetParent(string bigType, string smallType)
		{
			var uri = GetUri(bigType, smallType);
			Transform theParent;

			if (!Parents.TryGetValue(uri, out theParent))
			{
				var bigTypeObjName = string.Format("__{0}__", bigType);
				var bigTypeNode = Root.Find(bigTypeObjName);
				if (bigTypeNode == null)
				{
					var go = new GameObject(bigTypeObjName);
					Object.DontDestroyOnLoad(go);
					bigTypeNode = go.transform;
					bigTypeNode.SetParent(Root);
				}
				bigTypeNode.SetAsFirstSibling();

				theParent = new GameObject(smallType).transform;
				theParent.SetParent(bigTypeNode);
				Parents[uri] = theParent;
			}
			return theParent;
		}

		public static void Dispose()
		{
			if (_root != null)
			{
				Object.DestroyImmediate(_root.gameObject);
			}
			_root = null;

			Parents.Clear();
			Counts.Clear();
		}
	}
}
#endif