
#define USE_LUAC
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ResourceModule
{
	public static class LuaCodePacker
	{
		public static int PackLuaCode()
		{
#if USE_LUAC
			int exitCode = -1;
#if UNITY_EDITOR_WIN
			exitCode = SystemExt.RunShell("pack_lua.bat", "", "Assets/BundleResources/Lua");
#else
			exitCode = SystemExt.RunShell("/bin/bash", "pack_lua.sh", "Assets/BundleResources/Lua");
#endif

#else
			int exitCode = 0;
			var files = Directory.GetFiles("Assets/BundleResources/Lua/LuaCode", "*.lua", SearchOption.AllDirectories);
			foreach (var file in files)
			{
				string dest = file.Replace(".lua", ".bytes").Replace("LuaCode", "LuaPackedCode");
				string dir = Path.GetDirectoryName(dest);
				if (!Directory.Exists(dir))
				{
					Directory.CreateDirectory(dir);
				}

				File.Copy(file, dest, true);
			}
#endif
			return exitCode;
		}
	}
}