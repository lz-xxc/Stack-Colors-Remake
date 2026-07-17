using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProgressMgr : Singleton<ProgressMgr> {
    private float progress = 0;

    public void Init() {
    }

    public void InitMsg() {

    }

    private void OnProgressChange(object[] _obj) {
        progress = (int)_obj[0];

    }
}
