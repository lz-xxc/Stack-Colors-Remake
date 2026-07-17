using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RateWindow : BaseWindowWrapper<RateWindow>
{

    private Button btnYes;
    private Button btnNo;
    private Button btnClose;

    protected override void InitCtrl()
    {
        btnYes = gameObject.GetChildControl<Button>("imgBg/btnYes");
        btnNo = gameObject.GetChildControl<Button>("imgBg/btnNo");
        btnClose = gameObject.GetChildControl<Button>("imgBg/btnClose");
    }

    protected override void OnPreOpen()
    {
    }

    protected override void OnOpen()
    {

    }

    protected override void InitMsg()
    {
        btnYes.onClick.AddListener(OnYesClick);
        btnNo.onClick.AddListener(OnNoClick);
        btnClose.onClick.AddListener(OnNoClick);
    }

    protected override void ClearMsg()
    {
        btnYes.onClick.RemoveListener(OnYesClick);
        btnNo.onClick.RemoveListener(OnNoClick);
        btnClose.onClick.RemoveListener(OnNoClick);
    }

    private void OnYesClick()
    {
        RateMgr.Instance.ShowRate();
        WindowMgr.Instance.CloseWindow<RateWindow>();
    }

    private void OnNoClick()
    {
        WindowMgr.Instance.CloseWindow<RateWindow>();
    }
}