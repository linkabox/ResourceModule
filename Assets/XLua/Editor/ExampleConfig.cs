/*
 * Tencent is pleased to support the open source community by making xLua available.
 * Copyright (C) 2016 THL A29 Limited, a Tencent company. All rights reserved.
 * Licensed under the MIT License (the "License"); you may not use this file except in compliance with the License. You may obtain a copy of the License at
 * http://opensource.org/licenses/MIT
 * Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific language governing permissions and limitations under the License.
*/

using System.Collections.Generic;
using System;
using XLua;
using System.Reflection;
using System.Linq;
using UnityEngine;

//配置的详细介绍请看Doc下《XLua的配置.doc》
public static class ExampleConfig
{
    /// <summary>
    /// dotween的扩展方法在lua中调用
    /// </summary>
    [LuaCallCSharp]
    [ReflectionUse]
    public static List<Type> DOTweenExport = new List<Type>()
    {
        typeof(DG.Tweening.AutoPlay),
        typeof(DG.Tweening.AxisConstraint),
        typeof(DG.Tweening.Ease),
        typeof(DG.Tweening.LogBehaviour),
        typeof(DG.Tweening.LoopType),
        typeof(DG.Tweening.PathMode),
        typeof(DG.Tweening.PathType),
        typeof(DG.Tweening.RotateMode),
        typeof(DG.Tweening.ScrambleMode),
        typeof(DG.Tweening.TweenType),
        typeof(DG.Tweening.UpdateType),

        typeof(DG.Tweening.DOTween),
        typeof(DG.Tweening.DOVirtual),
        typeof(DG.Tweening.EaseFactory),
        typeof(DG.Tweening.Tweener),
        typeof(DG.Tweening.Tween),
        typeof(DG.Tweening.Sequence),
        typeof(DG.Tweening.TweenParams),
        typeof(DG.Tweening.Core.ABSSequentiable),

        typeof(DG.Tweening.Core.TweenerCore<Vector3, Vector3, DG.Tweening.Plugins.Options.VectorOptions>),

        typeof(DG.Tweening.TweenCallback),
        typeof(DG.Tweening.TweenExtensions),
        typeof(DG.Tweening.TweenSettingsExtensions),
        typeof(DG.Tweening.ShortcutExtensions),
        typeof(DG.Tweening.DOTweenModuleUI),
        typeof(DG.Tweening.DOTweenModuleSprite),

        typeof(TweenExt),
		//dotween pro 的功能
		typeof(DG.Tweening.DOTweenPath),
        typeof(DG.Tweening.DOTweenVisualManager),
    };

