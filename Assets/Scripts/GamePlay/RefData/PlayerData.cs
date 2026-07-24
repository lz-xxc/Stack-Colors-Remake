using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Model - 玩家数据与业务规则
/// </summary>
public enum E_PlayerState {
    Pickup,
    Speedup,
    Finish,
}

public class PlayerData {

    // ============ 配置数据（不变/仅初始化）============

    /// <summary>默认前进速度</summary>
    public float forwardSpeed { get; private set; } = 20f;

    /// <summary>横向拖拽速度系数</summary>
    public float horizontalSpeed { get; private set; } = 0.02f;

    /// <summary>玩家初始位置</summary>
    public Vector3 initPos { get; private set; } = new Vector3(0, 1, 0);

    /// <summary>默认皮肤名</summary>
    public string defaultSkinName { get; private set; } = "Player1";

    /// <summary>默认颜色索引</summary>
    public int defaultColorIndex { get; private set; } = 1;

    /// <summary>能量满时额外加速</summary>
    public float energyMaxSpeedBonus { get; private set; } = 5f;

    // --- 加速路段配置 ---
    public float maxSpeedup { get; private set; } = 20f;
    public float minSpeedup { get; private set; } = 6f;
    public float clickAddSpeed { get; private set; } = 3f;
    public float reduceSpeedup { get; private set; } = 3f;

    // --- 终点发射配置 ---
    public float maxForce { get; private set; } = 30f;
    public float minForce { get; private set; } = 5f;
    public float clickAddForce { get; private set; } = 0.5f;
    public float finalTime { get; private set; } = 2.5f;
    public float defaultTargetPosZ { get; private set; } = 1000f;

    // ============ 运行时数据（随游戏变化）============

    /// <summary>当前状态</summary>
    public E_PlayerState playerState { get; set; } = E_PlayerState.Pickup;

    /// <summary>当前皮肤名</summary>
    public string skinName { get; set; }

    /// <summary>当前颜色索引</summary>
    public int colorIndex { get; set; }

    /// <summary>当前前进速度</summary>
    public float currentForwSpeed { get; set; }

    /// <summary>是否在加速路段</summary>
    public bool isAtSpeedupRoad { get; set; }

    /// <summary>终点目标 Z 坐标</summary>
    public float targetPosZ { get; set; }

    /// <summary>终点推力比率（0~1）</summary>
    public float finalForceRate { get; set; } = 1f;

    /// <summary>是否已触发终点发射</summary>
    public bool isForce { get; set; }

    /// <summary>镜头移动是否开始</summary>
    public bool cameraMoveStart { get; set; }

    /// <summary>镜头移动是否结束</summary>
    public bool moveOver { get; set; }

    /// <summary>终点计时</summary>
    public float currentFinalTime { get; set; }

    // ============ 拾取物数据 ============

    private readonly Stack<PickupInfo> keepPickUps = new Stack<PickupInfo>();

    /// <summary>当前堆叠拾取物数量</summary>
    public int pickUpCount => keepPickUps.Count;

    // ============ 构造函数 ============

    public PlayerData() {
        colorIndex = defaultColorIndex;
        currentForwSpeed = forwardSpeed;
        targetPosZ = defaultTargetPosZ;
    }

    // ============ 拾取物方法（仅数据层，不含对象池/View）============

    public Stack<PickupInfo> GetKeepPickups() {
        return keepPickUps;
    }

    public void AddPickUp(PickupInfo pickup) {
        if (pickup == null) return;
        if (!keepPickUps.Contains(pickup)) {
            keepPickUps.Push(pickup);
        }
    }

    /// <summary>移除栈顶拾取物并返回（不回收对象）</summary>
    public void RemovePickUp() {
        if (keepPickUps.Count != 0)
            keepPickUps.Pop();

    }

    public void ClearPickUps() {
        keepPickUps.Clear();
    }

    public bool IsSameColor(int colorIndex) {
        return this.colorIndex == colorIndex;
    }

    public float GetToolScale(bool isMax) {
        if (isMax) {
            return skinName == "Player4" ? 5f : 10f;
        }
        else {
            return skinName == "Player4" ? 1f : 2f;
        }
    }

    // ============ 状态切换 ============

    public void EnterSpeedup() {
        if (playerState != E_PlayerState.Speedup) {
            playerState = E_PlayerState.Speedup;
        }
        currentForwSpeed = minSpeedup;
    }

    public void EnterFinish() {
        if (playerState != E_PlayerState.Finish) {
            playerState = E_PlayerState.Finish;
        }
        finalForceRate = CalcSpeedupRate();
    }

    public void BeginFinishLaunch() {
        if (isForce) return;

        isForce = true;
        currentForwSpeed = 0f;
    }

    // ============ 速度规则 ============

    public void OnSpeedupClick() {
        currentForwSpeed += clickAddSpeed;
        currentForwSpeed = Mathf.Min(currentForwSpeed, maxSpeedup);
    }

    public void TickSpeedupReduce(float deltaTime) {
        currentForwSpeed -= deltaTime * reduceSpeedup;
        currentForwSpeed = Mathf.Max(currentForwSpeed, minSpeedup);
    }

    public void ApplyEnergyMaxBonus() {
        currentForwSpeed += energyMaxSpeedBonus;
    }

    public void ResetForwardSpeed() {
        currentForwSpeed = forwardSpeed;
    }

    public void StartBattleSpeed() {
        currentForwSpeed = forwardSpeed;
    }

    // ============ 终点规则 ============

    public void TickFinalTime(float deltaTime) {
        if (moveOver) {
            currentFinalTime += deltaTime;
        }
    }

    public bool IsFinalTimeOver() {
        return currentFinalTime >= finalTime;
    }

    public void ResetFinalTime() {
        currentFinalTime = 0f;
    }

    public bool ReachedTarget(float currentPosZ) {
        return currentPosZ >= targetPosZ;
    }

    // ============ 计算/查询 ============

    /// <summary>当前加速进度 0~1，供 UI 进度条使用</summary>
    public float CalcSpeedupRate() {
        if (Mathf.Approximately(maxSpeedup, minSpeedup)) return 0f;
        return (currentForwSpeed - minSpeedup) / (maxSpeedup - minSpeedup);
    }

    /// <summary>计算单个拾取物发射力度</summary>
    public float CalcLaunchForce(int index) {
        return Mathf.Lerp(minForce, maxForce, finalForceRate)
               + index * ((3f * finalForceRate) + 1);
    }

    /// <summary>计算拾取物发射质量</summary>
    public float CalcLaunchMass(int index) {
        return 5f + (index * 0.01f);
    }

    // ============ 重置 ============

    public void Reset() {
        playerState = E_PlayerState.Pickup;
        colorIndex = defaultColorIndex;

        currentForwSpeed = 0f;
        isAtSpeedupRoad = false;

        targetPosZ = defaultTargetPosZ;
        finalForceRate = 1f;
        isForce = false;
        moveOver = false;
        cameraMoveStart = false;
        currentFinalTime = 0f;

        ClearPickUps();
    }
}
