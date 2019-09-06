using UnityEngine;
using System.Collections;
using UnityEditor;

public static class EditorUIDrawer
{
    public static void DrawBoxLabel(string title, string text, bool hasCopyBtn, bool needLayout = true)
    {
        if (needLayout) EditorGUILayout.BeginHorizontal();

        GUILayout.Box(title);
        GUILayout.Space(5f);
        GUILayout.Box(text);
        if (hasCopyBtn && GUILayout.Button("Copy", GUILayout.Width(60f)))
        {
            EditorGUIUtility.systemCopyBuffer = text;
        }

        if (needLayout) EditorGUILayout.EndHorizontal();
    }

    public static bool DrawHeader(string text) { return DrawHeader(text, text, false, false); }

    public static bool DrawHeader(string text, string key, bool forceOn, bool minimalistic)
    {
        bool state = EditorPrefs.GetBool(key, true);

        GUILayout.Space(3f);
        if (!forceOn && !state) GUI.backgroundColor = new Color(0.8f, 0.8f, 0.8f);
        GUILayout.BeginHorizontal();
        GUI.changed = false;

        text = "<b><size=11>" + text + "</size></b>";
        if (state) text = "\u25BC " + text;
        else text = "\u25BA " + text;
        if (!GUILayout.Toggle(true, text, "dragtab", GUILayout.MinWidth(20f))) state = !state;

        if (GUI.changed) EditorPrefs.SetBool(key, state);

        GUILayout.Space(2f);
        GUILayout.EndHorizontal();
        GUI.backgroundColor = Color.white;
        if (!forceOn && !state) GUILayout.Space(3f);
        return state;
    }
}
