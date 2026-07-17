using UnityEngine;
using UnityEngine.UI;

public class UnlockWindow : BaseWindowWrapper<UnlockWindow>
{
    private PropType propType;
    private RefPropUse refPropUse;
    private RefProp refProp;

    private GameObject unlockTip;
    private Animator anim;
    private Button btnClose;
    private Image imgIcon;
    private Text txtTitle;
    private Text txtDes;

    protected override void InitCtrl()
    {
        unlockTip = gameObject.GetChildControl<Transform>("unlockTip").gameObject;
        anim = gameObject.GetChildControl<Animator>("unlockTip");
        btnClose = gameObject.GetChildControl<Button>("unlockTip/imgBg/btnClose");
        imgIcon = gameObject.GetChildControl<Image>("unlockTip/imgBg/imgIcon");
        txtTitle = gameObject.GetChildControl<Text>("unlockTip/imgBg/txtTitle");
        txtDes = gameObject.GetChildControl<Text>("unlockTip/imgBg/txtTitle/txtdes");
    }

    protected override void OnPreOpen()
    {
        unlockTip.SetActive(false);
        imgIcon.SetSprite(refProp.Icon);
        txtTitle.SetText(refProp.Name);
        txtDes.SetText(refPropUse.Des);
    }

    protected override void OnOpen()
    {
        unlockTip.SetActive(true);
        anim.Play("hint_anim_01", 0, 0);
    }

    protected override void OnClose()
    {
        base.OnClose();
    }

    protected override void InitMsg()
    {
        btnClose.onClick.AddListener(OnCloseClick);
    }

    protected override void ClearMsg()
    {
        btnClose.onClick.RemoveListener(OnCloseClick);
    }

    private void OnCloseClick()
    {
        WindowMgr.Instance.CloseWindow<UnlockWindow>();
    }

    public void OpenPropType(PropType _propType)
    {
        propType = _propType;
        refPropUse = RefPropUse.GetRef(propType);
        refProp = RefProp.GetRefByPropType(propType);
        WindowMgr.Instance.OpenWindow<UnlockWindow>();
    }
}
