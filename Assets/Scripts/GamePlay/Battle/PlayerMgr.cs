using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

enum E_PlayerState {
    Pickup,
    Speedup,
    Finish,
}

/// <summary>
/// 玩家控制器 
/// </summary>
public class PlayerMgr : SingletonMonoBehavior<PlayerMgr> {
    private GameObject playerPref = null;
    public GameObject player = null;
    public Transform keepPickUpTran;
    public Transform pickupTool;
    private E_PlayerState playerState = E_PlayerState.Pickup;

    string skinName = "Player1";

    //速度
    private float forwardSpeed = 20;
    private float currentForwSpeed;
    private float horizontalSpeed = 0.1f;

    //位置
    private Vector3 initPos = new Vector3(0, 1, 0);//玩家初始位置

    //玩家水平移动x轴
    private float leftX = -3;
    private float midX = 0;
    private float rightX = 3;
    //玩家移动目标
    private float[] targetXPos;
    private int currentXIndex;
    private float targetX;

    //玩家颜色
    public int colorIndex { get; private set; } = 1;
    private List<GameObject> keepPickUps = new List<GameObject>();

    private int maxEnergyAddScale = 10;


    //到达终点
    private bool isAtFinishRoad = false;
    private float maxForce = 1f;
    private float clickAddForce = 0.5f;
    private float finalForce = 0.1f;
    float targetPosZ = 1000;
    float currentFinalTime = 0;
    float finalTime = 3;
    private bool isForce = false;
    //到加速地段
    private bool isAtSpeedupRoad = false;
    private float maxSpeedup = 20;//最大速度
    private float minSpeedup = 5;//最小速度
    private float clickAddSpeed = 4;//点击增加速度
    private float reduceSpeedup = 2;//加速通道衰减速度
    private float finishReduceSpeed = 0.01f;//完成通道衰减速度

    private Tweener moveTweener;

    //初始化
    public void Init() {
        currentForwSpeed = forwardSpeed;
        targetXPos = new float[] { leftX, midX, rightX };
        InitMsg();
        string skinName = ShopMgr.Instance.GetItemName(ShopMgr.Instance.UseItemId);
        Send.SendMsg(SendType.UseItemChange, skinName);
    }

    //清除数据
    public void Clear() {
        GameObject.Destroy(player);
        player = null;

        foreach (GameObject obj in keepPickUps) {
            ObjectPool.Instance.Recycle(obj);
        }

        currentForwSpeed = forwardSpeed;

        targetX = targetXPos[1];
    }

    //注册消息
    public void InitMsg() {
        Send.RegisterMsg(SendType.PlayerColorChange, PlayerChangeColor);
        Send.RegisterMsg(SendType.EnergyMax, OnEnergyMax);
        Send.RegisterMsg(SendType.EnergyEmpty, OnEnergyEmpty);
        Send.RegisterMsg(SendType.UseItemChange, OnPlayerSkinChange);
    }

    //反注册消息
    public void ClearMsg() {
        Send.UnregisterMsg(SendType.PlayerColorChange, PlayerChangeColor);
        Send.UnregisterMsg(SendType.EnergyMax, OnEnergyMax);
        Send.UnregisterMsg(SendType.EnergyEmpty, OnEnergyEmpty);
        Send.UnregisterMsg(SendType.UseItemChange, OnPlayerSkinChange);
    }

    //开始游戏时调用，根据需求实现，需要在Battle.StartBattle()中调用
    public void StartBattle() {
        CreatePlayer();
        pickupTool = player.transform.Find("PickupTool");
        keepPickUpTran = player.transform.Find("KeepPickUp");
        Send.SendMsg(SendType.PlayerColorChange, colorIndex);
    }

    //Update函数，根据需求实现，需要在Launch.Update()中调用
    public void OnUpdate() {
        switch (playerState) {
            case (E_PlayerState.Pickup):
                HandleMoveHorizontal();
                LerpMoveX();
                CameraMgr.Instance.FollowObj(player.transform);
                break;
            case (E_PlayerState.Speedup):
                if (Input.GetMouseButtonDown(0)) {
                    OnAddSpeedClick();
                }
                SpeedupReduce();
                CameraMgr.Instance.FollowObj(player.transform);
                BattleWindow.Instance.ForceAmount((currentForwSpeed - minSpeedup) / (maxSpeedup - minSpeedup));
                if (player.transform.position.z >= targetPosZ) {
                    EnterFinishMode();
                }
                break;
            case (E_PlayerState.Finish):
                if (!isForce) {
                    isForce = true;
                    AddRigidBody();
                    currentForwSpeed = 0;
                }
                currentFinalTime += Time.deltaTime;
                if (currentFinalTime >= finalTime) {
                    GameStateMgr.Instance.SwitchState(GameState.GameOver);
                    BattleMgr.Instance.state = BattleState.GameOver;
                }
                BattleWindow.Instance.HideForceBar();
                break;

        }
        AutoMoveForwardSpeed();
    }

    private void OnAddSpeedClick() {
        currentForwSpeed += clickAddSpeed;
        if (currentForwSpeed > maxSpeedup)
            currentForwSpeed = maxSpeedup;
    }

    private void SpeedupReduce() {
        currentForwSpeed -= Time.deltaTime * reduceSpeedup;
        if (currentForwSpeed < minSpeedup)
            currentForwSpeed = minSpeedup;
    }

