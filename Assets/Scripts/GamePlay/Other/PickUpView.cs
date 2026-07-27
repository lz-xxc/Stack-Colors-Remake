using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PickUpView : MonoBehaviour {
    public PickupInfo pickupInfo {
        get; private set;
    }

    public void SetData(PickupInfo info) {
        pickupInfo = info;

        gameObject.SetActive(true);
    }

    public void SetPosition(Vector3 pos) {
        transform.position = pos;
        if (pickupInfo != null)
            pickupInfo.UpdatePosition(pos);
    }

    public void SetTransform(Transform parent, float posY) {
        transform.SetParent(parent);
        pickupInfo.UpdatePosY(posY);
        transform.localPosition = new Vector3(0, posY, 0);
        if (pickupInfo != null)
            pickupInfo.UpdatePosY(posY);
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
            pickupInfo.PickupTriggered();
        }
    }

}
