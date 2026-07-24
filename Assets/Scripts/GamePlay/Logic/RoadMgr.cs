using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadMgr : SingletonMonoBehavior<RoadMgr> {
    // ============ Data ============
    public RoadData roadData { get; private set; }

    // ============ 运行时 ============
    private GameObject roadTran;
    private List<GameObject> activeRoad = new List<GameObject>();
    private List<GameObject> activeRateRoad = new List<GameObject>();

    public void Init() {
        roadData = new RoadData();
        InitMsg();

        if (roadTran == null)
            roadTran = new GameObject("RoadTran");
    }

    public void Clear() {
        foreach (GameObject road in activeRoad) {
            if (road != null) {
                RoadView view = road.GetComponent<RoadView>();
                if (view != null) view.ClearData();
                ObjectPool.Instance.Recycle(road);
            }
        }
        foreach (GameObject road in activeRateRoad) {
            if (road != null) {
                ObjectPool.Instance.Recycle(road);
            }
        }
        activeRoad.Clear();
        activeRateRoad.Clear();
        roadData?.Reset();
        StopAllCoroutines();
    }

    public void InitMsg() {
        Send.RegisterMsg(SendType.RecycleRoad, OnRecycleRoad);
        Send.RegisterMsg(SendType.EnterSpeedupRoad, OnEnterSpeedup);
    }

    public void ClearMsg() {
        Send.UnregisterMsg(SendType.RecycleRoad, OnRecycleRoad);
        Send.UnregisterMsg(SendType.EnterSpeedupRoad, OnEnterSpeedup);
    }

    public void StartBattle() {
        roadData.Reset();
        while (roadData.roadLogic < roadData.maxRoad) {
            CreateRoad();
        }
    }

    public void CreateRoad() {
        roadData.roadLogic++;
        int roadId = roadData.roadLogic;

        if (roadId >= roadData.speedupRoadStart) {
            if (roadId < roadData.finishRoadStart) {
                CreateSpeedupRoads();
            }
            else if (roadData.IsInRateRoadRange(roadId)) {
                CreateRateRoads();
            }
        }
        else {
            CreateNormalRoad(roadId);
        }
    }

    private void CreateSpeedupRoads() {
        for (int i = 0; i < roadData.finishRoadStart - roadData.speedupRoadStart; i++) {
            int roadId = roadData.roadLogic;

            GameObject speedRoad = ObjectPool.Instance.Get("Road", roadTran.transform, false);
            RoadView roadView = speedRoad.AddMissingComponent<RoadView>();
            RoadInfo roadInfo = new RoadInfo(roadId, roadData.nextRoadPos, E_RoadType.Speedup);
            roadView.SetData(roadInfo);

            speedRoad.GetComponent<Renderer>().material.color = Color.gray;
            roadData.nextRoadPos += new Vector3(0, 0, roadData.roadLength);
            activeRoad.Add(speedRoad);
            roadData.roadLogic++;
        }
    }

    private void CreateRateRoads() {
        for (int i = 0; i < roadData.rateRoadCount; i++) {
            roadData.rateRoadLogic++;

            GameObject rateRoad = ObjectPool.Instance.Get("RateRoad", roadTran.transform, false);
            RateRoad roadView = rateRoad.GetComponent<RateRoad>();
            if (roadView == null)
                roadView = rateRoad.AddComponent<RateRoad>();
            RoadInfo roadInfo = new RoadInfo(roadData.rateRoadLogic, roadData.startRate, 0, roadData.nextRoadPos, E_RoadType.Rate);
            roadView.SetData(roadInfo);

            roadData.nextRoadPos += new Vector3(0, 0, roadData.rateRoadLength);
            activeRateRoad.Add(rateRoad);
            roadData.roadLogic++;
        }
    }

    private void CreateNormalRoad(int roadId) {
        GameObject road = ObjectPool.Instance.Get("Road", roadTran.transform, false);
        RoadView roadView = road.AddMissingComponent<RoadView>();
        RoadInfo roadInfo = new RoadInfo(roadId, 0, roadData.deltaRecycleTime, roadData.nextRoadPos, E_RoadType.Normal);
        roadView.SetData(roadInfo);

        // ===== 转换器 =====
        if (roadData.IsConverterPos1(roadId)) {
            ColorChangerMgr.Instance.CreateColorChanger(road.transform.position, roadId, 0);
        }
        else if (roadData.IsConverterPos2(roadId)) {
            ColorChangerMgr.Instance.CreateColorChanger(road.transform.position, roadId, 1);
        }

        // ===== 前2格空（不生成）=====
        if (roadId > 2) {
            if (roadData.IsAllLanePosition(roadId)) {
                if (EnergyMgr.Instance.isEnergyMax) {
                    PickUpMgr.Instance.CreateAllLanes(road.transform.position.z, roadId, ColorChangerMgr.Instance.colorIndex);
                }
                else {
                    PickUpMgr.Instance.CreateAllLanes(road.transform.position.z, roadId);
                }
            }
            else if (roadData.IsLanePosition(roadId)) {
                PickUpMgr.Instance.CreateLane(road.transform.position.z, roadId);
            }
        }

        activeRoad.Add(road);
        roadData.nextRoadPos += new Vector3(0, 0, roadData.roadLength);
    }

    public void OnRecycleRoad(object[] _objs) {
        GameObject road = (GameObject)_objs[0];
        float DeltaRecycleTime = (float)_objs[2];

        StartCoroutine(IE_DeltaRecycleRoad(road, DeltaRecycleTime));

    }

    private IEnumerator IE_DeltaRecycleRoad(GameObject road, float deltaTime) {
        yield return new WaitForSeconds(deltaTime);
        activeRoad.Remove(road);

        RoadView view = road.GetComponent<RoadView>();
        if (view != null) view.ClearData();
        ObjectPool.Instance.Recycle(road, false);

        CreateRoad();
    }

    public float GetTotalDistence() {
        return roadData?.GetTotalDistance() ?? 0;
    }

    private void OnEnterSpeedup(object[] _objs) {
        int roadId = (int)_objs[0];
        Vector3 roadPos = (Vector3)_objs[1];
        if (roadId == roadData.speedupRoadStart) {
            Send.SendMsg(SendType.PlayerModeChange, E_PlayerState.Speedup);
        }

        if (roadId == roadData.finishRoadStart - 1) {
            PlayerMgr.Instance.setTargetZ(roadPos.z);
        }
    }
}

