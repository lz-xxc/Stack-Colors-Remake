using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RateMgr : Singleton<RateMgr> {

    private List<int> lvPassDic = new List<int>() { 3, 10 };

    public float maxRate { get; private set; } = 1;

    public void Init() {
        InitMsg();
    }

    public void Clear() {
        maxRate = 1;
    }

    public void InitMsg() {
        Send.RegisterMsg(SendType.ShowRate, OnShowRate);
        Send.RegisterMsg(SendType.TryUpdateRate, SetMaxRate);
    }

    public void ClearMsg() {
        Send.UnregisterMsg(SendType.ShowRate, OnShowRate);
        Send.UnregisterMsg(SendType.TryUpdateRate, SetMaxRate);
    }

    private void OnShowRate(object[] _objs) {
        int level = (int)_objs[0];
        ShowRateWindow(level);
    }

    /// <summary>
    /// չʾ����
    /// </summary>
    public void ShowRate() {
#if UNITY_IOS && !UNITY_EDITOR
        UnityStoreKit storeKit = new UnityStoreKit();
        storeKit.GoToCommnet();
#elif UNITY_ANDROID && !UNITY_EDITOR
        AndroidJavaClass pluginClass = new AndroidJavaClass("com.aar.rate.RateUtils");
        if (pluginClass != null) {
            pluginClass.CallStatic("ShowRate");
        }
#endif
    }

    private void ShowRateWindow(int level) {
        if (!lvPassDic.Contains(level)) {
            return;
        }
        WindowMgr.Instance.OpenWindow<RateWindow>();
    }

    public void SetMaxRate(object[] _objs) {
        float rate = (float)_objs[0];
        float posZ = (float)_objs[1];
        if (rate > maxRate) {
            maxRate = rate;
            Debug.Log(maxRate);
            Send.SendMsg(SendType.UpdateRate, posZ);
        }
    }

}