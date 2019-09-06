## ResourceModule
参考[KEngine](https://github.com/mr-kelly/KEngine)的资源加载模块，进行了精简和接口调整，还加上打包和生成热更补丁包的功能。

### Features
- ResManager封装了各资源加载模式下的接口，如：Resource，AssetDatabase，AssetBundle
- 支持Android下StreamingAsset包内资源同步读取与查询
- WebRequestMgr对UnityWebRequest的封装，加入失败重试，异步回调等
- ResourceModuleOptions面板设置当前加载模式，便于AssetBundle资源的加载调试
- ResourceDebuger对所加载资源的追踪，在Hierarchy视图中直观展示
- AssetBundleBuilder负责资源打包流程，包括：设置BundleName，BuildBundle，版本资源备份，生成热更补丁包等

### 引用外部库
- [SharpZipLib](https://github.com/icsharpcode/SharpZipLib)
- [LitJson](https://github.com/LitJSON/litjson)
- [DOTween](http://dotween.demigiant.com/documentation.php)
- [UIEffect](https://github.com/mob-sakai/UIEffect)