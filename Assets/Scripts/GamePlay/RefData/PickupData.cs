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
    public int colorIndexIncrement { get; private set; }
    public bool isEnergyMax { get; private set; }

    // 车道序列
    public int[] laneSequence { get; private set; }

    // 其他
    public float largestRatePosZ { get; private set; }
    public int pickUpCount { get; private set; }
    public float currentPickUpPosY { get; private set; }

    //场景中拾取物
    public List<GameObject> activePickUps { get; private set; } = new List<GameObject>();

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
        colorIndexIncrement = -1;
        isEnergyMax = false;

        // 车道序列
        laneSequence = new int[] { 1, 2, 3, 3, 2, 1 };

        // 其他
        largestRatePosZ = 1000f;
        pickUpCount = 0;
        currentPickUpPosY = 0f;
    }

    // 计算SpaceOfZ
    public void CalculateSpaceOfZ(float roadLength = 10f) {
        //（（地面长度-拾取物总长度） / 间隙个数）
        spaceOfZ = ((roadLength - maxPickUpRow) / (maxPickUpRow + 1));
    }

    //计算拾取后木板位置
    public void CalculatePosY(float height, float lastHeight) {
        currentPickUpPosY += height / 2 + lastHeight / 2 + spaceOfZ;
    }

    // 重置方法
    public void Reset() {
        colorIndexIncrement = -1;
        isEnergyMax = false;
        largestRatePosZ = 1000f;
        pickUpCount = 0;
        currentPickUpPosY = 0f;
    }

    //回收场景木板并清空列表
    public void ClearActivePickups() {
        foreach (GameObject pickup in activePickUps) {
            if (pickup != null) {
                ObjectPool.Instance.Recycle(pickup);
            }
        }

        activePickUps.Clear();
    }

    //场景木板列表增加
    public void AddActivePickup(GameObject obj) {
        activePickUps.Add(obj);
    }

    //场景木板列表移除
    public void RemoveActivePickup(GameObject obj) {
        activePickUps.Remove(obj);
    }

    //场景木板列表移除(下标)
    public void RemoveActivePickupAt(int index) {
        activePickUps.RemoveAt(index);
    }

    //改变满能量状态
    public void SetIsEnergyMax(bool isEnergyMax) {
        this.isEnergyMax = isEnergyMax;
    }

    //改变颜色下标增加量
    public void AddColorIndexIncrement() {
        colorIndexIncrement++;
    }

    public void ChangePickupCount(int amount) {
        pickUpCount += amount;
    }

    public void SetCurrentPosY(float posY) {
        currentPickUpPosY = posY;
    }

    public void SetLastestRatePosZ(float posZ) {
        largestRatePosZ = posZ;
    }
}