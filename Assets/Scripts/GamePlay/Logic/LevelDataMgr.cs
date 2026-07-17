using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 关卡数据类
/// </summary>
public class LevelDataMgr : Singleton<LevelDataMgr>
{
    // 关卡编号
    private const string LEVEL_KEY = "LEVEL_KEY";
    private int m_level;
    public int Level
    {
        get => m_level;
        set
        {
            m_level = value;
            LocalSave.SetInt(LEVEL_KEY, m_level);
        }
    }

    public void Init()
    {
        m_level = LocalSave.GetInt(LEVEL_KEY, 1);
    }

    // 过关
    public void LevelPass()
    {
        Level++;
        // 通知界面表现
        Send.SendMsg(SendType.LevelPass, Level);
    }

    public void SetLevel(int _level)
    {
        Level = _level;
        // 通知界面表现
        Send.SendMsg(SendType.LevelPass, Level);
    }
}