using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RefShop : RefBase {

    public static Dictionary<int, RefShop> cacheMap = new Dictionary<int, RefShop>();

    public int ItemId;                  //id
    public UnLockType UnLockType;       //解锁类型
    public int Param;                   //解锁参数
    public string Desc;                 //描述
    public string Name;                 //商品名称

    public override string GetFirstKeyName() {
        return "ItemId";
    }

    public override void LoadByLine(Dictionary<string, string> _value, int _line) {
        base.LoadByLine(_value, _line);
        ItemId = GetInt("ItemId");
        UnLockType = (UnLockType)GetEnum("UnLockType", typeof(UnLockType));
        Param = GetInt("Param");
        Desc = GetString("Desc");
        Name = GetString("Name");
    }

    public static RefShop GetRef(int itemID) {
        RefShop data = null;
        if (cacheMap.TryGetValue(itemID, out data)) {
            return data;
        }

        if (data == null) {
            Debug.LogError("error RefShop key:" + itemID);
        }
        return data;
    }
}