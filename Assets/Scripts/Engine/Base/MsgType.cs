using UnityEngine;
using System.Collections;


public delegate void BroadcastCallBack(params object[] _objs);

/// <summary>
/// 消息类型枚举 不可重复  管理类下发用SendType
/// </summary>
public enum SendType {
    TimeUpdate = 1,
    LoadingProgress,
    ShowRate,
    //change结尾 第一个参数固定是变化值，第二个参数是总值，第三个视情况而定
    SignChane,
    GradeChange,
    ExpChange,
    GoldChange,
    ScoreChange,
    RoleChange,
    SignDayChange,
    ProgressChange,
    PickupColorChange,
    PlayerColorChange,
    EnergyChange,
    PlayerModeChange,
    //shop
    TryUnLockItem,
    UnLockItemSuccess,
    UseItemChange,
    ClickShopIconChange,
    //task
    TaskComplete,
    TaskValueChange,
    LangChange,
    // topdata
    ShowFlyCoin,
    // Level
    LevelPass,
    UseProp,
    PropChange,
    // Recycle
    RecycleRoad,
    //Energy
    EnergyMax,
    EnergyEmpty,
    //EventTrigger
    CtrlDrag,
    //Game
    Pickup,
    EnterSpeedupRoad,
    //Rate
    TryUpdateRate,
    UpdateRate,
}

