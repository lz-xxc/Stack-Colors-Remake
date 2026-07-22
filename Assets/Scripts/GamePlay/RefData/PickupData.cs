using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickupData {
    // 生成参数
    public int maxPickUpRow { get; private set; }
    public float pickUpPosY { get; private set; }
    public float spaceOfZ { get; private set; }
    public float pickupSpace { get; private set; }

    // 拾取物高度
    public float smallPickupHeight { get; private set; }
    public float bigPickupHeight { get; private set; }

    // 分数和能量
    public int heightPickupScore { get; private set; }
    public int lowPickupScore { get; private set; }
    public int heightPickupEnergy { get; private set; }
    public int lowPickupEnergy { get; private set; }

    // 生成位置X轴
    public float leftX { get; private set; }
    public float midLeftX { get; private set; }
    public float midX { get; private set; }
    public float midRightX { get; private set; }
    public float rightX { get; private set; }
    public float[] createPosX { get; private set; }

    // 颜色相关
    public int addColorIndex { get; set; }
    public bool isEnergyMax { get; set; }

    // 车道序列
    public int[] laneSequence { get; private set; }

    // 其他
    public float largestRatePosZ { get; set; }
    public int pickUpCount { get; set; }
    public float lastPickupPosY { get; set; }
    public float currentPickUpPosY { get; set; }

    public PickupData() {
        // 生成参数
        maxPickUpRow = 7;
        pickUpPosY = 0.05f;
        pickupSpace = 0.05f;

        // 拾取物高度
        smallPickupHeight = 0.1f;
        bigPickupHeight = 0.5f;

        // 分数和能量
        heightPickupScore = 5;
        lowPickupScore = 1;
        heightPickupEnergy = 4;
        lowPickupEnergy = 2;

        // 生成位置X轴
        leftX = -6f;
        midLeftX = -3f;
        midX = 0f;
        midRightX = 3f;
        rightX = 6f;
        createPosX = new float[] { leftX, midLeftX, midX, midRightX, rightX };

        // 颜色相关
        addColorIndex = -1;
        isEnergyMax = false;

        // 车道序列
        laneSequence = new int[] { 1, 2, 3, 3, 2, 1 };

        // 其他
        largestRatePosZ = 1000f;
        pickUpCount = 0;
        lastPickupPosY = 0f;
        currentPickUpPosY = 0f;
    }

    // 计算SpaceOfZ
    public void CalculateSpaceOfZ(float roadLength = 10f) {
        //（（地面长度-拾取物总长度） / 间隙个数）
        spaceOfZ = ((roadLength - maxPickUpRow) / (maxPickUpRow + 1));
    }

    // 重置方法
    public void Reset() {
        addColorIndex = -1;
        isEnergyMax = false;
        largestRatePosZ = 1000f;
        pickUpCount = 0;
        lastPickupPosY = 0f;
        currentPickUpPosY = 0f;
    }
}