using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyMgr : Singleton<EnergyMgr> {
    public bool isEnergyMax { get; private set; }

    private int maxEnergy = 100;
    private float _currentEnergy = 0;
    private float reduceEnergyAmount = 20;

    public float Energy {
        get {
            return _currentEnergy;
        }
        set {
            if (value > maxEnergy) {
                value = maxEnergy;
                isEnergyMax = true;
                Send.SendMsg(SendType.EnergyMax, 1);
            }
            if (value <= 0) {
                value = 0;
                isEnergyMax = false;
                Send.SendMsg(SendType.EnergyEmpty, 1);
            }
            _currentEnergy = value;
            Send.SendMsg(SendType.EnergyChange, _currentEnergy);
        }
    }

    public void Init() {
    }


    public void OnUpdate() {
        if (isEnergyMax) {
            Energy -= Time.deltaTime * reduceEnergyAmount;
        }
    }
}
