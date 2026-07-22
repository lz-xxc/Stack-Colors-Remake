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
    public int type { get; private set; }



    public void SetData(Vector3 pos, float height, int belongRoadId, int colorIndex, int score, int energy, int type) {
        isTriggered = false;

        transform.position = pos;
        this.belongRoadId = belongRoadId;
        ChangeColor(colorIndex);
        this.score = score;
        this.energy = energy;
        this.height = height;
        transform.localScale = new Vector3(transform.localScale.x, height, transform.localScale.z);

        gameObject.SetActive(true);
    }

    public void SetTransform(Transform parent, float posY) {
        transform.SetParent(parent);
        transform.localPosition = new Vector3(0, posY, 0);
    }

    public bool isBelongRoadId(int roadId) {
        return belongRoadId == roadId;
    }

    public void ChangeColor(int colorIndex) {
        this.GetComponent<Renderer>().sharedMaterial = ColorProxy.Instance.materials[colorIndex];
        this.colorIndex = colorIndex;
    }

    void OnTriggerEnter(Collider other) {
        if (other.tag == "Player" && !isTriggered) {
            Send.SendMsg(SendType.Pickup, gameObject, colorIndex);
            isTriggered = true;
        }
    }
}
