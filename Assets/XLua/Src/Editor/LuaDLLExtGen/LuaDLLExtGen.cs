using UnityEngine;
using System.Collections.Generic;
using XLua;
using System.IO;
using System.Text;
using System.Linq;
using CSObjectWrapEditor;
using UnityEditor;

public class LuaDLLExtGen : ScriptableObject
{
	public TextAsset Template;

	public static IEnumerable<CustomGenTask> GetTasks(LuaEnv lua_env, UserConfig user_cfg)
	{
		LuaTable data = lua_env.NewTable();
		yield return new CustomGenTask
		{
			Data = data,
			Output = new StreamWriter(Application.dataPath + "/XLua/Src/LuaDLLExt.cs",
				false, Encoding.UTF8)
		};
	}

	[MenuItem("XLua/Generate LuaDLLExt", false, 4)]//加到Generate Code菜单里头
	public static void GenLinkXml()
	{
		Generator.CustomGen(ScriptableObject.CreateInstance<LuaDLLExtGen>().Template.text, GetTasks);
		AssetDatabase.Refresh();
	}

}