    //lua中要使用到C#库的配置，比如C#标准库，或者Unity API，第三方库等。
    [LuaCallCSharp]
    public static List<Type> CustomLuaCallCSharp = new List<Type>()
    {
        typeof(System.Collections.Generic.List<int>),
        typeof(Action<string>),
        typeof(Action<float>),
        typeof(System.IO.File),
        typeof(System.IO.Directory),
        typeof(RectTransform),
        typeof(RectTransform.Edge),
        typeof(RectTransform.Axis),
        typeof(UnityEngine.UI.Graphic),
        typeof(UnityEngine.UI.Image),
        typeof(UnityEngine.UI.RawImage),
        typeof(UnityEngine.UI.Button),
        typeof(UnityEngine.EventSystems.EventTrigger),
        typeof(UnityEngine.EventSystems.EventTriggerType),
        typeof(UnityEngine.SceneManagement.SceneManager),
        typeof(UnityEngine.SceneManagement.LoadSceneMode),
        typeof(UnityEngine.AsyncOperation),
        typeof(UnityEngine.AssetBundleRequest),
        typeof(UnityEngine.ResourceRequest),
        typeof(UnityEngine.Networking.UnityWebRequest),
        typeof(UnityEngine.Networking.DownloadHandler),
        typeof(UnityEngine.Networking.DownloadHandlerAssetBundle),
        typeof(UnityEngine.Networking.DownloadHandlerAudioClip),
        typeof(UnityEngine.Networking.DownloadHandlerBuffer),
        typeof(UnityEngine.Networking.DownloadHandlerFile),
        typeof(UnityEngine.Networking.DownloadHandlerScript),
        typeof(UnityEngine.Networking.DownloadHandlerTexture),
        typeof(UnityEngine.Networking.UnityWebRequestAsyncOperation),
        typeof(UnityEngine.NetworkReachability),
        typeof(UnityEngine.EventSystems.PointerEventData.InputButton),
        typeof(UnityEngine.EventSystems.PointerEventData.FramePressState),
        typeof(UnityEngine.RenderTextureFormat),
        typeof(UnityEngine.RenderTextureReadWrite),
        typeof(UnityEngine.RenderTextureMemoryless),
        typeof(UnityEngine.CameraClearFlags),
        typeof(UnityEngine.BatteryStatus),
        typeof(UnityEngine.NetworkReachability),

		//Custom
        typeof(ULogger),
        typeof(LitJson.JsonMapper),
        typeof(LitJson.JsonData),
        typeof(Localization),
        typeof(UILocalizeText),
        typeof(AnimationEventTrigger),
        typeof(UIDragger),
        typeof(GlobalTouchTrigger),
        typeof(HighlightClicker),
        typeof(UnityEngine.UI.PageScrollRect),
        typeof(UnityEngine.UI.PageScrollRect.ScrollerSnappedDelegate),
        typeof(UICenterOnChild),
        typeof(UICenterOnChild.OnCenterCallback),
        typeof(UIDragger.DragEvent),
        typeof(UGUIHUDAnchor),
        typeof(TrailRendererClearer),
        //typeof(FPSLabel),
        typeof(UnityEngine.Video.VideoClip),
        typeof(UnityEngine.Video.VideoPlayer),
        typeof(UIVideoImage),
        typeof(UIVideoImage.VideoPlayerEvent),
        typeof(UIVideoImage.VideoPlayerErrorEvent),

        //UGUIExt
        typeof(UnityEngine.UI.Extensions.ScrollSnapBase),
        typeof(UnityEngine.UI.Extensions.HorizontalScrollSnap),
        typeof(UnityEngine.UI.Extensions.VerticalScrollSnap),
        typeof(UnityEngine.UI.Extensions.ScrollRectEx),
        typeof(UnityEngine.UI.Extensions.ToolTips),

		//EnhancedScroller
		typeof(EnhancedUI.EnhancedScroller.EnhancedScroller),
        typeof(EnhancedUI.EnhancedScroller.EnhancedScrollerCellView),
        typeof(EnhancedUI.EnhancedScroller.EnhancedScroller.ScrollDirectionEnum),
        typeof(EnhancedUI.EnhancedScroller.EnhancedScroller.CellViewPositionEnum),
        typeof(EnhancedUI.EnhancedScroller.EnhancedScroller.ScrollbarVisibilityEnum),
        typeof(EnhancedUI.EnhancedScroller.EnhancedScroller.TweenType),
        typeof(EnhancedUI.EnhancedScroller.EnhancedScroller.LoopJumpDirectionEnum),
        typeof(LuaEnhancedScrollerController),

		//Coffee.UIExtensions
        typeof(Coffee.UIExtensions.ButtonEx),
        typeof(Coffee.UIExtensions.Unmask),

        typeof(Coffee.UIExtensions.BlurMode),
        typeof(Coffee.UIExtensions.ColorMode),
        typeof(Coffee.UIExtensions.EffectArea),
        typeof(Coffee.UIExtensions.ShadowStyle),
        typeof(Coffee.UIExtensions.EffectMode),

        typeof(Coffee.UIExtensions.EffectPlayer),
        typeof(Coffee.UIExtensions.UIDissolve),
        typeof(Coffee.UIExtensions.UIEffect),
        typeof(Coffee.UIExtensions.UIEffectCapturedImage),
        typeof(Coffee.UIExtensions.UIFlip),
        typeof(Coffee.UIExtensions.UIGradient),
        typeof(Coffee.UIExtensions.UIHsvModifier),
        typeof(Coffee.UIExtensions.UIShadow),
        typeof(Coffee.UIExtensions.UIShiny),
        typeof(Coffee.UIExtensions.UITransitionEffect),

		//ResourceModule
		typeof(ResourceModule.ResManager.LoadingLogLevel),
        typeof(ResourceModule.ResPathType),
        typeof(ResourceModule.ResPathPriorityType),

        //FMOD
        //typeof(FMODUnity.RuntimeManager),
    };

