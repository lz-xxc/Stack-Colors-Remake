using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SettingWindow : BaseWindowWrapper<SettingWindow>
{

    private Button btnClose;
    private SwitchView switchSound;
    private SwitchView switchMusic;
    private SwitchView switchVibrate;

    protected override void InitCtrl()
    {
        btnClose = gameObject.GetChildControl<Button>("Root/btnClose");

        GameObject soundGO = gameObject.GetChildControl<Transform>("Root/imgBg/sound").gameObject;
        GameObject musicGO = gameObject.GetChildControl<Transform>("Root/imgBg/music").gameObject;
        GameObject vibrateGO = gameObject.GetChildControl<Transform>("Root/imgBg/vibrate").gameObject;

        switchSound = new SwitchView(soundGO, SettingType.Sound);
        switchMusic = new SwitchView(musicGO, SettingType.Music);
        switchVibrate = new SwitchView(vibrateGO, SettingType.Vibrate);
    }

    protected override void OnPreOpen()
    {
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
        btnClose.onClick.AddListener(OnBtnCloseClick);
        switchSound.InitMsg();
        switchMusic.InitMsg();
        switchVibrate.InitMsg();
    }

    protected override void ClearMsg()
    {
        btnClose.onClick.RemoveListener(OnBtnCloseClick);
        switchSound.ClearMsg();
        switchMusic.ClearMsg();
        switchVibrate.ClearMsg();
    }

    private void OnBtnCloseClick()
    {
        WindowMgr.Instance.CloseWindow<SettingWindow>();
    }
}

/// <summary>
/// 开关滑块视图类，用于管理设置界面的开关滑块
/// </summary>
public class SwitchView
{
    public GameObject ItemGO;
    public Transform Trans;
    private Button btnSwitch;
    private Image imgSwitch;
    private RectTransform switchDot;

    private SettingType settingType;
    private bool isOn;

    public SwitchView(GameObject itemGO, SettingType _settingType)
    {
        this.ItemGO = itemGO;
        this.Trans = itemGO.transform;

        btnSwitch = itemGO.GetChildControl<Button>("btnSwitch");
        imgSwitch = itemGO.GetChildControl<Image>("btnSwitch");
        switchDot = itemGO.GetChildControl<RectTransform>("btnSwitch/imgDot");

        this.settingType = _settingType;
        isOn = SettingsMgr.Instance.GetSetting(settingType);
        UpdateSwitchState();
    }

    public void InitMsg()
    {
        btnSwitch.onClick.AddListener(OnSwitchClick);
    }

    public void ClearMsg()
    {
        btnSwitch.onClick.RemoveListener(OnSwitchClick);
    }

    private void OnSwitchClick()
    {
        if (settingType == SettingType.Vibrate)
        {
            SettingsMgr.Instance.Vibrate();
        }
        // 切换开关状态
        isOn = !isOn;
        SettingsMgr.Instance.SetSetting(settingType, isOn);
        // 更新开关显示状态
        UpdateSwitchState();
    }

    /// <summary>
    /// 更新开关显示状态
    /// </summary>
    private void UpdateSwitchState()
    {
        // 设置 switchDot 的 X 坐标
        float posX = isOn ? 30f : -30f;
        switchDot.anchoredPosition = new Vector2(posX, switchDot.anchoredPosition.y);

        // 设置 imgSwitch 的 sprite
        string spriteName = isOn ? "swOpen" : "swClose";
        imgSwitch.SetSprite(spriteName);
    }
}
