using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Threading;
using LitJson;
using ResourceModule;
using UnityEngine;

namespace ResourceModule
{
	public class FileExt
	{
		#region Delegates

		public delegate void OnError(string path, string error);

		public delegate void OnLoadFinish(FileExt file);

		#endregion

		private byte[] mBuffer;
		//buffer
		private int mBufferSize;
		//finish callback
		private OnError mErrorCallback;
		//input stream
		private OnLoadFinish mFinishCallback;
		//data buffer size
		private FileStream mInputStream;
		//error callback
		private string mPath;
		//path

		public string FilePath
		{
			get { return mPath; }
		}

		public byte[] Bytes
		{
			get { return mBuffer; }
		}

		/// <summary>
		///     Determines if is exist the specified Path.
		/// </summary>
		/// <returns><c>true</c> if is exist the specified Path; otherwise, <c>false</c>.</returns>
		/// <param name="path"></param>
		public static bool IsExist(string path)
		{
			return File.Exists(path);
		}

		/// <summary>
		///     Creates the directory.
		/// </summary>
		/// <param name="path">Path.</param>
		public static void CreateDirectory(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}
		}

		public static void DeleteDirectory(string dir, bool recursive)
		{
			if (Directory.Exists(dir))
			{
				Directory.Delete(dir, recursive);
			}
		}

		/// <summary>
		/// 根据不同平台生成对应本地文件的Url,Window下需要使用file:///
		/// </summary>
		/// <param name="filePath"></param>
		/// <returns></returns>
		public static string GetLocalFileUrl(string filePath)
		{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
			return "file:///" + filePath;
#else
            return "file://" + filePath;
#endif
		}

		public static void DeleteFile(string file)
		{
			if (File.Exists(file))
			{
				File.Delete(file);
			}
		}

		#region clipBoard

		public static string ClipBoard
		{
			get { return GUIUtility.systemCopyBuffer; }
			set { GUIUtility.systemCopyBuffer = value; }
		}

		#endregion

		#region Sync Read File

