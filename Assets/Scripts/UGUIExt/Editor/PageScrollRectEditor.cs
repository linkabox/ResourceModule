using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace UnityEditor.UI
{
    [CustomEditor(typeof(PageScrollRect))]
    [CanEditMultipleObjects]
    public class PageScrollRectEditor : ScrollRectEditor
    {
        private SerializedProperty m_CellNum;
        private SerializedProperty m_SnapOnEnable;
        private SerializedProperty m_SnapVelocityThreshold;
        private SerializedProperty m_SnapWatchOffset;
        private SerializedProperty m_SnapTweenTime;
        private SerializedProperty m_EaseType;
        private PageScrollRect _scrollRect;

        protected override void OnEnable()
        {
            _scrollRect = this.target as PageScrollRect;
            base.OnEnable();

            this.m_CellNum = this.serializedObject.FindProperty("cellNum");
            this.m_SnapOnEnable = this.serializedObject.FindProperty("snapOnEnable");
            this.m_SnapVelocityThreshold = this.serializedObject.FindProperty("snapVelocityThreshold");
            this.m_SnapWatchOffset = this.serializedObject.FindProperty("snapWatchOffset");
            this.m_SnapTweenTime = this.serializedObject.FindProperty("snapTweenTime");
            this.m_EaseType = this.serializedObject.FindProperty("easeType");
        }

        public override void OnInspectorGUI()
        {
            this.serializedObject.Update();
            EditorGUILayout.PropertyField(this.m_CellNum);
            EditorGUILayout.PropertyField(this.m_SnapOnEnable);
            EditorGUILayout.PropertyField(this.m_SnapVelocityThreshold);
            EditorGUILayout.PropertyField(this.m_SnapWatchOffset);
            EditorGUILayout.PropertyField(this.m_SnapTweenTime);
            EditorGUILayout.PropertyField(this.m_EaseType);
            this.serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            base.OnInspectorGUI();

            if (_scrollRect.cellNum > 0 && Application.isPlaying)
            {
                for (int i = 0; i < _scrollRect.cellNum; i++)
                {
                    if (GUILayout.Button("JumpTo:" + i))
                    {
                        _scrollRect.JumpTo(i, this.m_SnapTweenTime.floatValue);
                    }
                }
            }
        }
    }

}
