using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GuideWindow : BaseWindowWrapper<GuideWindow>
{

    private RectTransform hand;

    protected override void InitCtrl()
    {
        hand = gameObject.GetChildControl<RectTransform>("Hand");
    }

    protected override void OnPreOpen()
    {
        hand.gameObject.SetActive(false);
    }

    protected override void InitMsg()
    {
    }

    protected override void ClearMsg()
    {
    }

    /// <summary>
    /// 显示手指引导到世界坐标位置（3D场景中的游戏对象）
    /// </summary>
    public void ShowHandAtWorldPosition(Vector3 worldPosition)
    {
        WindowMgr.Instance.OpenWindow<GuideWindow>();
        hand.gameObject.SetActive(true);

        // 将世界坐标转换为UI锚点坐标
        Vector2 viewPos = Camera.main.WorldToViewportPoint(worldPosition);
        Vector2 anchoredPos = new Vector2(viewPos.x * 750f, viewPos.y * Screen.height * (750f / Screen.width));
        // 如果需要可以加上一定偏移量，如下：
        // anchoredPos += new Vector2(-10f, 30f);
        hand.anchoredPosition = anchoredPos;
    }

    /// <summary>
    /// 显示手指引导到UI位置（已转换好的屏幕坐标或UI坐标）
    /// </summary>
    public void ShowHandAtUIPosition(Vector3 uiPosition)
    {
        WindowMgr.Instance.OpenWindow<GuideWindow>();
        hand.gameObject.SetActive(true);
        hand.transform.position = uiPosition;
    }
}