    //C#静态调用Lua的配置（包括事件的原型），仅可以配delegate，interface
    [CSharpCallLua]
    public static List<Type> CustomCSharpCallLua = new List<Type>() {
        typeof(Action),
        typeof(Action<string>),
        typeof(Action<double>),
        typeof(Action<float>),
        typeof(Action<bool>),
        typeof(Action<int>),
        typeof(Func<double, double, double>),
        typeof(UnityEngine.Events.UnityAction),
        typeof(UnityEngine.Events.UnityAction<bool>),
        typeof(UnityEngine.Events.UnityAction<int>),
        typeof(UnityEngine.Events.UnityAction<float>),
        typeof(UnityEngine.Events.UnityAction<string>),
        typeof(UnityEngine.Events.UnityAction<UnityEngine.Object>),
        typeof(UnityEngine.Events.UnityAction<Vector2>),
        typeof(UnityEngine.Events.UnityAction<Vector3>),
        typeof(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.BaseEventData>),
        typeof(UnityEngine.Events.UnityAction<UnityEngine.EventSystems.PointerEventData>),
        typeof(UnityEngine.Events.UnityAction<UnityEngine.Video.VideoPlayer>),
        typeof(UnityEngine.Video.VideoPlayer.EventHandler),
        typeof(UnityEngine.Video.VideoPlayer.ErrorEventHandler),
        typeof(UnityEngine.Video.VideoPlayer.FrameReadyEventHandler),
        typeof(UnityEngine.Video.VideoPlayer.TimeEventHandler),
        typeof(System.Collections.IEnumerator),

        //UGUIExt
        typeof(Coffee.UIExtensions.ButtonEx.PressEvent),
        typeof(UnityEngine.UI.Extensions.ScrollSnapBase.SelectionChangeEndEvent),
        typeof(UnityEngine.UI.Extensions.ScrollSnapBase.SelectionChangeStartEvent),
        typeof(UnityEngine.UI.Extensions.ScrollSnapBase.SelectionPageChangedEvent),

        //DOTween
        typeof(DG.Tweening.TweenCallback),
        typeof(DG.Tweening.TweenCallback<int>),
        typeof(DG.Tweening.TweenCallback<float>),

		//EnhancedScroller
		typeof(EnhancedUI.EnhancedScroller.CellViewVisibilityChangedDelegate),
        typeof(EnhancedUI.EnhancedScroller.CellViewWillRecycleDelegate),
        typeof(EnhancedUI.EnhancedScroller.ScrollerScrolledDelegate),
        typeof(EnhancedUI.EnhancedScroller.ScrollerSnappedDelegate),
        typeof(EnhancedUI.EnhancedScroller.ScrollerScrollingChangedDelegate),
        typeof(EnhancedUI.EnhancedScroller.ScrollerTweeningChangedDelegate),
        typeof(EnhancedUI.EnhancedScroller.EnhancedScrollerCellView.CellViewRefreshDelegate),
        typeof(LuaEnhancedScrollerController.DataCountGetterDelegate),
        typeof(LuaEnhancedScrollerController.CellSizeGetterDelegate),
        typeof(LuaEnhancedScrollerController.CellDataSetterDelegate),
        typeof(LuaEnhancedScrollerController.PullScrollDelegate),
        typeof(LuaEnhancedScrollerController.PullRefreshDelegate),

		//ResourceModule
		typeof(ResourceModule.AbstractResourceLoader.LoaderDelgate),
        typeof(ResourceModule.AssetBundleLoader.OnLoadBundle),
        typeof(ResourceModule.AssetResolveLoader.OnLoadAsset),
        typeof(ResourceModule.AssetResolveLoader.OnLoadAssetList),
        typeof(ResourceModule.WebRequestMgr.OnRequestSuccess),
        typeof(ResourceModule.WebRequestMgr.OnRequestError),
        typeof(ResourceModule.WebRequestMgr.OnRequestProgress),
        //typeof(ResourceModule.FMODLoader.OnLoadBank),
    };

