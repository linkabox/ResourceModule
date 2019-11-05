#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ResourceModule
{
    public static class RawDataGenerator
    {
        public static string RawDataDir = Application.dataPath + "/../raw_data";

#if UNITY_EDITOR && !UNITY_EDITOR_OSX
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        public static void CheckRawData()
        {
            var activeScene = EditorSceneManager.GetActiveScene();
            if (activeScene.name == "GameMain")
            {
                //Win下检查配表数据变动，重新生成
                ResourceModule.RawDataGenerator.GenRawConfigData();
            }
        }
#endif

        public static void GenRawConfigData(bool force = false)
        {
            if (!Directory.Exists(RawDataDir))
            {
                Debug.Log("RawDataDir can not found:" + RawDataDir);
                return;
            }

            bool excute = force;
            if (!force)
            {
                string oldHash = EditorPrefs.GetString("RawDataHash", "");
                string hash = GetRawConfigHash();
                if (oldHash != hash)
                {
                    Debug.LogFormat("RawData update:{0} {1}", oldHash, hash);
                    EditorPrefs.SetString("RawDataHash", hash);
                    excute = true;
                }
                else
                {
                    Debug.LogFormat("RawData is newest:{0}", hash);
                }
            }

            if (excute)
            {
                SystemExt.RunShell(RawDataDir + "/gen_config.bat", "", RawDataDir);
            }
        }

        public static string GetRawConfigHash()
        {
            var files = Directory.GetFiles(RawDataDir, "*.xlsx");
            string content = "";
            foreach (var file in files)
            {
                if (file.Contains("~$")) continue;
                var modifyDate = File.GetLastWriteTime(file);
                content += modifyDate;
                //Debug.LogFormat("{0} {1}", file, modifyDate);
            }

            return MD5Hashing.HashString(content);
        }
    }
}

#endif