		/// <summary>
		///     Reads all bytes.
		/// </summary>
		/// <returns>The all bytes.</returns>
		/// <param name="path">Path.</param>
		public static byte[] ReadAllBytes(string path)
		{
			try
			{
				byte[] data = File.ReadAllBytes(path);
				return data;
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
			return null;
		}

		/// <summary>
		///     Reads all text.
		/// </summary>
		/// <returns>The all text.</returns>
		/// <param name="path">Path.</param>
		public static string ReadAllText(string path)
		{
			try
			{
				string data = File.ReadAllText(path);
				return data;
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
			return null;
		}

		#endregion

		#region Sync Write File
		public static void WriteAllBytes(string path, Texture2D texture2D, int quality = 75)
		{
			if (string.IsNullOrEmpty(path)) return;
			if (texture2D == null) return;

			string ext = Path.GetExtension(path);
			if (ext == ".jpg")
			{
				WriteAllBytes(path, texture2D.EncodeToJPG(quality));
			}
			else
			{
				WriteAllBytes(path, texture2D.EncodeToPNG());
			}
		}

		public static void WriteAllBytes(string path, TextAsset textAsset)
		{
			WriteAllBytes(path, textAsset.bytes);
		}

		public static void WriteAllBytes(string path, WWW www)
		{
			WriteAllBytes(path, www.bytes);
		}

		public static void WriteAllBytes(string path, byte[] bytes)
		{
			string dir = Path.GetDirectoryName(path);
			CreateDirectory(dir);

			File.WriteAllBytes(path, bytes);
		}

		/// <summary>
		///     Writes all text.
		/// </summary>
		/// <param name="path">Path.</param>
		/// <param name="text"></param>
		public static void WriteAllText(string path, string text)
		{
			string dir = Path.GetDirectoryName(path);
			CreateDirectory(dir);

			File.WriteAllText(path, text);
		}

		#endregion

		#region Json Func 仅用于框架代码
		public static T ReadJsonFile<T>(string path, bool isUncompress = false)
		{
			try
			{
				if (isUncompress)
				{
					var bytes = ZipTool.Uncompress(File.ReadAllBytes(path));
					return JsonMapper.ToObject<T>(Encoding.UTF8.GetString(bytes));
				}
				else
				{
					return JsonMapper.ToObject<T>(File.ReadAllText(path));
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
			return default(T);
		}

		public static T ReadJsonText<T>(string json, bool isUncompress = false)
		{
			try
			{
				if (isUncompress)
				{
					var bytes = ZipTool.Uncompress(Encoding.UTF8.GetBytes(json));
					return JsonMapper.ToObject<T>(Encoding.UTF8.GetString(bytes));
				}
				else
				{
					return JsonMapper.ToObject<T>(json);
				}
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
			return default(T);
		}

		public static T ReadJsonBytes<T>(byte[] jsonBytes, bool isUncompress = false)
		{
			try
			{
				byte[] bytes = jsonBytes;
				if (isUncompress)
				{
					bytes = ZipTool.Uncompress(jsonBytes);
				}
				return JsonMapper.ToObject<T>(Encoding.UTF8.GetString(bytes));
			}
			catch (Exception e)
			{
				Debug.LogError(e.Message);
			}
			return default(T);
		}

		public static void SaveJsonObj(object obj, string savePath, bool isCompress = false, bool prettyPrint = false)
		{
			string json = JsonMapper.ToJson(obj, prettyPrint);
			SaveJsonText(json, savePath, isCompress);
		}

		public static void SaveJsonText(string json, string savePath, bool isCompress = false)
		{
			if (isCompress)
			{
				WriteAllBytes(savePath, ZipTool.Compress(Encoding.UTF8.GetBytes(json)));
			}
			else
			{
				WriteAllText(savePath, json);
			}
		}
		#endregion

		#region Async Read File

		public static FileExt ReadFileAsync(string path, OnLoadFinish finishCallback, OnError errorCallback = null)
		{
			if (IsExist(path))
			{
				FileExt asynLoader = new FileExt();
				asynLoader.AsyncBeginReadFile(path, finishCallback, errorCallback);
				return asynLoader;
			}

			if (errorCallback != null)
				errorCallback(path, "Error: Could not find file " + path);
			else
				Debug.LogError("Error: Could not find file " + path);

			return null;
		}

		//get async progress
		public float GetAsyncProgress()
		{
			return mBufferSize * 1f / mBuffer.Length;
		}

		//async read file
		private void AsyncBeginReadFile(string path, OnLoadFinish finishCallback, OnError errorCallback)
		{
			mPath = path;
			mFinishCallback = finishCallback;
			mErrorCallback = errorCallback;

			try
			{
				mInputStream = new FileStream(path, FileMode.Open);
				mBuffer = new byte[mInputStream.Length];
				mBufferSize = 0;
				mInputStream.BeginRead(mBuffer, mBufferSize, mBuffer.Length, AsyncReadCallback, null);
			}
			catch (Exception e)
			{
				if (mErrorCallback != null)
				{
					mErrorCallback(mPath, "Error: " + e.Message);
				}
				else
				{
					Debug.LogError("Error: " + e.Message);
				}
			}
		}

		//async read callback
		private void AsyncReadCallback(IAsyncResult asyncResult)
		{
			try
			{
				int bytesRead = mInputStream.EndRead(asyncResult);
				if (bytesRead > 0)
				{
					mBufferSize += bytesRead;
					Thread.Sleep(TimeSpan.FromMilliseconds(10));
					mInputStream.BeginRead(mBuffer, mBufferSize, mBuffer.Length - mBufferSize, AsyncReadCallback, null);
				}
				else
				{
					if (mFinishCallback != null)
					{
						mFinishCallback(this);
					}
					mInputStream.Close();
				}
			}
			catch (Exception e)
			{
				Debug.LogError("Error: " + e.Message);
				if (mErrorCallback != null)
				{
					mErrorCallback(mPath, e.Message);
				}
			}
		}


		public static IEnumerator ReadFileAsyncByCoroutine(string path, OnLoadFinish finishCallback, OnError errorCallback = null)
		{
			var finishedFlag = false;
			ReadFileAsync(path,
				file =>
				{
					if (finishCallback != null)
					{
						finishCallback(file);
					}
					finishedFlag = true;
				},
				(s, error1) =>
				{
					if (errorCallback != null)
					{
						errorCallback(s, error1);
					}
					finishedFlag = true;
				});

			while (!finishedFlag)
			{
				yield return null;
			}
		}
		#endregion

		#region Async Write File

		public static FileExt WriteFileAsync(string path, byte[] bytes, OnLoadFinish finishCallback,
			OnError errorCallback = null)
		{
			if (bytes == null) return null;

			FileExt asynLoader = new FileExt();
			asynLoader.AsyncBeginWriteFile(path, bytes, finishCallback, errorCallback);
			return asynLoader;
		}

		//async write file
		private void AsyncBeginWriteFile(string path, byte[] bytes, OnLoadFinish finishCallback,
			OnError errorCallback)
		{
			string dir = Path.GetDirectoryName(path);
			CreateDirectory(dir);
			mPath = path;
			mFinishCallback = finishCallback;
			mErrorCallback = errorCallback;

			try
			{
				mInputStream = new FileStream(path, FileMode.Create,
					FileAccess.ReadWrite, FileShare.None, 4096, true);
				mBuffer = bytes;
				mBufferSize = 0;
				mInputStream.BeginWrite(mBuffer, mBufferSize, mBuffer.Length, AsyncWriteCallback, null);
			}
			catch (Exception e)
			{
				Debug.LogError("Error: " + e.Message);
				if (mErrorCallback != null)
				{
					mErrorCallback(mPath, e.Message);
				}
			}
		}

		//async read callback
		private void AsyncWriteCallback(IAsyncResult asyncResult)
		{
			if (mFinishCallback != null)
			{
				mFinishCallback(this);
			}
			mInputStream.Close();
		}

		#endregion
	}
}