    /***************如果你全lua编程，可以参考这份自动化配置***************/
    //--------------begin 纯lua编程配置参考----------------------------
    static readonly List<string> unity_filters = new List<string> {
        "HideInInspector", "ExecuteInEditMode",
        "AddComponentMenu", "ContextMenu",
        "RequireComponent", "DisallowMultipleComponent",
        "SerializeField", "AssemblyIsEditorAssembly",
        "Attribute", "Types",
        "UnitySurrogateSelector", "TrackedReference",
        "TypeInferenceRules", "FFTWindow",
        "RPC", "Network", "MasterServer",
        "BitStream", "HostData",
        "ConnectionTesterStatus", "GUI", "EventType",
        "EventModifiers", "FontStyle", "TextAlignment",
        "TextEditor", "TextEditorDblClickSnapping",
        "TextGenerator", "TextClipping", "Gizmos",
        "ADBannerView", "ADInterstitialAd",
        "Android", "Tizen", "jvalue",
        "iPhone", "iOS", "Windows", "CalendarIdentifier",
        "CalendarUnit", "CalendarUnit",
        "ClusterInput", "FullScreenMovieControlMode",
        "FullScreenMovieScalingMode", "Handheld",
        "LocalNotification", "NotificationServices",
        "RemoteNotificationType", "RemoteNotification",
        "SamsungTV", "TextureCompressionQuality",
        "TouchScreenKeyboardType", "TouchScreenKeyboard",
        "MovieTexture", "UnityEngineInternal",
        "Terrain", "Tree", "SplatPrototype",
        "DetailPrototype", "DetailRenderMode",
        "MeshSubsetCombineUtility", "AOT", "Social", "Enumerator",
        "SendMouseEvents", "Cursor", "Flash", "ActionScript",
        "OnRequestRebuild", "Ping",
        "ShaderVariantCollection", "SimpleJson.Reflection",
        "CoroutineTween", "GraphicRebuildTracker",
        "Advertisements", "UnityEditor", "WSA",
        "EventProvider", "Apple",
        "ClusterInput", "Motion",
        "UnityEngine.UI.ReflectionMethodsCache", "NativeLeakDetection",
        "NativeLeakDetectionMode", "WWWAudioExtensions", "UnityEngine.Experimental",
        "HoloLens",
        "Joint",
        "UnityEngine.Analytics",
        "UnityEngine.Advertisements",
        "UnityEngine.Purchasing",
        "UnityEngine.SpatialTracking",
        "UnityEngine.Timeline",
        "UnityEngine.TestTools",
        "UnityEngine.XR",
        "UnityEngine.Diagnostics",
        "UnityEngine.CrashReportHandler",
        "UnityEngine.Collections",
        "UnityEngine.Audio",
        "UnityEngine.AI",
        "UnityEngine.Tilemaps",
        "UnityEngine.Caching",
        "UnityEngine.Cache",
        "UnityEngine.Rendering",
        "UnityEngine.Playables",
        "UnityEngine.Accessibility",
        //"Collider",
        //"CharacterController",
        "Cloth",
        "Cubemap",
        "Human",
        "Avatar",
        //"Collision",
        "Wheel",
        "Wind",
        "Video",
        "AnimatorControllerParameter",
        "Effector",
        "Probe",
        "WebCam",
        "ScalableBufferManager",
        "UnityEngine.UI.Extensions",
        "LOD",
        "Navigation",
        "Occlusion",
        "tvOS",
        "UnityEngine.Jobs",
        "UnityEngine.TextCore",
        "DrivenRectTransformTracker"
    };

    private static readonly List<string> custom_filters = new List<string>
    {
        "XLua",
        "RawDataConfig",
        "Coffee.UIExtensions",
        "SerializableDictionary",
        "_2DxFX",
        "_2dxFX",
        "Test",
        "Driver",
        "Packer",
        "EnhancedScrollerDemos",
        "EnhancedScollerDemos",
        "CastleBurnRes",
        "LitJson",
        "Debugger",
        "RawDataGenerator",
        "GPUSkinningSampler",
        "UnityEngine.UI.Extensions",
        "MapGizmos",
        "OrbCreationExtensions",
        "Importer",
        "ParticleSystemForceField",
        "Editor",
        "Ejoysdk",
        "GameLauncher",
    };

    private static readonly List<string> assembly_filters = new List<string>
    {
        "TerrainModule",
        "TerrainPhysicsModule",
        "WindModule",
        "TilemapModule",
        "ClothModule",
        "ARModule",
        "VRModule",
        "XRModule",
        "UnityAnalyticsModule",
    };

    static bool isExcluded(Type type, List<string> filters)
    {
        var fullName = type.FullName;
        for (int i = 0; i < filters.Count; i++)
        {
            if (fullName != null && fullName.Contains(filters[i]))
            {
                return true;
            }
        }
        return false;
    }

    static bool isExcludedDLL(Assembly lib, List<string> filters)
    {
        var fullName = lib.FullName;
        for (int i = 0; i < filters.Count; i++)
        {
            if (fullName != null && fullName.Contains(filters[i]))
            {
                return true;
            }
        }
        //Debug.Log(fullName);
        return false;
    }

