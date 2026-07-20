using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

/// <summary>
/// 战场管理类，管理战场的进行逻辑
/// </summary>
public class BattleMgr : Singleton<BattleMgr> {
    private GameObject propPref;

    public BattleState state = BattleState.Wait;

    private List<GameObject> activeTexts = new List<GameObject>();

    private float floatDuration = 1;
    private float floatDistance = 100;

    public void Init() {
        InitMsg();
    }

    public void Clear() {
        DOTween.KillAll();
        ClearAllFloatingTexts();
    }

    public void InitMsg() {
    }

    public void ClearMsg() {
    }

    public void StartBattle() {
        PlayerMgr.Instance.StartBattle();
        RoadMgr.Instance.StartBattle();
        state = BattleState.Game;
    }

    public void ShowFloatingText(Vector3 worldPosition, int score) {
        if (state == BattleState.GameOver || state == BattleState.Wait) {
            return;
        }

        // 世界坐标转屏幕坐标
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPosition);
        Canvas canvas = BattleWindow.Instance.transform.GetComponentInParent<Canvas>();

        // 屏幕坐标转 UI 本地坐标
        RectTransform containerRect = BattleWindow.Instance.floatingTextTran.GetComponent<RectTransform>();
        Vector2 uiPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            containerRect,
            screenPos,
            canvas.worldCamera,
            out uiPos
        );

        // 从对象池获取
        GameObject go = ObjectPool.Instance.Get("txtAddScore", BattleWindow.Instance.floatingTextTran, false);


        // 设置位置
        RectTransform rect = go.GetComponent<RectTransform>();
        rect.anchoredPosition = uiPos;

        // 显示动画
        GetScoreText floatingText = go.GetComponent<GetScoreText>();
        floatingText.Show($"+{score}", floatDuration, floatDistance);
        activeTexts.Add(go);
    }

    public void RemoveActiveTxt(GameObject obj) {
        activeTexts.Remove(obj);
    }

    public void ClearAllFloatingTexts() {
        foreach (var go in activeTexts) {
            if (go != null) {
                // 回收
                ObjectPool.Instance.Recycle(go, false);
            }
        }
        activeTexts.Clear();
    }
}

public enum BattleState {
    Wait,
    Game,
    Pause,
    WaitRevive,
    GameOver,
}