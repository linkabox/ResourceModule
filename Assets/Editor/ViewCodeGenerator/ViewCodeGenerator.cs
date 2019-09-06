
using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;
using System.Text;
using System.Text.RegularExpressions;

public class ViewGeneratorSettings
{
    public Dictionary<string, string> remap = new Dictionary<string, string>();
}

public class ViewRefData
{
    public string name;
    public List<ViewComData> components;

    public string GetFileName()
    {
        return Regex.Replace(name, @"[A-Z]", match =>
        {
            if (match.Index == 0)
            {
                return match.Value.ToLower();
            }
            else
            {
                return "_" + match.Value.ToLower();
            }
        }) + ".lua";
    }

    public ViewRefData()
    {
    }

    public ViewRefData(string name)
    {
        this.name = name;
        this.components = new List<ViewComData>();
    }

    public bool TryGetValue(string uid, out ViewComData comData)
    {
        foreach (var viewComData in components)
        {
            if (viewComData.uid == uid)
            {
                comData = viewComData;
                return true;
            }
        }
        comData = null;
        return false;
    }
}

public class ViewComData
{
    public string uid;
    public string comType;
    public string fulleTypeName;
    public string member;
    public string path;

    public ViewComData()
    {
    }

    public ViewComData(Transform node, Transform root, Type type)
    {
        comType = type.Name;
        fulleTypeName = type.FullName;
        if (type == typeof(GameObject))
        {
            member = node.name;//+ "_go";
        }
        else if (type.IsSubclassOf(typeof(Transform)))
        {
            member = node.name;
        }
        else
        {
            member = node.name;
            //memberName = node.name + "_" + comType.Replace(".", "_");
        }

        path = AnimationUtility.CalculateTransformPath(node, root);
        uid = GenerateUID(node, root, type);
    }

    public string GetGetterName()
    {
        return member;
        //return "get" + member.Substring(0, 1).ToUpper() + member.Substring(1);
    }

    public string GetName()
    {
        return comType + " " + member;
    }

    public static string GenerateUID(Transform node, Transform root, Type type)
    {
        if (root == node)
        {
            return "root_" + type.Name;
        }
        return AnimationUtility.CalculateTransformPath(node, root) + "_" + type.Name;
    }
}

public class ViewCodeGenerator : EditorWindow
{
    /// <summary>
    ///     Validates the selection.
    ///     -1:不是当前Prefab的节点
    ///     0：是当前Prefab的根节点
    ///     1：是当前Prefab的子节点
    /// </summary>
    private enum PrefabNodeType
    {
        Error = -1,
        Root = 0,
        Child = 1
    }

    public static ViewCodeGenerator Instance;
    private const string CONFIG_ROOT = "Assets/Editor/ViewCodeGenerator/ViewConfig";
    private const string CS_CODE_ROOT = "Assets/Scripts/UI";
    private const string LUA_CODE_ROOT = "Assets/BundleResources/Lua/LuaCode/ui/view";

    private ViewComData _curComInfo;

    private Transform _prefabRoot;

    private ViewRefData _viewRefData;

    public ViewGeneratorSettings _generatorSetttings;
    private string _inputRemap;

    private Vector2 exportViewPos;

    private Vector2 selectionViewPos;

    [MenuItem("Window/ViewCodeGenerator #&u")]
    public static void ShowWindow()
    {
        if (Instance == null)
        {
            var window = GetWindow<ViewCodeGenerator>(false, "UIViewGenerator", true);
            window.Show();
        }
        else
        {
            Instance.Close();
        }
    }

    private void OnSelectionChange()
    {
        EditorGUIUtility.editingTextField = false;
        Repaint();
    }

    private void OnEnable()
    {
        Instance = this;
        LoadGeneratorSettings();
        CleanUp();
    }

    private const string SettingsPath = "Assets/Editor/ViewCodeGenerator/settings.json";
    private void LoadGeneratorSettings()
    {
        if (File.Exists(SettingsPath))
        {
            string json = File.ReadAllText(SettingsPath);
            _generatorSetttings = LitJson.JsonMapper.ToObject<ViewGeneratorSettings>(json);
        }
        else
        {
            _generatorSetttings = new ViewGeneratorSettings();
            SaveGeneratorSettings();
        }
    }

    private void SaveGeneratorSettings()
    {
        File.WriteAllText(SettingsPath, LitJson.JsonMapper.ToJson(_generatorSetttings));
        AssetDatabase.Refresh();
    }

    public void RemapViewName(string prefabName, string remap)
    {
        _generatorSetttings.remap[prefabName] = remap;
        SaveGeneratorSettings();
    }

    private void OnDestroy()
    {
        //退出前保存下配置
        SaveViewConfig(_viewRefData);
        AssetDatabase.Refresh();
    }

