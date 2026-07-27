using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpMgr : SingletonMonoBehavior<PickUpMgr> {
    private GameObject pickUpTran;
    private Transform keepPickUp;
    private PickupInfo lastInfo;

    public PickupData pickupData { get; private set; }

    public Dictionary<PickupInfo, PickUpView> pickupViewS = new Dictionary<PickupInfo, PickUpView>();

    public void Init() {
        InitMsg();

        // 初始化数据
        pickupData = new PickupData();
        pickupData.CalculateSpaceOfZ(10f); // 假设路面长度为10

        pickUpTran = pickUpTran == null ? new GameObject("pickUpTran") : pickUpTran;
    }

    public void Clear() {

        foreach (PickUpView view in pickupViewS.Values) {
            if (view == null || view.gameObject == null)
                continue;
            GameObject obj = view.gameObject;
            Rigidbody rb = obj.GetComponent<Rigidbody>();
            if (rb != null) {
                rb.isKinematic = true;
            }
            obj.transform.rotation = Quaternion.identity;
            ObjectPool.Instance.Recycle(obj, true);
        }

        lastInfo = null;

        pickupViewS.Clear();
        keepPickUp = null;

        pickupData?.Reset(); // 使用数据类的Reset

    }

    public void InitMsg() {
        Send.RegisterMsg(SendType.PickupColorChange, PickupChangeColor);
        Send.RegisterMsg(SendType.RecycleRoad, RecyclePickUp);
        Send.RegisterMsg(SendType.EnergyMax, OnEnergyMax);
        Send.RegisterMsg(SendType.Pickup, OnPickup);
        Send.RegisterMsg(SendType.UpdateRate, UpdateRatePosZ);
    }

    public void ClearMsg() {
        Send.UnregisterMsg(SendType.PickupColorChange, PickupChangeColor);
        Send.UnregisterMsg(SendType.RecycleRoad, RecyclePickUp);
        Send.UnregisterMsg(SendType.EnergyMax, OnEnergyMax);
        Send.UnregisterMsg(SendType.Pickup, OnPickup);
        Send.UnregisterMsg(SendType.UpdateRate, UpdateRatePosZ);
    }

    //全边不统一颜色拾取物
    public void CreateAllLanes(float roadPosZ, int roadId) {
        int colorIndex;
        for (int i = 0; i < pickupData.maxPickUpRow; i++) {
            float posZ = (roadPosZ - 4.5f + pickupData.spaceOfZ) + ((pickupData.spaceOfZ + 1) * i);
            for (int j = 1; j < pickupData.createPosX.Length - 1; j++) {
                colorIndex = (j + pickupData.colorIndexIncrement + 1) % 3;
                Vector3 pos = new Vector3(pickupData.createPosX[j], pickupData.pickUpPosY, posZ);

                PickupInfo info = new PickupInfo(
                 roadId,
                 colorIndex,
                 pickupData.smallPickupHeight,
                 pickupData.lowPickupScore,
                 pickupData.lowPickupEnergy,
                 pos
                 );

                CreatePickup(info);

                pickupData.AddActivePickup(info);
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

                PickupInfo info = new PickupInfo(
                 roadId,
                 colorIndex,
                 pickupData.smallPickupHeight,
                 pickupData.lowPickupScore,
                 pickupData.lowPickupEnergy,
                 pos);

                CreatePickup(info);

                pickupData.AddActivePickup(info);
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

            PickupInfo info = new PickupInfo(
             roadId,
             colorIndex,
             pickupData.bigPickupHeight,
             pickupData.heightPickupScore,
             pickupData.heightPickupEnergy,
             pos);

            CreatePickup(info);

            pickupData.AddActivePickup(info);
        }
    }

    public GameObject CreatePickup(PickupInfo info) {
        GameObject Pickup = ObjectPool.Instance.Get("PickUp", pickUpTran.transform, false);
        PickUpView view = Pickup.GetComponent<PickUpView>();
        view.SetData(info);
        view.SetPosition(info.position);
        view.UpdataColor();
        pickupViewS.Add(info, view);
        Send.SendMsg(SendType.PickupColorChange, view.pickupInfo, info.colorIndex);

        // 设置缩放
        Pickup.transform.localScale = new Vector3(
            Pickup.transform.localScale.x,
            info.height,
            Pickup.transform.localScale.z
        );

        return Pickup;
    }

    public void RecyclePickUp(object[] _obj) {
        int roadId = (int)_obj[1];
        float deltaRecycleTime = (float)_obj[2];
        ToolMgr.Instance.DelayCallBack(() => {
            List<PickupInfo> roadPickups = pickupData.GetPickupsByRoadId(roadId);
            foreach (PickupInfo info in roadPickups) {
                if (info.belongRoadId == roadId && pickupViewS.TryGetValue(info, out PickUpView view)) {
                    ObjectPool.Instance.Recycle(view.gameObject);
                    pickupViewS.Remove(info);
                }
                pickupData.RemoveActivePickup(info);
            }
        }, deltaRecycleTime);

    }

    public void OnPickup(object[] _objs) {
        string pickupId = (string)_objs[0];
        int colorIndex = (int)_objs[1];
        PickupInfo info = pickupData.GetPickupInfo(pickupId);
        PickUpView pickUpView = pickupViewS[info];

        if (PlayerMgr.Instance.ColorIndex != colorIndex) {
            HandleWrongColorPickup(info, pickUpView);
        }
        else {
            HandleRightColorPickup(info, pickUpView);
        }


    }

    //碰到不同颜色木板
    private void HandleWrongColorPickup(PickupInfo info, PickUpView pickUpView) {
        if (pickupData.pickUpCount > 0) {
            if (EnergyMgr.Instance.Energy > 0)
                EnergyMgr.Instance.Energy -= info.energy;

            // 移除最后拾取的拾取物
            if (lastInfo != null) {
                // 从玩家数据移除
                if (pickupViewS[lastInfo] != null) {
                    lastInfo = PlayerMgr.Instance.RemovePickUp();
                }
                // 更新位置
                pickupData.ChangePickupCount(-1);
                if (lastInfo != null)
                    pickupData.SetCurrentPosY(lastInfo.posY);
                else
                    pickupData.SetCurrentPosY(0);
            }

        }
        else {
            BattleMgr.Instance.CalcOverReward();
            if (GameStateMgr.Instance.curState != GameState.GameOver)
                GameStateMgr.Instance.SwitchState(GameState.GameOver);
            BattleMgr.Instance.state = BattleState.GameOver;
        }
    }

    //碰到相同颜色木板
    private void HandleRightColorPickup(PickupInfo info, PickUpView pickUpView) {
        if (pickupData.pickUpCount == 0) {
            keepPickUp = PlayerMgr.Instance.KeepPickUpAnchor();
            pickupData.CalculatePosY(pickupData.smallPickupHeight, info.height);
        }
        else {
            pickupData.CalculatePosY(info.height, lastInfo.height);
        }
        pickUpView.SetTransform(keepPickUp, pickupData.currentPickUpPosY);

        pickupData.RemoveActivePickup(info);
        PlayerMgr.Instance.AddPickUp(info);

        ScoreMgr.Instance.Score += info.score;
        if (!pickupData.isEnergyMax)
            EnergyMgr.Instance.Energy += info.energy;

        BattleMgr.Instance.ShowFloatingText(keepPickUp.transform.position, info.score);

        pickupData.ChangePickupCount(1);
        lastInfo = info;
    }

    public void PickupChangeColor(object[] _obj) {
        PickupInfo pickupInfo = (PickupInfo)_obj[0];
        int colorIndex = (int)_obj[1];
        pickupInfo.ChangeColor(colorIndex);
        pickupViewS[pickupInfo].UpdataColor();
    }

    private void OnEnergyMax(object[] _obj) {
        pickupData.SetIsEnergyMax(true);
        int colorIndex = PlayerMgr.Instance.ColorIndex;
        foreach (PickupInfo info in pickupData.activePickUps) {
            if (ColorChangerMgr.Instance.colorChanger != null &&
                info.belongRoadId >= ColorChangerMgr.Instance.roadId) {
                colorIndex = ColorChangerMgr.Instance.colorIndex;
            }
            Send.SendMsg(SendType.PickupColorChange, info, colorIndex);
        }
    }

    private void UpdateRatePosZ(object[] _objs) {
        float posZ = (float)_objs[0];
        pickupData.SetLastestRatePosZ(posZ);
    }

}

public class PickupInfo {
    public string Id { get; private set; }
    public bool isTriggered { get; private set; } = false;
    public int belongRoadId { get; private set; }
    public int colorIndex { get; private set; }
    public float height { get; private set; }
    public int score { get; private set; }
    public int energy { get; private set; }
    public Vector3 position { get; private set; }
    public float posY { get; private set; }

    public PickupInfo(int belongRoadId, int colorIndex, float height, int score, int energy, Vector3 pos) {
        this.Id = System.Guid.NewGuid().ToString();
        isTriggered = false;
        this.belongRoadId = belongRoadId;
        this.colorIndex = colorIndex;
        this.height = height;
        this.score = score;
        this.energy = energy;
        position = pos;
        this.posY = posY;
        isTriggered = false;
    }

    public void PickupTriggered() {
        isTriggered = true;
    }

    public void ChangeColor(int newColorIndex) {
        colorIndex = newColorIndex;
    }

    public void UpdatePosition(Vector3 newPosition) {
        position = newPosition;
    }

    public void UpdatePosY(float newPosY) {
        posY = newPosY;
    }

}