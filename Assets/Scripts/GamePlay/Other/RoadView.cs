using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadView : MonoBehaviour {
    private bool isTriggered = false;
    private bool isSpeedupRoad;

    private int roadId;

    private Renderer render;

    private void Awake() {
        render = GetComponent<Renderer>();
    }

    public void SetData(int id, Vector3 pos, bool isSpeedupRoad) {
        roadId = id;
        transform.position = pos;
        this.isSpeedupRoad = isSpeedupRoad;

        isTriggered = false;
        gameObject.SetActive(true);
        if (isSpeedupRoad)
            render.material = ColorProxy.Instance.matGray;
    }

    public void ClearData() {
        isTriggered = false;

        gameObject.SetActive(false);
        render.material = ColorProxy.Instance.matBlack;
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.tag == "Player" && !isTriggered && !isSpeedupRoad) {

            if (!isSpeedupRoad)
                RoadMgr.Instance.RecycleRoad(gameObject, roadId);

            isTriggered = true;
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player" && !isTriggered && isSpeedupRoad) {
            if (roadId == RoadMgr.Instance.roadData.speedupRoadStart) {
                PlayerMgr.Instance.EnterSpeedupMode();
            }

            if (roadId == RoadMgr.Instance.roadData.finishRoadStart - 1) {
                PlayerMgr.Instance.setTargetZ(transform.position.z);
            }

            isTriggered = true;
        }
    }
}
