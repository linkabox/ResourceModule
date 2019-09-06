
namespace ResourceModule
{
    public class ResourceModuleConfig
    {
        //编辑器下需要打包为Bundle的资源根目录
        public const string BundleResoucesDir = "Assets/BundleResources";
        public const string GameResourcesDir = "Assets/GameRes";
        public const string AssetBundleExt = ".bytes";
        public const string LuaBundleName = "lua";
        public const string ResVerFileName = "res_ver.txt";
        public const string PatchInfoPrefix = "patch_info";

        public const string IsEdiotrMode = "IsEdiotrMode";
        public const string EditorModeLoadDelay = "EditorModeLoadDelay";
        public const string IsLoadAssetBundle = "IsLoadAssetBundle";
        public const string LoadPackedLuaCode = "LoadPackedLuaCode";
    }
}
