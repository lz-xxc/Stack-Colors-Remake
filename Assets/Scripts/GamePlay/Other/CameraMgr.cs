using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMgr : SingletonMonoBehavior<CameraMgr> {
    private Camera mainCamera;
    private float moveSpeed = 20;
    private Camera MainCamera {
        get {
            if (mainCamera == null)
                mainCamera = Camera.main;
            return mainCamera;
        }
    }

    private Quaternion rotation;
    private Vector3 pivot;

    public void Init() {
        rotation = Quaternion.Euler(25, -4, 0);
        pivot = new Vector3(1.2f, 9, -10);
    }

    public void Clear() {
        moveSpeed = 16;
    }

    public void FollowObj(Transform obj) {
        MainCamera.transform.position = obj.position + pivot;
        MainCamera.transform.rotation = rotation;
    }

    public bool LerpMove() {
        MainCamera.transform.position += Vector3.forward * Time.deltaTime * moveSpeed;
        if (MainCamera.transform.position.z >= PickUpMgr.Instance.largestRatePosZ + pivot.z) {
            moveSpeed = 0;
            return true;
        }
        else {
            moveSpeed = 17;
        }
        return false;
    }
}

