using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RewardWindow : BaseWindowWrapper<RewardWindow>
{
    private GameObject propItem;
    private Transform rewardsTR;
    private Button btnClose;

    private Dictionary<PropType, int> props = new Dictionary<PropType, int>();
    private List<PropItemView> rewardList = new List<PropItemView>();

    protected override void InitCtrl()
    {
        propItem = gameObject.GetChildControl<Transform>("Root/prop").gameObject;
        rewardsTR = gameObject.GetChildControl<Transform>("Root/Rewards");
        propItem.SetActive(false);
        btnClose = gameObject.GetChildControl<Button>("Root/btnClose");
    }

    protected override void OnPreOpen()
    {
        int index = 0;
        PropItemView view;
        foreach (var item in props)
        {
            if (index >= rewardList.Count)
            {
                GameObject itemGO = GameObject.Instantiate(propItem, rewardsTR);
                itemGO.SetActive(true);
                view = new PropItemView(itemGO);
                rewardList.Add(view);
            }
            else
            {
                view = rewardList[index];
            }
            view.SetData(item.Key, item.Value);
            index++;
        }
    }

    protected override void OnOpen()
    {
    }

    protected override void OnClose()
    {
        base.OnClose();
        foreach (PropItemView item in rewardList)
        {
            item.ClearData();
        }
    }

    protected override void InitMsg()
    {
        btnClose.onClick.AddListener(OnCloseClick);
    }

    protected override void ClearMsg()
    {
        btnClose.onClick.AddListener(OnCloseClick);
    }

    private void OnCloseClick()
    {
        WindowMgr.Instance.CloseWindow<RewardWindow>();
    }

    public void ShowRewards(Dictionary<PropType, int> _props)
    {
        props = _props;
        WindowMgr.Instance.OpenWindow<RewardWindow>();
    }
}

public class PropItemView
{
    private RefProp refProp;

    public GameObject ItemGO { get; }
    public Transform Trans { get; }
    private Image imgIcon;
    private Text txtNum;
    public bool isCoin = false;

    public PropItemView(GameObject go)
    {
        ItemGO = go;
        Trans = ItemGO.transform;
        imgIcon = ItemGO.GetComponent<Image>();
        txtNum = ItemGO.GetChildControl<Text>("Text");
    }

    public void SetData(PropType propType, int num)
    {
        refProp = RefProp.GetRefByPropType(propType);
        imgIcon.SetSprite(refProp.Icon);
        txtNum.text = $"+{num}";
        ItemGO.SetActive(true);
    }

    public void SetData(PropItem item)
    {
        refProp = RefProp.GetRef(item.ID);
        imgIcon.SetSprite(refProp.Icon);
        txtNum.text = $"+{item.Num}";
        ItemGO.SetActive(true);
    }

    public void ClearData()
    {
        ItemGO.SetActive(false);
    }
}