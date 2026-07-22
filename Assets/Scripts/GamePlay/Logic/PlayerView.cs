using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

/// <summary>
/// 玩家视图 - 只负责显示，不处理逻辑
/// </summary>
public class PlayerView : MonoBehaviour {
    [Header("玩家对象引用")]
    [SerializeField] private GameObject playerObject;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Renderer playerRenderer;

    [Header("玩家子物体")]
    [SerializeField] private Transform pickupTool;
    [SerializeField] private Renderer toolRenderer;
    [SerializeField] private Transform keepPickUpTran;

    [Header("动画设置")]
    [SerializeField] private float toolScaleSpeed = 0.5f;
    [SerializeField] private float normalToolScale = 2f;
    [SerializeField] private float maxToolScale = 10f;

    // 缓存组件
    private Tweener currentTweener;

    // 公开属性供外部访问
    public Transform PlayerTransform => playerTransform;
    public Transform KeepPickUpTran => keepPickUpTran;
    public GameObject PlayerObject => playerObject;

    private void Awake() {
        // 如果没有手动赋值，尝试自动查找
        if (playerObject == null) playerObject = gameObject;
        if (playerTransform == null) playerTransform = transform;
        if (playerRenderer == null) playerRenderer = GetComponent<Renderer>();

        if (pickupTool == null) pickupTool = transform.Find("PickupTool");
        if (keepPickUpTran == null) keepPickUpTran = transform.Find("KeepPickUp");

        if (pickupTool != null) toolRenderer = pickupTool.GetComponent<Renderer>();
    }


    /// <summary>
    /// 更新玩家位置
    /// </summary>
    public void UpdatePosition(Vector3 position) {
        if (playerTransform != null) {
            playerTransform.position = position;
        }
    }

    /// <summary>
    /// 更新玩家X轴位置（带边界限制）
    /// </summary>
    public void UpdatePositionX(float x, float roadWidth) {
        if (playerTransform == null) return;

        Vector3 pos = playerTransform.position;
        pos.x = Mathf.Clamp(x, -roadWidth / 2 - 0.5f, roadWidth / 2 - 0.5f);
        playerTransform.position = pos;
    }

    /// <summary>
    /// 前进移动
    /// </summary>
    public void MoveForward(float speed) {
        if (playerTransform != null) {
            playerTransform.Translate(Vector3.forward * Time.deltaTime * speed, Space.Self);
        }
    }

    /// <summary>
    /// 更新玩家颜色
    /// </summary>
    public void UpdateColor(Material material) {
        if (playerRenderer != null) {
            playerRenderer.sharedMaterial = material;
        }

        if (toolRenderer != null) {
            toolRenderer.sharedMaterial = material;
        }

        // 更新所有拾取物的颜色
        if (keepPickUpTran != null) {
            foreach (Transform pickup in keepPickUpTran) {
                Renderer renderer = pickup.GetComponent<Renderer>();
                if (renderer != null) {
                    renderer.sharedMaterial = material;
                }
            }
        }
    }

    /// <summary>
    /// 工具缩放动画
    /// </summary>
    public void ScaleTool(bool isMax, string playerName = "") {
        if (pickupTool == null) return;

        // 停止当前动画
        if (currentTweener != null) {
            currentTweener.Kill();
            currentTweener = null;
        }

        float targetScale;
        if (isMax) {
            // 根据玩家名称决定缩放大小
            targetScale = playerName == "Player4(Clone)" ? 5f : maxToolScale;
        }
        else {
            targetScale = playerName == "Player4(Clone)" ? 1f : normalToolScale;
        }

        // 执行缩放动画
        currentTweener = pickupTool.DOScaleX(targetScale, toolScaleSpeed)
            .SetEase(Ease.OutBack)
            .OnComplete(() => {
                currentTweener = null;
            });
    }

    /// <summary>
    /// 直接设置工具缩放
    /// </summary>
    public void SetToolScale(float scale) {
        if (pickupTool != null) {
            Vector3 localScale = pickupTool.localScale;
            localScale.x = scale;
            pickupTool.localScale = localScale;
        }
    }


    /// <summary>
    /// 设置拾取物位置（添加到堆叠）
    /// </summary>
    public void SetPickupPosition(GameObject pickup, Vector3 position) {
        if (pickup == null || keepPickUpTran == null) return;

        pickup.transform.SetParent(keepPickUpTran);
        pickup.transform.position = position;
        pickup.transform.localRotation = Quaternion.identity;
    }

    /// <summary>
    /// 从堆叠移除拾取物
    /// </summary>
    public void RemovePickupFromStack(GameObject pickup) {
        if (pickup == null) return;
        pickup.transform.SetParent(null);
    }

    /// <summary>
    /// 清空堆叠显示
    /// </summary>
    public void ClearPickUpStack() {
        if (keepPickUpTran == null) return;

        foreach (Transform child in keepPickUpTran) {
            child.SetParent(null);
        }
    }

    /// <summary>
    /// 发射拾取物
    /// </summary>
    public void LaunchPickup(GameObject pickup, Vector3 force, float mass = 3f) {
        if (pickup == null) return;

        pickup.transform.SetParent(null);

        Rigidbody rb = pickup.AddMissingComponent<Rigidbody>();
        rb.isKinematic = false;
        rb.useGravity = true;
        rb.mass = mass;
        rb.drag = 0.15f;
        rb.angularDrag = 0.1f;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.constraints = RigidbodyConstraints.None;
        rb.AddForce(force, ForceMode.Impulse);
    }


    /// <summary>
    /// 重置View到初始状态
    /// </summary>
    public void ResetView() {
        SetToolScale(normalToolScale);
        ClearPickUpStack();

        if (playerRenderer != null && ColorProxy.Instance != null) {
            playerRenderer.sharedMaterial = ColorProxy.Instance.materials[1];
        }
    }

}