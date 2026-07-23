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
    private GameObject player = null;
    private Transform keepPickUpTran;
    private Transform pickupTool;
    public PlayerData playerData { get; private set; }

    private PlayerView playerView;

    public int ColorIndex {
        get {
            return playerData.colorIndex;
        }
        private set { }
    }

    private Tweener moveTweener;

    //初始化
    public void Init() {
        InitMsg();
        playerData = new PlayerData();
        ShopMgr.Instance.UseItemId = ShopMgr.Instance.UseItemId;
    }

    //清除数据
    public void Clear() {
        if (playerData != null && playerData.GetKeepPickups() != null) {
            foreach (GameObject obj in playerData.GetKeepPickups()) {
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

    //反注册消息
    public void ClearMsg() {
        Send.UnregisterMsg(SendType.PlayerColorChange, PlayerChangeColor);
        Send.UnregisterMsg(SendType.EnergyMax, OnEnergyMax);
        Send.UnregisterMsg(SendType.EnergyEmpty, OnEnergyEmpty);
        Send.UnregisterMsg(SendType.UseItemChange, OnPlayerSkinChange);
        Send.UnregisterMsg(SendType.CtrlDrag, OnCtrlDrag);
    }

    //开始游戏时调用 
    public void StartBattle() {
        CreatePlayer();
        pickupTool = player.transform.Find("PickupTool");
        keepPickUpTran = player.transform.Find("KeepPickUp");
        playerData.StartBattleSpeed();
        Send.SendMsg(SendType.PlayerColorChange, playerData.colorIndex);
    }

    //Update函数
    public void OnUpdate() {
        switch (playerData.playerState) {
            case (E_PlayerState.Pickup):
                CameraMgr.Instance.FollowObj(player.transform);
                break;
            case (E_PlayerState.Speedup):
                if (Input.GetMouseButtonDown(0)) {
                    playerData.OnSpeedupClick();
                }
                playerData.TickSpeedupReduce(Time.deltaTime);
                CameraMgr.Instance.FollowObj(player.transform);
                BattleWindow.Instance.ForceAmount(playerData.CalcSpeedupRate());
                if (playerData.ReachedTarget(player.transform.position.z)) {
                    EnterFinishMode();
                }
                break;
            case (E_PlayerState.Finish):
                if (!playerData.isForce) {
                    playerData.BeginFinishLaunch();
                    AddRigidBody();
                }
                ToolMgr.Instance.DelayCallBack(() => {
                    playerData.moveOver = CameraMgr.Instance.LerpMove();
                }, 1f);
                playerData.TickFinalTime(Time.deltaTime);
                if (playerData.IsFinalTimeOver()) {
                    LevelDataMgr.Instance.LevelPass();
                    GameStateMgr.Instance.SwitchState(GameState.GameOver);
                    BattleMgr.Instance.state = BattleState.GameOver;
                }
                BattleWindow.Instance.HideForceBar();
                break;
        }
        AutoMoveForwardSpeed();
    }

    public Transform KeepPickUpAnchor() {
        return keepPickUpTran;
    }

    public void EnterSpeedupMode() {
        if (PickUpMgr.Instance.pickupData.isEnergyMax) {
            EnergyMgr.Instance.Energy = 0;
        }
        playerData.EnterSpeedup();
        BattleWindow.Instance.ShowForceBar();
    }

    public void EnterFinishMode() {
        playerData.EnterFinish();
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
            playerView.MoveForward(playerData.currentForwSpeed);
        }
        BattleWindow.Instance.ProcessAmount(playerView.GetPlayerPositionZ() / RoadMgr.Instance.GetTotalDistence());
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
    }

    public void OnEnergyMax(object[] _obj) {
        if (playerView != null) {
            playerView.ScaleTool(playerData.GetToolScale(true));
        }
        playerData.ApplyEnergyMaxBonus();
        PickUpMgr.Instance.pickupData.SetIsEnergyMax(true);
    }

    public void OnEnergyEmpty(object[] _obj) {
        if (playerView != null) {
            playerView.ScaleTool(playerData.GetToolScale(false));
        }
        else {
            if (player?.name == "Player4(Clone)")
                pickupTool.DOScaleX(1, 0.5f);
            else
                pickupTool.DOScaleX(2, 0.5f);
        }
        playerData.ResetForwardSpeed();
        PickUpMgr.Instance.pickupData.SetIsEnergyMax(false);
    }

    // AddPickUp
    public void AddPickUp(GameObject pickup) {
        if (pickup == null) return;
        playerData.AddPickUp(pickup);
    }

    // RemovePickUp
    public GameObject RemovePickUp() {
        if (playerData.GetKeepPickups().Count == 0)
            return null;
        if (playerData.GetKeepPickups().Count > 0) {
            ObjectPool.Instance.Recycle(playerData.GetKeepPickups()[playerData.GetKeepPickups().Count - 1], true);
            playerData.RemovePickUp();
            if (playerData.GetKeepPickups().Count > 0)
                return playerData.GetKeepPickups()[playerData.GetKeepPickups().Count - 1];
            else
                return null;
        }
        return null;
    }

    public void AddRigidBody() {
        LaunchAllPickups();
    }

    public void ResetCurrentTime() {
        playerData.ResetFinalTime();
    }

    // OnCtrlDrag
    public void OnCtrlDrag(object[] _obj) {
        PointerEventData data = (PointerEventData)_obj[0];
        Vector3 delta = data.delta;
        if (delta.x != 0) {
            float posX = delta.x * playerData.horizontalSpeed;
            float roadWidth = RoadMgr.Instance.roadData.roadWidth;
            playerView.UpdatePositionX(posX, roadWidth);
        }
    }

    private void OnPlayerSkinChange(object[] _obj) {
        playerData.skinName = (string)_obj[0];
        playerPref = LocalAssetMgr.Instance.Load_Prefab(playerData.skinName);
    }

    /// <summary>
    /// 发射所有拾取物
    /// </summary>
    private void LaunchAllPickups() {
        if (keepPickUpTran == null) return;

        for (int i = 0; i < playerData.GetKeepPickups().Count; i++) {
            LaunchPickup(playerData.GetKeepPickups()[i], i);
        }
    }

    private void LaunchPickup(GameObject pickup, int index) {
        if (pickup == null) return;

        if (playerView != null) {
            float forwardForce = playerData.CalcLaunchForce(index);
            playerView.LaunchPickup(pickup, Vector3.forward * forwardForce, playerData.CalcLaunchMass(index));
        }
    }
}