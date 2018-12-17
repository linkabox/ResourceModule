﻿#region Copyright (c) 2015 KEngine / Kelly <http://github.com/mr-kelly>, All rights reserved.

// KEngine - Toolset and framework for Unity3D
// ===================================
// 
// Filename: AbstractResourceLoader.cs
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
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ResourceModule;
using UnityEngine;

namespace ResourceModule
{
	/// <summary>
	/// 所有资源Loader继承这个
	/// </summary>
	public abstract class AbstractResourceLoader : IAsyncObject
	{
		public delegate void LoaderDelgate(bool isOk, object resultObject);

		private static readonly Dictionary<Type, Dictionary<string, AbstractResourceLoader>> LoadersPool =
			new Dictionary<Type, Dictionary<string, AbstractResourceLoader>>();

		private readonly List<LoaderDelgate> _afterFinishedCallbacks = new List<LoaderDelgate>();

		#region 垃圾回收 Garbage Collect

		/// <summary>
		/// Loader延迟Dispose
		/// </summary>
		private const float LoaderDisposeTime = 0;

		/// <summary>
		/// 间隔多少秒做一次GC(在AutoNew时)
		/// </summary>
		public static float GcIntervalTime
		{
			get
			{
#if UNITY_EDITOR
				return 1f;
#else
				return 10f;
#endif
			}
		}

		/// <summary>
		/// 上次做GC的时间
		/// </summary>
		private static float _lastGcTime = -1;

		/// <summary>
		/// 缓存起来要删掉的，供DoGarbageCollect函数用, 避免重复的new List
		/// </summary>
		internal static readonly List<AbstractResourceLoader> CacheLoaderToRemoveFromUnUsed =
			new List<AbstractResourceLoader>();

		/// <summary>
		/// 进行垃圾回收
		/// </summary>
		internal static readonly Dictionary<AbstractResourceLoader, float> UnUsesLoaders =
			new Dictionary<AbstractResourceLoader, float>();

		#endregion

		/// <summary>
		/// 最终加载结果的资源
		/// </summary>
		public object ResultObject { get; private set; }

		/// <summary>
		/// 是否已经完成，它的存在令Loader可以用于协程StartCoroutine
		/// </summary>
		public bool IsCompleted { get; private set; }

		/// <summary>
		/// 类似WWW, IsFinished再判断是否有错误对吧
		/// </summary>
		public bool IsError { get; private set; }

		/// <summary>
		/// 异步过程返回的信息
		/// </summary>
		public virtual string AsyncMessage
		{
			get { return null; }
		}

		/// <summary>
		/// 同ResultObject
		/// </summary>
		public object AsyncResult
		{
			get { return ResultObject; }
		}
		/// <summary>
		/// 是否可用
		/// </summary>
		public bool IsSuccess
		{
			get { return !IsError && ResultObject != null && !IsReadyDisposed; }
		}

		/// <summary>
		/// 是否处于Application退出状态
		/// </summary>
		private bool _isQuitApplication = false;

		/// <summary>
		/// ForceNew的，非AutoNew
		/// </summary>
		protected bool IsForceNew;

		/// <summary>
		/// RefCount 为 0，进入预备状态
		/// </summary>
		protected bool IsReadyDisposed { get; private set; }

		/// <summary>
		///  销毁事件
		/// </summary>
		public event Action DisposeEvent;

		[System.NonSerialized]
		public float InitTiming = -1;
		[System.NonSerialized]
		public float FinishTiming = -1;

		/// <summary>
		/// 用时
		/// </summary>
		public float FinishUsedTime
		{
			get
			{
				if (!IsCompleted) return -1;
				return FinishTiming - InitTiming;
			}
		}

		/// <summary>
		/// 引用计数
		/// </summary>
		public int RefCount { get; private set; }

		public virtual bool IsUnique
		{
			get { return false; }
		}

		public string Url { get; private set; }

		/// <summary>
		/// 进度百分比~ 0-1浮点
		/// </summary>
		public virtual float Progress { get; protected set; }

		protected static int GetCount<T>()
		{
			return GetTypeDict(typeof(T)).Count;
		}

		protected static Dictionary<string, AbstractResourceLoader> GetTypeDict(Type type)
		{
			Dictionary<string, AbstractResourceLoader> typesDict;
			if (!LoadersPool.TryGetValue(type, out typesDict))
			{
				typesDict = LoadersPool[type] = new Dictionary<string, AbstractResourceLoader>();
			}
			return typesDict;
		}

