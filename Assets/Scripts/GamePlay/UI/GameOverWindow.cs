using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOverWindow : BaseWindowWrapper<GameOverWindow> {
    private Text txtScore;
    private Text txtGold;

    protected override void InitCtrl() {
        Transform overPanel = transform.Find("OverPanel");
        Transform imgGoldBk = overPanel.Find("imgGoldBk");
        txtScore = overPanel.gameObject.GetChildControl<Text>("txtScore");
        txtGold = imgGoldBk.gameObject.GetChildControl<Text>("txtGold");
    }

    protected override void OnPreOpen() {
        txtScore.text = $"Your Score\n{BattleMgr.Instance.overScore}";
        txtGold.text = $"+{BattleMgr.Instance.overGold}";
        CurrencyMgr.Instance.Gold += (int)(ScoreMgr.Instance.Score * RateMgr.Instance.maxRate);
    }

    protected override void OnOpen() {
    }

    protected override void InitMsg() {
    }

    protected override void ClearMsg() {
    }

}
