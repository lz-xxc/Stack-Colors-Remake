using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GlobalConfig))]
public class GlobalConfigEditor : Editor
{
    public override void OnInspectorGUI()
    {
        GlobalConfig config = (GlobalConfig)target;

        EditorGUILayout.Space(10);

        // 标题
        GUIStyle titleStyle = new GUIStyle(EditorStyles.boldLabel)
        {
            fontSize = 14,
            alignment = TextAnchor.MiddleCenter
        };
        EditorGUILayout.LabelField("全局配置", titleStyle);

        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("开发者选项", EditorStyles.boldLabel);
        EditorGUILayout.BeginVertical(GUI.skin.box);

        EditorGUI.BeginChangeCheck();

        // GM模式
        config.enableGMMode = EditorGUILayout.Toggle("GM模式", config.enableGMMode);
        if (config.enableGMMode)
        {
            EditorGUILayout.HelpBox("勾选后游戏中将支持GM窗口", MessageType.Info);
            GUIStyle warningStyle = new GUIStyle(EditorStyles.label)
            {
                normal = { textColor = Color.red },
                fontStyle = FontStyle.Bold
            };
            EditorGUILayout.LabelField("⚠ 正式包务必关闭！！！", warningStyle);
        }

        EditorGUILayout.Space(5);

        // 无广告模式
        config.noAD = EditorGUILayout.Toggle("无广告模式", config.noAD);
        EditorGUILayout.HelpBox("测试时关闭广告，业务端实现才有效", MessageType.None);

        EditorGUILayout.EndVertical();

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(config);
        }

        EditorGUILayout.Space(10);

        // 快捷按钮
        EditorGUILayout.LabelField("快捷操作", EditorStyles.boldLabel);

        GUI.backgroundColor = new Color(1f, 0.9f, 0.9f);
        if (GUILayout.Button("关闭所有开发选项", GUILayout.Height(30)))
        {
            config.enableGMMode = false;
            config.noAD = false;
            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            Debug.Log("✓ 已关闭所有开发选项");
        }
        GUI.backgroundColor = Color.white;

        EditorGUILayout.Space(10);

        // 打包检查提示
        if (config.enableGMMode || config.noAD)
        {
            EditorGUILayout.HelpBox("⚠ 注意：打包时会弹窗提醒关闭这些开发选项！", MessageType.Warning);
        }
        else
        {
            EditorGUILayout.HelpBox("✓ 所有开发选项已关闭，可以打正式包。", MessageType.Info);
        }
    }
}

