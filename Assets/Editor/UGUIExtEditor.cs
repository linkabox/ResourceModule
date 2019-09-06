using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.UI.Extensions;

public static class UGUIExtEditor
{
    [MenuItem("UGUIExt/Set BgSize")]
    public static void SetBgSize()
    {
        foreach (var node in Selection.transforms)
        {
            var rect = node as RectTransform;
            if (rect != null)
            {
                rect.set_corner_offset(-200, -200, -200, -200);
            }
        }
    }

    [MenuItem("UGUIExt/Add Canvas")]
    public static void AddCanvasSelected()
    {
        AddCanvas(Selection.activeGameObject);
    }

    public static void AddCanvas(GameObject go)
    {
        var canvas = go.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = go.AddComponent<Canvas>();
        }

        canvas.additionalShaderChannels = AdditionalCanvasShaderChannels.TexCoord1;
        var raycaster = go.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            go.AddComponent<GraphicRaycaster>();
        }
    }

    [MenuItem("UGUIExt/Add PanelTween")]
    public static void AddPanelTweenSelectedGameObject()
    {
        AddPanelTween(Selection.activeGameObject);
    }

    public static void AddPanelTween(GameObject go)
    {
        var animator = go.GetComponent<Animator>();
        if (animator == null)
        {
            animator = go.AddComponent<Animator>();
            var defaultAnimator = AssetDatabase.LoadMainAssetAtPath("Assets/GameRes/UITween/default_panel.controller");
            animator.runtimeAnimatorController = defaultAnimator as RuntimeAnimatorController;
        }
        var group = go.GetComponent<CanvasGroup>();
        if (group == null)
        {
            group = go.AddComponent<CanvasGroup>();
        }
    }

    [MenuItem("UGUIExt/Add ButtonTween")]
    public static void AddButtonTweenSelectedGameObject()
    {
        AddButtonTween(Selection.activeGameObject);
    }

    public static void AddButtonTween(GameObject go)
    {
        var animator = go.GetComponent<Animator>();
        if (animator == null)
        {
            animator = go.AddComponent<Animator>();
            var defaultAnimator = AssetDatabase.LoadMainAssetAtPath("Assets/GameRes/UITween/button_tween.controller");
            animator.runtimeAnimatorController = defaultAnimator as RuntimeAnimatorController;
        }
        var buttonTweener = go.GetComponent<ButtonTweener>();
        if (buttonTweener == null)
        {
            buttonTweener = go.AddComponent<ButtonTweener>();
        }
        buttonTweener.animator = animator;
    }


    //[MenuItem("UGUIExt/Add ClickSound")]
    //public static void AddClickSoundSelection()
    //{
    //    foreach (var gameObject in Selection.gameObjects)
    //    {
    //        AddClickSound(gameObject);
    //    }
    //}

    //public static void AddClickSound(GameObject go)
    //{
    //    var fmodEvent = go.GetComponent<FMODUnity.FMODOneShotEvent>();
    //    if (fmodEvent == null)
    //    {
    //        fmodEvent = go.AddComponent<FMODUnity.FMODOneShotEvent>();
    //        fmodEvent.Event = "event:/UI/Button_Click";
    //    }
    //    var btn = go.GetComponent<Button>();
    //    if (btn != null)
    //    {
    //        UnityEventTools.AddVoidPersistentListener(btn.onClick, fmodEvent.PlayOneShot);
    //    }
    //    EditorUtility.SetDirty(go);
    //}

    [MenuItem("UGUIExt/Add ToolTips")]
    public static void AddToolTips()
    {
        AddToolTips(Selection.activeGameObject);
    }

    public static void AddToolTips(GameObject go)
    {
        var toolTips = go.GetComponent<ToolTips>();
        if (toolTips != null)
        {
            return;
        }

        toolTips = go.AddComponent<ToolTips>();
        var animator = go.GetComponent<Animator>();
        if (animator == null)
        {
            animator = go.AddComponent<Animator>();
            var defaultAnimator = AssetDatabase.LoadMainAssetAtPath("Assets/GameRes/UITween/tooltips_default.controller");
            animator.runtimeAnimatorController = defaultAnimator as RuntimeAnimatorController;
        }
        toolTips.animator = animator;
    }

    //[MenuItem("UGUIExt/Add TextOutline %#o")]
    //public static void AddTextOutlineSelections()
    //{
    //    foreach (var gameObject in Selection.gameObjects)
    //    {
    //        AddTextOutline(gameObject);
    //    }
    //    AssetDatabase.SaveAssets();
    //}


    //public static void AddTextOutline(GameObject go)
    //{
    //    var shadows = go.GetComponents<Shadow>();
    //    if (shadows != null)
    //    {
    //        foreach (var com in shadows)
    //        {
    //            Object.DestroyImmediate(com);
    //        }
    //    }

    //    var outlines = go.GetComponents<Outline>();
    //    if (outlines != null)
    //    {
    //        foreach (var com in outlines)
    //        {
    //            Object.DestroyImmediate(com);
    //        }
    //    }

    //    var uiShadows = go.GetComponents<Coffee.UIExtensions.UIShadow>();
    //    if (uiShadows.Length < 2)
    //    {
    //        for (int i = 0; i < 2 - uiShadows.Length; i++)
    //        {
    //            go.AddComponent<Coffee.UIExtensions.UIShadow>();
    //        }
    //    }

    //    uiShadows = go.GetComponents<Coffee.UIExtensions.UIShadow>();
    //    for (int i = 0; i < 2; i++)
    //    {
    //        var com = uiShadows[i];
    //        if (i == 0)
    //        {
    //            com.style = Coffee.UIExtensions.ShadowStyle.Shadow;
    //            com.effectDistance = new Vector2(0, -5);
    //            com.effectColor = Color.black;
    //        }
    //        else
    //        {
    //            com.style = Coffee.UIExtensions.ShadowStyle.Outline;
    //            com.effectDistance = new Vector2(-1, 1);
    //            com.effectColor = Color.black;
    //        }
    //    }

    //    EditorUtility.SetDirty(go);
    //}
}
