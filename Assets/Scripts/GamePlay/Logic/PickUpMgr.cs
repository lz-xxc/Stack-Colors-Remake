using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpMgr : SingletonMonoBehavior<PickUpMgr> {
    private GameObject pickUpTran;
    private Transform keepPickUp;
    private GameObject lastPickupObj;

    int addColorIndex = -1;//给颜色下标增加量（用于每次刷新拾取物时改变颜色摆放顺序）

    private int MaxPickUpRow = 7;
    private float PickUpPosY = 0.05f;
    private float SpaceOfZ;//拾取物间隙的长度
    private float pickupSpace = 0.01f;//拾取物在玩家上的间隙

    //拾取物物理变量
    private float pickUpPosY = 0;
    private float lastPickupPosY = 0;
    private float smallPickupHeight = 0.1f;
    private float bigPickupHeight = 0.5f;
    private int pickUpCount = 0;

    //分数和能量
    private int heightPickupScore = 5;
    private int lowPickupScore = 1;
    private int heighrPickupEnergy = 4;
    private int lowPickupEnergy = 2;



    public bool isEnergyMax = false;

    private float[] createPosX;
    private int[] laneSequence = new int[] { 0, 1, 2, 2, 1, 0 };

    private List<GameObject> activePickUps = new List<GameObject>();

    public void Init() {
        InitMsg();

        pickUpTran = pickUpTran == null ? new GameObject("pickUpTran") : pickUpTran;
        createPosX = PlayerMgr.Instance.GetAllLaneX();
        SpaceOfZ = ((10 - 7) / 8f);//（（地面长度-拾取物总长度） / 间隙个数）
    }

    public void Clear() {
        foreach (GameObject pickup in activePickUps) {
            ObjectPool.Instance.Recycle(pickup);
        }
        activePickUps.Clear();
    }

    public void InitMsg() {
        Send.RegisterMsg(SendType.PickupColorChange, PickupChangeColor);
        Send.RegisterMsg(SendType.RecycleRoad, RecyclePickUp);
        Send.RegisterMsg(SendType.EnergyMax, OnEnergyMax);
    }

    public void ClearMsg() {
        Send.UnregisterMsg(SendType.PickupColorChange, PickupChangeColor);
        Send.UnregisterMsg(SendType.RecycleRoad, RecyclePickUp);
        Send.UnregisterMsg(SendType.EnergyMax, OnEnergyMax);
    }

    //全边不统一颜色拾取物
    public void CreateAllLanes(float roadPosZ, int roadId) {
        int colorIndex;
        for (int i = 0; i < MaxPickUpRow; i++) {
            float posZ = (roadPosZ - 4.5f + SpaceOfZ) + ((SpaceOfZ + 1) * i);//初始位置：（路面中心位置 - 路面中心到边缘减去拾取物宽度一半 + 间隔长度） + 拾取物中心间隔长度：（（间隔长度 + 拾取物长度） * 第几行拾取物）
            for (int j = 0; j < 3; j++) {
                colorIndex = (j + addColorIndex + 1) % 3;
                Vector3 pos = new Vector3(createPosX[j], PickUpPosY, posZ);
                GameObject Pickup = CreatePickup(pos, smallPickupHeight, roadId, colorIndex, lowPickupScore, lowPickupEnergy);
                activePickUps.Add(Pickup);
            }
        }
        addColorIndex++;
    }

    //全边统一颜色拾取物
    public void CreateAllLanes(float roadPosZ, int roadId, int colorIndex) {
        for (int i = 0; i < MaxPickUpRow; i++) {
            float posZ = (roadPosZ - 4.5f + SpaceOfZ) + ((SpaceOfZ + 1) * i);//初始位置：（路面中心位置 - 路面中心到边缘减去拾取物宽度一半 + 间隔长度） + 拾取物中心间隔长度：（（间隔长度 + 拾取物长度） * 第几行拾取物）
            for (int j = 0; j < 3; j++) {
                Vector3 pos = new Vector3(createPosX[j], PickUpPosY, posZ);
                GameObject Pickup = CreatePickup(pos, smallPickupHeight, roadId, colorIndex, lowPickupScore, lowPickupEnergy);
                activePickUps.Add(Pickup);
            }
        }
        addColorIndex++;
    }

    //v形生成拾取物
    public void CreateLane(float roadPosZ, int roadId) {
        int colorIndex = ColorChangerMgr.Instance.colorIndex;
        for (int i = 0; i < laneSequence.Length; i++) {
            float posZ = (roadPosZ - 4.5f + SpaceOfZ) + ((SpaceOfZ + 1) * i * 2);
            Vector3 pos = new Vector3(createPosX[laneSequence[i]], PickUpPosY * 5, posZ);
            GameObject pickup = CreatePickup(pos, bigPickupHeight, roadId, colorIndex, heightPickupScore, heighrPickupEnergy);
            activePickUps.Add(pickup);
        }
    }

    public GameObject CreatePickup(Vector3 pos, float height, int roadId, int colorIndex, int score, int energy) {
        GameObject Pickup = ObjectPool.Instance.Get("PickUp", pickUpTran.transform, false);
        Send.SendMsg(SendType.PickupColorChange, Pickup, colorIndex);
        Pickup.GetComponent<PickUpView>().SetData(pos, height, roadId, colorIndex, score, energy);
        return Pickup;
    }

    public void RecyclePickUp(object[] _obj) {
        for (int i = activePickUps.Count - 1; i >= 0; i--) {
            GameObject pickup = activePickUps[i];
            if (pickup != null && pickup.GetComponent<PickUpView>().isBelongRoadId((int)_obj[0])) {
                ObjectPool.Instance.Recycle(pickup);
                activePickUps.RemoveAt(i);
            }
        }
    }

    public void PickUp(GameObject obj, int colorIndex) {
        PickUpView pickUpView = obj.GetComponent<PickUpView>();
        PickUpView lastView = lastPickupObj?.GetComponent<PickUpView>();

        if (PlayerMgr.Instance.colorIndex != colorIndex) {
            if (pickUpCount > 0) {
                if (EnergyMgr.Instance.Energy > 0)
                    EnergyMgr.Instance.Energy -= pickUpView.energy;
                lastPickupObj = PlayerMgr.Instance.RemovePickUp();
                pickUpCount--;
                if (lastPickupObj != null) {
                    PickUpView removedView = lastPickupObj.GetComponent<PickUpView>();
                    if (removedView != null) {
                        lastPickupPosY = removedView.posY;
                        pickUpPosY = lastPickupPosY;
                    }

                }
            }
            else {
                GameStateMgr.Instance.SwitchState(GameState.GameOver);
                BattleMgr.Instance.state = BattleState.GameOver;
            }
            return;
        }

        if (pickUpCount == 0) {
            keepPickUp = PlayerMgr.Instance.keepPickUpTran;
            pickUpPosY = pickUpView.height + pickupSpace;
            obj.transform.position = keepPickUp.position + Vector3.up * pickUpPosY;
        }
        else {
            lastPickupPosY = pickUpPosY;
            pickUpPosY += pickUpView.height / 2 + lastView.height / 2 + pickupSpace;
            obj.transform.position = keepPickUp.position + Vector3.up * pickUpPosY;
        }
        pickUpView.SetPosY(pickUpPosY);
        lastPickupObj = obj;

        obj.transform.SetParent(keepPickUp);

        activePickUps.Remove(obj);
        PlayerMgr.Instance.AddPickUp(obj);


        ScoreMgr.Instance.Score += pickUpView.score;
        if (!isEnergyMax)
            EnergyMgr.Instance.Energy += pickUpView.energy;

        BattleMgr.Instance.ShowFloatingText(keepPickUp.transform.position, pickUpView.score);

        pickUpCount++;
    }

    public void PickupChangeColor(object[] _obj) {
        GameObject pickup = (GameObject)_obj[0];
        int colorIndex = (int)_obj[1];
        pickup.GetComponent<PickUpView>().ChangeColorIndex(colorIndex);
        Renderer renderer = pickup.GetComponent<Renderer>();
        switch (colorIndex) {
            case (0):
                renderer.material.color = Color.yellow;
                break;
            case (1):
                renderer.material.color = Color.red;
                break;
            case (2):
                renderer.material.color = Color.green;
                break;
        }
    }

    private void OnEnergyMax(object[] _obj) {
        int colorIndex = PlayerMgr.Instance.colorIndex;
        foreach (GameObject obj in activePickUps) {
            if (ColorChangerMgr.Instance.colorChanger != null && obj.GetComponent<PickUpView>().belongRoadId >= ColorChangerMgr.Instance.roadId) {
                colorIndex = ColorChangerMgr.Instance.colorIndex;
            }
            Send.SendMsg(SendType.PickupColorChange, obj, colorIndex);
        }
    }

}
