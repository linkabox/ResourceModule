using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.IO;
using ResourceModule;
using UnityEngine;
using XLua;

[LuaCallCSharp]
public class GameMain : MonoBehaviour
{
    protected LuaEnv luaEnv;
    protected bool __dispose = false;

    public static GameMain Instance;
    public string luaPath;
    public string launchCfg;

#if UNITY_EDITOR
    public LuaEnv Env
    {
        get { return luaEnv; }
    }
    private bool _loadPackedCode = true;
#endif
    internal float lastGCTime = 0;
    internal const float GCInterval = 1;//1 second 

    private Action luaStart;
    private Action<float> luaUpdate;
    private Action<float> luaFixedUpdate;
    private Action<bool> luaOnPause;
    private Action<bool> luaOnFocus;
    private Action<string, string, string> luaOnError;
    private Action luaOnDestroy;
    private GameObject rootGo;

    void Awake()
    {
        Debug.Log("GameMain Awake......");
        this.rootGo = this.gameObject;
        Instance = this;
        DontDestroyOnLoad(this.gameObject);

        Application.logMessageReceived += logCallback;

        if (luaEnv == null)
        {
            luaEnv = new LuaEnv();
        }

#if UNITY_EDITOR
        _loadPackedCode = PlayerPrefs.GetInt(ResourceModuleConfig.LoadPackedLuaCode, 0) != 0;
        //为了方便调试，可以_loadPackedCode设为false,IsEdiotrMode为true
        //只加载资源Bundle，Lua代码依然读取编辑器目录
        if (!ResManager.IsEdiotrMode && _loadPackedCode)
        {
            luaEnv.AddLoader(LoadFromAssetBundle);
        }
        luaEnv.AddLoader(LoadFromBundleResources);
#else
		luaEnv.AddLoader(LoadFromAssetBundle);
#endif

        luaEnv.DoString(string.Format("require '{0}'", luaPath));
        var scriptEnv = luaEnv.Global;
        if (scriptEnv != null)
        {
            Action<string> luaAwake = scriptEnv.Get<Action<string>>("g_awake");
            scriptEnv.Get("g_start", out luaStart);
            scriptEnv.Get("g_update", out luaUpdate);
            scriptEnv.Get("g_fixed_update", out luaFixedUpdate);
            scriptEnv.Get("g_on_pause", out luaOnPause);
            scriptEnv.Get("g_on_focus", out luaOnFocus);
            scriptEnv.Get("g_on_destroy", out luaOnDestroy);
            scriptEnv.Get("g_on_error", out luaOnError);

            if (luaAwake != null)
            {
                luaAwake(launchCfg);
            }
        }
    }

    private void logCallback(string condition, string trace, LogType type)
    {
        if (luaOnError != null)
        {
            if (type == LogType.Exception || type == LogType.Error || type == LogType.Assert)
            {
                luaOnError(type.ToString(), condition, trace);
            }
        }
    }

    #region Custom Loader
    private byte[] LoadFromAssetBundle(ref string filepath)
    {
        string lua_file = filepath.Replace('.', '/');
        filepath = "Lua/LuaPackedCode/" + lua_file + ".bytes";
        var loader = ResManager.LoadAsset(filepath, ResourceModuleConfig.LuaBundleName);
        var ta = loader.ResultObject as TextAsset;
        if (ta != null)
        {
            return ta.bytes;
        }

        return null;
    }

#if UNITY_EDITOR
    private byte[] LoadFromBundleResources(ref string filepath)
    {
        string lua_dir = Application.dataPath + "/BundleResources/Lua/";
        string lua_file = filepath.Replace('.', '/');
        if (_loadPackedCode)
        {
            filepath = lua_dir + "LuaPackedCode/" + lua_file + ".bytes";
            if (File.Exists(filepath))
            {
                return File.ReadAllBytes(filepath);
            }
        }
        else
        {
            filepath = lua_dir + "LuaCode/" + lua_file + ".lua";
            if (File.Exists(filepath))
            {
                return File.ReadAllBytes(filepath);
            }
        }

        filepath = lua_dir + "LuaTestCode/" + lua_file + ".lua";
        if (File.Exists(filepath))
        {
            return File.ReadAllBytes(filepath);
        }

        return null;
    }
#endif
    #endregion

    // Use this for initialization
    #region Global Mono Event

    void Start()
    {
        if (luaStart != null)
        {
            luaStart();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (luaEnv == null) return;

        if (luaUpdate != null)
        {
            luaUpdate(Time.deltaTime);
        }
        if (Time.time - lastGCTime > GCInterval)
        {
            luaEnv.Tick();
            lastGCTime = Time.time;
        }
    }

    void FixedUpdate()
    {
        if (luaFixedUpdate != null)
        {
            luaFixedUpdate(Time.fixedDeltaTime);
        }
    }

    void OnApplicationPause(bool pause)
    {
        if (luaOnPause != null)
            luaOnPause(pause);
    }

    void OnApplicationFocus(bool focus)
    {
        if (luaOnFocus != null)
            luaOnFocus(focus);
    }

    #endregion

    void OnDestroy()
    {
        Dispose();
    }

    public void Dispose(bool dispose = false)
    {
        if (__dispose) return;

        luaStart = null;
        luaOnPause = null;
        luaOnFocus = null;
        luaUpdate = null;
        luaFixedUpdate = null;
        luaOnError = null;

        if (luaOnDestroy != null)
        {
            luaOnDestroy();
        }
        luaOnDestroy = null;

        if (dispose)
        {
            try
            {
                luaEnv.Dispose();
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            Destroy(this.rootGo);
        }

        luaEnv = null;
        Instance = null;
        __dispose = true;
        Application.logMessageReceived -= logCallback;
        Debug.Log("GameMain Dispose:" + dispose);
    }

    void DoReload()
    {
        this.rootGo.SetActive(false);
        this.Invoke("__DelayReload", 1f);
    }

    void __DelayReload()
    {
        string prefabName = this.gameObject.name;
        this.Dispose(true);
        ResManager.Dispose();

        var loader = ResManager.LoadAsset(prefabName + ".prefab");
        var prefab = loader.ResultObject as GameObject;
        var go = Instantiate(prefab);
        go.name = prefabName;
    }

    public static void Reload()
    {
        var inst = GameMain.Instance;
        if (inst != null)
        {
            inst.DoReload();
        }
    }
}