    private void OnGUI()
    {
        DrawTitleGroup();

        if (!_prefabRoot)
            return;

        EditorGUILayout.BeginHorizontal();
        DrawSelectionComponentView();

        EditorGUILayout.BeginVertical(); //Right Part Vertical Begin
        DrawComponentInfoView();
        DrawExportListView();
        EditorGUILayout.EndVertical(); //Right Part Vertical End

        EditorGUILayout.EndHorizontal();
    }

    private void DrawTitleGroup()
    {
        GUILayout.Space(15f);
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.BeginVertical(GUILayout.Width(80f));
        if (GUILayout.Button("Setup"))
        {
            SetTarget(Selection.activeGameObject);
        }
        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField("UIPrefab: " + (_prefabRoot == null ? "NONE" : _prefabRoot.name));
        if (_viewRefData != null)
        {
            EditorGUILayout.LabelField("ClassName: " + _viewRefData.name);
        }
        EditorGUILayout.EndVertical();

        _inputRemap = EditorGUILayout.TextField(_inputRemap);
        if (GUILayout.Button("Remap"))
        {
            if (Selection.activeGameObject != null)
            {
                RemapViewName(Selection.activeGameObject.name, _inputRemap);
            }
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
    }

    private void SetTarget(GameObject targetGo)
    {
        if (targetGo == null) return;

        if (PrefabInstanceCheck(targetGo))
        {
            var newPrefab = PrefabUtility.GetOutermostPrefabInstanceRoot(targetGo);
            if (newPrefab != null)
            {
                var newPrefabRoot = newPrefab.transform;
                if (newPrefabRoot != _prefabRoot)
                {
                    CleanUp();
                    _prefabRoot = newPrefabRoot;
                    RemoveNotification();
                    LoadViewConfig(newPrefab);
                }
                else
                {
                    ShowNotification(new GUIContent("这是同一个UIPrefab"));
                }
            }
        }
        else
        {
            ShowNotification(new GUIContent("这不是一个PrefabInstance"));
        }
    }

    private bool PrefabInstanceCheck(Object target)
    {
        var type = PrefabUtility.GetPrefabInstanceStatus(target);

        if (type == PrefabInstanceStatus.NotAPrefab)
        {
            return false;
        }
        return true;
    }

    private void CleanUp()
    {
        _prefabRoot = null;
        _viewRefData = null;
        _curComInfo = null;
    }

    private void LoadViewConfig(GameObject prefab)
    {
        string viewName = prefab.name;
        if (_generatorSetttings.remap.ContainsKey(viewName))
        {
            viewName = _generatorSetttings.remap[viewName];
        }
        string configPath = Path.Combine(CONFIG_ROOT, viewName + ".json");
        if (File.Exists(configPath))
        {
            var rawText = File.ReadAllText(configPath);
            var viewConfig = LitJson.JsonMapper.ToObject<ViewRefData>(rawText);
            _viewRefData = viewConfig ?? new ViewRefData(viewName);
        }
        else
        {
            _viewRefData = new ViewRefData(viewName);
        }
    }

    private void SaveViewConfig(ViewRefData viewRefData)
    {
        if (viewRefData == null) return;
        string configPath = Path.Combine(CONFIG_ROOT, viewRefData.name + ".json");
        var json = LitJson.JsonMapper.ToJson(viewRefData, true);
        File.WriteAllText(configPath, json);
    }

    private void DrawSelectionComponentView()
    {
        EditorGUILayout.BeginVertical(GUILayout.MinWidth(150f));
        if (EditorUIDrawer.DrawHeader("Node Components"))
        {
            selectionViewPos = EditorGUILayout.BeginScrollView(selectionViewPos, "TextField");

            //根节点只显示除GameObject、Transform以外的组件
            var target = Selection.activeTransform;
            var nodeType = ValidateSelection(target);
            if (nodeType != PrefabNodeType.Error)
            {
                if (nodeType != PrefabNodeType.Root && GUILayout.Button("---GameObject---"))
                {
                    string comUID = ViewComData.GenerateUID(target, _prefabRoot, typeof(GameObject));
                    ViewComData comInfo;
                    if (_viewRefData.TryGetValue(comUID, out comInfo))
                    {
                        _curComInfo = comInfo;
                    }
                    else
                    {
                        if (EditorUtility.DisplayDialog("提示", "是否添加该组件到配置中?\nUID:" + comUID, "确定", "取消"))
                        {
                            var newComInfo = new ViewComData(target, _prefabRoot, typeof(GameObject));
                            _viewRefData.components.Add(newComInfo);
                            _curComInfo = newComInfo;
                        }
                    }

                    EditorGUIUtility.editingTextField = false;
                }
                GUILayout.Space(3f);

                GUILayout.Label("ComponentList");
                Component[] comList = target.GetComponents<Component>();
                foreach (Component com in comList)
                {
                    var comType = com.GetType();
                    if (GUILayout.Button(comType.Name))
                    {
                        string comUID = ViewComData.GenerateUID(target, _prefabRoot, comType);
                        ViewComData comInfo;
                        if (_viewRefData.TryGetValue(comUID, out comInfo))
                        {
                            _curComInfo = comInfo;
                            comInfo.fulleTypeName = string.IsNullOrEmpty(comInfo.fulleTypeName)
                                ? comType.FullName
                                : comInfo.fulleTypeName;
                        }
                        else
                        {
                            if (EditorUtility.DisplayDialog("提示", "是否添加该组件到配置中?\nUID:" + comUID, "确定", "取消"))
                            {
                                var newComInfo = new ViewComData(target, _prefabRoot, comType);
                                _viewRefData.components.Add(newComInfo);
                                _curComInfo = newComInfo;
                            }
                        }

                        EditorGUIUtility.editingTextField = false;
                    }
                    GUILayout.Space(3f);
                }
            }
            else
                EditorGUILayout.HelpBox("This is not a node of UIPrefab", MessageType.Info);
            EditorGUILayout.EndScrollView();
        }

        EditorGUILayout.EndVertical();
    }

    private PrefabNodeType ValidateSelection(Transform selection)
    {
        if (selection == _prefabRoot)
            return PrefabNodeType.Root;

        Transform trans = selection;
        while (trans != null)
        {
            if (trans == _prefabRoot)
                return PrefabNodeType.Child;
            trans = trans.parent;
        }
        return PrefabNodeType.Error;
    }

    private void DrawComponentInfoView()
    {
        EditorUIDrawer.DrawHeader("ComponentInfo");
        if (_curComInfo != null)
        {
            EditorGUIUtility.labelWidth = 100f;
            EditorUIDrawer.DrawBoxLabel("Type: ", _curComInfo.comType, false);
            EditorUIDrawer.DrawBoxLabel("Path: ",
                string.IsNullOrEmpty(_curComInfo.path) ? "This is a root node" : _curComInfo.path, true);

            string memberName = EditorGUILayout.TextField("MemberName: ", _curComInfo.member, GUILayout.Height(20));
            if (!string.Equals(memberName, _curComInfo.member, StringComparison.Ordinal))
            {
                _curComInfo.member = memberName;
            }
        }
    }

    private void DrawExportListView()
    {
        EditorUIDrawer.DrawHeader("ExportList");
        GUILayout.BeginHorizontal();
        GUILayout.Space(3f);
        GUILayout.BeginVertical();
        exportViewPos = EditorGUILayout.BeginScrollView(exportViewPos);
        if (_viewRefData != null)
        {
            int removeIndex = -1;
            for (var index = 0; index < _viewRefData.components.Count; index++)
            {
                var comInfo = _viewRefData.components[index];
                GUILayout.Space(-1f);
                bool highlight = _curComInfo == comInfo;
                Transform node = _prefabRoot.Find(comInfo.path);
                if (node == null || (comInfo.comType != "GameObject" && node.GetComponent(comInfo.comType) == null))
                {
                    GUI.backgroundColor = Color.red;
                }
                else
                {
                    GUI.backgroundColor = highlight ? Color.white : new Color(0.8f, 0.8f, 0.8f);
                }

                GUILayout.BeginHorizontal("TextArea", GUILayout.MinHeight(20f));

                GUI.backgroundColor = Color.white;
                GUILayout.Label(index.ToString(), GUILayout.Width(20f));

                if (GUILayout.Button(comInfo.GetName(), "Label", GUILayout.Height(20f)))
                {
                    _curComInfo = comInfo;
                    Selection.activeTransform = _prefabRoot.Find(_curComInfo.path);
                }

                if (GUILayout.Button("X", GUILayout.Width(22f)))
                {
                    removeIndex = index;
                }

                GUILayout.EndHorizontal();
            }

            if (removeIndex != -1)
            {
                _viewRefData.components.RemoveAt(removeIndex);
            }
        }
        EditorGUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUILayout.Space(3f);
        GUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("SaveViewConfig", GUILayout.Width(120f), GUILayout.Height(40f)))
        {
            SaveViewConfig(_viewRefData);
        }

        if (GUILayout.Button("GenerateViewCode", GUILayout.Width(120f), GUILayout.Height(40f)))
        {
            GenerateViewCode();
        }

        EditorGUILayout.EndHorizontal();
        GUILayout.Space(10f);
    }

