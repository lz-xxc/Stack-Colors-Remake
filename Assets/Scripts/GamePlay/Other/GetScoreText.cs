using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class GetScoreText : MonoBehaviour {
    private Text txtGetScore;
    private RectTransform rectTransform;
    private Vector2 startPos;

    private void Awake() {
        txtGetScore = GetComponent<Text>();
        rectTransform = GetComponent<RectTransform>();
    }

    public void Show(string text, float duration, float distance) {

        // ✅ 组件检查
        if (txtGetScore == null || rectTransform == null) {
            Debug.LogWarning($"⚠️ {gameObject.name} 组件缺失");
            return;
        }

        txtGetScore.text = text;
        //重置位置
        startPos = rectTransform.anchoredPosition;
        //重置大小
        rectTransform.localScale = Vector3.one;
        //重置透明度
        txtGetScore.DOFade(1, 0);

        PlayAnimation(duration, distance);
    }

    private void PlayAnimation(float duration, float distance) {
        Sequence sequence = DOTween.Sequence();

        //向上飘动
        sequence.Join(rectTransform.DOAnchorPosY(startPos.y + distance, duration));

        //淡出
        sequence.Join(txtGetScore.DOFade(0f, duration).SetEase(Ease.OutQuad));

        sequence.OnComplete(() => {
            ObjectPool.Instance.Recycle(gameObject, false);
            BattleMgr.Instance.RemoveActiveTxt(gameObject);
        });
    }
}
