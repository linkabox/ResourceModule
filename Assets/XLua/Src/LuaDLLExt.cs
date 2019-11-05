
using System;
using System.Runtime.InteropServices;

using LuaAPI = XLua.LuaDLL.Lua;
using RealStatePtr = System.IntPtr;
using LuaCSFunction = XLua.LuaDLL.lua_CSFunction;

namespace XLua.LuaDLL
{
	public partial class Lua
	{
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaopen_core_clib_world_c(System.IntPtr L);

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		internal static int cs_luaopen_core_clib_world_c(RealStatePtr L)
		{
			return luaopen_core_clib_world_c(L);
		}
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaopen_rvo2(System.IntPtr L);

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		internal static int cs_luaopen_rvo2(RealStatePtr L)
		{
			return luaopen_rvo2(L);
		}
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaopen_rapidjson(System.IntPtr L);

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		internal static int cs_luaopen_rapidjson(RealStatePtr L)
		{
			return luaopen_rapidjson(L);
		}
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaopen_sproto_core(System.IntPtr L);

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		internal static int cs_luaopen_sproto_core(RealStatePtr L)
		{
			return luaopen_sproto_core(L);
		}
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaopen_lpeg(System.IntPtr L);

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		internal static int cs_luaopen_lpeg(RealStatePtr L)
		{
			return luaopen_lpeg(L);
		}
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaopen_crypt(System.IntPtr L);

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		internal static int cs_luaopen_crypt(RealStatePtr L)
		{
			return luaopen_crypt(L);
		}
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaopen_rc4_c(System.IntPtr L);

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		internal static int cs_luaopen_rc4_c(RealStatePtr L)
		{
			return luaopen_rc4_c(L);
		}
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaopen_md5(System.IntPtr L);

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		internal static int cs_luaopen_md5(RealStatePtr L)
		{
			return luaopen_md5(L);
		}
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaopen_lzma(System.IntPtr L);

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		internal static int cs_luaopen_lzma(RealStatePtr L)
		{
			return luaopen_lzma(L);
		}
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaopen_lsocket(System.IntPtr L);

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		internal static int cs_luaopen_lsocket(RealStatePtr L)
		{
			return luaopen_lsocket(L);
		}
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaopen_sconn_socket(System.IntPtr L);

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		internal static int cs_luaopen_sconn_socket(RealStatePtr L)
		{
			return luaopen_sconn_socket(L);
		}
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaopen_timesync(System.IntPtr L);

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		internal static int cs_luaopen_timesync(RealStatePtr L)
		{
			return luaopen_timesync(L);
		}
		
		[DllImport(LUADLL, CallingConvention = CallingConvention.Cdecl)]
		public static extern int luaopen_timer_c(System.IntPtr L);

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		internal static int cs_luaopen_timer_c(RealStatePtr L)
		{
			return luaopen_timer_c(L);
		}
		
	}
}

namespace XLua
{
	public static class LuaDLLExt
	{
		public static void AddLib(LuaEnv env)
		{
			env.AddBuildin("core.clib.world_c", LuaDLL.Lua.cs_luaopen_core_clib_world_c);
			env.AddBuildin("core.clib.rvo2", LuaDLL.Lua.cs_luaopen_rvo2);
			env.AddBuildin("core.rapidjson", LuaDLL.Lua.cs_luaopen_rapidjson);
			env.AddBuildin("ejoy2dx.sproto.c", LuaDLL.Lua.cs_luaopen_sproto_core);
			env.AddBuildin("ejoy2dx.lpeg.c", LuaDLL.Lua.cs_luaopen_lpeg);
			env.AddBuildin("ejoy2dx.crypt.c", LuaDLL.Lua.cs_luaopen_crypt);
			env.AddBuildin("ejoy2dx.rc4.c", LuaDLL.Lua.cs_luaopen_rc4_c);
			env.AddBuildin("ejoy2dx.md5.c", LuaDLL.Lua.cs_luaopen_md5);
			env.AddBuildin("ejoy2dx.lzma.c", LuaDLL.Lua.cs_luaopen_lzma);
			env.AddBuildin("ejoy2dx.socket.c", LuaDLL.Lua.cs_luaopen_lsocket);
			env.AddBuildin("sconn_socket.c", LuaDLL.Lua.cs_luaopen_sconn_socket);
			env.AddBuildin("ejoy2dx.timesync.c", LuaDLL.Lua.cs_luaopen_timesync);
			env.AddBuildin("ejoy2dx.timer.c", LuaDLL.Lua.cs_luaopen_timer_c);
			
			
		}
	}
}

