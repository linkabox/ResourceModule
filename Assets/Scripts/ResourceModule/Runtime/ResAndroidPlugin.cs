
using System;
using UnityEngine;
using System.Collections;
using System.IO;

namespace ResourceModule
{
	public class ResAndroidPlugin
	{
#if UNITY_ANDROID
		private static AndroidJavaClass _helper;

		/// <summary>
		/// Get AndroidHelper from Java jar
		/// </summary>
		private static AndroidJavaClass AndroidHelper
		{
			get
			{
				if (_helper != null) return _helper;

				_helper = new AndroidJavaClass("com.resourcemodule.AndroidHelper");

				if (_helper == null)
					ErrorNotSupport();

				return _helper;
			}
		}
#endif

		private static void ErrorNotSupport()
		{
			throw new Exception("Error on Android Plugin. Check if AndroidHelper.jar file exist in your Plugins/Android");
		}

		/// <summary>
		/// Check if path exist in asset folder?
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static bool IsAssetExists(string path)
		{
#if UNITY_ANDROID
#if UNITY_EDITOR
			return File.Exists(Path.Combine(Application.streamingAssetsPath, path));
#else
			return AndroidHelper.CallStatic<bool>("isAssetExists", path);
#endif
#else
            ErrorNotSupport();
            return false;
#endif
		}

		/// <summary>
		/// Call from java get asset file bytes and convert to string
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static string GetAssetString(string path)
		{
#if UNITY_ANDROID
#if UNITY_EDITOR
			return File.ReadAllText(Path.Combine(Application.streamingAssetsPath, path));
#else
			return AndroidHelper.CallStatic<string>("getAssetString", path);
#endif
#else
            ErrorNotSupport();
            return null;
#endif
		}

		/// <summary>
		/// Call from java get asset file bytes
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		public static byte[] GetAssetBytes(string path)
		{
#if UNITY_ANDROID
#if UNITY_EDITOR
			return File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, path));
#else
			return AndroidHelper.CallStatic<byte[]>("getAssetBytes", path);
#endif
#else
            ErrorNotSupport();
            return null;
#endif
		}
	}
}

