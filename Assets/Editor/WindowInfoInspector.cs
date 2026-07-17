using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace XS.XSEditor
{
    [CustomEditor(typeof(WindowInfo))]
    public class WindowInfoInspector : Editor
    {
        WindowInfo windowInfo;
        float minVal = 0f;
        float maxVal = 10.0f;
        bool animSet = true;

        private void OnEnable()
        {
            windowInfo = (WindowInfo)target;
        }

        public override void OnInspectorGUI()
        {
            //base.OnInspectorGUI();
            EditorGUILayout.BeginVertical();

            //空两行
            EditorGUILayout.Space();
            EditorGUILayout.Space();
            windowInfo.windowType = (WindowType)EditorGUILayout.EnumPopup("Window Type", windowInfo.windowType);
            EditorGUILayout.Space();

            animSet = EditorGUILayout.Foldout(animSet, "Window Anim", true);
            if (animSet)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                windowInfo.openAnimType = (OpenAnimType)EditorGUILayout.EnumPopup("Open Anim Type", windowInfo.openAnimType);
                windowInfo.closeAnimType = (OpenAnimType)EditorGUILayout.EnumPopup("Close Anim Type", windowInfo.closeAnimType);
                if (windowInfo.openAnimType != OpenAnimType.None || windowInfo.closeAnimType != OpenAnimType.None)
                {
                    windowInfo.animTime = EditorGUILayout.Slider("Anim Time", windowInfo.animTime, minVal, maxVal);
                }

                // 为 Scale 和 ScaleAndAlpha 类型添加缓动选项
                if (windowInfo.openAnimType == OpenAnimType.Scale || windowInfo.openAnimType == OpenAnimType.ScaleAndAlpha)
                {
                    windowInfo.openEase = (DG.Tweening.Ease)EditorGUILayout.EnumPopup("Open Ease", windowInfo.openEase);
                }
                if (windowInfo.closeAnimType == OpenAnimType.Scale || windowInfo.closeAnimType == OpenAnimType.ScaleAndAlpha)
                {
                    windowInfo.closeEase = (DG.Tweening.Ease)EditorGUILayout.EnumPopup("Close Ease", windowInfo.closeEase);
                }

                if (windowInfo.openAnimType == OpenAnimType.Position || windowInfo.closeAnimType == OpenAnimType.Position)
                {
                    if (windowInfo.defaultPos == Vector3.zero && windowInfo.openPos == Vector3.zero)
                    {
                        EditorGUILayout.HelpBox("位移动画需要填入位置信息！！", MessageType.Warning);
                    }
                    windowInfo.defaultPos = EditorGUILayout.Vector3Field("Default Pos", windowInfo.defaultPos);
                    windowInfo.openPos = EditorGUILayout.Vector3Field("Open Pos", windowInfo.openPos);
                }
                EditorGUILayout.EndVertical();
            }
            EditorGUILayout.Space();

            windowInfo.closeOnEmpty = EditorGUILayout.Toggle("Close On Empty", windowInfo.closeOnEmpty);
            windowInfo.mask = EditorGUILayout.Toggle("Mask", windowInfo.mask);
            windowInfo.maskAlpha = EditorGUILayout.Slider("Mask Alpha", windowInfo.maskAlpha, 0f, 1f);
            windowInfo.group = EditorGUILayout.IntField("Group", windowInfo.group);


            EditorGUILayout.EndVertical();

            if (GUI.changed)
            {
                EditorUtility.SetDirty(windowInfo);
            }
        }
    }
}