    public void EnterSpeedupMode() {
        if (playerState != E_PlayerState.Speedup)
            playerState = E_PlayerState.Speedup;
        BattleWindow.Instance.ShowForceBar();
        currentForwSpeed = minSpeedup;
    }

    public void EnterFinishMode() {
        if (playerState != E_PlayerState.Finish)
            playerState = E_PlayerState.Finish;

        finalForce = (currentForwSpeed - minSpeedup) / (maxSpeedup - minSpeedup) * maxForce;

    }

    public void CreatePlayer() {
        if (playerPref != null && player == null) {
            player = GameObject.Instantiate<GameObject>(playerPref, initPos, Quaternion.identity);
        }
    }

    public void AutoMoveForwardSpeed() {
        player.transform.Translate(Vector3.forward * Time.deltaTime * currentForwSpeed, Space.Self);
        BattleWindow.Instance.ProcessAmount(player.transform.position.z / RoadMgr.Instance.GetTotalDistence());
    }

    public void HandleMoveHorizontal() {
        if (Input.GetKeyDown(KeyCode.A)) {
            currentXIndex = Math.Max(currentXIndex - 1, 0);
            targetX = targetXPos[currentXIndex];
        }

        if (Input.GetKeyDown(KeyCode.D)) {
            currentXIndex = Math.Min(currentXIndex + 1, targetXPos.Length - 1);
            targetX = targetXPos[currentXIndex];
        }
    }

    private void LerpMoveX() {
        Vector3 playerPos = player.transform.position;
        if (playerPos.x != targetX)
            playerPos.x = Mathf.Lerp(playerPos.x, targetX, horizontalSpeed);
        player.transform.position = playerPos;
    }

    public void setTargetZ(float z) {
        targetPosZ = z;
    }

    public float GetLaneX(int index) {
        switch (index) {
            case 0:
                return leftX;
            case 1:
                return midX;
            case 2:
                return rightX;
        }
        return 0;
    }

    public float[] GetAllLaneX() {
        return targetXPos;
    }


    public void PlayerChangeColor(object[] _obj) {
        int colorIndex = (int)_obj[0];
        Color color = new Color();
        switch (colorIndex) {
            case (0):
                color = Color.yellow;
                break;
            case (1):
                color = Color.red;
                break;
            case (2):
                color = Color.green;
                break;
        }
        player.GetComponent<Renderer>().material.color = color;
        pickupTool.GetComponent<Renderer>().material.color = color;
        foreach (Transform pickup in keepPickUpTran) {
            pickup.GetComponent<Renderer>().material.color = color;
        }
        this.colorIndex = colorIndex;
    }

    public void OnEnergyMax(object[] _obj) {
        pickupTool.DOScaleX(10, 0.5f);
        currentForwSpeed += 5;
        PickUpMgr.Instance.isEnergyMax = true;
    }


    public void OnEnergyEmpty(object[] _obj) {
        pickupTool.DOScaleX(2, 0.5f);
        currentForwSpeed = forwardSpeed;
        PickUpMgr.Instance.isEnergyMax = false;
    }

    public void AddPickUp(GameObject pickup) {
        if (pickup == null) return;
        if (!keepPickUps.Contains(pickup)) {
            keepPickUps.Add(pickup);
        }
    }

    public GameObject RemovePickUp() {
        if (keepPickUps.Count > 0) {
            ObjectPool.Instance.Recycle(keepPickUps[keepPickUps.Count - 1], true);
            keepPickUps.RemoveAt(keepPickUps.Count - 1);
            if (keepPickUps.Count == 0)
                return null;
            return keepPickUps[keepPickUps.Count - 1];
        }
        return null;
    }

    public void AddRigidBody() {
        foreach (GameObject pickup in keepPickUps) {
            LaunchAllPickups();
        }
    }

    public void ResetCurrentTime() {
        currentFinalTime = 0;
    }

    public void OnCtrlDrag(object[] _obj) {
        PointerEventData data = (PointerEventData)_obj[0];
        Vector3 point = data.position;
    }


    /// <summary>
    /// 发射所有拾取物（自然倒塌 + 滑行）
    /// </summary>
    private void LaunchAllPickups() {
        if (keepPickUpTran == null) return;

        for (int i = 0; i < keepPickUps.Count; i++) {
            LaunchPickup(keepPickUps[keepPickUps.Count - 1 - i], i);
        }
    }

    private void LaunchPickup(GameObject pickup, int index) {
        if (pickup == null) return;

        pickup.transform.SetParent(null);

        Rigidbody rb = pickup.GetComponent<Rigidbody>();
        if (rb == null) {
            rb = pickup.AddComponent<Rigidbody>();
        }

        // 物理参数
        rb.useGravity = true;
        rb.mass = 2f + index * 0.1f;
        rb.drag = 0.3f;
        rb.angularDrag = 0.3f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.None;

        // 1. 向前的力（底部大，顶部小）
        float forwardForce = finalForce; // + (0.1f / (index + 1));
        rb.AddForce(Vector3.forward * forwardForce, ForceMode.Impulse);
    }

    private void OnPlayerSkinChange(object[] _obj) {
        skinName = (string)_obj[0];
        playerPref = LocalAssetMgr.Instance.Load_Prefab(skinName);
    }
}