using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainWindow : BaseWindowWrapper<MainWindow> {
    private Button btnShop;
    private Button btnPlay;

    protected override void InitCtrl() {
        btnShop = gameObject.GetChildControl<Button>("btnShop");
        btnPlay = gameObject.GetChildControl<Button>("btnPlay");
    }

    protected override void OnPreOpen() {
    }

    protected override void OnOpen() {
    }

    protected override void InitMsg() {
        btnShop.onClick.AddListener(OnBtnShopClick);
        btnPlay.onClick.AddListener(OnBtnPlayClick);
    }

    protected override void ClearMsg() {
        btnShop.onClick.RemoveListener(OnBtnShopClick);
        btnPlay.onClick.RemoveListener(OnBtnPlayClick);
    }

    private void OnBtnShopClick() {
        WindowMgr.Instance.OpenWindow<ShopWindow>();
        WindowMgr.Instance.CloseWindow<MainWindow>();
    }

    private void OnBtnPlayClick() {
        GameStateMgr.Instance.SwitchState(GameState.Battle);
    }

}