		public static T GetLoader<T>(string url) where T : AbstractResourceLoader
		{
			var dict = GetTypeDict(typeof(T));
			AbstractResourceLoader loader = null;
			if (dict.TryGetValue(url, out loader))
			{
				return (T)loader;
			}
			return null;
		}

		public static int GetRefCount<T>(string url)
		{
			var dict = GetTypeDict(typeof(T));
			AbstractResourceLoader loader;
			if (dict.TryGetValue(url, out loader))
			{
				return loader.RefCount;
			}
			return 0;
		}

		/// <summary>
		/// 统一的对象工厂
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="url"></param>
		/// <param name="callback"></param>
		/// <param name="forceCreateNew"></param>
		/// <returns></returns>
		protected static T AutoNew<T>(string url, LoaderDelgate callback = null, bool forceCreateNew = false,
			params object[] initArgs) where T : AbstractResourceLoader, new()
		{
			Dictionary<string, AbstractResourceLoader> typesDict = GetTypeDict(typeof(T));
			AbstractResourceLoader loader;
			if (string.IsNullOrEmpty(url))
			{
				Debug.LogErrorFormat("[{0}:AutoNew]url为空", typeof(T));
			}

			if (forceCreateNew || !typesDict.TryGetValue(url, out loader))
			{
				loader = new T();
				if (!forceCreateNew)
				{
					typesDict[url] = loader;
				}

				loader.IsForceNew = forceCreateNew;
				loader.Init(url, initArgs);

#if UNITY_EDITOR
				KResourceLoaderDebugger.Create(typeof(T).Name, url, loader);
#endif
			}
			else
			{
				if (loader.RefCount < 0)
				{
					//loader.IsDisposed = false;  // 转死回生的可能
					Debug.LogErrorFormat("Error RefCount!");
				}
			}

			if (!loader.IsUnique || loader.RefCount == 0)
				loader.RefCount++;

			// RefCount++了，重新激活，在队列中准备清理的Loader
			if (UnUsesLoaders.ContainsKey(loader))
			{
				UnUsesLoaders.Remove(loader);
				loader.Revive();
			}

			loader.AddCallback(callback);

			return loader as T;
		}

		/// <summary>
		/// 是否进行垃圾收集
		/// </summary>
		public static void CheckGcCollect()
		{
			if (_lastGcTime.Equals(-1) || (Time.time - _lastGcTime) >= GcIntervalTime)
			{
				DoGarbageCollect();
				_lastGcTime = Time.time;
			}
		}

		/// <summary>
		/// 进行垃圾回收
		/// </summary>
		internal static void DoGarbageCollect()
		{
			foreach (var kv in UnUsesLoaders)
			{
				var loader = kv.Key;
				var time = kv.Value;
				if ((Time.time - time) >= LoaderDisposeTime)
				{
					CacheLoaderToRemoveFromUnUsed.Add(loader);
				}
			}

			for (var i = CacheLoaderToRemoveFromUnUsed.Count - 1; i >= 0; i--)
			{
				try
				{
					var loader = CacheLoaderToRemoveFromUnUsed[i];
					UnUsesLoaders.Remove(loader);
					CacheLoaderToRemoveFromUnUsed.RemoveAt(i);
					loader.Dispose();
				}
				catch (Exception e)
				{
					Debug.LogException(e);
				}
			}

			if (CacheLoaderToRemoveFromUnUsed.Count > 0)
			{
				Debug.LogErrorFormat("[DoGarbageCollect]CacheLoaderToRemoveFromUnUsed muse be empty!!");
			}
		}

		/// <summary>
		/// 复活
		/// </summary>
		protected virtual void Revive()
		{
			IsReadyDisposed = false; // 复活！
		}

		protected AbstractResourceLoader()
		{
			RefCount = 0;
		}

		protected virtual void Init(string url, params object[] args)
		{
			InitTiming = Time.realtimeSinceStartup;
			ResultObject = null;
			IsReadyDisposed = false;
			IsError = false;
			IsCompleted = false;

			Url = url;
			Progress = 0;
		}

		protected virtual void OnFinish(object resultObj)
		{
			Action doFinish = () =>
			{
				// 如果ReadyDispose，无效！不用传入最终结果！
				ResultObject = resultObj;

				// 如果ReadyDisposed, 依然会保存ResultObject, 但在回调时会失败~无回调对象
				var callbackObject = !IsReadyDisposed ? ResultObject : null;

				FinishTiming = Time.realtimeSinceStartup;
				Progress = 1;
				IsError = callbackObject == null;

				IsCompleted = true;
				DoCallback(IsSuccess, callbackObject);

				if (IsReadyDisposed)
				{
					//Dispose();
					Debug.LogFormat("[BaseResourceLoader:OnFinish]时，准备Disposed {0}", Url);
				}
			};

			doFinish();
		}

