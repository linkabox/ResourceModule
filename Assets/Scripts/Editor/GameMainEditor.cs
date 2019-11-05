using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameMain))]
public class GameMainEditor : Editor
{
    private SerializedProperty m_LuaPath;
    private SerializedProperty m_LaunchJson;
    //private readonly GUIContent m_cfgtitle = new GUIContent("Setup LauchCfg:");
    private SerializedObject m_LaunchCfg;
    private string _hackCode = HACK_CODE_TEMPLATE;

    private TextAsset _hackCodeFile;
    private const string HACK_CODE_TEMPLATE =
    @"local res_mgr = require 'res_mgr'
local global = require 'global'
local network = require 'net.network'
local beholder = require 'lib.beholder'
local msg_box = require 'ui.msg_box'
local game_mgr = require 'game.game_mgr'
local ui_mgr = require 'ui.ui_mgr'
local data = require 'core.data'
local import = require 'core.import'

";

    private const string HACK_CODE_FILE = "Assets/BundleResources/Lua/LuaTestCode/hack_code.lua.txt";
    private void OnEnable()
    {
        this.m_LuaPath = this.serializedObject.FindProperty("luaPath");
        this.m_LaunchJson = this.serializedObject.FindProperty("launchCfg");

        string path = EditorPrefs.GetString("Last_LaunchCfg", "Assets/Scripts/Editor/LaunchCfg.asset");
        var cfg = AssetDatabase.LoadAssetAtPath<LaunchCfg>(path);
        SetupLaunchCfg(cfg);

        if (!File.Exists(HACK_CODE_FILE))
        {
            File.WriteAllText(HACK_CODE_FILE, HACK_CODE_TEMPLATE);
            AssetDatabase.ImportAsset(HACK_CODE_FILE, ImportAssetOptions.ForceSynchronousImport);
            Debug.Log("Create temp hack_code_file:" + HACK_CODE_FILE);
        }
        _hackCodeFile = AssetDatabase.LoadAssetAtPath<TextAsset>(HACK_CODE_FILE);
    }

    public void SetupLaunchCfg(LaunchCfg cfg)
    {
        if (cfg == null) return;

        m_LaunchCfg = new SerializedObject(cfg);
    }

    public override void OnInspectorGUI()
    {
        this.serializedObject.Update();
        EditorGUILayout.PropertyField(this.m_LuaPath);

        EditorGUILayout.BeginVertical("HelpBox", GUILayout.MaxWidth(500f));
        //EditorGUI.BeginChangeCheck();
        //var obj = EditorGUILayout.ObjectField(m_cfgtitle, null, typeof(LaunchCfg), false);
        //if (EditorGUI.EndChangeCheck())
        //{
        //    var cfg = obj as LaunchCfg;
        //    if (cfg != null)
        //    {
        //        SetupLaunchCfg(cfg);
        //    }
        //}

        if (m_LaunchCfg != null)
        {
            if (GUILayout.Button("Convert to json"))
            {
                Apply();
            }
            m_LaunchCfg.Update();
            EditorGUI.BeginChangeCheck();
            DrawPropertiesExcluding(m_LaunchCfg);
            if (EditorGUI.EndChangeCheck())
            {
                m_LaunchCfg.ApplyModifiedProperties();
                Apply();
            }
        }
        EditorGUILayout.PrefixLabel("生成json:");
        EditorGUILayout.TextArea(this.m_LaunchJson.stringValue, GUILayout.MaxWidth(500f));
        EditorGUILayout.EndVertical();

        this.serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();

        EditorGUILayout.BeginVertical("HelpBox", GUILayout.MaxWidth(500f));
        EditorGUILayout.PrefixLabel("HackCode:");
        _hackCodeFile = (TextAsset)EditorGUILayout.ObjectField(_hackCodeFile, typeof(TextAsset), false);
        _hackCode = EditorGUILayout.TextArea(_hackCode, GUILayout.MinHeight(200f));
        EditorGUILayout.BeginHorizontal();
        if (Application.isPlaying)
        {
            if (GUILayout.Button("Run", GUILayout.Height(50f)))
            {
                GameMain.Instance.Env.DoString(_hackCode);
            }
        }

        if (GUILayout.Button("Rest", GUILayout.Height(50f)))
        {
            _hackCode = HACK_CODE_TEMPLATE;
        }
        EditorGUILayout.EndHorizontal();

        if (Application.isPlaying)
        {
            if (GUILayout.Button("Run File", GUILayout.Height(50f)))
            {
                GameMain.Instance.Env.DoString(_hackCodeFile.text);
            }
        }
        EditorGUILayout.EndVertical();
    }

    public void Apply()
    {
        LaunchCfg cfg = m_LaunchCfg.targetObject as LaunchCfg;
        if (cfg != null)
        {
            m_LaunchJson.stringValue = GenerateLaunchCfgJson(cfg);

            string path = AssetDatabase.GetAssetPath(cfg);
            Debug.Log("Convert LaunchCfg to json:" + path, cfg);
            EditorPrefs.SetString("Last_LaunchCfg", path);
            this.serializedObject.ApplyModifiedProperties();
        }
    }

    public static string GenerateLaunchCfgJson(LaunchCfg launchCfg)
    {
        return JsonUtility.ToJson(launchCfg, false);
    }
}
