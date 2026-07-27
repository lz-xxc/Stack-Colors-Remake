using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 商店管理
/// </summary>
public class ShopMgr : Singleton<ShopMgr> {
    private const string USE_ITEM_ID = "UseItemID";
    // 默认解锁存档Key固定
    private const string DEFAULT_UNLOCK_KEY = "UnLockItemId100";
    public List<ShopItemInfo> itemInfoList = new List<ShopItemInfo>();
    public ShopItemInfo selectItemInfo;

    public int UseItemId {
        get {
            return LocalSave.GetInt(USE_ITEM_ID, 0);
        }
        set {
            LocalSave.SetInt(USE_ITEM_ID, value);
            string skinName = GetItemName(value);
            Send.SendMsg(SendType.UseItemChange, skinName);
        }
    }

    public void Init() {
        InitList();
        Send.RegisterMsg(SendType.TryUnLockItem, OnUnLockItem);
        Send.RegisterMsg(SendType.TaskComplete, OnTaskComplete);
        Send.RegisterMsg(SendType.UseItemChange, OnUseItemChange);
        Send.RegisterMsg(SendType.UnLockItemSuccess, OnUnLockSuccess);
    }

    public void Clear() {
        Send.UnregisterMsg(SendType.TryUnLockItem, OnUnLockItem);
        Send.UnregisterMsg(SendType.TaskComplete, OnTaskComplete);
        Send.UnregisterMsg(SendType.UseItemChange, OnUseItemChange);
        Send.UnregisterMsg(SendType.UnLockItemSuccess, OnUnLockSuccess);
    }

    private void InitList() {
        itemInfoList.Clear();
        foreach (RefShop refshop in RefShop.cacheMap.Values) {
            ShopItemInfo itemInfo = new ShopItemInfo(refshop);
            itemInfoList.Add(itemInfo);
        }

        // 默认解锁 Player1（ItemId = 100）
        UnlockDefaultSkin();
    }

    // 默认解锁 Player1
    private void UnlockDefaultSkin() {
        ShopItemInfo player1 = GetItemInfo(100);
        if (player1 != null && player1.itemState == ShopItemState.Lock) {
            LocalSave.SetBool(DEFAULT_UNLOCK_KEY, true);
            player1.UnLock();
        }

        // 无使用皮肤时默认选中使用100
        if (UseItemId == 0) {
            UseItemId = 100;
        }
    }

    public string GetItemName(int itemId) {
        RefShop refShop = RefShop.GetRef(itemId);
        if (refShop == null) {
            Debug.LogError($"商品不存在: {itemId}");
            return "Player1";
        }
        return refShop.Name;
    }

    private void OnUnLockItem(object[] objs) {
        int itemId = (int)objs[0];
        TryUnLockItem(itemId);
    }

    public void TryUnLockItem(int itemId) {
        ShopItemInfo itemInfo = GetItemInfo(itemId);
        if (itemInfo == null) {
            Debug.LogError("iteminfo is null:" + itemId);
            return;
        }
        if (itemInfo.CanUnLock()) {
            itemInfo.UnLock();
            if (itemInfo.refShop.UnLockType == UnLockType.Gold)
                CurrencyMgr.Instance.Gold -= itemInfo.refShop.Param;
        }
        else {
            Debug.LogError("解锁条件不满足:" + itemId);
        }
    }

    public ShopItemInfo GetItemInfo(int itemId) {
        foreach (ShopItemInfo shopItemInfo in itemInfoList) {
            if (shopItemInfo.refShop.ItemId == itemId) {
                return shopItemInfo;
            }
        }
        return null;
    }

    private void OnTaskComplete(object[] objs) {
        int taskId = (int)objs[0];
        foreach (ShopItemInfo shopItemInfo in itemInfoList) {
            if (shopItemInfo.refShop.UnLockType == UnLockType.Task && shopItemInfo.refShop.Param == taskId) {
                shopItemInfo.UnLock();
            }
        }
    }

    public bool HasNewItem() {
        foreach (ShopItemInfo shopItemInfo in itemInfoList) {
            if (shopItemInfo.HasNewItem()) {
                return true;
            }
        }
        return false;
    }

    public void RefreshItem() {
        string txtBuy = "";
        bool canClickBtn = false;

        if (selectItemInfo == null && itemInfoList.Count > 0) {
            selectItemInfo = itemInfoList[0];
        }

        switch (selectItemInfo.itemState) {
            case (ShopItemState.CanUse):
                txtBuy = "使用";
                canClickBtn = true;
                break;
            case (ShopItemState.InUse):
                txtBuy = "已使用";
                canClickBtn = false;
                break;
            case (ShopItemState.Lock):
                int param = selectItemInfo.refShop.Param;
                txtBuy = $"解锁需要：{param}";
                if (selectItemInfo.CanUnLock()) {
                    canClickBtn = true;
                }
                else {
                    canClickBtn = false;
                    txtBuy = $"解锁需要{param}";
                }
                break;
        }
        Send.SendMsg(SendType.ClickShopIconChange, txtBuy, canClickBtn);
    }

