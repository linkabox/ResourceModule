
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ResourceModule
{
    /// <summary>
    /// 资源路径优先级，优先使用
    /// </summary>
    public enum ResPathPriorityType
    {
        Invalid,

        /// <summary>
        /// 忽略PersitentDataPath, 优先寻找Resources或StreamingAssets路径 (取决于ResourcePathType)
        /// </summary>
        InAppPathPriority,

        /// <summary>
        /// 尝试在Persistent目錄尋找，找不到再去StreamingAssets,
        /// 这一般用于进行热更新版本号判断后，设置成该属性
        /// </summary>
        PersistentDataPathPriority,
    }

    /// <summary>
    /// 用于GetResourceFullPath函数，返回的类型判断
    /// </summary>
    public enum ResPathType
    {
        Invalid,
        InApp,
        InDocument,
    }

    public class ResManager : MonoBehaviour
    {
        static ResManager()
        {
            InitResourcePath();
        }

        /// <summary>
        /// Initialize the path of AssetBundles store place ( Maybe in PersitentDataPath or StreamingAssetsPath )
        /// </summary>
        /// <returns></returns>
        static void InitResourcePath()
        {
            BundlePathRoot = GetBuildPlatformName() + "/";
            DocumentDirUrl = FileProtocol + DocumentDirPath;
            DocumentBundlePath = DocumentDirPath + BundlePathRoot;

            switch (Application.platform)
            {
                case RuntimePlatform.WindowsEditor:
                case RuntimePlatform.OSXEditor:
                    {
                        AppPathUrl = FileProtocol + Application.dataPath;
                        PackagePathUrl = FileProtocol + Application.streamingAssetsPath + "/";
                        PackagePath = Application.streamingAssetsPath + "/";
                    }
                    break;
                case RuntimePlatform.WindowsPlayer:
                case RuntimePlatform.OSXPlayer:
                    {
                        string streamingAssetsPath = Application.streamingAssetsPath.Replace('\\', '/');
                        AppPathUrl = FileProtocol + Application.dataPath;
                        PackagePathUrl = FileProtocol + streamingAssetsPath + "/";
                        PackagePath = Application.streamingAssetsPath + "/";
                    }
                    break;
                case RuntimePlatform.Android:
                    {
                        AppPathUrl = Application.streamingAssetsPath;
                        PackagePathUrl = string.Concat(Application.streamingAssetsPath, "/");
                        PackagePath = string.Concat(Application.dataPath, "!/assets/");
                    }
                    break;
                case RuntimePlatform.IPhonePlayer:
                    {
                        // MacOSX下，带空格的文件夹，空格字符需要转义成%20
                        AppPathUrl = Uri.EscapeUriString(FileProtocol + Application.streamingAssetsPath);
                        PackagePathUrl = string.Format("{0}/", AppPathUrl);
                        PackagePath = Application.streamingAssetsPath + "/";
                    }
                    break;
                default:
                    {
                        Debug.Assert(false);
                    }
                    break;
            }
        }

        public enum LoadingLogLevel
        {
            None,
            ShowTime,
            ShowDetail,
        }

        private static ResManager _instance;

        public static ResManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject resMgr = GameObject.Find("_ResourceModule_");
                    if (resMgr == null)
                    {
                        resMgr = new GameObject("_ResourceModule_");
                        GameObject.DontDestroyOnLoad(resMgr);
                    }

                    _instance = resMgr.AddComponent<ResManager>();
                }
                return _instance;
            }
        }

        public static LoadingLogLevel LogLevel = LoadingLogLevel.ShowTime;

        public static string BuildPlatformName
        {
            get { return GetBuildPlatformName(); }
        } // ex: IOS, Android, AndroidLD

        /// <summary>
        /// On Windows, file protocol has a strange rule that has one more slash
        /// </summary>
        /// <value>string, file protocol string</value>
        public static string FileProtocol
        {
            get
            {
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
                return "file:///";
#else
				return "file://";
#endif
            }
        }

        /// <summary>
        /// StreamingAssetsPath/Bundles/Android/ etc.
        /// WWW的读取，是需要Protocol前缀的
        /// </summary>
        public static string PackagePathUrl { get; private set; }

        public static string PackagePath { get; private set; }

        /// <summary>
        /// Bundles/Android/ etc... no prefix for streamingAssets
        /// </summary>
        public static string BundlePathRoot { get; private set; }

        /// <summary>
        /// 獲取app數據目錄，可寫，同Application.PersitentDataPath，但在windows平台時為了避免www類中文目錄無法讀取問題，單獨實現
        /// </summary>
        /// <value></value>
        public static string PersistentDataPath
        {
            get { return Application.persistentDataPath; }
        }

        public static string AppPathUrl { get; private set; }

        public static string DocumentDirPath
        {
            get
            {
                return PersistentDataPath + "/"; // 各平台通用
            }
        }

        public static string DocumentDirUrl { get; private set; }
        public static string DocumentBundlePath { get; private set; }  //热更资源包目录

        /// <summary>
        /// 是否優先找下載的資源?還是app本身資源優先. 优先下载的资源，即采用热更新的资源
        /// </summary>
        public static ResPathPriorityType ResourcePathPriorityType =
            ResPathPriorityType.PersistentDataPathPriority;

        // 检查资源是否存在
        public static bool ContainsResourceUrl(string resourceUrl)
        {
            string fullPath;
            return GetResourceFullPath(resourceUrl, false, out fullPath, false) != ResPathType.Invalid;
        }

        /// <summary>
        /// 完整路径，www加载
        /// </summary>
        /// <param name="url"></param>
        /// <param name="inAppPathType"></param>
        /// <param name="withFileProtocol">是否带有file://前缀</param>
        /// <param name="isLog"></param>
        /// <returns></returns>
        public static string GetResourceFullPath(string url, bool withFileProtocol = true, bool isLog = true)
        {
            string fullPath;
            if (GetResourceFullPath(url, withFileProtocol, out fullPath, isLog) != ResPathType.Invalid)
                return fullPath;

            return null;
        }

        public static bool IsResourceExist(string url)
        {
            string fullPath;
            var hasDocUrl = TryGetDocumentResourceUrl(url, false, out fullPath);

            var hasInAppUrl = TryGetInAppStreamingUrl(url, false, out fullPath);
            return hasDocUrl || hasInAppUrl;
        }

        /// <summary>
        /// 根据相对路径，获取到StreamingAssets完整路径，或Resources中的路径
        /// </summary>
        /// <param name="url"></param>
        /// <param name="fullPath"></param>
        /// <param name="inAppPathType"></param>
        /// <param name="isLog"></param>
        /// <returns></returns>
        public static ResPathType GetResourceFullPath(string url, bool withFileProtocol, out string fullPath,
             bool isLog = true)
        {
            if (string.IsNullOrEmpty(url))
                Debug.LogErrorFormat("尝试获取一个空的资源路径！");

            string docUrl;
            bool hasDocUrl = TryGetDocumentResourceUrl(url, withFileProtocol, out docUrl);

            string inAppUrl;
            bool hasInAppUrl;
            {
                hasInAppUrl = TryGetInAppStreamingUrl(url, withFileProtocol, out inAppUrl);
            }

            if (ResourcePathPriorityType == ResPathPriorityType.PersistentDataPathPriority) // 優先下載資源模式
            {
                if (hasDocUrl)
                {
                    if (Application.isEditor)
                        Debug.LogWarningFormat("[Use PersistentDataPath] {0}", docUrl);
                    fullPath = docUrl;
                    return ResPathType.InDocument;
                }
                // 優先下載資源，但又沒有下載資源文件！使用本地資源目錄 
            }

            if (!hasInAppUrl) // 连本地资源都没有，直接失败吧 ？？ 沒有本地資源但又遠程資源？竟然！!?
            {
                if (isLog)
                    Debug.LogErrorFormat("[Not Found] StreamingAssetsPath Url Resource: {0}", url);
                fullPath = null;
                return ResPathType.Invalid;
            }

            fullPath = inAppUrl; // 直接使用本地資源！

            return ResPathType.InApp;
        }

        public static ResPathType GetBundleFullPath(string url, out string fullPath, bool isLog = true)
        {
            if (string.IsNullOrEmpty(url))
                Debug.LogErrorFormat("尝试获取一个空的资源路径！");

            bool hasDocUrl = TryGetDocumentResourceUrl(url, false, out fullPath);
            // 优先检索persistentDataPath,读取热更资源
            if (ResourcePathPriorityType == ResPathPriorityType.PersistentDataPathPriority)
            {
                if (hasDocUrl)
                {
#if UNITY_EDITOR
                    Debug.LogFormat("[Use PersistentDataPath] {0}", fullPath);
#endif
                    return ResPathType.InDocument;
                }
            }

            bool hasInAppUrl;
            if (Application.platform == RuntimePlatform.Android)
                hasInAppUrl = TryGetInAppStreamingUrl(url, true, out fullPath);
            else
                hasInAppUrl = TryGetInAppStreamingUrl(url, false, out fullPath);
            //包内包外都没有报错提示
            if (!hasInAppUrl)
            {
                if (isLog)
                    Debug.LogErrorFormat("[Not Found] StreamingAssetsPath Url Resource: {0}", url);
                fullPath = null;
                return ResPathType.Invalid;
            }
            return ResPathType.InApp;
        }

        public static float EditorModeLoadDelay
        {
            get
            {
                return PlayerPrefs.GetFloat(ResourceModuleConfig.EditorModeLoadDelay, 0.25f);
            }
        }

        public static bool IsEdiotrMode
        {
            get
            {
#if UNITY_EDITOR
                return PlayerPrefs.GetInt(ResourceModuleConfig.IsEdiotrMode, 1) != 0;
#else
				return false;
#endif
            }
        }

        /// <summary>
        /// (not android ) only! Android资源不在目录！
        /// Editor返回文件系统目录，运行时返回StreamingAssets目录
        /// </summary>
        /// <param name="url"></param>
        /// <param name="withFileProtocol">是否带有file://前缀</param>
        /// <param name="newUrl"></param>
        /// <returns></returns>
        public static bool TryGetInAppStreamingUrl(string url, bool withFileProtocol, out string newUrl)
        {
            if (withFileProtocol)
                newUrl = PackagePathUrl + url;
            else
                newUrl = PackagePath + url;

#if UNITY_ANDROID
            // Android平台下StreamingAssetsPath，在apk包里面，使用原生插件做文件检查
            if (!ResAndroidPlugin.IsAssetExists(url))
                return false;
#else
			if (!File.Exists(PackagePath + url))
			{
				return false;
			}
#endif
            return true;
        }

        /// <summary>
        /// 可被WWW读取的Resource路径
        /// </summary>
        /// <param name="url"></param>
        /// <param name="withFileProtocol">是否带有file://前缀</param>
        /// <param name="newUrl"></param>
        /// <returns></returns>
        public static bool TryGetDocumentResourceUrl(string url, bool withFileProtocol, out string newUrl)
        {
            if (withFileProtocol)
                newUrl = DocumentDirUrl + url;
            else
                newUrl = DocumentDirPath + url;

            return File.Exists(DocumentDirPath + url);
        }

        private void Awake()
        {
            if (_instance != null)
                Debug.Assert(_instance == this);

            //InvokeRepeating("CheckGcCollect", 0f, 3f);
            if (Debug.isDebugBuild)
            {
                Debug.LogFormat("====================================================================================");
                Debug.LogFormat("ResourceManager AppPathUrl: {0}", AppPathUrl);
                Debug.LogFormat("ResourceManager PackagePathUrl: {0}", PackagePathUrl);
                Debug.LogFormat("ResourceManager PackagePath: {0}", PackagePath);
                Debug.LogFormat("ResourceManager DocumentDirUrl: {0}", DocumentDirUrl);
                Debug.LogFormat("ResourceManager DocumentBundlePath: {0}", DocumentBundlePath);
                Debug.LogFormat("====================================================================================");
            }
        }

        private void Update()
        {
            AbstractResourceLoader.CheckGcCollect();
        }

        /// <summary>
        /// Different platform's assetBundles is incompatible.
        /// CosmosEngine put different platform's assetBundles in different folder.
        /// Here, get Platform name that represent the AssetBundles Folder.
        /// </summary>
        /// <returns>Platform folder Name</returns>
        public static string GetBuildPlatformName()
        {
            string buildPlatformName = "Windows"; // default
#if UNITY_EDITOR
            var buildTarget = UnityEditor.EditorUserBuildSettings.activeBuildTarget;
            switch (buildTarget)
            {
                case UnityEditor.BuildTarget.StandaloneOSX:
                    buildPlatformName = "MacOS";
                    break;
                case UnityEditor.BuildTarget.StandaloneWindows:
                case UnityEditor.BuildTarget.StandaloneWindows64:
                    buildPlatformName = "Windows";
                    break;
                case UnityEditor.BuildTarget.Android:
                    buildPlatformName = "Android";
                    break;
                case UnityEditor.BuildTarget.iOS:
                    buildPlatformName = "iOS";
                    break;
                default:
                    Debug.Assert(false);
                    break;
            }
#else
			switch (Application.platform)
			{
				case RuntimePlatform.OSXPlayer:
					buildPlatformName = "MacOS";
					break;
				case RuntimePlatform.Android:
					buildPlatformName = "Android";
					break;
				case RuntimePlatform.IPhonePlayer:
					buildPlatformName = "iOS";
					break;
				case RuntimePlatform.WindowsPlayer:
					buildPlatformName = "Windows";
					break;
				default:
					Debug.Assert(false);
					break;
			}
#endif
            return buildPlatformName;
        }

        #region Load Scene
        public static SceneResolveLoader LoadScene(string sceneName, string package = null, LoadSceneMode mode = LoadSceneMode.Single, AbstractResourceLoader.LoaderDelgate callback = null)
        {
            var request = SceneResolveLoader.Load(sceneName, package, mode, callback, LoaderMode.Sync);
            return request;
        }

        public static SceneResolveLoader LoadSceneAsync(string sceneName, string package = null, LoadSceneMode mode = LoadSceneMode.Single, AbstractResourceLoader.LoaderDelgate callback = null)
        {
            var request = SceneResolveLoader.Load(sceneName, package, mode, callback);
            return request;
        }

        /// <summary>
        /// 一般只有Addtive方式加载的Scene需要使用到异步Unload
        /// </summary>
        /// <param name="sceneName"></param>
        /// <param name="callback"></param>
        public static void UnloadSceneAsync(string sceneName, SceneResolveLoader.OnUnloadScene callback = null)
        {
            SceneResolveLoader.UnloadSceneAsync(sceneName, callback);
        }
        #endregion

        #region Load Asset

        /// <summary>
        /// Load Async Asset Bundle
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback">cps style async</param>
        /// <returns></returns>
        public static AssetResolveLoader LoadAssetAsync(string path, string package = null, Type type = null, AssetResolveLoader.OnLoadAsset callback = null)
        {
            var request = AssetResolveLoader.Load(path, package, type, callback);
            return request;
        }

        public static AssetResolveLoader LoadAllAssetAsync(string path, string package = null, Type type = null, AssetResolveLoader.OnLoadAssetList callback = null)
        {
            var request = AssetResolveLoader.LoadAll(path, package, type, callback);
            return request;
        }

        /// <summary>
        /// load asset bundle immediatly
        /// </summary>
        /// <param name="path"></param>
        /// <param name="callback"></param>
        /// <returns></returns>
        public static AssetResolveLoader LoadAsset(string path, string package = null, Type type = null, AssetResolveLoader.OnLoadAsset callback = null)
        {
            var request = AssetResolveLoader.Load(path, package, type, callback, LoaderMode.Sync);
            return request;
        }

        public static AssetResolveLoader LoadAllAsset(string path, string package = null, Type type = null, AssetResolveLoader.OnLoadAssetList callback = null)
        {
            var request = AssetResolveLoader.LoadAll(path, package, type, callback, LoaderMode.Sync);
            return request;
        }

        #endregion

        /// <summary>
        /// check file exists of streamingAssets. On Android will use plugin to do that.
        /// </summary>
        /// <param name="path">relative path,  when file is "file:///android_asset/test.txt", the pat is "test.txt"</param>
        /// <returns></returns>
        public static bool IsStreamingAssetsExists(string path)
        {
#if UNITY_ANDROID && !UNITY_EDITOR
			return ResAndroidPlugin.IsAssetExists(path);
#else
            return File.Exists(Path.Combine(Application.streamingAssetsPath, path));
#endif
        }

        /// <summary>
        /// Load file from streamingAssets. On Android will use plugin to do that.
        /// </summary>
        /// <param name="path">relative path,  when file is "file:///android_asset/test.txt", the pat is "test.txt"</param>
        /// <returns></returns>
        public static byte[] LoadByteFromStreamingAssets(string path)
        {
            if (!IsStreamingAssetsExists(path))
                throw new Exception("Not exist StreamingAssets path: " + path);

#if UNITY_ANDROID
            return ResAndroidPlugin.GetAssetBytes(path);
#else
            return File.ReadAllBytes(Path.Combine(Application.streamingAssetsPath, path));
#endif
        }

        public static string LoadTextFromStreamingAssets(string path)
        {
            if (!IsStreamingAssetsExists(path))
                throw new Exception("Not exist StreamingAssets path: " + path);

#if UNITY_ANDROID
            return ResAndroidPlugin.GetAssetString(path);
#else
            return File.ReadAllText(Path.Combine(Application.streamingAssetsPath, path));
#endif
        }

        public static string ReadAllText(string path)
        {
            string fullPath;
            var getResPathType = GetBundleFullPath(path, out fullPath);
            if (getResPathType == ResPathType.Invalid) return null;

#if UNITY_ANDROID
            if (getResPathType == ResPathType.InApp)
            {
                return ResAndroidPlugin.GetAssetString(path);
            }
#endif
            return File.ReadAllText(fullPath);
        }


        public static byte[] ReadAllBytes(string path)
        {
            var getResPathType = GetBundleFullPath(path, out var fullPath);
            if (getResPathType == ResPathType.Invalid) return null;

#if UNITY_ANDROID
            if (getResPathType == ResPathType.InApp)
            {
                return ResAndroidPlugin.GetAssetBytes(path);
            }
#endif
            return File.ReadAllBytes(fullPath);
        }

        internal static void LogRequest(string resType, string resPath)
        {
            if (LogLevel < LoadingLogLevel.ShowDetail)
                return;

            Debug.LogFormat("[Request] {0}, {1}", resType, resPath);
        }

        internal static void LogLoadTime(string resType, LoaderMode loadMode, string resPath, System.DateTime begin)
        {
            if (LogLevel < LoadingLogLevel.ShowTime)
                return;

            Debug.LogFormat("[Load] {0}, {1}, {2}, {3}s", resType, loadMode, resPath, (System.DateTime.Now - begin).TotalSeconds);
        }

        /// <summary>
        /// Collect all KEngine's resource unused loaders
        /// </summary>
        public static void Collect()
        {
            while (AbstractResourceLoader.UnUsesLoaders.Count > 0)
                AbstractResourceLoader.DoGarbageCollect();

            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            Debug.Log("ResManager GC Collect!");
        }

        /// <summary>
        /// 游戏重启时，释放ResManager加载的所有资源
        /// </summary>
        public static void Dispose()
        {
#if UNITY_EDITOR
            KDebuggerObjectTool.Dispose();
#endif
            AbstractResourceLoader.DisposeAll();
            BundleManifest = null;

            AssetBundle.UnloadAllAssetBundles(false);
            Resources.UnloadUnusedAssets();
            System.GC.Collect();
            Debug.Log("ResManager Dispose!");
        }

        public static AssetBundleManifest BundleManifest { get; private set; }

        internal static void PreLoadManifest()
        {
            if (BundleManifest != null)
                return;

            var mainAssetBundle = AssetBundleLoader.LoadBundle(BuildPlatformName);
            var manifest = mainAssetBundle.LoadAsset<AssetBundleManifest>("AssetBundleManifest");
            mainAssetBundle.Unload(false);

            BundleManifest = manifest;
        }

        public static string[] FindAssetBundleName(string pattern)
        {
            PreLoadManifest();

            var resutl = new List<string>();
            var allBundleNames = BundleManifest.GetAllAssetBundles();
            foreach (var bundleName in allBundleNames)
            {
                if (System.Text.RegularExpressions.Regex.IsMatch(bundleName, pattern))
                {
                    resutl.Add(bundleName);
                }
            }

            return resutl.ToArray();
        }

        public static string GetAssetBundleHash(string bundleName)
        {
            PreLoadManifest();

            var hash = BundleManifest.GetAssetBundleHash(bundleName);
            if (hash.isValid)
            {
                return hash.ToString();
            }
            return null;
        }
    }
}
