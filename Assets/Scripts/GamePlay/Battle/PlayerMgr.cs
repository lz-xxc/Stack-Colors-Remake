using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;

/// <summary>
/// 玩家控制器 
/// </summary>
public class PlayerMgr : SingletonMonoBehavior<PlayerMgr> {
    private GameObject playerPref = null;
    public GameObject player = null;
    public Transform keepPickUpTran;
    private Transform pickupTool;
    public PlayerData playerData { get; private set; }

    private PlayerView playerView;

    //速度
    private float currentForwSpeed;

    //到达终点
    float currentFinalTime = 0;

    private Tweener moveTweener;

    //初始化
    public void Init() {
        playerData = new PlayerData();
        InitMsg();
        Send.SendMsg(SendType.UseItemChange, playerData.skinName);
    }

    //清除数据
    public void Clear() {
        if (playerData != null && playerData.keepPickUps != null) {
            foreach (GameObject obj in playerData.keepPickUps) {
                if (obj != null) {
                    Rigidbody rb = obj.GetComponent<Rigidbody>();
                    if (rb != null) {
                        rb.isKinematic = true;
                    }
                    obj.transform.rotation = Quaternion.identity;
                    ObjectPool.Instance.Recycle(obj, true);
                }
            }
        }

        if (player != null) {
            GameObject.Destroy(player);
            player = null;
        }

        playerData.Reset();
        currentForwSpeed = 0;
        keepPickUpTran = null;
        pickupTool = null;

        if (playerView != null) {
            playerView.ResetView();
        }
    }

    //注册消息 - 保持不变
    public void InitMsg() {
        Send.RegisterMsg(SendType.PlayerColorChange, PlayerChangeColor);
        Send.RegisterMsg(SendType.EnergyMax, OnEnergyMax);
        Send.RegisterMsg(SendType.EnergyEmpty, OnEnergyEmpty);
        Send.RegisterMsg(SendType.UseItemChange, OnPlayerSkinChange);
        Send.RegisterMsg(SendType.CtrlDrag, OnCtrlDrag);
    }

    //反注册消息 - 保持不变
    public void ClearMsg() {
        Send.UnregisterMsg(SendType.PlayerColorChange, PlayerChangeColor);
        Send.UnregisterMsg(SendType.EnergyMax, OnEnergyMax);
        Send.UnregisterMsg(SendType.EnergyEmpty, OnEnergyEmpty);
        Send.UnregisterMsg(SendType.UseItemChange, OnPlayerSkinChange);
        Send.UnregisterMsg(SendType.CtrlDrag, OnCtrlDrag);
    }

    //开始游戏时调用 - 保持不变
    public void StartBattle() {
        playerData = new PlayerData();
        CreatePlayer();
        pickupTool = player.transform.Find("PickupTool");
        keepPickUpTran = player.transform.Find("KeepPickUp");
        currentForwSpeed = playerData.forwardSpeed;
        Send.SendMsg(SendType.PlayerColorChange, playerData.colorIndex);
    }

    //Update函数 - 保持不变
    public void OnUpdate() {
        switch (playerData.playerState) {
            case (E_PlayerState.Pickup):
                CameraMgr.Instance.FollowObj(player.transform);
                break;
            case (E_PlayerState.Speedup):
                if (Input.GetMouseButtonDown(0)) {
                    OnAddSpeedClick();
                }
                SpeedupReduce();
                CameraMgr.Instance.FollowObj(player.transform);
                BattleWindow.Instance.ForceAmount((currentForwSpeed - playerData.minSpeedup) / (playerData.maxSpeedup - playerData.minSpeedup));
                if (player.transform.position.z >= playerData.targetPosZ) {
                    EnterFinishMode();
                }
                break;
            case (E_PlayerState.Finish):
                if (!playerData.isForce) {
                    playerData.isForce = true;
                    AddRigidBody();
                    currentForwSpeed = 0;
                }
                bool moveOver = CameraMgr.Instance.LerpMove();
                if (moveOver) {
                    currentFinalTime += Time.deltaTime;
                }
                if (currentFinalTime >= playerData.finalTime) {
                    Debug.Log(1);
                    GameStateMgr.Instance.SwitchState(GameState.GameOver);
                    BattleMgr.Instance.state = BattleState.GameOver;
                }
                BattleWindow.Instance.HideForceBar();
                break;
        }
        AutoMoveForwardSpeed();
    }

    private void OnAddSpeedClick() {
        currentForwSpeed += playerData.clickAddSpeed;
        if (currentForwSpeed > playerData.maxSpeedup)
            currentForwSpeed = playerData.maxSpeedup;
    }

    private void SpeedupReduce() {
        currentForwSpeed -= Time.deltaTime * playerData.reduceSpeedup;
        if (currentForwSpeed < playerData.minSpeedup)
            currentForwSpeed = playerData.minSpeedup;
    }

    public void EnterSpeedupMode() {
        if (PickUpMgr.Instance.pickupData.isEnergyMax) {
            EnergyMgr.Instance.Energy = 0;
        }
        if (playerData.playerState != E_PlayerState.Speedup)
            playerData.playerState = E_PlayerState.Speedup;
        BattleWindow.Instance.ShowForceBar();
        currentForwSpeed = playerData.minSpeedup;
    }

    public void EnterFinishMode() {
        if (playerData.playerState != E_PlayerState.Finish) {
            playerData.playerState = E_PlayerState.Finish;
        }
        playerData.finalForceRate = ((currentForwSpeed - playerData.minSpeedup) / (playerData.maxSpeedup - playerData.minSpeedup));
    }

