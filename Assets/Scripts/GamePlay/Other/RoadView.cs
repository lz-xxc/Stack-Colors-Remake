using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadView : MonoBehaviour {
    private bool isTriggered = false;
    private bool isSpeedupRoad;

    private int roadId;

    public void SetData(int id, Vector3 pos, bool isSpeedupRoad) {
        roadId = id;
        transform.position = pos;
        this.isSpeedupRoad = isSpeedupRoad;

        isTriggered = false;
        gameObject.SetActive(true);
    }

    public void ClearData() {
        isTriggered = false;

        gameObject.SetActive(false);
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
            if (roadId == RoadMgr.Instance.speedupRoadStart) {
                PlayerMgr.Instance.EnterSpeedupMode();
            }

            if (roadId == RoadMgr.Instance.finishRoadStart - 1) {
                PlayerMgr.Instance.setTargetZ(transform.position.z);
            }

            isTriggered = true;
        }
    }
}
