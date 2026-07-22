using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_PlayerState {
    Pickup,
    Speedup,
    Finish,
}

public class PlayerData {
    // 状态
    public E_PlayerState playerState;
    public string skinName;

    // 速度
    public float forwardSpeed;
    public float horizontalSpeed;

    // 位置
    public Vector3 initPos;

    // 玩家颜色
    public int colorIndex;

    // 玩家当前拾取物列表
    public List<GameObject> keepPickUps;

    //玩家前进速度
    public float currentForwSpeed;

    // 到达终点相关
    public float maxForce { get; private set; }
    public float minForce { get; private set; }
    public float clickAddForce { get; private set; }
    public float finalForceRate;
    public float targetPosZ;
    public float currentFinalTime;
    public float finalTime { get; private set; }
    public bool isForce;
    public bool moveOver;

    // 加速地段相关
    public bool isAtSpeedupRoad;
    public float maxSpeedup { get; private set; }
    public float minSpeedup { get; private set; }
    public float clickAddSpeed { get; private set; }
    public float reduceSpeedup { get; private set; }

    public PlayerData() {
        // 状态和标识
        playerState = E_PlayerState.Pickup;
        skinName = "Player1";
        isForce = false;
        isAtSpeedupRoad = false;

        // 速度参数
        currentForwSpeed = 20;
        forwardSpeed = 20f;
        horizontalSpeed = 0.02f;

        // 位置
        initPos = new Vector3(0, 1, 0);

        // 颜色
        colorIndex = 1;

        // 引用类型 
        keepPickUps = new List<GameObject>();

        // 终点相关
        maxForce = 30f;
        minForce = 5f;
        clickAddForce = 0.5f;
        finalForceRate = 1f;
        targetPosZ = 1000f;
        currentFinalTime = 0f;
        finalTime = 2.5f;
        moveOver = false;

        // 加速相关
        maxSpeedup = 20f;
        minSpeedup = 6f;
        clickAddSpeed = 3f;
        reduceSpeedup = 3f;
    }

    public void AddPickUp(GameObject pickup) {
        if (!keepPickUps.Contains(pickup)) {
            keepPickUps.Add(pickup);
        }
    }

    public GameObject RemovePickUp() {
        if (keepPickUps.Count == 0) return null;
        GameObject last = keepPickUps[keepPickUps.Count - 1];
        keepPickUps.RemoveAt(keepPickUps.Count - 1);
        return last;
    }

    // 重置方法
    public void Reset() {
        playerState = E_PlayerState.Pickup;
        isForce = false;
        isAtSpeedupRoad = false;
        currentForwSpeed = 0;
        forwardSpeed = 20f;
        horizontalSpeed = 0.02f;
        initPos = new Vector3(0, 1, 0);
        colorIndex = 1;
        keepPickUps.Clear();
        finalForceRate = 1f;
        targetPosZ = 1000f;
        currentFinalTime = 0f;
        isForce = false;
        moveOver = false;
    }
}