    public void CreatePlayer() {
        if (playerPref != null && player == null) {
            player = GameObject.Instantiate<GameObject>(playerPref, playerData.initPos, Quaternion.identity);
            if (playerView == null) {
                playerView = player.GetComponent<PlayerView>();
                if (playerView == null) {
                    playerView = player.AddComponent<PlayerView>();
                }
            }
        }
    }

    public void AutoMoveForwardSpeed() {
        if (playerView != null) {
            playerView.MoveForward(currentForwSpeed);
        }
        else {
            player.transform.Translate(Vector3.forward * Time.deltaTime * currentForwSpeed, Space.Self);
        }
        BattleWindow.Instance.ProcessAmount(player.transform.position.z / RoadMgr.Instance.GetTotalDistence());
    }

    public void setTargetZ(float z) {
        playerData.targetPosZ = z;
    }

    public void PlayerChangeColor(object[] _obj) {
        playerData.colorIndex = (int)_obj[0];
        Material mat = ColorProxy.Instance.materials[playerData.colorIndex];

        if (playerView != null) {
            playerView.UpdateColor(mat);
        }
        else {
            // 备用方案
            player.GetComponent<Renderer>().sharedMaterial = mat;
            pickupTool.GetComponent<Renderer>().sharedMaterial = mat;
            foreach (Transform pickup in keepPickUpTran) {
                pickup.GetComponent<Renderer>().sharedMaterial = mat;
            }
        }
    }

    public void OnEnergyMax(object[] _obj) {
        if (playerView != null) {
            playerView.ScaleTool(true, player?.name);
        }
        else {
            // 备用方案
            if (player?.name == "Player4(Clone)")
                pickupTool.DOScaleX(5, 0.5f);
            else
                pickupTool.DOScaleX(10, 0.5f);
        }
        currentForwSpeed += 5;
        PickUpMgr.Instance.pickupData.isEnergyMax = true;
    }

    public void OnEnergyEmpty(object[] _obj) {
        if (playerView != null) {
            playerView.ScaleTool(false, player?.name);
        }
        else {
            if (player?.name == "Player4(Clone)")
                pickupTool.DOScaleX(1, 0.5f);
            else
                pickupTool.DOScaleX(2, 0.5f);
        }
        currentForwSpeed = playerData.forwardSpeed;
        PickUpMgr.Instance.pickupData.isEnergyMax = false;
    }

    // AddPickUp - 保持不变
    public void AddPickUp(GameObject pickup) {
        if (pickup == null) return;
        if (!playerData.keepPickUps.Contains(pickup)) {
            playerData.keepPickUps.Add(pickup);
        }
    }

    // RemovePickUp - 保持不变
    public GameObject RemovePickUp() {
        if (playerData.keepPickUps.Count == 0)
            return null;
        if (playerData.keepPickUps.Count > 0) {
            ObjectPool.Instance.Recycle(playerData.keepPickUps[playerData.keepPickUps.Count - 1], true);
            playerData.keepPickUps.RemoveAt(playerData.keepPickUps.Count - 1);
            if (playerData.keepPickUps.Count > 0)
                return playerData.keepPickUps[playerData.keepPickUps.Count - 1];
            else
                return null;
        }
        return null;
    }

    public void AddRigidBody() {
        LaunchAllPickups();
    }

    public void ResetCurrentTime() {
        currentFinalTime = 0;
    }

    // OnCtrlDrag - 保持不变
    public void OnCtrlDrag(object[] _obj) {
        PointerEventData data = (PointerEventData)_obj[0];
        Vector3 delta = data.delta;
        if (delta.x != 0) {
            player.transform.position += new Vector3(delta.x * playerData.horizontalSpeed, 0, 0);
            float roadWidth = RoadMgr.Instance.roadData.roadWidth;
            if (player.transform.position.x > roadWidth / 2) {
                player.transform.position = new Vector3(roadWidth / 2, player.transform.position.y, player.transform.position.z);
            }
            else if (player.transform.position.x < -(roadWidth / 2)) {
                player.transform.position = new Vector3(-(roadWidth / 2), player.transform.position.y, player.transform.position.z);
            }
        }
    }

    // OnPlayerSkinChange 
    private void OnPlayerSkinChange(object[] _obj) {
        playerData.skinName = (string)_obj[0];
        playerPref = LocalAssetMgr.Instance.Load_Prefab(playerData.skinName);
    }

    /// <summary>
    /// 发射所有拾取物
    /// </summary>
    private void LaunchAllPickups() {
        if (keepPickUpTran == null) return;

        for (int i = 0; i < playerData.keepPickUps.Count; i++) {
            LaunchPickup(playerData.keepPickUps[playerData.keepPickUps.Count - 1 - i], i);
        }
    }

    private void LaunchPickup(GameObject pickup, int index) {
        if (pickup == null) return;

        // 使用 finalForceRate 在 minForce 和 maxForce 之间插值，保证最低也有推力
        float forwardForce = Mathf.Lerp(playerData.minForce, playerData.maxForce, playerData.finalForceRate) + index * 0.1f;

        // if (playerView != null) {
        //     playerView.LaunchPickup(pickup, Vector3.forward * forwardForce, 3f + (index * 0.02f));
        // }
        // else {
        // 备用方案
        pickup.transform.SetParent(null);
        Rigidbody rb = pickup.GetComponent<Rigidbody>();
        if (rb == null) {
            rb = pickup.AddComponent<Rigidbody>();
        }
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.mass = 2f + (index * 0.02f);
        rb.drag = 0.15f;
        rb.angularDrag = 0.1f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.None;
        rb.AddForce(Vector3.forward * forwardForce, ForceMode.Impulse);
        //  }
    }
}