    [LuaCallCSharp]
    public static IEnumerable<Type> LuaCallCSharp
    {
        get
        {
            var unityTypes = (from assembly in AppDomain.CurrentDomain.GetAssemblies()
                              where !(assembly.ManifestModule is System.Reflection.Emit.ModuleBuilder)
                                    && !isExcludedDLL(assembly, assembly_filters)
                              from type in assembly.GetExportedTypes()
                              where type.Namespace != null && type.Namespace.StartsWith("UnityEngine")
                                        && type.BaseType != typeof(MulticastDelegate)
                                        && !type.IsInterface
                                        && !type.IsEnum
                                        && !type.IsSubclassOf(typeof(Attribute))
                                        && !type.IsSubclassOf(typeof(UnityEditor.Editor))
                                        && !type.IsSubclassOf(typeof(UnityEditor.EditorWindow))
                                        && !isExcluded(type, unity_filters)
                              select type);

            Assembly[] customAssemblys = new Assembly[] {
                typeof(GameMain).Assembly,
                typeof(TMPro.TMP_Text).Assembly,
            };
            var customTypes = (from assembly in customAssemblys.Select(s => s)
                               from type in assembly.GetExportedTypes()
                               where type.BaseType != typeof(MulticastDelegate)
                                     && !type.IsInterface
                                     && !type.IsEnum
                                     && !type.IsSubclassOf(typeof(Attribute))
                                     && !isExcluded(type, custom_filters)
                               select type);

            return unityTypes.Concat(customTypes);
        }
    }

    private static readonly List<string> delegate_filters = new List<string>
    {
        "OnRequestRebuild"
    };
    //自动把LuaCallCSharp涉及到的delegate加到CSharpCallLua列表，后续可以直接用lua函数做callback
    [CSharpCallLua]
    public static List<Type> CSharpCallLua
    {
        get
        {
            var lua_call_csharp = LuaCallCSharp;
            var delegate_types = new List<Type>();
            var flag = BindingFlags.Public | BindingFlags.Instance
                | BindingFlags.Static | BindingFlags.IgnoreCase | BindingFlags.DeclaredOnly;
            foreach (var field in (from type in lua_call_csharp select type).SelectMany(type => type.GetFields(flag)))
            {
                if (typeof(Delegate).IsAssignableFrom(field.FieldType) && !isExcluded(field.FieldType, delegate_filters))
                {
                    delegate_types.Add(field.FieldType);
                }
            }

            foreach (var method in (from type in lua_call_csharp select type).SelectMany(type => type.GetMethods(flag)))
            {
                if (typeof(Delegate).IsAssignableFrom(method.ReturnType) && !isExcluded(method.ReturnType, delegate_filters))
                {
                    delegate_types.Add(method.ReturnType);
                }
                foreach (var param in method.GetParameters())
                {
                    var paramType = param.ParameterType.IsByRef ? param.ParameterType.GetElementType() : param.ParameterType;
                    if (typeof(Delegate).IsAssignableFrom(paramType) && !isExcluded(paramType, delegate_filters))
                    {
                        delegate_types.Add(paramType);
                    }
                }
            }
            return delegate_types.Distinct().ToList();
        }
    }
    //--------------end 纯lua编程配置参考----------------------------

    /***************热补丁可以参考这份自动化配置***************/
    //[Hotfix]
    //static IEnumerable<Type> HotfixInject
    //{
    //    get
    //    {
    //        return (from type in Assembly.Load("Assembly-CSharp").GetExportedTypes()
    //                           where type.Namespace == null || !type.Namespace.StartsWith("XLua")
    //                           select type);
    //    }
    //}
    //--------------begin 热补丁自动化配置-------------------------
    //static bool hasGenericParameter(Type type)
    //{
    //    if (type.IsGenericTypeDefinition) return true;
    //    if (type.IsGenericParameter) return true;
    //    if (type.IsByRef || type.IsArray)
    //    {
    //        return hasGenericParameter(type.GetElementType());
    //    }
    //    if (type.IsGenericType)
    //    {
    //        foreach (var typeArg in type.GetGenericArguments())
    //        {
    //            if (hasGenericParameter(typeArg))
    //            {
    //                return true;
    //            }
    //        }
    //    }
    //    return false;
    //}