    private void GenerateViewCode()
    {
#if GEN_CS_CODE
		GenerateCSharpViewCode(_viewConfig, _prefabRoot);
#else
        GenerateViewLuaCode(_viewRefData, _prefabRoot);
#endif
    }

#if GEN_CS_CODE
	private const string CS_CODE_TEMPLATE =
@"using UnityEngine;

public class <className> : BaseUIView
{<member>}";

	private void GenerateCSharpViewCode(ViewRefData viewRefData, Transform prefabRoot)
	{
		if (viewRefData == null) return;

		var memberWriter = new StringBuilder();
		memberWriter.Append("\r\n");
		memberWriter.AppendFormat("    public const string NAME = \"{0}\";\r\n\r\n", prefabRoot.name);
		foreach (var comInfo in viewRefData.components.Values)
		{
			memberWriter.AppendFormat("    private {0} _{1};\r\n", comInfo.fulleTypeName, comInfo.member);
			if (comInfo.comType == "GameObject")
			{
				memberWriter.AppendFormat(
@"    public {0} {1}
    {{
        get {{ return _{1} ?? (_{1} = root.Find(""{2}"").gameObject); }}
    }}
", comInfo.comType, comInfo.member, comInfo.path);
			}
			else if (comInfo.comType == "Transform")
			{
				memberWriter.AppendFormat(
@"    public {0} {1}
    {{
        get {{ return _{1} ?? (_{1} = root.Find(""{2}"")); }}
    }}
", comInfo.comType, comInfo.member, comInfo.path);
			}
			else
			{
				memberWriter.AppendFormat(
@"    public {0} {1}
    {{
        get {{ return _{1} ?? (_{1} = root.Find(""{2}"").GetComponent<{0}>()); }}
    }}
", comInfo.fulleTypeName, comInfo.member, comInfo.path);
			}
			memberWriter.Append("\r\n");
		}

		var codeWriter = new StringBuilder(CS_CODE_TEMPLATE);
		codeWriter.Replace("<className>", viewRefData.name);
		codeWriter.Replace("<member>", memberWriter.ToString());

		SaveViewConfig(viewRefData);
		File.WriteAllText(Path.Combine(CS_CODE_ROOT, viewRefData.name + ".cs"), codeWriter.ToString());
		this.ShowNotification(new GUIContent("生成ViewCode成功!"));
	}
#endif

    private const string LUA_CODE_TEMPLATE =
@"
<localRef>
local view_getter = require('ui.view.view_getter')
local <className> = {}
<className>.__index = view_getter

function <className>:new(rootGo)
	local inst = setmetatable({}, <className>)
	inst.rootGo = rootGo
	inst.root = rootGo.transform
	return inst
end

<member>
return <className>
";
    private void GenerateViewLuaCode(ViewRefData viewRefData, Transform prefabRoot)
    {
        if (viewRefData == null) return;

        var nameSpaceSet = new HashSet<string>();
        var refWriter = new StringBuilder();
        foreach (var comInfo in viewRefData.components)
        {
            //兼容处理，以前旧的ViewConfig没有保存type.FullName
            string typeName = string.IsNullOrEmpty(comInfo.fulleTypeName) ? comInfo.comType : comInfo.fulleTypeName;
            if (!nameSpaceSet.Contains(typeName))
            {
                nameSpaceSet.Add(typeName);
                refWriter.AppendFormat("local {0} = CS.{1}\n", comInfo.comType, typeName);
            }
        }

        var memberWriter = new StringBuilder();
        foreach (var comInfo in viewRefData.components)
        {
            string className = viewRefData.name;
            string getterName = comInfo.GetGetterName();
            if (comInfo.comType == "GameObject")
            {
                memberWriter.AppendFormat(@"function {0}:{1}()
    return self.root:Find('{2}').gameObject
end", className, getterName, comInfo.path);
            }
            else if (comInfo.comType == "Transform" || comInfo.comType == "RectTransform")
            {
                memberWriter.AppendFormat(@"function {0}:{1}()
    return self.root:Find('{2}')
end", className, getterName, comInfo.path);
            }
            else
            {
                if (string.IsNullOrEmpty(comInfo.path))
                {
                    memberWriter.AppendFormat(@"function {0}:{1}()
    return self.root:GetComponent(typeof({2}))
end", className, getterName, comInfo.comType);
                }
                else
                {
                    memberWriter.AppendFormat(@"function {0}:{1}()
    return self.root:Find('{2}'):GetComponent(typeof({3}))
end", className, getterName, comInfo.path, comInfo.comType);
                }
            }
            memberWriter.Append("\n\n");
        }

        var codeWriter = new StringBuilder(LUA_CODE_TEMPLATE);
        codeWriter.Replace("<localRef>", refWriter.ToString());
        codeWriter.Replace("<className>", viewRefData.name);
        codeWriter.Replace("<member>", memberWriter.ToString());

        SaveViewConfig(viewRefData);
        File.WriteAllText(Path.Combine(LUA_CODE_ROOT, viewRefData.GetFileName()), codeWriter.ToString());
        this.ShowNotification(new GUIContent("生成ViewCode成功!"));
        AssetDatabase.Refresh();
    }
}