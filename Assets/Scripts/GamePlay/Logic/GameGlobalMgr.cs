/// <summary>
/// 游戏全局控制
/// </summary>
public class GameGlobalMgr : Singleton<GameGlobalMgr>
{
    // 从 GlobalConfig 读取配置
    public bool GMMode => GlobalConfig.Instance?.enableGMMode ?? false;
    public bool NoAD => GlobalConfig.Instance?.noAD ?? false;
    // 提审开关，静态变量，代码中直接修改
    public static bool IsShen = false;

    public void Init()
    {
    }

    public void Clear()
    {
    }
}

