using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// 打包前预处理，检查全局配置
/// </summary>
public class BuildPreprocessor : IPreprocessBuildWithReport
{
    public int callbackOrder => 0;

    public void OnPreprocessBuild(BuildReport report)
    {
        // 加载全局配置
        GlobalConfig config = GlobalConfig.Instance;

        if (config == null)
        {
            Debug.LogWarning("未找到 GlobalConfig 配置文件，跳过检查。");
            return;
        }

        // 检查是否有开发选项被启用
        bool hasWarning = false;
        string warningMessage = "检测到以下开发选项已启用：\n\n";

        if (config.enableGMMode)
        {
            hasWarning = true;
            warningMessage += "• GM模式：已启用 ⚠️\n";
        }

        if (config.noAD)
        {
            hasWarning = true;
            warningMessage += "• 无广告模式：已启用 ⚠️\n";
        }

        // 如果有警告，弹窗提示
        if (hasWarning)
        {
            warningMessage += "\n这些选项不应该在正式包中启用！\n";
            warningMessage += "\n是否继续打包？";

            bool shouldContinue = EditorUtility.DisplayDialog(
                "打包前检查 - 发现开发选项",
                warningMessage,
                "继续打包",
                "终止打包"
            );

            if (!shouldContinue)
            {
                // 终止打包
                throw new BuildFailedException("用户取消打包：需要关闭开发选项");
            }
            else
            {
                Debug.LogWarning("用户选择继续打包，但仍存在开发选项！请确认这是开发版本。");
            }
        }
        else
        {
            Debug.Log("✓ 打包前检查通过：所有开发选项已关闭。");
        }
    }
}

