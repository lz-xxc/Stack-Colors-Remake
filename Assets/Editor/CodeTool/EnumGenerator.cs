using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace XS.EditorTools
{
    /// <summary>
    /// 从配置表自动生成枚举工具
    /// </summary>
    public class EnumGenerator
    {
        private static bool isGenerating = false; // 防止循环触发

        [MenuItem("Tools/生成 PropType 枚举")]
        public static void GeneratePropTypeEnum()
        {
            GeneratePropTypeEnumInternal(true);
        }

        /// <summary>
        /// 内部生成方法，支持控制是否显示日志
        /// </summary>
        private static void GeneratePropTypeEnumInternal(bool showLog = true)
        {
            if (isGenerating) return; // 防止递归调用

            isGenerating = true;
            try
            {
                // 读取配置表
                string configPath = "Assets/Resources/Refdata/Prop.txt";
                if (!File.Exists(configPath))
                {
                    Debug.LogError($"配置表不存在: {configPath}");
                    return;
                }

                // 解析配置表获取所有类型
                HashSet<string> types = new HashSet<string>();
                string[] lines = File.ReadAllLines(configPath);

                if (lines.Length < 3)
                {
                    Debug.LogError("配置表格式错误，至少需要3行（中文标题、英文标题、数据）");
                    return;
                }

                // 第2行是英文列名，找到 PropType 列的索引
                string[] headers = lines[1].Split('\t');
                int propTypeIndex = System.Array.IndexOf(headers, "PropType");

                if (propTypeIndex == -1)
                {
                    Debug.LogError("配置表中找不到 'PropType' 列");
                    return;
                }

                // 从第3行开始读取数据
                for (int i = 2; i < lines.Length; i++)
                {
                    if (string.IsNullOrWhiteSpace(lines[i])) continue;

                    string[] columns = lines[i].Split('\t');
                    if (columns.Length > propTypeIndex && !string.IsNullOrWhiteSpace(columns[propTypeIndex]))
                    {
                        types.Add(columns[propTypeIndex].Trim());
                    }
                }

                if (types.Count == 0)
                {
                    Debug.LogWarning("配置表中没有找到任何道具类型数据");
                    return;
                }

                // 生成枚举代码
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("/// <summary>");
                sb.AppendLine("/// 道具类型枚举（自动生成，请勿手动修改）");
                sb.AppendLine("/// 生成路径: Tools/生成配置表枚举/PropType 枚举");
                sb.AppendLine("/// </summary>");
                sb.AppendLine("public enum PropType");
                sb.AppendLine("{");

                var sortedTypes = types.OrderBy(t => t).ToList();
                for (int i = 0; i < sortedTypes.Count; i++)
                {
                    string type = sortedTypes[i];
                    if (i == sortedTypes.Count - 1)
                    {
                        sb.AppendLine($"    {type}      // {type}");
                    }
                    else
                    {
                        sb.AppendLine($"    {type},      // {type}");
                    }
                }

                sb.AppendLine("}");

                // 更新 Enum.cs 文件
                UpdateEnumFile("PropType", sb.ToString());

                if (showLog)
                {
                    Debug.Log($"✓ PropType 枚举生成成功！共 {types.Count} 个类型: {string.Join(", ", sortedTypes)}");
                }
            }
            catch (System.Exception ex)
            {
                if (showLog)
                {
                    Debug.LogError($"生成 PropType 枚举失败: {ex.Message}");
                }
            }
            finally
            {
                isGenerating = false;
            }
        }

        /// <summary>
        /// 更新 PropMgr.cs 文件中的指定枚举
        /// </summary>
        private static void UpdateEnumFile(string enumName, string enumCode)
        {
            string enumFilePath = "Assets/Scripts/GamePlay/Logic/PropMgr.cs";

            if (!File.Exists(enumFilePath))
            {
                Debug.LogError($"PropMgr.cs 文件不存在: {enumFilePath}");
                return;
            }

            string content = File.ReadAllText(enumFilePath);

            // 使用更精确的方式查找枚举位置
            int enumIndex = FindEnumBlock(content, enumName, out int startIndex, out int endIndex);

            if (enumIndex >= 0)
            {
                // 找到了枚举，替换它
                string before = content.Substring(0, startIndex);
                string after = content.Substring(endIndex);
                content = before + enumCode + after;
                Debug.Log($"已更新 PropMgr.cs 中的 {enumName} 枚举");
            }
            else
            {
                // 追加新枚举到文件末尾
                content = content.TrimEnd() + "\n\n" + enumCode;
                Debug.Log($"已在 PropMgr.cs 中添加新的 {enumName} 枚举");
            }

            File.WriteAllText(enumFilePath, content, Encoding.UTF8);
            AssetDatabase.Refresh();
        }

        /// <summary>
        /// 查找指定枚举的完整代码块（包括注释）
        /// </summary>
        private static int FindEnumBlock(string content, string enumName, out int startIndex, out int endIndex)
        {
            startIndex = -1;
            endIndex = -1;

            // 查找枚举定义的位置
            string enumPattern = $@"public\s+enum\s+{enumName}\s*{{";
            var match = System.Text.RegularExpressions.Regex.Match(content, enumPattern);

            if (!match.Success)
                return -1;

            int enumDefStart = match.Index;

            // 向前查找注释开始位置（/// <summary>）
            int commentStart = content.LastIndexOf("/// <summary>", enumDefStart);
            if (commentStart >= 0 && commentStart < enumDefStart)
            {
                // 确保注释和枚举之间没有其他代码
                string between = content.Substring(commentStart, enumDefStart - commentStart);
                if (System.Text.RegularExpressions.Regex.IsMatch(between, @"^\s*///.*$", System.Text.RegularExpressions.RegexOptions.Multiline))
                {
                    startIndex = commentStart;
                }
                else
                {
                    startIndex = enumDefStart;
                }
            }
            else
            {
                startIndex = enumDefStart;
            }

            // 向后查找枚举结束的右大括号
            int braceCount = 0;
            bool inEnum = false;
            for (int i = match.Index + match.Length - 1; i < content.Length; i++)
            {
                if (content[i] == '{')
                {
                    braceCount++;
                    inEnum = true;
                }
                else if (content[i] == '}')
                {
                    braceCount--;
                    if (inEnum && braceCount == 0)
                    {
                        endIndex = i + 1;
                        break;
                    }
                }
            }

            if (endIndex < 0)
                return -1;

            return startIndex;
        }
    }

    /// <summary>
    /// 配置表文件监听器，自动触发枚举生成
    /// </summary>
    public class ConfigTableWatcher : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            bool needRegenerate = false;

            // 检查是否有 Prop.txt 被修改
            foreach (string path in importedAssets)
            {
                if (path.EndsWith("Refdata/Prop.txt"))
                {
                    needRegenerate = true;
                    break;
                }
            }

            if (needRegenerate)
            {
                // 延迟执行，确保资源已经完全导入
                EditorApplication.delayCall += () =>
                {
                    Debug.Log("检测到 Prop.txt 变化，自动重新生成 PropType 枚举...");
                    EnumGenerator.GeneratePropTypeEnum();
                };
            }
        }
    }
}

