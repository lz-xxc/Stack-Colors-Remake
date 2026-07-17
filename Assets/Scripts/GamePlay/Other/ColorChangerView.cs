using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChangerView : MonoBehaviour {
    private int colorIndex = 0;
    public int beLongRoadId = 0;

    public void SetData(int roadId, int index) {
        colorIndex = index;
        beLongRoadId = roadId;
        Color color = new Color();
        switch (colorIndex) {
            case (0):
                color = Color.yellow;
                break;
            case (1):
                color = Color.red;
                break;
            case (2):
                color = Color.green;
                break;
        }
        GetComponent<Renderer>().material.color = color;
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