		/// <summary>
		/// 在IsFinisehd后悔执行的回调
		/// </summary>
		/// <param name="callback"></param>
		protected void AddCallback(LoaderDelgate callback)
		{
			if (callback != null)
			{
				if (IsCompleted)
				{
					if (ResultObject == null)
						Debug.LogWarningFormat("Null ResultAsset {0}", Url);
					callback(ResultObject != null, ResultObject);
				}
				else
					_afterFinishedCallbacks.Add(callback);
			}
		}

		protected void DoCallback(bool isOk, object resultObj)
		{
			Action justDo = () =>
			{
				foreach (var callback in _afterFinishedCallbacks)
					callback(isOk, resultObj);
				_afterFinishedCallbacks.Clear();
			};


			{
				justDo();
			}
		}

		/// <summary>
		/// 减少引用计数，立即执行DoGarbageCollect
		/// </summary>
		public virtual void ReleaseImmediate()
		{
			Release();
			ResManager.Collect();
		}

		/// <summary>
		/// 减少引用计数,加入UnUsesLoaders队列
		/// </summary>
		public virtual void Release()
		{
			if (IsReadyDisposed && Debug.isDebugBuild)
			{
				Debug.LogWarningFormat("[{0}]Too many dipose! {1}, Count: {2}", GetType().Name, this.Url, RefCount);
			}

			RefCount--;
			if (RefCount <= 0)
			{
				// TODO: 全部Loader整理好后再设这里吧
				if (Debug.isDebugBuild)
				{
					if (RefCount < 0)
					{
						Debug.LogErrorFormat("[{3}]RefCount< 0, {0} : {1}, NowRefCount: {2}, Will be fix to 0", GetType().Name,
							Url, RefCount, GetType());

						RefCount = Mathf.Max(0, RefCount);
					}

					if (UnUsesLoaders.ContainsKey(this))
					{
						Debug.LogErrorFormat("[{1}]UnUsesLoader exists: {0}", this, GetType());
					}
				}

				// 加入队列，准备Dispose
				UnUsesLoaders[this] = Time.time;

				IsReadyDisposed = true;
				OnReadyDisposed();

				//DoGarbageCollect();
			}
		}

		/// <summary>
		/// 引用为0时，进入准备Disposed状态时触发
		/// </summary>
		protected virtual void OnReadyDisposed()
		{
		}

		/// <summary>
		/// Dispose是有引用检查的， DoDispose一般用于继承重写
		/// </summary>
		private void Dispose()
		{
			if (DisposeEvent != null)
				DisposeEvent();

			if (!IsForceNew)
			{
				var type = GetType();
				var typeDict = GetTypeDict(type);
				//if (Url != null) // TODO: 以后去掉
				{
					var bRemove = typeDict.Remove(Url);
					if (!bRemove)
					{
						Debug.LogWarningFormat("[{0}:Dispose]No Url: {1}, Cur RefCount: {2}", type.Name, Url, RefCount);
					}
				}
			}

			if (IsCompleted)
				DoDispose();
			// 未完成，在OnFinish时会执行DoDispose
		}

		protected virtual void DoDispose()
		{
			this.ResultObject = null;
		}

		/// <summary>
		/// 强制进行Dispose，无视Ref引用数，建议用在RefCount为1的Loader上
		/// </summary>
		public virtual void ForceDispose()
		{
			if (_isQuitApplication)
				return;
			if (RefCount != 1)
			{
				Debug.LogWarningFormat("[ForceDisose]Use force dispose to dispose loader, recommend this loader RefCount == 1");
			}
			Dispose();
		}

		/// <summary>
		/// By Unity Reflection
		/// </summary>
		protected void OnApplicationQuit()
		{
			_isQuitApplication = true;
		}

		/// <summary>
		/// 强制卸载所有Loader,清空所有静态缓存列表数据
		/// </summary>
		public static void DisposeAll()
		{
			foreach (var type_pair in LoadersPool)
			{
				var loaders = type_pair.Value.Values.ToList();
				foreach (var loader in loaders)
				{
					if (loader != null)
					{
						loader.ForceDispose();
					}
				}
			}

			LoadersPool.Clear();
			CacheLoaderToRemoveFromUnUsed.Clear();
			UnUsesLoaders.Clear();
		}
	}
}