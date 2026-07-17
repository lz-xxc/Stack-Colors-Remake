using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefPropUse : RefBase
{

    public static Dictionary<PropType, RefPropUse> cacheMap = new Dictionary<PropType, RefPropUse>();

    public PropType PropType;           // 道具类型
    public int CostNum;                 // 消耗数量
    public int AddNum;                  // 购买数量
    public int UnlockLv;                // 解锁关卡
    public string Des;                  // 描述

    public override string GetFirstKeyName()
    {
        return "PropType";
    }

    public override void LoadByLine(Dictionary<string, string> _value, int _line)
    {
        base.LoadByLine(_value, _line);
        PropType = (PropType)GetEnum("PropType", typeof(PropType));
        CostNum = GetInt("CostNum");
        AddNum = GetInt("AddNum");
        UnlockLv = GetInt("UnlockLv");
        Des = GetString("Des");
    }

    public static RefPropUse GetRef(PropType propType)
    {
        RefPropUse data = null;
        if (cacheMap.TryGetValue(propType, out data))
        {
            return data;
        }

        if (data == null)
        {
            Debug.LogError("error RefPropUse key:" + propType);
        }
        return data;
    }
}


