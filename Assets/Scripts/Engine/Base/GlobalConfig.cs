using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// 全局配置（打包时保存的配置）
/// </summary>
[CreateAssetMenu(fileName = "GlobalConfig", menuName = "Config/Global Config", order = 0)]
public class GlobalConfig : ScriptableObject
{
    [Header("开发者选项")]
    [Tooltip("勾选后游戏中将支持GM窗口，正式包务必关闭！！！")]
    public bool enableGMMode = false;

    [Tooltip("关闭广告（测试用）")]
    public bool noAD = false;

    // 单例访问
    private static GlobalConfig _instance;
    public static GlobalConfig Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = Resources.Load<GlobalConfig>("GlobalConfig");
                if (_instance == null)
                {
#if UNITY_EDITOR
                    Debug.LogWarning("找不到 GlobalConfig，请在 Resources 文件夹中创建！使用 Tools/创建全局配置 来快速创建。");
#else
                    Debug.LogError("找不到 GlobalConfig 配置文件！");
#endif
                }
            }
            return _instance;
        }
    }

#if UNITY_EDITOR
    [MenuItem("Tools/全局配置", false, 100)]
    static void OpenOrCreateGlobalConfig()
    {
        // 检查 Resources 文件夹是否存在
        string resourcesPath = "Assets/Resources";
        string configPath = resourcesPath + "/GlobalConfig.asset";

        // 尝试加载现有配置
        GlobalConfig config = AssetDatabase.LoadAssetAtPath<GlobalConfig>(configPath);

        if (config != null)
        {
            // 配置已存在，直接打开
            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);
        }
        else
        {
            // 配置不存在，创建新配置
            if (!AssetDatabase.IsValidFolder(resourcesPath))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            config = CreateInstance<GlobalConfig>();
            config.enableGMMode = false;
            config.noAD = false;

            AssetDatabase.CreateAsset(config, configPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Selection.activeObject = config;
            EditorGUIUtility.PingObject(config);

            Debug.Log("GlobalConfig 创建成功！路径: " + configPath);
        }
    }
#endif
}

