using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpView : MonoBehaviour {
    public PickupInfo pickupInfo {
        get; private set;
    }

    public void SetData(PickupInfo info) {
        pickupInfo = info;
        transform.position = info.position;

        gameObject.SetActive(true);
    }

    public void SetTransform(Transform parent) {
        transform.SetParent(parent);
        transform.localPosition = new Vector3(0, pickupInfo.posY, 0);
    }


    public bool isBelongRoadId(int roadId) {
        return pickupInfo.belongRoadId == roadId;
    }

    public void UpdataColor() {
        this.GetComponent<Renderer>().sharedMaterial = ColorProxy.Instance.materials[pickupInfo.colorIndex];
    }

    void OnTriggerEnter(Collider other) {
        if (other.tag == "Player" && !pickupInfo.isTriggered) {
            Send.SendMsg(SendType.Pickup, pickupInfo.Id, pickupInfo.colorIndex);
        }
    }

}
