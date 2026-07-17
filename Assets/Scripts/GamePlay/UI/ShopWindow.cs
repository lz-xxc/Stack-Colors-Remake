using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
/// 商店界面
/// </summary>
public class ShopWindow : BaseWindowWrapper<ShopWindow> {
    private List<ShopItemView> itemProxyList = new List<ShopItemView>();
    private GameObject itemPrefab;
    private Transform transListParent;
    public Transform clickIconPos;

    private Button btnClose;
    private Button btnBuy;
    private Text txtBuy;

    protected override void InitCtrl() {
        itemPrefab = gameObject.GetChildControl<RectTransform>("Panel/items/item").gameObject;
        transListParent = gameObject.GetChildControl<Transform>("Panel/items");
        btnClose = gameObject.GetChildControl<Button>("btnClose");
        btnBuy = gameObject.GetChildControl<Button>("btnBuy");
        txtBuy = gameObject.GetChildControl<Text>("btnBuy/txtGold");
        clickIconPos = gameObject.GetChildControl<Transform>("Panel/ClickIconPos");
        itemPrefab.SetActive(false);
    }

    protected override void OnPreOpen() {
        RefreshList();
    }

    protected override void OnOpen() {
        if (itemProxyList.Count > 0) {
            foreach (var item in itemProxyList) {
                item.SetSelect(false);
            }
            itemProxyList[0].SetSelect(true);
            ShopMgr.Instance.selectItemInfo = itemProxyList[0].shopItemInfo;
            ShopMgr.Instance.RefreshItem();
        }
    }

    protected override void OnClose() {
        base.OnClose();
    }

    protected override void InitMsg() {
        Send.RegisterMsg(SendType.ClickShopIconChange, RefreshBtnBuy);
        btnClose.onClick.AddListener(OnCloseClick);
        btnBuy.onClick.AddListener(OnBuyClick);
    }

    protected override void ClearMsg() {
        Send.UnregisterMsg(SendType.ClickShopIconChange, RefreshBtnBuy);
        btnClose.onClick.RemoveListener(OnCloseClick);
        btnBuy.onClick.RemoveListener(OnBuyClick);
    }


    private void OnCloseClick() {
        WindowMgr.Instance.CloseWindow<ShopWindow>();
        WindowMgr.Instance.OpenWindow<MainWindow>();
    }

    private void RefreshList() {
        for (int i = transListParent.childCount - 1; i > 0; i--) {
            GameObject.Destroy(transListParent.GetChild(i).gameObject);
        }
        itemProxyList.Clear();

        int length = ShopMgr.Instance.itemInfoList.Count;
        for (int index = 0; index < length; index++) {
            ShopItemInfo itemInfo = ShopMgr.Instance.itemInfoList[index];
            GameObject itemGo = GameObject.Instantiate(itemPrefab, transListParent, false);
            ShopItemView itemView = new ShopItemView(itemGo);
            itemView.SetData(itemInfo);
            itemProxyList.Add(itemView);
        }

    }

    public ShopItemView GetItem(int id) {
        foreach (var item in itemProxyList) {
            if (item.shopItemInfo.refShop.ItemId == id)
                return item;
        }
        return null;
    }

    private void RefreshBtnBuy(object[] _obj) {
        txtBuy.text = (string)_obj[0];
        btnBuy.interactable = (bool)_obj[1];

    }

    private void OnBuyClick() {
        ShopMgr.Instance.OnItemStateChange();
        ShopMgr.Instance.RefreshItem();
    }
}


/// <summary>
/// 单物品显示类
/// </summary>
public class ShopItemView {
    private GameObject itemGo;
    public ShopItemInfo shopItemInfo { get; private set; }
    private Button btnClick;
    private Image iconImage;
    private GameObject icon;

    // 静态变量，所有商品共享
    private static GameObject lastIcon = null;
    private static int lastItemId = -1;

    public ShopItemView(GameObject _itemGo) {
        itemGo = _itemGo;
        btnClick = itemGo.GetComponent<Button>();
        btnClick.onClick.AddListener(OnClick);

        iconImage = itemGo.GetChildControl<Image>("Image");
        iconImage.color = Color.gray;
    }

    public void SetData(ShopItemInfo itemInfo) {
        shopItemInfo = itemInfo;
        itemGo.SetActive(true);
        Refresh();
    }

    public void ClearData() {
        shopItemInfo = null;
        itemGo.SetActive(false);
    }

    private void Refresh() {
        if (shopItemInfo == null || iconImage == null) return;

        string iconName = shopItemInfo.refShop.Name;
        Sprite sprite = LocalAssetMgr.Instance.Load_Asset<Sprite>("Atlas", iconName);

        if (sprite != null) {
            iconImage.sprite = sprite;
            iconImage.color = Color.gray;
        }
    }

    public void SetSelect(bool select) {
        if (iconImage != null)
            iconImage.color = select ? Color.white : Color.gray;

        if (select) {
            // 如果已经显示了同一个商品，不重复创建
            if (lastIcon != null && lastItemId == shopItemInfo.refShop.ItemId) {
                return;
            }

            // 回收上一个预览
            RecycleLastIcon();

            icon = ObjectPool.Instance.Get(shopItemInfo.refShop.Name, ShopWindow.Instance.clickIconPos, true);
            icon.layer = LayerMask.NameToLayer("UI");
            icon.transform.SetScale(100);

            icon.transform.DOKill();
            icon.transform.DORotate(new Vector3(0, 360, 0), 4f, RotateMode.LocalAxisAdd)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);
            icon.transform.DOScale(70, 2f)
                .SetLoops(-1, LoopType.Yoyo)
                .SetEase(Ease.InOutQuad);

            lastIcon = icon;
            lastItemId = shopItemInfo.refShop.ItemId;
        }
        else {
            RecycleLastIcon();
            icon = null;
        }
    }

    // 回收上一个预览
    private void RecycleLastIcon() {
        if (lastIcon != null) {
            lastIcon.transform.DOKill();
            ObjectPool.Instance.Recycle(lastIcon);
        }
    }

    private void OnClick() {
        if (shopItemInfo == null) return;

        // 清除上一选中
        if (shopItemInfo.refShop.ItemId != lastItemId)
            ShopWindow.Instance.GetItem(lastItemId).SetSelect(false);

        // 选中当前
        SetSelect(true);
        ShopMgr.Instance.selectItemInfo = shopItemInfo;
        ShopMgr.Instance.RefreshItem();

    }
}
