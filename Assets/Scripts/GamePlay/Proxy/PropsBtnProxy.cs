using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PropsBtnProxy : MonoBehaviour
{
    public PropType Type;

    private Button btnItem;
    private Image imgIcon;
    private Image imgAdd;
    private Image imgLock;
    private Text txtLock;
    private Text txtNum;
    private Transform numTR;
    private RefPropUse refPropUse;
    private bool unlocked = false;

    void Awake()
    {
        btnItem = gameObject.GetComponent<Button>();
        imgIcon = gameObject.GetChildControl<Image>("imgIcon");
        imgAdd = gameObject.GetChildControl<Image>("imgIcon/imgAdd");
        imgLock = gameObject.GetChildControl<Image>("imgLock");
        txtLock = gameObject.GetChildControl<Text>("imgLock/txtLock");
        numTR = gameObject.GetChildControl<Transform>("imgIcon/quantity"); // 道具数量
        txtNum = gameObject.GetChildControl<Text>("imgIcon/quantity/txtNum");
        refPropUse = RefPropUse.GetRef(Type);
    }

    void Start()
    {
        btnItem.onClick.AddListener(OnClick);
        Send.RegisterMsg(SendType.PropChange, OnPropChange);
    }

    void OnEnable()
    {
        Refresh();
        if (NeedUnlock())
        {
            // 默认送一个道具
            PropMgr.Instance.PropNumChange(Type, 1);
            unlocked = true;
            UnlockWindow.Instance.OpenPropType(Type);
        }
    }

    void OnDestroy()
    {
        btnItem.onClick.RemoveListener(OnClick);
        Send.UnregisterMsg(SendType.PropChange, OnPropChange);
    }

    public void Refresh()
    {
        bool isLock = IsLock();
        imgIcon.gameObject.SetActive(!isLock);
        imgLock.gameObject.SetActive(isLock);
        imgIcon.SetSprite($"{Type.ToString().ToLower()}");
        txtLock.SetTextFormat("Level {0}", refPropUse.UnlockLv);

        // 道具数量
        int num = PropMgr.Instance.GetPropNum(Type);
        numTR.gameObject.SetActive(num > 0);
        txtNum.text = $"{num}";
        imgAdd.gameObject.SetActive(num <= 0);
    }

    private void OnClick()
    {
        if (IsLock())
        {
            return;
        }


        // 如果有道具直接使用
        int num = PropMgr.Instance.GetPropNum(Type);
        if (num > 0)
        {
            UsePropNum();
        }
        else
        {
            PropUseWindow.Instance.OpenByType(Type);
        }
    }

    private void OnPropChange(object[] _objs)
    {
        Refresh();
    }

    public bool IsLock()
    {
        if (unlocked)
        {
            return false;
        }

        bool isLock = LevelDataMgr.Instance.Level < refPropUse.UnlockLv;
        return isLock;
    }

    private bool NeedUnlock()
    {
        if (unlocked)
        {
            return false;
        }
        return LevelDataMgr.Instance.Level == refPropUse.UnlockLv && PropMgr.Instance.GetPropNum(Type) <= 0;
    }

    public void UsePropNum()
    {
        if (PropMgr.Instance.PropCanUse(Type))
        {
            UseProp();
        }
    }

    private void UseProp()
    {
        // TODO: 自行实现使用道具逻辑，通过Send解耦处理逻辑
        Send.SendMsg(SendType.UseProp, Type);
        Refresh();
    }
}
