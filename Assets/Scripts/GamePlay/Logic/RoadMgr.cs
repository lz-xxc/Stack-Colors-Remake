using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadMgr : SingletonMonoBehavior<RoadMgr> {
    private GameObject roadTran;

    private int roadLogic = 0;
    private int rateRoadLogic = 0;
    private int maxRoad = 8;
    public int roadLength { get; private set; } = 10;
    private int rateRoadLength = 10;
    private float deltaRecycleTime = 0.5f;

    private Vector3 nextRoadPos = Vector3.zero;

    private List<GameObject> activeRoad = new List<GameObject>();

    // ===== 地图配置（只初始化一次）=====
    private int converterPos1 = 7;   // 第2个全路段后
    private int converterPos2 = 21;  // 中间第2个顺序后

    private List<int> allLanePositions = new List<int> {
        // 前面5个全路段（间隔1格）
        3, 5, 9, 11, 13,
        // 结尾3个全路段（间隔1格）
        25, 27, 29 ,31,33
    };

    private List<int> lanePositions = new List<int> {
        16, 19, 23  // 3个顺序（间隔2格）
    };

    public int speedupRoadStart { get; private set; } = 35;

    public int finishRoadStart { get; private set; } = 40;

    private int rateRoadCount = 40;

    public void Init() {
        InitMsg();

        if (roadTran == null)
            roadTran = new GameObject("RoadTran");
    }

    public void Clear() {
        foreach (GameObject road in activeRoad) {
            ObjectPool.Instance.Recycle(road);
        }
    }

    public void InitMsg() {
    }

    public void ClearMsg() {
    }

    public void StartBattle() {
        roadLogic = 0;
        nextRoadPos = Vector3.zero;
        while (roadLogic < maxRoad) {
            CreateRoad();
        }
    }

    public void CreateRoad() {
        roadLogic++;

        if (roadLogic >= speedupRoadStart) {
            if (roadLogic < finishRoadStart) {
                // 一次性生成所有加速路段
                roadLogic--;
                for (int i = 0; i < finishRoadStart - speedupRoadStart; i++) {
                    roadLogic++;
                    GameObject speedRoad = ObjectPool.Instance.Get("Road", roadTran.transform, false);
                    RoadView roadView = speedRoad.GetComponent<RoadView>();
                    if (roadView == null)
                        roadView = speedRoad.AddComponent<RoadView>();
                    roadView.SetData(roadLogic, nextRoadPos, true);

                    speedRoad.GetComponent<Renderer>().material.color = Color.gray;
                    nextRoadPos += new Vector3(0, 0, roadLength);
                    activeRoad.Add(speedRoad);
                }
            }
            if (roadLogic >= finishRoadStart && roadLogic < finishRoadStart + rateRoadCount) {
                // 一次性生成所有倍率路段
                for (int i = 0; i < rateRoadCount; i++) {
                    rateRoadLogic++;
                    roadLogic++;
                    GameObject rateRoad = ObjectPool.Instance.Get("RateRoad", roadTran.transform, false);
                    RateRoad roadView = rateRoad.GetComponent<RateRoad>();
                    if (roadView == null)
                        roadView = rateRoad.AddComponent<RateRoad>();
                    roadView.SetData(i, nextRoadPos);
                    nextRoadPos += new Vector3(0, 0, rateRoadLength);  // 倍率路段用 rateRoadLength
                    activeRoad.Add(rateRoad);
                }
            }
        }
        else {
            GameObject road = ObjectPool.Instance.Get("Road", roadTran.transform, false);
            RoadView roadView = road.GetComponent<RoadView>();
            if (roadView == null)
                roadView = road.AddComponent<RoadView>();
            roadView.SetData(roadLogic, nextRoadPos, false);

            // ===== 转换器 =====
            if (roadLogic == converterPos1) {
                ColorChangerMgr.Instance.CreateColorChanger(road.transform.position, roadLogic, 0);
            }
            else if (roadLogic == converterPos2) {
                ColorChangerMgr.Instance.CreateColorChanger(road.transform.position, roadLogic, 1);
            }

            // ===== 前2格空（不生成）=====
            if (roadLogic > 2) {
                // ===== 全路段 =====
                if (allLanePositions.Contains(roadLogic)) {
                    if (EnergyMgr.Instance.isEnergyMax) {
                        PickUpMgr.Instance.CreateAllLanes(road.transform.position.z, roadLogic, ColorChangerMgr.Instance.colorIndex);
                    }
                    else {
                        PickUpMgr.Instance.CreateAllLanes(road.transform.position.z, roadLogic);
                    }
                }
                // ===== 顺序 =====
                else if (lanePositions.Contains(roadLogic)) {
                    PickUpMgr.Instance.CreateLane(road.transform.position.z, roadLogic);
                }
            }

            activeRoad.Add(road);

            nextRoadPos += new Vector3(0, 0, roadLength);  // 普通路段用 roadLength
        }
    }

    public void RecycleRoad(GameObject road, int roadId) {
        activeRoad.Remove(road);
        StartCoroutine(DeltaRecycleRoad(road, roadId));
    }

    private IEnumerator DeltaRecycleRoad(GameObject road, int roadId) {
        yield return new WaitForSeconds(deltaRecycleTime);

        // 先发送回收消息
        Send.SendMsg(SendType.RecycleRoad, roadId);

        // 再回收道路
        ObjectPool.Instance.Recycle(road, false);

        CreateRoad();
    }

    public float GetTotalDistence() {
        return roadLength * (speedupRoadStart - 1);
    }
}