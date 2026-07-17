using UnityEngine;
using UnityEngine.UI;

public class PropUseWindow : BaseWindowWrapper<PropUseWindow>
{

    private PropType propType;
    private RefPropUse refPropUse;
    private RefProp refProp;

    private Image imgIcon;
    private Text txtTitle;
    private Text txtDes;
    private Button btnClose;
    private Button btnGold;
    private Button btnAds;
    private Text txtCost;

    protected override void InitCtrl()
    {
        imgIcon = gameObject.GetChildControl<Image>("Root/imgIcon");
        txtTitle = gameObject.GetChildControl<Text>("Root/txtTitle");
        txtDes = gameObject.GetChildControl<Text>("Root/txtDes");
        btnClose = gameObject.GetChildControl<Button>("btnClose");
        btnGold = gameObject.GetChildControl<Button>("Root/btnCoin");
        btnAds = gameObject.GetChildControl<Button>("Root/btnAds");
        txtCost = gameObject.GetChildControl<Text>("Root/btnCoin/txtCost");
    }

    protected override void OnPreOpen()
    {
        Refresh();
    }

    protected override void OnOpen()
    {
    }

    protected override void OnClose()
    {
        base.OnClose();
    }

    protected override void InitMsg()
    {
        btnClose.onClick.AddListener(OnCloseClick);
        btnGold.onClick.AddListener(BuyProp);
        btnAds.onClick.AddListener(AdProp);
    }

    protected override void ClearMsg()
    {
        btnClose.onClick.RemoveListener(OnCloseClick);
        btnGold.onClick.RemoveListener(BuyProp);
        btnAds.onClick.RemoveListener(AdProp);
    }

    public void OpenByType(PropType type)
    {
        propType = type;
        refPropUse = RefPropUse.GetRef(propType);
        refProp = RefProp.GetRefByPropType(propType);
        WindowMgr.Instance.OpenWindow<PropUseWindow>();
    }

    private void Refresh()
    {
        imgIcon.SetSprite(refProp.Icon);
        txtTitle.SetText(refProp.Name);
        txtDes.SetText(refPropUse.Des);
        txtCost.SetText(refPropUse.CostNum.ToString());
    }

    private void OnCloseClick()
    {
        WindowMgr.Instance.CloseWindow<PropUseWindow>();
    }

    public void BuyProp()
    {
        if (PropMgr.Instance.PropCanUse(PropType.Gold, refPropUse.CostNum))
        {
            CurrencyMgr.Instance.AddCoin(-refPropUse.CostNum);
            PropMgr.Instance.PropNumChange(propType, 1);
            // TODO 自行实现使用道具逻辑，通过Send解耦处理逻辑
            Send.SendMsg(SendType.UseProp, propType);
        }
        else
        {
            MsgTipWindow.Instance.ShowTip("Insufficient coins");
        }
    }

    public void AdProp()
    {
        // 看广告
        SuccessAD();
    }

    private void SuccessAD()
    {
        PropMgr.Instance.PropNumChange(propType, 1);
        // TODO 自行实现使用道具逻辑，通过Send解耦处理逻辑
        Send.SendMsg(SendType.UseProp, propType);
    }
}
