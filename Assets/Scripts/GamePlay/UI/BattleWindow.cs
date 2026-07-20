using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BattleWindow : BaseWindowWrapper<BattleWindow> {
    private Text txtScore;
    private Text txtGold;
    private EventTrigger imgCtrl;
    private Image energyFill;
    private Image processFill;
    private Image forceFill;
    private Camera uiCamera;
    private GameObject ForceBar;

    private GameObject txtGetScorePref;
    public RectTransform floatingTextTran { get; private set; }

    protected override void InitCtrl() {
        txtScore = gameObject.GetChildControl<Text>("txtScore");
        txtGold = gameObject.GetChildControl<Text>("txtGold");
        energyFill = gameObject.GetChildControl<Image>("EnergyBar/Fill");
        processFill = gameObject.GetChildControl<Image>("ProcessBar/Fill");
        forceFill = gameObject.GetChildControl<Image>("ForceBar/Fill");
        imgCtrl = gameObject.GetChildControl<EventTrigger>("imgCtrl");

        ForceBar = transform.Find("ForceBar").gameObject;

        if (floatingTextTran == null) {
            Transform found = transform.Find("floatingTexts");
            if (found != null) {
                floatingTextTran = found.GetComponent<RectTransform>();
            }
        }
    }

    protected override void OnPreOpen() {
    }

    protected override void OnOpen() {
        txtGold.text = CurrencyMgr.Instance.Gold.ToString();
        HideForceBar();
        EnergyMgr.Instance.Energy = 0;
        ScoreMgr.Instance.Score = 0;
    }

    protected override void OnPreClose() {
        base.OnPreClose();
    }

    protected override void OnClose() {
        energyFill.fillAmount = 0;
        processFill.fillAmount = 0;
        base.OnClose();
    }

    protected override void InitMsg() {
        Send.RegisterMsg(SendType.ScoreChange, OnScoreChange);
        Send.RegisterMsg(SendType.EnergyChange, OnEnergyChange);
        imgCtrl.AddListener(EventTriggerType.Drag, OnDrag);
    }

    protected override void ClearMsg() {
        Send.UnregisterMsg(SendType.ScoreChange, OnScoreChange);
        Send.UnregisterMsg(SendType.EnergyChange, OnEnergyChange);
    }

    public void OnScoreChange(object[] _obj) {
        int score = (int)_obj[1];
        txtScore.text = score.ToString();
    }

    public void OnEnergyChange(object[] _obj) {
        energyFill.fillAmount = (float)_obj[0] / 100;
    }

    public void ProcessAmount(float amount) {
        processFill.fillAmount = amount;
    }

    public void ForceAmount(float amount) {
        forceFill.fillAmount = amount;
    }

    public void ShowForceBar() {
        ForceBar.SetActive(true);
    }

    public void HideForceBar() {
        ForceBar.SetActive(false);
    }

    private void OnDrag(BaseEventData arg0) {
        Send.SendMsg(SendType.CtrlDrag, arg0);
    }
}


