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
    private float horizontalSpeed = 0.02f;

    //位置
    private Vector3 initPos = new Vector3(0, 1, 0);//玩家初始位置

    //玩家颜色
    public int colorIndex { get; private set; } = 1;
    private List<GameObject> keepPickUps = new List<GameObject>();

    private int maxEnergyAddScale = 10;


    //到达终点
    private bool isAtFinishRoad = false;
    private float maxForce = 0.5f;
    private float clickAddForce = 0.5f;
    private float finalForce = 1f;
    float targetPosZ = 1000;
    float currentFinalTime = 0;
    float finalTime = 3;
    private bool isForce = false;
    //到加速地段
    private bool isAtSpeedupRoad = false;
    private float maxSpeedup = 20;//最大速度
    private float minSpeedup = 4;//最小速度
    private float clickAddSpeed = 3;//点击增加速度
    private float reduceSpeedup = 2;//加速通道衰减速度
    private float finishReduceSpeed = 0.01f;//完成通道衰减速度

    private Tweener moveTweener;

    //初始化
    public void Init() {
        currentForwSpeed = forwardSpeed;
        InitMsg();
        string skinName = ShopMgr.Instance.GetItemName(ShopMgr.Instance.UseItemId);
        Send.SendMsg(SendType.UseItemChange, skinName);
    }

    //清除数据
    public void Clear() {
        GameObject.Destroy(player);
        player = null;

        foreach (GameObject obj in keepPickUps) {
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null) {
                rb.isKinematic = true;
            }
            obj.transform.rotation = Quaternion.identity;
            ObjectPool.Instance.Recycle(obj, true);
        }

        currentFinalTime = 0;
        currentForwSpeed = forwardSpeed;

    }

    //注册消息
    public void InitMsg() {
        Send.RegisterMsg(SendType.PlayerColorChange, PlayerChangeColor);
        Send.RegisterMsg(SendType.EnergyMax, OnEnergyMax);
        Send.RegisterMsg(SendType.EnergyEmpty, OnEnergyEmpty);
        Send.RegisterMsg(SendType.UseItemChange, OnPlayerSkinChange);
        Send.RegisterMsg(SendType.CtrlDrag, OnCtrlDrag);
    }

    //反注册消息
    public void ClearMsg() {
        Send.UnregisterMsg(SendType.PlayerColorChange, PlayerChangeColor);
        Send.UnregisterMsg(SendType.EnergyMax, OnEnergyMax);
        Send.UnregisterMsg(SendType.EnergyEmpty, OnEnergyEmpty);
        Send.UnregisterMsg(SendType.UseItemChange, OnPlayerSkinChange);
        Send.UnregisterMsg(SendType.CtrlDrag, OnCtrlDrag);
    }

    //开始游戏时调用，根据需求实现，需要在Battle.StartBattle()中调用
    public void StartBattle() {
        playerState = E_PlayerState.Pickup;
        CreatePlayer();
        pickupTool = player.transform.Find("PickupTool");
        keepPickUpTran = player.transform.Find("KeepPickUp");
        Send.SendMsg(SendType.PlayerColorChange, colorIndex);
    }

    //Update函数，根据需求实现，需要在Launch.Update()中调用
    public void OnUpdate() {
        switch (playerState) {
            case (E_PlayerState.Pickup):
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
                bool moveOver = CameraMgr.Instance.LerpMove();
                if (moveOver) {
                    currentFinalTime += Time.deltaTime;
                }
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
        if (PickUpMgr.Instance.isEnergyMax) {
            EnergyMgr.Instance.Energy = 0;
        }
        if (playerState != E_PlayerState.Speedup)
            playerState = E_PlayerState.Speedup;
        BattleWindow.Instance.ShowForceBar();
        currentForwSpeed = minSpeedup;

    }

    public void EnterFinishMode() {
        if (playerState != E_PlayerState.Finish)
            playerState = E_PlayerState.Finish;

        finalForce = ((currentForwSpeed - minSpeedup) / (maxSpeedup - minSpeedup)) * maxForce;

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

    #region 键盘移动（弃用）
    // public void HandleMoveHorizontal() {
    //     if (Input.GetKeyDown(KeyCode.A)) {
    //         currentXIndex = Math.Max(currentXIndex - 1, 0);
    //         targetX = targetXPos[currentXIndex];
    //     }

    //     if (Input.GetKeyDown(KeyCode.D)) {
    //         currentXIndex = Math.Min(currentXIndex + 1, targetXPos.Length - 1);
    //         targetX = targetXPos[currentXIndex];
    //     }
    // }

    // private void LerpMoveX() {
    //     Vector3 playerPos = player.transform.position;
    //     if (playerPos.x != targetX)
    //         playerPos.x = Mathf.Lerp(playerPos.x, targetX, horizontalSpeed);
    //     player.transform.position = playerPos;
    // }
    #endregion

    public void setTargetZ(float z) {
        targetPosZ = z;
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
        if (player?.name == "Player4(Clone)")
            pickupTool.DOScaleX(5, 0.5f);
        else
            pickupTool.DOScaleX(10, 0.5f);
        currentForwSpeed += 5;
        PickUpMgr.Instance.isEnergyMax = true;
    }


    public void OnEnergyEmpty(object[] _obj) {
        if (player?.name == "Player4(Clone)")
            pickupTool.DOScaleX(1, 0.5f);
        else
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
        if (keepPickUps.Count == 0)
            return null;
        if (keepPickUps.Count > 0) {
            ObjectPool.Instance.Recycle(keepPickUps[keepPickUps.Count - 1], true);
            keepPickUps.RemoveAt(keepPickUps.Count - 1);
            if (keepPickUps.Count > 0)
                return keepPickUps[keepPickUps.Count - 1];
            else
                return null;
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
        Vector3 delta = data.delta;
        if (delta.x != 0) {
            player.transform.position += new Vector3(delta.x * horizontalSpeed, 0, 0);
            float roadWidth = RoadMgr.Instance.roadWidth;
            if (player.transform.position.x > roadWidth / 2) {
                player.transform.position = new Vector3(roadWidth / 2, player.transform.position.y, player.transform.position.z);
            }
            else if (player.transform.position.x < -(roadWidth / 2)) {
                player.transform.position = new Vector3(-(roadWidth / 2), player.transform.position.y, player.transform.position.z);
            }
        }
    }


    private void OnPlayerSkinChange(object[] _obj) {
        skinName = (string)_obj[0];
        playerPref = LocalAssetMgr.Instance.Load_Prefab(skinName);
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
        rb.isKinematic = false;

        int type = pickup.GetComponent<PickUpView>().type;

        // ✅ 物理参数（优化后）
        rb.useGravity = true;
        rb.mass = 2.5f;           // 减轻质量，更容易被推动
        rb.drag = 0.15f;                              // 减小空气阻力，滑行更远
        rb.angularDrag = 0.1f;                        // 减小角阻力，旋转更持久
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.None;

        // ✅ 向前的力（底部大，顶部小）- 优化力度分布
        float forwardForce = finalForce + (1.5f / (index + 1));  // 减小基础力，增大顶部补偿
        rb.AddForce(Vector3.forward * forwardForce, ForceMode.Impulse);
    }
}