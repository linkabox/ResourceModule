using System.Collections;
using System.Collections.Generic;
using UnityEditor.UI;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(TiledImage), true)]
    [CanEditMultipleObjects]
    public class TiledImageEditor : GraphicEditor
    {
        private SerializedProperty m_seg;
        private SerializedProperty m_Sprite;
        private GUIContent m_SpriteContent;

        protected override void OnEnable()
        {
            base.OnEnable();
            this.m_SpriteContent = new GUIContent("Source Image");
            this.m_seg = this.serializedObject.FindProperty("m_seg");
            this.m_Sprite = this.serializedObject.FindProperty("m_Sprite");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            this.SpriteGUI();
            EditorGUILayout.PropertyField(this.m_seg);
            this.AppearanceControlsGUI();
            this.RaycastControlsGUI();
            this.serializedObject.ApplyModifiedProperties();
        }

        protected void SpriteGUI()
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(this.m_Sprite, this.m_SpriteContent);
            if (!EditorGUI.EndChangeCheck())
                return;
        }
    }
}