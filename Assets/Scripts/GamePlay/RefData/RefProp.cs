using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefProp : RefBase
{

    public static Dictionary<int, RefProp> cacheMap = new Dictionary<int, RefProp>();
    public static Dictionary<PropType, int> propTypeToIdMap = new Dictionary<PropType, int>(); // PropType -> ID 映射

    public int ID;                      // 道具编号
    public PropType PropType;           // 道具枚举
    public string Name;             // 道具名称
    public PropGroup PropGroup;         // 道具类型分组（Coin/Item）
    public string Icon;                 // icon

    public override string GetFirstKeyName()
    {
        return "ID";
    }

    public override void LoadByLine(Dictionary<string, string> _value, int _line)
    {
        base.LoadByLine(_value, _line);
        ID = GetInt("ID");
        PropType = (PropType)GetEnum("PropType", typeof(PropType));
        Name = GetString("Name");
        PropGroup = (PropGroup)GetEnum("PropGroup", typeof(PropGroup));
        Icon = GetString("Icon");

        // 自动填充 PropType -> ID 映射
        if (!propTypeToIdMap.ContainsKey(PropType))
        {
            propTypeToIdMap.Add(PropType, ID);
        }
    }

    public static RefProp GetRef(int id)
    {
        RefProp data = null;
        if (cacheMap.TryGetValue(id, out data))
        {
            return data;
        }

        if (data == null)
        {
            Debug.LogError("error RefProp key:" + id);
        }
        return data;
    }

    /// <summary>
    /// 通过 PropType 枚举获取道具数据
    /// </summary>
    public static RefProp GetRefByPropType(PropType propType)
    {
        int id;
        if (propTypeToIdMap.TryGetValue(propType, out id))
        {
            return GetRef(id);
        }

        Debug.LogError($"PropType 不存在: {propType}");
        return null;
    }

    /// <summary>
    /// 通过 PropType 枚举获取道具 ID
    /// </summary>
    public static int GetIdByPropType(PropType propType)
    {
        int id;
        if (propTypeToIdMap.TryGetValue(propType, out id))
        {
            return id;
        }

        Debug.LogError($"PropType 不存在: {propType}");
        return 0;
    }
}

