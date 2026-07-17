using UnityEngine;
using UnityEngine.EventSystems;

public class UIScalerEffect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    [Tooltip("按钮按下时的缩放比例")]
    [SerializeField] private float pressedScale = 0.9f;

    [Tooltip("按钮恢复原始大小的速度")]
    [SerializeField] private float restoreSpeed = 10f;

    private Vector3 originalScale;
    private Vector3 targetScale;
    private bool isPressed = false;

    private void Awake() {
        // 保存按钮的原始大小
        originalScale = transform.localScale;
        targetScale = originalScale;
    }

    private void Update() {
        // 平滑过渡到目标大小
        if (transform.localScale != targetScale) {
            transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * restoreSpeed);
        }
    }

    // 当按下按钮时调用
    public void OnPointerDown(PointerEventData eventData) {
        isPressed = true;
        // 设置按钮缩小的目标大小
        targetScale = originalScale * pressedScale;
    }

    // 当释放按钮时调用
    public void OnPointerUp(PointerEventData eventData) {
        isPressed = false;
        // 恢复按钮原始大小
        targetScale = originalScale;
    }

    // 当禁用脚本时确保按钮恢复原始大小
    private void OnDisable() {
        transform.localScale = originalScale;
    }
}