    public void OnItemStateChange() {
        ShopItemInfo item = selectItemInfo;

        switch (item.itemState) {
            case ShopItemState.Lock:
                item.TryUnLockItem();  // 内部会发送 TryUnLockItem 消息
                break;

            case ShopItemState.CanUse:
                int oldUseItemId = ShopMgr.Instance.UseItemId;
                ShopMgr.Instance.UseItemId = item.refShop.ItemId;

                if (oldUseItemId != 0) {
                    GetItemInfo(oldUseItemId).itemState = ShopItemState.CanUse;
                }

                item.itemState = ShopItemState.InUse;
                break;

            case ShopItemState.InUse:
                // 不会触发
                break;
        }
    }

    private void OnUseItemChange(object[] objs) {
        RefreshItem();
    }

    // ✅ 解锁成功时刷新按钮
    private void OnUnLockSuccess(object[] objs) {
        RefreshItem();
    }
}

/// <summary>
/// 物品信息
/// </summary>
public class ShopItemInfo {
    private const string UNLOCK_KEY = "UnLockItemId";
    private const string NEW_ITEM_KEY = "NewItemId";
    public RefShop refShop;
    public ShopItemState itemState;

    public ShopItemInfo(RefShop _refShop) {
        refShop = _refShop;
        Refresh();
    }

    public void Refresh() {
        // 读取解锁存档
        bool isUnlock = LocalSave.GetBool(UNLOCK_KEY + refShop.ItemId, false);
        itemState = isUnlock ? ShopItemState.CanUse : ShopItemState.Lock;
        // 当前正在使用则覆盖状态
        if (ShopMgr.Instance.UseItemId == refShop.ItemId) {
            itemState = ShopItemState.InUse;
        }
    }

    public void UnLock() {
        LocalSave.SetBool(UNLOCK_KEY + refShop.ItemId, true);
        // 标记为新物品
        LocalSave.SetBool(NEW_ITEM_KEY + refShop.ItemId, true);
        Refresh();

        Send.SendMsg(SendType.UnLockItemSuccess, itemState);
    }

    /// <summary>
    /// 是否是新物品（红点）
    /// </summary>
    public bool HasNewItem() {
        if (itemState != ShopItemState.CanUse)
            return false;
        return LocalSave.GetBool(NEW_ITEM_KEY + refShop.ItemId, false);
    }

    /// <summary>
    /// 消除新物品标记（修复原代码BUG）
    /// </summary>
    public void FindItem() {
        LocalSave.SetBool(NEW_ITEM_KEY + refShop.ItemId, false);
    }

    /// <summary>
    /// 获取解锁描述
    /// </summary>
    public string GetDesc() {
        string desc = "";
        switch (refShop.UnLockType) {
            case UnLockType.None:
                desc = refShop.Desc;
                break;
            case UnLockType.Gold:
                desc = string.Format(refShop.Desc, refShop.Param);
                break;
            case UnLockType.Task:
                RefTask refTask = RefTask.GetRef(refShop.Param);
                desc = refTask != null ? string.Format(refShop.Desc, refTask.Condition) : "任务不存在";
                break;
            default:
                Debug.LogError("未定义解锁类型:" + refShop.UnLockType);
                break;
        }
        return desc;
    }

    /// <summary>
    /// 解锁进度文字
    /// </summary>
    public string GetProgress() {
        string progress = "";
        bool success = CanUnLock();

        switch (refShop.UnLockType) {
            case UnLockType.None:
                progress = "";
                break;
            case UnLockType.Gold:
                progress = success
                    ? $"<color=#00FF00>{CurrencyMgr.Instance.Gold}</color>/{refShop.Param}"
                    : $"<color=#FF0000>{CurrencyMgr.Instance.Gold}</color>/{refShop.Param}";
                break;
            case UnLockType.Task:
                TaskInfo taskInfo = TaskMgr.Instance.GetTaskInfo(refShop.Param);
                if (taskInfo == null) {
                    Debug.LogError("taskinfo is null:" + refShop.Param);
                }
                else {
                    progress = success
                        ? $"<color=#00FF00>{taskInfo.CurValue}</color>/{taskInfo.refTask.Condition}"
                        : $"<color=#FF0000>{taskInfo.CurValue}</color>/{taskInfo.refTask.Condition}";
                }
                break;
            default:
                Debug.LogError("未定义解锁类型:" + refShop.UnLockType);
                break;
        }
        return progress;
    }

    /// <summary>
    /// 判断是否满足解锁条件
    /// </summary>
    public bool CanUnLock() {
        bool can = false;
        switch (refShop.UnLockType) {
            case UnLockType.None:
                can = true;
                break;
            case UnLockType.Gold:
                can = CurrencyMgr.Instance.Gold >= refShop.Param;
                break;
            case UnLockType.Task:
                can = TaskMgr.Instance.TaskHasComplete(refShop.Param);
                break;
            default:
                Debug.LogError("未定义解锁类型:" + refShop.UnLockType);
                break;
        }
        return can;
    }

    public UnLockItemResult TryUnLockItem() {
        if (itemState != ShopItemState.Lock)
            return UnLockItemResult.Unlocked;

        if (CanUnLock()) {
            Send.SendMsg(SendType.TryUnLockItem, refShop.ItemId);
            return UnLockItemResult.Success;
        }
        return UnLockItemResult.Fail;
    }
}
