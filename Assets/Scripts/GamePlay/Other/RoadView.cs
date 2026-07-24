using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadView : MonoBehaviour {
    public RoadInfo info { get; private set; }

    private Renderer render;

    private void Awake() {
        render = GetComponent<Renderer>();
    }

    public void SetData(RoadInfo roadInfo) {
        info = roadInfo;
        transform.position = info.roadPos;
        gameObject.SetActive(true);
        if (info.roadType == E_RoadType.Speedup)
            render.material = ColorProxy.Instance.matGray;
    }

    public void ClearData() {
        info.ClearInfo();

        gameObject.SetActive(false);
        render.material = ColorProxy.Instance.matBlack;
    }

    private void OnTriggerExit(Collider other) {
        if (other.gameObject.tag == "Player" && !info.isTriggered && info.roadType == E_RoadType.Normal) {
            Send.SendMsg(SendType.RecycleRoad, gameObject, info.roadId, info.deltaRecycleTime);

            info.RoadTriggered();
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player" && !info.isTriggered && info.roadType == E_RoadType.Speedup) {
            Send.SendMsg(SendType.EnterSpeedupRoad, info.roadId, transform.position);

            info.RoadTriggered();
        }
    }
}
