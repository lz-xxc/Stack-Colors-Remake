using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpMgr : SingletonMonoBehavior<PickUpMgr> {
    private GameObject pickUpTran;
    private Transform keepPickUp;
    private GameObject lastPickupObj;

    public PickupData pickupData { get; private set; }

    public void Init() {
        InitMsg();

        // 初始化数据
        pickupData = new PickupData();
        pickupData.CalculateSpaceOfZ(10f); // 假设路面长度为10

        pickUpTran = pickUpTran == null ? new GameObject("pickUpTran") : pickUpTran;
    }

    public void Clear() {
        lastPickupObj = null;
        pickupData?.Reset(); // 使用数据类的Reset

        pickupData.ClearActivePickups();
    }

    public void InitMsg() {
        Send.RegisterMsg(SendType.PickupColorChange, PickupChangeColor);
        Send.RegisterMsg(SendType.RecycleRoad, RecyclePickUp);
        Send.RegisterMsg(SendType.EnergyMax, OnEnergyMax);
        Send.RegisterMsg(SendType.Pickup, OnPickup);
    }

    public void ClearMsg() {
        Send.UnregisterMsg(SendType.PickupColorChange, PickupChangeColor);
        Send.UnregisterMsg(SendType.RecycleRoad, RecyclePickUp);
        Send.UnregisterMsg(SendType.EnergyMax, OnEnergyMax);
        Send.UnregisterMsg(SendType.Pickup, OnPickup);
    }

    //全边不统一颜色拾取物
    public void CreateAllLanes(float roadPosZ, int roadId) {
        int colorIndex;
        for (int i = 0; i < pickupData.maxPickUpRow; i++) {
            float posZ = (roadPosZ - 4.5f + pickupData.spaceOfZ) + ((pickupData.spaceOfZ + 1) * i);
            for (int j = 1; j < pickupData.createPosX.Length - 1; j++) {
                colorIndex = (j + pickupData.colorIndexIncrement + 1) % 3;
                Vector3 pos = new Vector3(pickupData.createPosX[j], pickupData.pickUpPosY, posZ);
                GameObject Pickup = CreatePickup(pos, pickupData.smallPickupHeight, roadId, colorIndex,
                    pickupData.lowPickupScore, pickupData.lowPickupEnergy, 1);
                pickupData.AddActivePickup(Pickup);
            }
        }
        pickupData.AddColorIndexIncrement();
    }

    //全边统一颜色拾取物
    public void CreateAllLanes(float roadPosZ, int roadId, int colorIndex) {
        for (int i = 0; i < pickupData.maxPickUpRow; i++) {
            float posZ = (roadPosZ - 4.5f + pickupData.spaceOfZ) + ((pickupData.spaceOfZ + 1) * i);
            for (int j = 1; j < pickupData.createPosX.Length - 1; j++) {
                Vector3 pos = new Vector3(pickupData.createPosX[j], pickupData.pickUpPosY, posZ);
                GameObject Pickup = CreatePickup(pos, pickupData.smallPickupHeight, roadId, colorIndex,
                    pickupData.lowPickupScore, pickupData.lowPickupEnergy, 1);
                pickupData.AddActivePickup(Pickup);
            }
        }
        pickupData.AddColorIndexIncrement();
    }

    //v形生成拾取物
    public void CreateLane(float roadPosZ, int roadId) {
        int colorIndex = ColorChangerMgr.Instance.colorIndex;
        for (int i = 0; i < pickupData.laneSequence.Length; i++) {
            float posZ = (roadPosZ - 4.5f + pickupData.spaceOfZ) + ((pickupData.spaceOfZ + 1) * i * 2);
            Vector3 pos = new Vector3(pickupData.createPosX[pickupData.laneSequence[i]],
                pickupData.pickUpPosY * 5, posZ);
            GameObject pickup = CreatePickup(pos, pickupData.bigPickupHeight, roadId, colorIndex,
                pickupData.heightPickupScore, pickupData.heightPickupEnergy, 2);
            pickupData.AddActivePickup(pickup);
        }
    }

    public GameObject CreatePickup(Vector3 pos, float height, int roadId, int colorIndex, int score, int energy, int type) {
        GameObject Pickup = ObjectPool.Instance.Get("PickUp", pickUpTran.transform, false);
        Send.SendMsg(SendType.PickupColorChange, Pickup, colorIndex);
        Pickup.GetComponent<PickUpView>().SetData(pos, height, roadId, colorIndex, score, energy, type);
        return Pickup;
    }

    public void RecyclePickUp(object[] _obj) {
        int roadId = (int)_obj[0];
        for (int i = pickupData.activePickUps.Count - 1; i >= 0; i--) {
            GameObject pickup = pickupData.activePickUps[i];
            if (pickup != null && pickup.GetComponent<PickUpView>().isBelongRoadId(roadId)) {
                ObjectPool.Instance.Recycle(pickup);
                pickupData.RemoveActivePickupAt(i);
            }
        }
    }

    public void OnPickup(object[] _objs) {
        GameObject obj = (GameObject)_objs[0];
        int colorIndex = (int)_objs[1];
        PickUpView pickUpView = obj.GetComponent<PickUpView>();

        if (PlayerMgr.Instance.ColorIndex != colorIndex) {
            HandleWrongColorPickup(pickUpView);
        }
        else {
            HandleRightColorPickup(obj, pickUpView);
        }


    }

    //碰到不同颜色木板
    private void HandleWrongColorPickup(PickUpView pickUpView) {
        if (pickupData.pickUpCount > 0) {
            if (EnergyMgr.Instance.Energy > 0)
                EnergyMgr.Instance.Energy -= pickUpView.energy;
            lastPickupObj = PlayerMgr.Instance.RemovePickUp();
            pickupData.ChangePickupCount(-1);
            if (lastPickupObj != null) {
                PickUpView removedView = lastPickupObj.GetComponent<PickUpView>();
                if (removedView != null) {
                    pickupData.SetCurrentPosY(removedView.posY);
                }
            }
        }
        else {
            if (GameStateMgr.Instance.curState != GameState.GameOver)
                GameStateMgr.Instance.SwitchState(GameState.GameOver);
            BattleMgr.Instance.state = BattleState.GameOver;
        }
    }

    //碰到相同颜色木板
    private void HandleRightColorPickup(GameObject obj, PickUpView pickUpView) {
        PickUpView lastView = lastPickupObj?.GetComponent<PickUpView>();

        if (pickupData.pickUpCount == 0) {
            keepPickUp = PlayerMgr.Instance.KeepPickUpAnchor();
            pickupData.CalculatePosY(pickUpView.height, pickUpView.height);
        }
        else {
            pickupData.CalculatePosY(pickUpView.height, lastView.height);
        }
        pickUpView.SetTransform(keepPickUp, pickupData.currentPickUpPosY);
        lastPickupObj = obj;

        pickupData.RemoveActivePickup(obj);
        PlayerMgr.Instance.AddPickUp(obj);

        ScoreMgr.Instance.Score += pickUpView.score;
        if (!pickupData.isEnergyMax)
            EnergyMgr.Instance.Energy += pickUpView.energy;

        BattleMgr.Instance.ShowFloatingText(keepPickUp.transform.position, pickUpView.score);

        pickupData.ChangePickupCount(1);
    }

    public void PickupChangeColor(object[] _obj) {
        GameObject pickup = (GameObject)_obj[0];
        int colorIndex = (int)_obj[1];
        pickup.GetComponent<PickUpView>().ChangeColor(colorIndex);
    }

    private void OnEnergyMax(object[] _obj) {
        pickupData.SetIsEnergyMax(true);
        int colorIndex = PlayerMgr.Instance.ColorIndex;
        foreach (GameObject obj in pickupData.activePickUps) {
            if (ColorChangerMgr.Instance.colorChanger != null &&
                obj.GetComponent<PickUpView>().belongRoadId >= ColorChangerMgr.Instance.roadId) {
                colorIndex = ColorChangerMgr.Instance.colorIndex;
            }
            Send.SendMsg(SendType.PickupColorChange, obj, colorIndex);
        }
    }

}