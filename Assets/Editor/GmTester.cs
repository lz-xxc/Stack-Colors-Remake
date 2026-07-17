using UnityEditor;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class GmTester : EditorWindow
{
    private string txtJumpLevel;

    // 是否展开某个方法区域
    private bool showCheat = true;

    [MenuItem("Tools/打开作弊界面", false, 300)]
    public static void ShowWindow()
    {
        var window = GetWindow<GmTester>();
        window.titleContent = new GUIContent("作弊界面");
        window.Show();
    }

    private void OnGUI()
    {
        showCheat = EditorGUILayout.Foldout(showCheat, "作弊");
        if (showCheat)
        {
            txtJumpLevel = EditorGUILayout.TextField("跳转关卡", txtJumpLevel);
            if (GUILayout.Button("跳转", GUILayout.Height(30)))
            {
                int level = -1;
                if (int.TryParse(txtJumpLevel, out level) && level >= 1)
                {
                    LevelDataMgr.Instance.SetLevel(level);
                }
                else
                {
                    Debug.LogError("请输入正确等级！等级需要大等于1");
                }
            }

            if (GUILayout.Button("打开胜利界面", GUILayout.Height(30)))
            {

            }

            if (GUILayout.Button("打开失败界面", GUILayout.Height(30)))
            {

            }

            if (GUILayout.Button("下一关", GUILayout.Height(30)))
            {
                LevelDataMgr.Instance.LevelPass();
            }
        }
    }
}
