using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public static class Localization
{
    private static string[] _langs;

    public static string[] Langs
    {
        get
        {
            return _langs;
        }
    }

    private static int _langIndex;
    public static int LangIndex
    {
        get { return _langIndex; }
        set { SelectLanguageByIndex(value); }
    }


    public static string language
    {
        get { return _langs[_langIndex]; }
        set { SelectLanguage(value); }
    }

    private static List<string> _langKeys;                  //保持与csv一致的索引

    public static List<string> LangKeys
    {
        get
        {
            return _langKeys;
        }
    }
    private static Dictionary<string, string[]> _langMap;  //Key,Lang[]
    private static Dictionary<string, string[]> LangMap
    {
        get
        {
            return _langMap;
        }
    }

    private static GameObject _uiRoot;
    public static GameObject UIRoot
    {
        get
        {
            if (_uiRoot == null)
            {
                var canvas = Object.FindObjectOfType<Canvas>();
                if (canvas != null)
                {
                    _uiRoot = canvas.gameObject;
                }
                else
                {
                    _uiRoot = Object.FindObjectOfType<GameObject>();
                    Debug.LogWarning("场景不存在UIRoot");
                }
            }

            return _uiRoot;
        }
        set { _uiRoot = value; }
    }

    public static bool TryGet(string key, out string val)
    {
        if (!string.IsNullOrEmpty(key))
        {
            var langMap = LangMap;
            if (langMap != null)
            {
                string[] langs;
                if (langMap.TryGetValue(key, out langs))
                {
                    if (langs != null && _langIndex < langs.Length)
                    {
                        val = langs[_langIndex];
                        return true;
                    }
                }
            }
        }

        val = key;
        return false;
    }

    public static bool ContainsKey(string key)
    {
        return LangMap.ContainsKey(key);
    }

    public static string Get(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            var langMap = LangMap;
            if (langMap != null)
            {
                string[] langs;
                if (langMap.TryGetValue(key, out langs))
                {
                    if (langs != null && _langIndex < langs.Length)
                    {
                        return langs[_langIndex];
                    }
                }
            }
        }

        return key;
    }

    public static string[] GetLangsByKey(string key)
    {
        if (string.IsNullOrEmpty(key)) return null;

        string[] langs;
        LangMap.TryGetValue(key, out langs);
        return langs;
    }

    public static int GetKeyIndex(string key)
    {
        if (!string.IsNullOrEmpty(key))
        {
            return LangKeys.IndexOf(key);
        }

        return -1;
    }

    public static int GetLanguageIndex(string lang)
    {
        for (int index = 0; index < _langs.Length; ++index)
        {
            if (string.Equals(lang, _langs[index], StringComparison.Ordinal))
                return index;
        }
        return -1;
    }

    public static bool SelectLanguage(string language)
    {
        int newIndex = GetLanguageIndex(language);
        return SelectLanguageByIndex(newIndex);
    }

    public static bool SelectLanguageByIndex(int newIndex)
    {
        if (newIndex != -1 && newIndex < Langs.Length)
        {
            if (newIndex != _langIndex)
            {
                _langIndex = newIndex;
                UIRoot.BroadcastMessage("OnLocalize", SendMessageOptions.DontRequireReceiver);
                PlayerPrefs.SetString("Localization", language);
                return true;
            }
        }

        return false;
    }

    public static bool LoadCSV(byte[] bytes)
    {
        if (bytes == null) return false;
        ByteReader reader = new ByteReader(bytes);

        // The first line should contain "KEY", followed by languages.
        BetterList<string> header = reader.ReadCSV();

        // There must be at least two columns in a valid CSV file
        if (header.size < 2) return false;
        header.RemoveAt(0);

        _langIndex = 0;
        _langs = new string[header.size];
        for (int i = 0; i < header.size; i++)
        {
            _langs[i] = header[i];
        }

        _langMap = new Dictionary<string, string[]>();
        _langKeys = new List<string>();
        for (; ; )
        {
            BetterList<string> temp = reader.ReadCSV();
            if (temp == null || temp.size == 0) break;

            string key = temp[0];
            if (string.IsNullOrEmpty(key)) continue;

            var fields = new string[_langs.Length];
            for (int i = 1; i < temp.size; i++)
            {
                try
                {
                    fields[i - 1] = temp[i];
                }
                catch (Exception)
                {
                    Debug.LogErrorFormat("当前key解析出错：{0}", key, string.Join("|", temp.ToArray()));
                    throw;
                }
            }

            if (_langMap.ContainsKey(key))
            {
                Debug.LogErrorFormat("存在重复ID,请检查配置表：<{0}> 行号：{1}", key, _langKeys.Count + 2);
            }
            else
            {
                _langMap.Add(key, fields);
                _langKeys.Add(key);
            }
        }

        UIRoot.BroadcastMessage("OnLocalize", SendMessageOptions.DontRequireReceiver);
        Debug.LogFormat("Load Localization csv success! langs:{0} key:{1}", _langs.Length, _langMap.Count);
        return true;
    }
}
