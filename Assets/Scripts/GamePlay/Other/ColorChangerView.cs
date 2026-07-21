using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChangerView : MonoBehaviour {
    private int colorIndex = 0;
    public int beLongRoadId = 0;

    public void SetData(int roadId, int index) {
        colorIndex = index;
        beLongRoadId = roadId;

        GetComponent<Renderer>().sharedMaterial = ColorProxy.Instance.materials[index];
    }

    public bool isBelongRoadId(int roadId) {
        return beLongRoadId == roadId;
    }

    void OnTriggerEnter(Collider other) {
        if (other.gameObject.tag == "Player") {
            Send.SendMsg(SendType.PlayerColorChange, colorIndex, gameObject);
        }
    }
}
