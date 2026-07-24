using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Model - 道路数据
/// </summary>
public class RoadData {
    // ============ 配置数据 ============
    public int maxRoad { get; private set; } = 8;
    public float roadLength { get; private set; } = 10;
    public float roadWidth { get; private set; } = 16;
    public int rateRoadLength { get; private set; } = 10;
    public float deltaRecycleTime { get; private set; } = 0.5f;

    public int converterPos1 { get; private set; } = 8;
    public int converterPos2 { get; private set; } = 22;

    public List<int> allLanePositions { get; private set; }
    public List<int> lanePositions { get; private set; }

    public int speedupRoadStart { get; private set; } = 37;
    public int finishRoadStart { get; private set; } = 42;
    public int rateRoadCount { get; private set; } = 60;

    public float startRate { get; private set; } = 1;

    // ============ 运行时数据 ============
    public int roadLogic { get; set; }
    public int rateRoadLogic { get; set; }
    public Vector3 nextRoadPos { get; set; }

    // ============ 构造函数 ============
    public RoadData() {
        allLanePositions = new List<int> { 4, 6, 10, 12, 14, 27, 29, 31, 33, 35 };
        lanePositions = new List<int> { 16, 19, 24 };
        Reset();
    }

    public void Reset() {
        roadLogic = 0;
        rateRoadLogic = 0;
        nextRoadPos = Vector3.zero;
    }

    // ============ 判断方法 ============
    public float GetTotalDistance() {
        return roadLength * (speedupRoadStart - 1);
    }

    public bool IsSpeedupRoad(int roadId) {
        return roadId >= speedupRoadStart && roadId < finishRoadStart;
    }

    public bool IsFinishRoadStart(int roadId) {
        return roadId == finishRoadStart - 1;
    }

    public bool IsSpeedupRoadStart(int roadId) {
        return roadId == speedupRoadStart;
    }

    public bool IsConverterPos1(int roadId) {
        return roadId == converterPos1;
    }

    public bool IsConverterPos2(int roadId) {
        return roadId == converterPos2;
    }

    public bool IsAllLanePosition(int roadId) {
        return allLanePositions.Contains(roadId);
    }

    public bool IsLanePosition(int roadId) {
        return lanePositions.Contains(roadId);
    }

    public bool IsInRateRoadRange(int roadId) {
        return roadId >= finishRoadStart && roadId < finishRoadStart + rateRoadCount;
    }
}