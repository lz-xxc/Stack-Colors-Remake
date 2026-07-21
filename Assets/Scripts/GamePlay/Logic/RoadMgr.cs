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
    }

    public void InitMsg() {
    }

    public void ClearMsg() {
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
            roadView.SetData(roadId, roadData.nextRoadPos, true);

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
            roadView.SetData(roadData.rateRoadLogic, roadData.nextRoadPos);

            roadData.nextRoadPos += new Vector3(0, 0, roadData.rateRoadLength);
            activeRateRoad.Add(rateRoad);
            roadData.roadLogic++;
        }
    }

    private void CreateNormalRoad(int roadId) {
        GameObject road = ObjectPool.Instance.Get("Road", roadTran.transform, false);
        RoadView roadView = road.GetComponent<RoadView>();
        if (roadView == null)
            roadView = road.AddComponent<RoadView>();
        roadView.SetData(roadId, roadData.nextRoadPos, false);

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

    public void RecycleRoad(GameObject road, int roadId) {
        activeRoad.Remove(road);
        StartCoroutine(DeltaRecycleRoad(road, roadId));
    }

    private IEnumerator DeltaRecycleRoad(GameObject road, int roadId) {
        yield return new WaitForSeconds(roadData.deltaRecycleTime);

        Send.SendMsg(SendType.RecycleRoad, roadId);

        RoadView view = road.GetComponent<RoadView>();
        if (view != null) view.ClearData();
        ObjectPool.Instance.Recycle(road, false);

        CreateRoad();
    }

    public float GetTotalDistence() {
        return roadData?.GetTotalDistance() ?? 0;
    }
}