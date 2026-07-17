using UnityEngine;
using DG.Tweening;

public class UIBreathingEffect : MonoBehaviour
{
    [Header("呼吸效果设置")]
    [SerializeField] private float minScale = 0.9f;       // 最小缩放值
    [SerializeField] private float maxScale = 1.1f;       // 最大缩放值
    [SerializeField] private float breathDuration = 1f;   // 单次呼吸时长
    [SerializeField] private Ease easeType = Ease.InOutSine; // 缓动类型

    private RectTransform rectTransform;
    private Sequence breathingSequence;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        StartBreathing();
    }

    private void OnDisable()
    {
        StopBreathing();
    }

    private void StartBreathing()
    {
        // 停止之前的动画
        StopBreathing();

        // 创建新的呼吸序列
        breathingSequence = DOTween.Sequence();

        // 设置初始缩放为最小值
        rectTransform.localScale = Vector3.one * minScale;

        // 创建一个完整的呼吸周期：从最小值到最大值再回到最小值
        breathingSequence.Append(rectTransform.DOScale(maxScale, breathDuration / 2)
            .SetEase(Ease.InOutSine))
            .Append(rectTransform.DOScale(minScale, breathDuration / 2)
            .SetEase(Ease.InOutSine));

        // 设置循环
        breathingSequence.SetLoops(-1);
    }

    private void StopBreathing()
    {
        if (breathingSequence != null)
        {
            breathingSequence.Kill();
            breathingSequence = null;
        }

        // 重置缩放
        if (rectTransform != null)
        {
            rectTransform.localScale = Vector3.one;
        }
    }

    private void OnDestroy()
    {
        StopBreathing();
    }
}