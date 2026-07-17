using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpView : MonoBehaviour {
    private bool isTriggered = false;
    public int belongRoadId { get; private set; }
    private int colorIndex;
    public float height { get; private set; }
    public int score { get; private set; }
    public int energy { get; private set; }
    public float posY { get; private set; }



    public void SetData(Vector3 pos, float height, int belongRoadId, int colorIndex, int score, int energy) {
        isTriggered = false;

        transform.position = pos;
        this.belongRoadId = belongRoadId;
        ChangeColorIndex(colorIndex);
        this.score = score;
        this.energy = energy;
        this.height = height;
        transform.localScale = new Vector3(transform.localScale.x, height, transform.localScale.z);

        gameObject.SetActive(true);
    }

    public void SetPosY(float posY) {
        this.posY = posY;
    }

    public bool isBelongRoadId(int roadId) {
        return belongRoadId == roadId;
    }

    public void ChangeColorIndex(int colorIndex) {
        this.colorIndex = colorIndex;
    }

    void OnTriggerEnter(Collider other) {
        if (other.tag == "Player" && !isTriggered) {
            PickUpMgr.Instance.PickUp(gameObject, colorIndex);
            isTriggered = true;
        }
    }
}
