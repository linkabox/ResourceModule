using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using System.IO;

[CanEditMultipleObjects]
[CustomEditor(typeof(UILocalizeImage), true)]
public class UILocalizeImageEditor : UILocalizeBaseEditor
{
    private SerializedProperty _spSpriteMap;
    private UILocalizeImage _target;
    void OnEnable()
    {
        _spSpriteMap = serializedObject.FindProperty("m_spriteMap");
        _target = target as UILocalizeImage;
    }

    public override void OnInspectorGUI()
    {
        if (!DrawLangPopup()) return;

        serializedObject.Update();

        if (GUILayout.Button("Reset"))
        {
            ResetSpriteMap();
        }

        EditorGUILayout.PropertyField(_spSpriteMap);
        serializedObject.ApplyModifiedProperties();
    }

    public void ResetSpriteMap()
    {
        var newMap = new UILocalizeImage.SpriteLocalizeDictionary();
        var oldMap = _target.SpriteMap;
        foreach (var lang in Localization.Langs)
        {
            Sprite oldSprite;
            if (oldMap.TryGetValue(lang, out oldSprite))
            {
                newMap.Add(lang, oldSprite);
            }
            else
            {
                newMap.Add(lang, null);
            }
        }

        _target.SpriteMap = newMap;
    }
}