    static bool typeHasEditorRef(Type type)
    {
        if (type.Namespace != null && (type.Namespace == "UnityEditor" || type.Namespace.StartsWith("UnityEditor.")))
        {
            return true;
        }
        if (type.IsNested)
        {
            return typeHasEditorRef(type.DeclaringType);
        }
        if (type.IsByRef || type.IsArray)
        {
            return typeHasEditorRef(type.GetElementType());
        }
        if (type.IsGenericType)
        {
            foreach (var typeArg in type.GetGenericArguments())
            {
                if (typeHasEditorRef(typeArg))
                {
                    return true;
                }
            }
        }
        return false;
    }

    static bool delegateHasEditorRef(Type delegateType)
    {
        if (typeHasEditorRef(delegateType)) return true;
        var method = delegateType.GetMethod("Invoke");
        if (method == null)
        {
            return false;
        }
        if (typeHasEditorRef(method.ReturnType)) return true;
        return method.GetParameters().Any(pinfo => typeHasEditorRef(pinfo.ParameterType));
    }

    // 配置某Assembly下所有涉及到的delegate到CSharpCallLua下，Hotfix下拿不准那些delegate需要适配到lua function可以这么配置
    //[CSharpCallLua]
    //static IEnumerable<Type> AllDelegate
    //{
    //    get
    //    {
    //        BindingFlags flag = BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public;
    //        List<Type> allTypes = new List<Type>();
    //        var allAssemblys = new Assembly[]
    //        {
    //            Assembly.Load("Assembly-CSharp")
    //        };
    //        foreach (var t in (from assembly in allAssemblys from type in assembly.GetTypes() select type))
    //        {
    //            var p = t;
    //            while (p != null)
    //            {
    //                allTypes.Add(p);
    //                p = p.BaseType;
    //            }
    //        }
    //        allTypes = allTypes.Distinct().ToList();
    //        var allMethods = from type in allTypes
    //                         from method in type.GetMethods(flag)
    //                         select method;
    //        var returnTypes = from method in allMethods
    //                          select method.ReturnType;
    //        var paramTypes = allMethods.SelectMany(m => m.GetParameters()).Select(pinfo => pinfo.ParameterType.IsByRef ? pinfo.ParameterType.GetElementType() : pinfo.ParameterType);
    //        var fieldTypes = from type in allTypes
    //                         from field in type.GetFields(flag)
    //                         select field.FieldType;
    //        return (returnTypes.Concat(paramTypes).Concat(fieldTypes)).Where(t => t.BaseType == typeof(MulticastDelegate) && !hasGenericParameter(t) && !delegateHasEditorRef(t)).Distinct();
    //    }
    //}
    //--------------end 热补丁自动化配置-------------------------

