using System.Text;
using System.Security.Cryptography;
using System.IO;
using UnityEngine;

public static class MD5Hashing
{
	private static MD5 _md5;

	private static MD5 Md5
	{
		get
		{
			if (_md5 == null)
				_md5 = MD5.Create();
			return _md5;
		}
	}

	/// <summary> 
	/// 使用utf8编码将字符串散列 
	/// </summary> 
	/// <param name="str">要散列的字符串</param> 
	/// <returns>散列后的字符串</returns> 
	public static string HashString(string str)
	{
		return HashString(str, Encoding.UTF8);
	}

	/// <summary> 
	/// 使用指定的编码将字符串散列 
	/// </summary>
	/// <param name="str">要散列的字符串</param>
	/// <param name="encode">编码</param>
	/// <returns>散列后的字符串</returns> 
	public static string HashString(string str, Encoding encode)
	{
		return HashBytes(encode.GetBytes(str));
	}

	public static string HashFile(string path)
	{
		try
		{
			byte[] fileBytes = File.ReadAllBytes(path);
			return HashBytes(fileBytes);
		}
		catch (System.Exception e)
		{
			Debug.LogError(e.Message);
			return "";
		}
	}

	public static string HashBytes(byte[] bytes)
	{
		byte[] hashBytes = Md5.ComputeHash(bytes);
		string result = System.BitConverter.ToString(hashBytes);
		result = result.Replace("-", "").ToLower();
		return result;
	}
}