public class RoadInfo {
    public bool isTriggered { get; private set; } = false;
    public E_RoadType roadType { get; private set; } = E_RoadType.Normal;

    public int roadId { get; private set; } = 0;
    public float deltaRecycleTime { get; private set; } = 0;

    public Vector3 roadPos { get; private set; }

    public float rateRoadRate { get; private set; } = 0;
    public float startRate { get; private set; } = 1;
    public float rateAdd { get; private set; } = 0.1f;

    public RoadInfo(int roadId, float startRate, float deltaRecycleTime, Vector3 pos, E_RoadType roadType) {
        this.roadId = roadId;
        this.deltaRecycleTime = deltaRecycleTime;
        this.roadType = roadType;
        isTriggered = false;
        roadPos = pos;
        if (roadType == E_RoadType.Rate)
            this.rateRoadRate = startRate + rateAdd * roadId;
    }

    public RoadInfo(int roadId, Vector3 pos, E_RoadType roadType) {
        this.roadId = roadId;
        this.roadType = roadType;
        isTriggered = false;
        roadPos = pos;
        if (roadType == E_RoadType.Rate)
            this.rateRoadRate = startRate + rateAdd * roadId;
        this.deltaRecycleTime = 0;
    }

    public void RoadTriggered() {
        isTriggered = true;
    }

    public void ClearInfo() {
        isTriggered = false;
    }
}

public enum E_RoadType {
    Normal,
    Speedup,
    Rate,
}