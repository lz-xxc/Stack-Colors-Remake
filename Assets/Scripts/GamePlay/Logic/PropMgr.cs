using System.Collections.Generic;
using UnityEngine;

public class PropMgr : Singleton<PropMgr>
{
    // 道具列表
    private const string PROP_LIST_KEY = "PROP_LIST";
    private List<PropItem> m_propList;
    public List<PropItem> PropList
    {
        get => m_propList;
        set
        {
            m_propList = value;
            LocalSave.SetList(PROP_LIST_KEY, value);
        }
    }

    public void Init()
    {
        m_propList = LocalSave.GetList<PropItem>(PROP_LIST_KEY);
    }

    // 保存道具列表
    private void SavePropItemList()
    {
        LocalSave.SetList(PROP_LIST_KEY, m_propList);
        Send.SendMsg(SendType.PropChange);
    }

    // 添加道具到列表
    private void AddPropItem(int id, int num)
    {
        PropItem existingItem = m_propList.Find(item => item.ID == id);
        if (existingItem != null)
        {
            existingItem.Num += num;
        }
        else
        {
            m_propList.Add(new PropItem { ID = id, Num = num });
        }
    }

    // 获取道具数量
    public int GetPropItemNum(int id)
    {
        PropItem item = m_propList.Find(i => i.ID == id);
        return item != null ? item.Num : 0;
    }

    // 添加奖励
    public void AddProp(Dictionary<PropType, int> addProps, Reason reason = Reason.None)
    {
        bool changeItem = false; // 道具表更标志
        foreach (var item in addProps)
        {
            RefProp refProp = RefProp.GetRefByPropType(item.Key);
            if (refProp.PropGroup == PropGroup.Coin)
            {
                switch (refProp.PropType)
                {
                    case PropType.Gold:
                        CurrencyMgr.Instance.Gold += item.Value;
                        break;
                }
            }
            else if (refProp.PropGroup == PropGroup.Item)
            {
                AddPropItem(refProp.ID, item.Value); // 添加道具
                changeItem = true;
            }
        }

        // 有道具变更，保存数据
        if (changeItem)
        {
            SavePropItemList();
        }

        // 显示奖励弹窗
        if (reason == Reason.Normal)
        {
            // RewardWindow.Instance.ShowRewards(PropList);
        }
    }

    public int GetPropNum(PropType propType)
    {
        RefProp refProp = RefProp.GetRefByPropType(propType);
        if (refProp.PropGroup == PropGroup.Coin)
        {
            switch (refProp.PropType)
            {
                case PropType.Gold:
                    return CurrencyMgr.Instance.Gold;
            }
        }
        else if (refProp.PropGroup == PropGroup.Item)
        {
            // 获取道具数量
            return GetPropItemNum(refProp.ID);
        }
        return 0;
    }

    public void PropNumChange(PropType propType, int num)
    {
        RefProp refProp = RefProp.GetRefByPropType(propType);
        if (refProp.PropGroup == PropGroup.Coin)
        {
            switch (refProp.PropType)
            {
                case PropType.Gold:
                    CurrencyMgr.Instance.Gold += num;
                    break;
            }
        }
        else if (refProp.PropGroup == PropGroup.Item)
        {
            AddPropItem(refProp.ID, num);
            SavePropItemList();
        }
    }

    public bool PropCanUse(PropType propType, int num = 1)
    {
        return GetPropNum(propType) >= num;
    }
}

[System.Serializable]
public class PropItem
{
    public int ID;
    public int Num;
}

public enum Reason
{
    None, // 普通无弹窗
    Normal, // 普通奖励弹窗
}

/// <summary>
/// 道具分组枚举（手动维护）
/// </summary>
public enum PropGroup
{
    Coin,       // 金币
    Item,       // 道具
}

/// <summary>
/// 道具类型枚举（自动生成，请勿手动修改）
/// 生成路径: Tools/生成配置表枚举/PropType 枚举
/// </summary>
public enum PropType
{
    Clear,      // Clear
    Gold,      // Gold
    Hint,      // Hint
    Magnet      // Magnet
}

