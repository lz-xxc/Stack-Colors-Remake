using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RateRoad : MonoBehaviour {
    private RoadInfo info;
    private TMP_Text txtRate;

    private void Awake() {
        txtRate = gameObject.GetChildControl<TMP_Text>("txtRate");
    }

    public void SetData(RoadInfo info) {
        this.info = info;
        transform.position = info.roadPos;
        txtRate.text = $"×{info.rateRoadRate}";
        gameObject.SetActive(true);
        initPosZ();
    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.tag == "Prop") {
            Send.SendMsg(SendType.TryUpdateRate, info.rateRoadRate, other.transform.position.z);
        }
    }

    private void initPosZ() {
        if (info.rateRoadRate == 1)
            Send.SendMsg(SendType.UpdateRate, transform.position.z);
    }
}
