using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorChangerMgr : Singleton<ColorChangerMgr> {
    public GameObject colorChanger { get; private set; }
    public int colorIndex { get; private set; }
    public int roadId { get; private set; }

    public void Init() {
        InitMsg();
    }

    public void Clear() {
        ObjectPool.Instance.Recycle(colorChanger);
    }

    public void InitMsg() {
        Send.RegisterMsg(SendType.RecycleRoad, RecycleColorChanger);
    }

    public void ClearMsg() {
        Send.UnregisterMsg(SendType.RecycleRoad, RecycleColorChanger);
    }

    public void CreateColorChanger(Vector3 roadPos, int roadId, int colorIndex) {
        this.colorIndex = colorIndex;
        this.roadId = roadId;
        colorChanger = ObjectPool.Instance.Get("ColorChanger", true);
        colorChanger.transform.position = roadPos;
        ColorChangerView view = colorChanger.GetComponent<ColorChangerView>();
        if (view == null)
            view = colorChanger.AddComponent<ColorChangerView>();
        view.SetData(roadId, colorIndex);
    }

    public void RecycleColorChanger(object[] _obj) {
        if (colorChanger == null)
            return;
        int roadId = (int)_obj[1];
        float deltaRecycleTime = (float)_obj[2];
        ToolMgr.Instance.DelayCallBack(() => {
            ColorChangerView view = colorChanger.GetComponent<ColorChangerView>();

            if (view.isBelongRoadId(roadId)) {
                ObjectPool.Instance.Recycle(colorChanger, false);
            }
        }, deltaRecycleTime);
    }
}