    //黑名单
    [BlackList]
    public static List<List<string>> BlackList = new List<List<string>>()  {
                new List<string>(){"System.Xml.XmlNodeList", "ItemOf"},
                new List<string>(){"UnityEngine.WWW", "movie"},
    #if UNITY_WEBGL
                new List<string>(){"UnityEngine.WWW", "threadPriority"},
    #endif
                new List<string>(){"UnityEngine.Texture2D", "alphaIsTransparency"},
                new List<string>(){"UnityEngine.Security", "GetChainOfTrustValue"},
                new List<string>(){"UnityEngine.CanvasRenderer", "onRequestRebuild"},
                new List<string>(){"UnityEngine.Light", "areaSize"},
                new List<string>(){"UnityEngine.Light", "lightmapBakeType"},
                new List<string>(){"UnityEngine.Light", "shadowRadius"},
                new List<string>(){"UnityEngine.Light", "shadowAngle"},
                new List<string>(){"UnityEngine.Light", "SetLightDirty"},
                new List<string>(){"UnityEngine.ParticleSystemForceField", "FindAll"},
                new List<string>(){"UnityEngine.WWW", "MovieTexture"},
                new List<string>(){"UnityEngine.WWW", "GetMovieTexture"},
                new List<string>(){"UnityEngine.AnimatorOverrideController", "PerformOverrideClipListCleanup"},
    #if !UNITY_WEBPLAYER
                new List<string>(){"UnityEngine.Application", "ExternalEval"},
    #endif
                new List<string>(){"UnityEngine.GameObject", "networkView"}, //4.6.2 not support
                new List<string>(){"UnityEngine.Component", "networkView"},  //4.6.2 not support
                new List<string>(){"System.IO.FileInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.FileInfo", "SetAccessControl", "System.Security.AccessControl.FileSecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "GetAccessControl", "System.Security.AccessControl.AccessControlSections"},
                new List<string>(){"System.IO.DirectoryInfo", "SetAccessControl", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "CreateSubdirectory", "System.String", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"System.IO.DirectoryInfo", "Create", "System.Security.AccessControl.DirectorySecurity"},
                new List<string>(){"UnityEngine.MonoBehaviour", "runInEditMode"},
                new List<string>(){"UnityEngine.UI.Graphic", "OnRebuildRequested"},
                new List<string>(){"UnityEngine.UI.Text", "OnRebuildRequested"},
                new List<string>(){"UnityEngine.Input", "IsJoystickPreconfigured","System.String"},
                new List<string>(){"UnityEngine.AnimatorControllerParameter", "name"},
                new List<string>(){"UnityEngine.Texture", "imageContentsHash"},
                new List<string>(){"UnityEngine.QualitySettings", "set_streamingMipmapsRenderersPerFrame"},
                new List<string>(){"Coffee.UIExtensions.UIShadow", "OnBeforeSerialize"},
                new List<string>(){"Coffee.UIExtensions.UIShadow", "OnAfterDeserialize"},
                new List<string>(){"Coffee.UIExtensions.UIEffectCapturedImage", "OnBeforeSerialize"},
                new List<string>(){"Coffee.UIExtensions.UIEffectCapturedImage", "OnAfterDeserialize"},
                new List<string>(){"FMODUnity.RuntimeManager", "Destroy"},
                new List<string>(){"FMODUnity.RuntimeManager", "IsQuitting"},
                new List<string>(){"GameMain", "Env"},
                new List<string>(){"GPUSkinningPlayerMono", "DeletePlayer"},
                new List<string> () {
                    "System.IO.File",
                    "Create",
                    "System.String",
                    "System.Int32",
                    "System.IO.FileOptions"
                },
                new List<string> () {
                    "System.IO.File",
                    "Create",
                    "System.String",
                    "System.Int32",
                    "System.IO.FileOptions",
                    "System.Security.AccessControl.FileSecurity"
                },
                new List<string> () {
                    "System.IO.File",
                    "GetAccessControl",
                    "System.String"
                },
                new List<string> () {
                    "System.IO.File",
                    "GetAccessControl",
                    "System.String",
                    "System.Security.AccessControl.AccessControlSections"
                },
                new List<string> () {
                    "System.IO.File",
                    "SetAccessControl",
                    "System.String",
                    "System.Security.AccessControl.FileSecurity"
                },
                new List<string> (){ "System.IO.FileInfo", "GetAccessControl" },
                new List<string> () {
                    "System.IO.FileInfo",
                    "GetAccessControl",
                    "System.Security.AccessControl.AccessControlSections"
                },
                new List<string> () {
                    "System.IO.FileInfo",
                    "SetAccessControl",
                    "System.Security.AccessControl.FileSecurity"
                },
                new List<string> () {
                    "System.IO.Directory",
                    "CreateDirectory",
                    "System.String",
                    "System.Security.AccessControl.DirectorySecurity"
                },
                new List<string> () {
                    "System.IO.Directory",
                    "GetAccessControl",
                    "System.String"
                },
                new List<string> () {
                    "System.IO.Directory",
                    "GetAccessControl",
                    "System.String",
                    "System.Security.AccessControl.AccessControlSections"
                },
                new List<string> () {
                    "System.IO.Directory",
                    "SetAccessControl",
                    "System.String",
                    "System.Security.AccessControl.DirectorySecurity"
                },
                new List<string> () {
                    "System.IO.DirectoryInfo",
                    "GetAccessControl"
                },
                new List<string> () {
                    "System.IO.DirectoryInfo",
                    "GetAccessControl",
                    "System.Security.AccessControl.AccessControlSections"
                },
                new List<string> () {
                    "System.IO.DirectoryInfo",
                    "SetAccessControl",
                    "System.Security.AccessControl.DirectorySecurity"
                },
                new List<string> () {
                    "System.IO.DirectoryInfo",
                    "CreateSubdirectory",
                    "System.String",
                    "System.Security.AccessControl.DirectorySecurity"
                },
                new List<string> () {
                    "System.IO.DirectoryInfo",
                    "Create",
                    "System.Security.AccessControl.DirectorySecurity"
                },
            };
}
