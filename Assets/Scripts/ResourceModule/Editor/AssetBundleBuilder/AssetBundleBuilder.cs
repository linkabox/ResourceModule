using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using ResourceModule;
using ResourceModule.Editor;
using UnityEditor;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace ResourceModule
{
    public class AssetBundleBuilder : EditorWindow
    {
        public const string BuildExportRoot = "_GameBundles";
        public static bool SlientMode = false;

        public static AssetBundleBuilder Instance;

        [MenuItem("ResourceModule/AssetBundleBuilder %#e")]
        public static void ShowWindow()
        {
            if (Instance == null)
            {
                var window = GetWindow<AssetBundleBuilder>(false, "AssetBundleBuilder", true);
                window.minSize = new Vector2(920f, 640f);
                window.Show();
            }
            else
            {
                Instance.Close();
            }
        }

        private void OnEnable()
        {
            Instance = this;
            RefreshBundleNameData();
        }

        private void OnDestroy()
        {
            Instance = null;
        }

        #region Editor UI

        private int _rightTab;
        private Vector2 _leftContentScroll;

        private void OnGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(10f);

            EditorGUILayout.BeginVertical(GUILayout.MinWidth(400f), GUILayout.MaxWidth(400f)); //Left Cotent Begin
            if (!ResManager.IsLoadAssetBundle)
            {
                GUILayout.Label("当前不是使用AssetBundle加载模式");
            }
            else
            {
                _leftContentScroll = EditorGUILayout.BeginScrollView(_leftContentScroll);
                {
                    //BundleName处理
                    EditorGUILayout.BeginVertical("HelpBox");
                    {
                        GUILayout.Label("BundleName处理", "BoldLabel");
                        //if (GUILayout.Button("修复所有BundleName", "LargeButton", GUILayout.Height(50f)))
                        //{
                        //    if (EditorUtility.DisplayDialog("确认", "是否重新设置所有资源BundleName?", "继续", "取消"))
                        //    {
                        //        EditorApplication.delayCall += FixBundleNames;
                        //        _rightTab = 0;
                        //    }
                        //}
                        //EditorGUILayout.Space();

                        if (GUILayout.Button("清空未使用的BundleName", "LargeButton", GUILayout.Height(50f)))
                        {
                            if (EditorUtility.DisplayDialog("确认", "是否清空未使用的BundleName?", "Yes", "No"))
                            {
                                EditorApplication.delayCall += () =>
                                {
                                    CleanUpBundleName(false);
                                };
                            }
                        }
                        EditorGUILayout.Space();

                        if (GUILayout.Button("清空全部的BundleName", "LargeButton", GUILayout.Height(50f)))
                        {
                            if (EditorUtility.DisplayDialog("确认", "是否清空全部的BundleName?", "Yes", "No"))
                            {
                                EditorApplication.delayCall += () =>
                                {
                                    CleanUpBundleName(true);
                                };
                            }
                        }
                        EditorGUILayout.Space();
                    }
                    EditorGUILayout.EndVertical();

                    //打包选项
                    EditorGUILayout.BeginVertical("HelpBox");
                    {
                        GUILayout.Label("打包", "BoldLabel");
                        if (GUILayout.Button("一键打包新版本资源", "LargeButton", GUILayout.Height(50f)))
                        {
                            int option = EditorUtility.DisplayDialogComplex("确认", string.Format("是否更新 res_ver: {0} ?", GetCurResVersion()), "版本号不变", "版本号+1", "取消");
                            if (option < 2)
                            {
                                EditorApplication.delayCall += () =>
                                {
                                    OneKeyBuildGameRes(true, option > 0);
                                };
                                _rightTab = 0;
                            }
                        }
                        if (GUILayout.Button("打包新版本资源(不拷贝到StreamingAssets)", "LargeButton", GUILayout.Height(50f)))
                        {
                            int option = EditorUtility.DisplayDialogComplex("确认", string.Format("是否更新 res_ver: {0} ?", GetCurResVersion()), "版本号不变", "版本号+1", "取消");
                            if (option < 2)
                            {
                                EditorApplication.delayCall += () =>
                                {
                                    OneKeyBuildGameRes(false, option > 0);
                                };
                                _rightTab = 0;
                            }
                        }
                        EditorGUILayout.Space();

                        if (GUILayout.Button("加密所有LuaCode", "LargeButton", GUILayout.Height(50f)))
                        {
                            if (EditorUtility.DisplayDialog("确认", "是否重新加密所有LuaCode?", "继续", "取消"))
                            {
                                EditorApplication.delayCall += EncryptLuaCode;
                                _rightTab = 0;
                            }
                        }
                        EditorGUILayout.Space();

                        if (GUILayout.Button("生成包内整包资源", "LargeButton", GUILayout.Height(50f)))
                        {
                            EditorApplication.delayCall += () =>
                            {
                                string backupDir = EditorUtility.OpenFolderPanel("选择资源备份目录", BackupExportPath, "");
                                CopyToStreamingAssets(backupDir);
                            };
                        }
                        EditorGUILayout.Space();
                    }
                    EditorGUILayout.EndVertical();

                    //补丁
                    EditorGUILayout.BeginVertical("HelpBox");
                    {
                        GUILayout.Label("补丁", "BoldLabel");
                        if (GUILayout.Button("生成最新版本补丁包", "LargeButton", GUILayout.Height(50f)))
                        {
                            EditorApplication.delayCall += GenLastestPatch;
                        }
                        EditorGUILayout.Space();
                        if (GUILayout.Button("生成到指定版本补丁包\n如：0.0.1 → 0.0.6", "LargeButton", GUILayout.Height(50f)))
                        {
                            EditorApplication.delayCall += GenPatch;
                        }
                        EditorGUILayout.Space();
                        if (GUILayout.Button("递进生成所有版本补丁包\n如：0.1 → 0.2 → 0.3", "LargeButton", GUILayout.Height(50f)))
                        {
                            EditorApplication.delayCall += GenPatchLink;
                        }
                        EditorGUILayout.Space();
                    }
                    EditorGUILayout.EndVertical();
                }
                EditorGUILayout.EndScrollView();
            }
            EditorGUILayout.EndVertical(); //Left Cotent End

            GUILayout.Space(5f);
            EditorGUILayout.BeginVertical(GUILayout.MinWidth(500f)); //Right Cotent Begin
            {
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("BuildCache目录", "ButtonLeft"))
                {
                    EditorApplication.delayCall += () => { OpenDirectory(BundleBuildCachePath); };
                }
                if (GUILayout.Button("Backup目录", "ButtonMid"))
                {
                    EditorApplication.delayCall += () => { OpenDirectory(BackupExportPath); };
                }
                if (GUILayout.Button("Patch目录", "ButtonRight"))
                {
                    EditorApplication.delayCall += () => { OpenDirectory(PatchExportPath); };
                }
                EditorGUILayout.EndHorizontal();

                if (_rightTab == 0)
                {
                    DrawBundleNamePanel();
                }
                else if (_rightTab == 1)
                {
                    //DrawResConfigPanel();
                }
                else if (_rightTab == 2)
                {
                    //DrawPatchInfoPanel();
                }
            }
            EditorGUILayout.EndVertical(); //Right Cotent End

            GUILayout.Space(10f);
            EditorGUILayout.EndHorizontal();
        }

        #endregion

        #region BundleNamePanel

        private string _projectSearchFilter = "";
        private string _selectedProjectBundleName = "";
        private Vector2 _bundleNamePanelScroll;
        private Vector2 _bundleNamePanelDetailScroll;
        private static List<string> _projectBundleNames; //当前项目里BundleName分组信息
        private static int _projectBundleNameTotalCount; //当前项目里BundleName总数
        private static HashSet<string> _unusedBundleNameSet; //当前项目里未使用的BundleName集合

        private static void RefreshBundleNameData()
        {
            var unusedBundleNames = AssetDatabase.GetUnusedAssetBundleNames();
            _unusedBundleNameSet = new HashSet<string>(unusedBundleNames);

            var bundleNames = AssetDatabase.GetAllAssetBundleNames();
            _projectBundleNameTotalCount = bundleNames.Length;
            _projectBundleNames = bundleNames.ToList();
        }

        private void DrawBundleNamePanel()
        {
            // Search field
            GUILayout.BeginHorizontal();
            {
                var after = EditorGUILayout.TextField("", _projectSearchFilter, "SearchTextField");

                if (GUILayout.Button("", "SearchCancelButton", GUILayout.Width(18f)))
                {
                    after = "";
                    GUIUtility.keyboardControl = 0;
                }

                if (_projectSearchFilter != null && _projectSearchFilter != after)
                {
                    _projectSearchFilter = after;
                }
            }
            GUILayout.EndHorizontal();

            //BundleName列表
            if (_projectBundleNames != null && _projectBundleNames.Count > 0)
            {
                int hitCount = 0;
                EditorGUILayout.BeginVertical("HelpBox", GUILayout.Height(300f));
                {
                    GUILayout.Label("Total BundleName:" + _projectBundleNameTotalCount + " Unused BundleName:" + _unusedBundleNameSet.Count, "LargeLabel");
                    EditorGUILayout.Space();
                    _bundleNamePanelScroll = EditorGUILayout.BeginScrollView(_bundleNamePanelScroll);

                    for (int i = 0; i < _projectBundleNames.Count; i++)
                    {
                        var bundleName = _projectBundleNames[i];
                        if (!String.IsNullOrEmpty(_projectSearchFilter) &&
                            bundleName.IndexOf(_projectSearchFilter, StringComparison.OrdinalIgnoreCase) < 0)
                            continue;
                        hitCount++;
                        GUILayout.Space(-1f);
                        GUI.backgroundColor = _selectedProjectBundleName == bundleName
                            ? Color.white
                            : new Color(0.8f, 0.8f, 0.8f);
                        GUILayout.BeginHorizontal(GUILayout.MinHeight(20f));
                        GUI.backgroundColor = Color.white;

                        //编号
                        GUILayout.Label((i + 1).ToString(), GUILayout.Width(40f));

                        GUI.color = _unusedBundleNameSet.Contains(bundleName) ? Color.yellow : Color.white;
                        if (GUILayout.Button(bundleName, GUILayout.Height(20f)))
                        {
                            _selectedProjectBundleName = bundleName;
                        }
                        GUI.color = Color.white;

                        GUILayout.EndHorizontal();
                    }
                    EditorGUILayout.EndScrollView();
                    EditorGUILayout.Space();
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.BeginVertical("HelpBox");
                GUILayout.Label("Search Result:\n" + hitCount + "/" + _projectBundleNames.Count + "  ", "LargeLabel");
                EditorGUILayout.EndVertical();
            }
            else
            {
                GUILayout.Box("ResInfoList is null");
            }

            //绘制选中BundleName详细信息
            _bundleNamePanelDetailScroll = DrawBundleNameDetailPanel(_selectedProjectBundleName,
                _bundleNamePanelDetailScroll);
        }

        private Vector2 DrawBundleNameDetailPanel(string bundleName, Vector2 scrollPos)
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos, "HelpBox");
            if (!String.IsNullOrEmpty(bundleName))
            {
                if (_unusedBundleNameSet != null && _unusedBundleNameSet.Contains(bundleName))
                {
                    GUI.color = Color.yellow;
                    GUILayout.Label("该BundleName未在项目中使用");
                    GUI.color = Color.white;
                }
                GUILayout.Label("=====================================");
                var assetPaths = AssetDatabase.GetAssetPathsFromAssetBundle(bundleName);
                GUILayout.Label("Include Asset Path:" + assetPaths.Length);
                for (var i = 0; i < assetPaths.Length; ++i)
                {
                    EditorGUILayout.BeginHorizontal();
                    if (GUILayout.Button("选中", GUILayout.Width(50f)))
                    {
                        Selection.activeObject = AssetDatabase.LoadMainAssetAtPath(assetPaths[i]);
                    }
                    GUILayout.Label(assetPaths[i]);
                    EditorGUILayout.EndHorizontal();
                }

                GUILayout.Label("=====================================");
            }
            EditorGUILayout.EndScrollView();
            return scrollPos;
        }

        #endregion


        #region Handle BundleName
        [MenuItem("ResourceModule/Setup BundleName")]
        public static void SetupSelectionBundleName()
        {
            foreach (var asset in Selection.objects)
            {
                SetupAssetBundleName(AssetDatabase.GetAssetPath(asset));
            }
        }

        private static void SetupAssetBundleName(string assetPath)
        {
            if (string.IsNullOrEmpty(assetPath)) return;

            if (assetPath.StartsWith(ResourceModuleConfig.BundleResoucesDir) || assetPath.StartsWith(ResourceModuleConfig.GameResourcesDir))
            {
                var importer = AssetImporter.GetAtPath(assetPath);
                if (importer != null)
                {
                    string bundleName = assetPath.Replace(ResourceModuleConfig.BundleResoucesDir + "/", "").Replace(ResourceModuleConfig.GameResourcesDir + "/", "") + ResourceModuleConfig.AssetBundleExt;
                    if (ChangeAssetBundleName(importer, bundleName))
                    {
                        Debug.LogFormat("Setup BundleName:\n{0}\n{1}", assetPath, bundleName);
                    }
                }
            }
        }

        private static void CleanUpBundleName(bool cleanAll)
        {
            AssetDatabase.RemoveUnusedAssetBundleNames();
            if (cleanAll)
            {
                var allBundleNames = AssetDatabase.GetAllAssetBundleNames();
                for (int i = 0; i < allBundleNames.Length; i++)
                {
                    var bundleName = allBundleNames[i];
                    AssetDatabase.RemoveAssetBundleName(bundleName, true);
                    EditorUtility.DisplayProgressBar("移除所有资源BundleName中", String.Format(" {0} / {1} ", i, allBundleNames.Length),
                        i / (float)allBundleNames.Length);
                }
                EditorUtility.ClearProgressBar();
            }

            RefreshBundleNameData();
            AssetDatabase.Refresh();
            EditorUtility.DisplayDialog("确认", cleanAll ? "清空所有资源BundleName成功" : "清空未使用的BundleName成功", "Yes");
        }

        private static void FixBundleNames()
        {
            var sb = new StringBuilder();
            int changeCount = 0;
            string bundleExt = ResourceModuleConfig.AssetBundleExt;

            var pathUIDs = AssetDatabase.FindAssets("b:");
            foreach (var uid in pathUIDs)
            {
                string path = AssetDatabase.GUIDToAssetPath(uid);
                var importer = AssetImporter.GetAtPath(path);

                if (importer != null)
                {
                    var bundleName = importer.assetBundleName;
                    if (!bundleName.EndsWith(bundleExt))
                    {
                        string fixName = bundleName + bundleExt;
                        if (ChangeAssetBundleName(importer, fixName))
                        {
                            changeCount++;
                            sb.AppendLine(bundleName + "=>" + fixName);
                        }
                    }
                }
            }
            Debug.LogFormat("Make all asset name successs! ChangeCount:{0}\n{1}", changeCount, sb.ToString());

            AssetDatabase.Refresh();
            RefreshBundleNameData();
        }

        public static bool ChangeAssetBundleName(AssetImporter importer, string bundleName)
        {
            if (importer == null) return false;

            bundleName = bundleName.ToLower();
            var oldBundleName = importer.assetBundleName;
            if (oldBundleName == bundleName) return false;

            importer.SetAssetBundleNameAndVariant(bundleName, null);
            return true;
        }
        #endregion

        #region Build AssetBundle
        public static Version GetCurResVersion()
        {
            var resVer = new Version("0.0.1");
            if (File.Exists(ResVerPath))
            {
                string txt = File.ReadAllText(ResVerPath);
                return new Version(txt);
            }
            else
            {
                File.WriteAllText(ResVerPath, resVer.ToString());
            }

            return resVer;
        }

        public static void UpdateResVersion()
        {
            var curVer = GetCurResVersion();
            var nextVer = new Version(curVer.Major, curVer.Minor, curVer.Build + 1);
            string text = nextVer.ToString();
            File.WriteAllText(ResVerPath, text);
            Debug.LogFormat("[UpdateResVersion] res_ver: {0}", text);
        }

        /// <summary>
        /// 生成加密后的LuaCode，并设置好BundleName
        /// </summary>
        private static void EncryptLuaCode()
        {
            var begin = DateTime.Now;
            if (Directory.Exists(LuaPackedPath))
            {
                Directory.Delete(LuaPackedPath, true);
            }
            Directory.CreateDirectory(LuaPackedPath);

            LuaCodePacker.PackLuaCode();
            AssetDatabase.Refresh(ImportAssetOptions.ForceSynchronousImport);
            var importer = AssetImporter.GetAtPath(LuaPackedPath);
            if (importer != null)
            {
                importer.SetAssetBundleNameAndVariant(ResourceModuleConfig.LuaBundleName + ResourceModuleConfig.AssetBundleExt, null);
            }
            Debug.LogFormat("[EncryptLuaCode] {0}s", (DateTime.Now - begin).TotalSeconds);
        }

        private static AssetBundleManifest BuildBundles()
        {
            var stopwatch = Stopwatch.StartNew();
            var buildDir = BundleBuildCachePath;

            CreateDirectory(buildDir);
            var buildOptions = BuildAssetBundleOptions.DeterministicAssetBundle | BuildAssetBundleOptions.ChunkBasedCompression;
            var manifest = BuildPipeline.BuildAssetBundles(buildDir, buildOptions, EditorUserBuildSettings.activeBuildTarget);
            stopwatch.Stop();
            var elapsed = stopwatch.Elapsed;
            Debug.LogFormat("[BuildBundles]总耗时:{0:00}:{1:00}:{2:00}:{3:00}\n", elapsed.Hours, elapsed.Minutes,
                elapsed.Seconds, elapsed.Milliseconds / 10);
            return manifest;
        }

        /// <summary>
        /// 备份当前版本BuildCache资源包
        /// </summary>
        private static string BackupBuildCache(AssetBundleManifest manifest)
        {
            if (manifest == null) return null;

            var curResVer = GetCurResVersion();
            string platformName = ResManager.GetBuildPlatformName();
            var buildDir = BundleBuildCachePath;
            var backupDir = BackupExportPath + "/res_ver_" + curResVer;
            //先删除之前已存在的资源目录
            if (Directory.Exists(backupDir))
            {
                Directory.Delete(backupDir, true);
                Debug.Log("旧版本资源目录已存在,将清空后重新备份:" + backupDir);
            }
            Directory.CreateDirectory(backupDir);

            //先备份AssetBundleManifest信息
            CopyFile(buildDir + "/" + platformName, backupDir + "/" + platformName, true);

            var sb = new StringBuilder();
            var allAssetBundles = manifest.GetAllAssetBundles();
            for (var index = 0; index < allAssetBundles.Length; index++)
            {
                var assetBundle = allAssetBundles[index];
                string bundlePath = buildDir + "/" + assetBundle;
                sb.AppendLine("[Backup Bundle]=========================");
                sb.AppendLine(assetBundle);
                sb.AppendLine("Hash:" + manifest.GetAssetBundleHash(assetBundle));
                uint crc;
                BuildPipeline.GetCRCForAssetBundle(bundlePath, out crc);
                sb.AppendLine("CRC:" + crc);

                CopyFile(bundlePath, backupDir + "/" + assetBundle, true);
                int finishCount = index + 1;
                EditorUtility.DisplayProgressBar("备份AssetBundle中",
                    String.Format(" {0} / {1} ", finishCount, allAssetBundles.Length),
                    finishCount / (float)allAssetBundles.Length);
                sb.AppendLine();
            }
            EditorUtility.ClearProgressBar();

            CopyFile(ResVerPath, backupDir + "/" + ResourceModuleConfig.ResVerFileName);
            Debug.Log(sb);
            return backupDir;
        }

        public static AssetBundleManifest LoadAssetBundleManifest(string path)
        {
            var assetBundle = AssetBundle.LoadFromFile(path);
            var manifest = assetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            assetBundle.Unload(false);
            return manifest;
        }

        public static void CopyToStreamingAssets(string backupDir)
        {
            if (string.IsNullOrEmpty(backupDir)) return;

            string platformName = ResManager.GetBuildPlatformName();
            string manifestPath = backupDir + "/" + platformName;
            var manifest = LoadAssetBundleManifest(manifestPath);
            if (manifest != null)
            {
                string packageDir = Application.streamingAssetsPath + "/" + platformName;
                //先删除之前已存在的资源目录
                if (Directory.Exists(packageDir))
                {
                    Directory.Delete(packageDir, true);
                }
                Directory.CreateDirectory(packageDir);

                //先拷贝AssetBundleManifest
                CopyFile(manifestPath, packageDir + "/" + platformName);

                var sb = new StringBuilder();
                sb.AppendLine("[CopyToStreamingAssets]=========================");
                var allAssetBundles = manifest.GetAllAssetBundles();
                for (var index = 0; index < allAssetBundles.Length; index++)
                {
                    var assetBundle = allAssetBundles[index];
                    string bundlePath = backupDir + "/" + assetBundle;
                    sb.AppendLine(bundlePath);
                    CopyFile(bundlePath, packageDir + "/" + assetBundle);
                    int finishCount = index + 1;
                    EditorUtility.DisplayProgressBar("拷贝AssetBundle中",
                        String.Format(" {0} / {1} ", finishCount, allAssetBundles.Length),
                        finishCount / (float)allAssetBundles.Length);
                }
                EditorUtility.ClearProgressBar();

                CopyFile(backupDir + "/" + ResourceModuleConfig.ResVerFileName, packageDir + "/" + ResourceModuleConfig.ResVerFileName);
                Debug.Log(sb);
                AssetDatabase.Refresh();
            }
        }

        public static void OneKeyBuildGameRes(bool copyToPackage, bool updateVersion = false)
        {
            SlientMode = true;
            if (updateVersion)
            {
                UpdateResVersion();
            }
            EncryptLuaCode();
            var manifest = BuildBundles();
            var backupDir = BackupBuildCache(manifest);
            if (copyToPackage)
                CopyToStreamingAssets(backupDir);
            SlientMode = false;
        }
        #endregion

        #region Patch

        public class PatchInfo
        {
            public string version; //当前版本
            public string nextVer; //升级版本
            public string hash;    //补丁包MD5值
            public long fileSize;  //补丁包文件大小
            public string fileName;

            public Dictionary<string, string> fileHash;
        }

        /// <summary>
        /// 生成升级到指定版本的补丁包
        /// 如果选择同版本目录，则只生成patch_info.json,一般用于生成最新版本的patch_info.json，用于热更流程中判断是否升级到最新版本
        /// </summary>
        public static void GenPatch()
        {
            string oldResDir = EditorUtility.OpenFolderPanel("oldRes目录", BackupExportPath, "");
            string newResDir = EditorUtility.OpenFolderPanel("newRes目录", BackupExportPath, "");

            try
            {
                if (GenPatchPackages(oldResDir, newResDir))
                {
                    EditorUtility.DisplayDialog("提示", "生成补丁包成功！", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("提示", "生成补丁包出错！", "OK");
                }
            }
            catch (Exception)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("提示", "生成补丁包出错！", "OK");
                throw;
            }
        }

        /// <summary>
        /// 读取Backup目录下所有版本信息
        /// </summary>
        /// <returns></returns>
        public static List<Version> ReadBackupDirVerInfo()
        {
            var folders = Directory.GetDirectories(AssetBundleBuilder.BackupExportPath);
            var allResVerList = new List<Version>();

            foreach (var folder in folders)
            {
                var resVer = GetResVersion(folder);
                if (resVer != null)
                {
                    allResVerList.Add(resVer);
                }
            }
            allResVerList.Sort();
            return allResVerList;
        }

        public static void GenLastestPatch()
        {
            try
            {
                var allResVerList = ReadBackupDirVerInfo();
                if (allResVerList.Count > 1)
                {
                    var preVer = allResVerList[allResVerList.Count - 2];
                    var oldResDir = BackupExportPath + "/res_ver_" + preVer;
                    var lastestVer = allResVerList[allResVerList.Count - 1];
                    var newResDir = BackupExportPath + "/res_ver_" + lastestVer;

                    GenPatchPackages(oldResDir, newResDir);
                    GenLastestPatchInfo(lastestVer);
                }
                else
                {
                    throw new Exception("Backup目录数据异常，请检查");
                }

                EditorUtility.DisplayDialog("提示", "生成补丁包成功！", "OK");
            }
            catch (Exception)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("提示", "生成补丁包出错！", "OK");
                throw;
            }
        }

        /// <summary>
        /// 遍历Backup目录递进生成所有版本补丁包
        /// </summary>
        public static void GenPatchLink()
        {
            try
            {
                var allResVerList = ReadBackupDirVerInfo();
                for (var i = 0; i < allResVerList.Count; i++)
                {
                    var resVer = allResVerList[i];
                    var oldResDir = BackupExportPath + "/res_ver_" + resVer;
                    var nextVer = i + 1 < allResVerList.Count ? allResVerList[i + 1] : allResVerList[i];
                    var newResDir = BackupExportPath + "/res_ver_" + nextVer;

                    GenPatchPackages(oldResDir, newResDir);
                }

                EditorUtility.DisplayDialog("提示", "生成补丁包成功！", "OK");
            }
            catch (Exception)
            {
                EditorUtility.ClearProgressBar();
                EditorUtility.DisplayDialog("提示", "生成补丁包出错！", "OK");
                throw;
            }
        }

        public static void GenLastestPatchInfo(Version lastestVer)
        {
            //最新版本资源，无需生成补丁包
            var patchInfo = new PatchInfo
            {
                version = lastestVer.ToString(),
                nextVer = lastestVer.ToString(),
            };

            string jsonPath = string.Format("{0}/{1}_{2}.json", PatchExportPath, ResourceModuleConfig.PatchInfoPrefix, lastestVer);
            FileExt.SaveJsonObj(patchInfo, jsonPath, false, true);
        }

        public static bool GenPatchPackages(string oldResDir, string newResDir)
        {
            if (!Directory.Exists(oldResDir) || !Directory.Exists(newResDir)) return false;

            var oldResVer = GetResVersion(oldResDir);
            var newResVer = GetResVersion(newResDir);
            if (oldResVer == null || newResVer == null)
            {
                Debug.LogError("资源版本信息加载失败");
                return false;
            }

            if (oldResVer > newResVer)
            {
                Debug.LogErrorFormat("oldResVer > newResVer:{0} {1}", oldResVer, newResVer);
                return false;
            }
            else if (oldResVer == newResVer)
            {
                //最新版本资源，无需生成补丁包
                GenLastestPatchInfo(newResVer);
                return true;
            }

            string platformName = ResManager.GetBuildPlatformName();
            var oldManifest = LoadAssetBundleManifest(oldResDir + "/" + platformName);
            var newManifest = LoadAssetBundleManifest(newResDir + "/" + platformName);

            var oldBundleNames = oldManifest.GetAllAssetBundles();
            var newBundleNames = newManifest.GetAllAssetBundles();
            var allNameSet = new HashSet<string>(oldBundleNames);
            allNameSet.UnionWith(newBundleNames);

            var patchedNames = new List<string>();
            var sb = new StringBuilder();
            int index = 0;
            foreach (var bundleName in allNameSet)
            {
                string oldResPath = oldResDir + "/" + bundleName;
                string newResPath = newResDir + "/" + bundleName;
                bool oldResExists = File.Exists(oldResPath);
                bool newResExists = File.Exists(newResPath);
                if (oldResExists && newResExists)
                {
                    //新旧版本都存在的，读取Bundle的Hash和CRC判断
                    var oldHash = oldManifest.GetAssetBundleHash(bundleName);
                    var newHash = newManifest.GetAssetBundleHash(bundleName);
                    if (oldHash != newHash)
                    {
                        patchedNames.Add(bundleName);
                        sb.AppendFormat("[Patched] 新旧Hash值不一致：{0}\n{1}\n{2}\n\n", bundleName, oldHash, newHash);
                    }
                    else
                    {
                        uint oldCRC;
                        uint newCRC;
                        BuildPipeline.GetCRCForAssetBundle(oldResPath, out oldCRC);
                        BuildPipeline.GetCRCForAssetBundle(newResPath, out newCRC);
                        if (oldCRC != newCRC)
                        {
                            patchedNames.Add(bundleName);
                            sb.AppendFormat("[Patched] 新旧CRC值不一致：{0}\n{1}\n{2}\n\n", bundleName, oldCRC, newCRC);
                        }
                        else
                        {
                            sb.AppendFormat("[ignored] 资源未变更：{0}\n{1}\n{2}\n\n", bundleName, newHash, newCRC);
                        }
                    }
                }
                else if (!oldResExists && newResExists)
                {
                    patchedNames.Add(bundleName);
                    sb.AppendFormat("[Patched] 新增版本资源：{0}\n\n", bundleName);
                }
                else if (oldResExists)
                {
                    sb.AppendFormat("[ignored] 新版本资源被移除或者BundleName变更：{0}\n\n", bundleName);
                }
                else
                {
                    throw new Exception(string.Format("[Error] Backup目录文件被异常删除：{0}\n\n", newResPath));
                }

                index++;
                EditorUtility.DisplayProgressBar("对比Bundle包版本信息", String.Format(" {0} / {1} ", index, allNameSet.Count),
                    index / (float)allNameSet.Count);
            }
            Debug.Log(sb);

            if (patchedNames.Count > 0)
            {
                string patchDir = string.Format("{0}/patch_{1}_{2}", PatchExportPath, oldResVer, newResVer);
                EditorUtility.DisplayProgressBar("生成补丁包中", patchDir, 0);
                if (Directory.Exists(patchDir))
                    Directory.Delete(patchDir, true);

                //AssetBundleManifest也需要拷贝
                Dictionary<string, string> fileHash = new Dictionary<string, string>();
                patchedNames.Add(ResManager.GetBuildPlatformName());
                foreach (var patchedName in patchedNames)
                {
                    string patchFile = patchDir + "/" + patchedName;
                    CopyFile(newResDir + "/" + patchedName, patchFile);
                    fileHash[patchedName] = SHA256Hashing.HashFile(patchFile);
                }

                string zipFile = patchDir + ".zip";
                ZipTool.CompressFolder(zipFile, patchDir);
                string zipHash = MD5Hashing.HashFile(zipFile);
                long fileSize = new FileInfo(zipFile).Length;
                string finalZipFile = string.Format("{0}-{1}.zip", patchDir, zipHash);
                if (File.Exists(finalZipFile))
                {
                    File.Delete(finalZipFile);
                }
                File.Move(zipFile, finalZipFile);
                var patchInfo = new PatchInfo
                {
                    version = oldResVer.ToString(),
                    nextVer = newResVer.ToString(),
                    hash = zipHash,
                    fileSize = fileSize,
                    fileName = Path.GetFileName(finalZipFile),
                    fileHash = fileHash,
                };

                string jsonPath = string.Format("{0}/{1}_{2}.json", PatchExportPath, ResourceModuleConfig.PatchInfoPrefix, oldResVer);
                FileExt.SaveJsonObj(patchInfo, jsonPath, false, true);
                Directory.Delete(patchDir, true);
                Debug.Log("Build Patch Package:" + finalZipFile);
            }
            else
            {
                Debug.Log("Build Patch Package Skip!!!");
            }
            EditorUtility.ClearProgressBar();

            return true;
        }
        #endregion

        #region Helper Func

        public static Version GetResVersion(string resDir)
        {
            string path = resDir + "/" + ResourceModuleConfig.ResVerFileName;
            if (File.Exists(path))
            {
                string verStr = File.ReadAllText(path);
                return new Version(verStr);
            }
            return null;
        }

        public static void CopyFile(string source, string dest, bool copyManifest = false)
        {
            CreateDirectory(Path.GetDirectoryName(dest));

            FileUtil.CopyFileOrDirectory(source, dest);
            if (copyManifest)
                FileUtil.CopyFileOrDirectory(source + ".manifest", dest + ".manifest");
        }

        public static void DisplayDialog(string hint)
        {
            if (!SlientMode)
                EditorUtility.DisplayDialog("提示", hint, "Yes");
            Debug.Log(hint);
        }

        private static void CreateDirectory(string dir)
        {
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
        }

        public static void OpenDirectory(string path)
        {
            var dir = Path.GetFullPath(path);
            if (Directory.Exists(dir))
                Process.Start(dir);
            //if (EditorUtility.DisplayDialog("确认", "是否打开导出资源目录？", "打开", "取消"))
            //{
            //	var dir = Path.GetFullPath(path);
            //	if (Directory.Exists(dir))
            //		Process.Start(dir);
            //	else
            //	{
            //		EditorUtility.DisplayDialog("提示", "不存在:" + path + " 目录", "OK");
            //	}
            //}
        }

        public static string StripHashSuffix(string bundleName)
        {
#if BUNDLE_APPEND_HASH
            int index = bundleName.LastIndexOf('_');
            if (index > 0)
            {
                return bundleName.Substring(0, index);
            }
            return bundleName;
#else
            return bundleName;
#endif
        }

        /// <summary>
        ///     unixTimestamp单位为毫秒
        /// </summary>
        /// <param name="unixTimestamp"></param>
        /// <returns></returns>
        public static DateTime UnixTimeStampToDateTime(long unixTimestamp)
        {
            DateTime dateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return dateTime.AddTicks(unixTimestamp * 10000).ToLocalTime();
        }

        /// <summary>
        ///     返回的unixTimestamp单位为毫秒
        /// </summary>
        /// <param name="dateTime"></param>
        /// <returns></returns>
        public static long DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (dateTime - new DateTime(1970, 1, 1, 0, 0, 0, 0).ToLocalTime()).Ticks / 10000;
        }
        #endregion

        private static string BundleBuildCachePath
        {
            get { return PlatformExportPath + "/" + ResManager.BuildPlatformName; }
        }

        public static string BackupExportPath
        {
            get { return PlatformExportPath + "/Backup"; }
        }

        public static string ResVerPath
        {
            get { return ResourceModuleConfig.BundleResoucesDir + "/" + ResManager.BuildPlatformName + "_" + ResourceModuleConfig.ResVerFileName; }
        }

        public static string LuaPackedPath
        {
            get { return ResourceModuleConfig.BundleResoucesDir + "/Lua/LuaPackedCode"; }
        }

        public static string PatchExportPath
        {
            get { return PlatformExportPath + "/Patch"; }
        }

        public static string PlatformExportPath
        {
            get { return BuildExportRoot + "/" + ResManager.BuildPlatformName; }
        }
    }
}
