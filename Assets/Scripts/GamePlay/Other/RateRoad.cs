using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class RateRoad : MonoBehaviour {
    private int rateRoadId;
    private float startRate = 1;
    private float rate = 0;

    private TMP_Text txtRate;

    private void Awake() {
        txtRate = gameObject.GetChildControl<TMP_Text>("txtRate");
    }

    public void SetData(int id, Vector3 pos) {
        rateRoadId = id;
        transform.position = pos;
        rate = startRate + 0.1f * rateRoadId;
        txtRate.text = $"×{rate}";
        gameObject.SetActive(true);

    }

    private void OnCollisionEnter(Collision other) {
        if (other.gameObject.tag == "Prop") {
            if (RateMgr.Instance.SerMaxRate(rate)) {
                PickUpMgr.Instance.pickupData.largestRatePosZ = other.transform.position.z;
                PlayerMgr.Instance.ResetCurrentTime();
            }
        }
    }
}
