using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class TopDataWindow : BaseWindowWrapper<TopDataWindow>
{
    public Action OnFlyOverEvent; // 金币飞行结束回调

    private Button btnGm;
    private Button btnSetting;
    private Text txtCoin;
    // 金币相关
    private Image imgGold;
    private Transform flyBG; // 背景
    private Transform coinTR; // 模型
    private GameObject rawCoin; // 金币的RawImage
    private ParticleSystem shimmerFX; // 下方特效
    private ParticleSystem shimmerTopFX; // 上方方特效
    private Transform topCoin;
    private Text txtNum; // 金币数量
    private Transform rawCoinRoot;
    private List<FxCoinView> flyList = new List<FxCoinView>();
    private int tempCoin = 0; // 暂存金币数量
    private float originY;
    private bool flyAfterOpen = false; // 打开窗口后播放
    private int flyCoinNum = 0; // 飞行金币数量

    private List<Tween> activeTweens = new List<Tween>(); // 存储所有活动的 Tween 和 Sequence

    protected override void InitCtrl()
    {
        btnGm = gameObject.GetChildControl<Button>("Root/Top/btnGm");
        btnGm.gameObject.SetActive(GameGlobalMgr.Instance.GMMode);
        btnSetting = gameObject.GetChildControl<Button>("Root/Top/Right/btnSetting");
        txtCoin = gameObject.GetChildControl<Text>("Root/Top/Right/coin/txtNum");
        // 金币相关
        imgGold = gameObject.GetChildControl<Image>("Root/Top/Right/coin/imgCoin");
        flyBG = gameObject.GetChildControl<Transform>("FlyBG");
        coinTR = gameObject.GetChildControl<Transform>("FlyBG/cion/fly");
        rawCoin = gameObject.GetChildControl<Transform>("FlyBG/RawCoin").gameObject;
        shimmerFX = gameObject.GetChildControl<ParticleSystem>("FlyBG/FX_shimmer");
        shimmerTopFX = gameObject.GetChildControl<ParticleSystem>("Root/Top/Right/coin/FX_shimmer_Top_01");
        topCoin = gameObject.GetChildControl<Transform>("Root/Top/Right/coin");
        txtNum = gameObject.GetChildControl<Text>("FlyBG/FlyTxt/txtNum");
        rawCoinRoot = gameObject.GetChildControl<Transform>("FlyBG/RowCoinRoot");
        rawCoin.SetActive(false);
    }

    protected override void OnPreOpen()
    {
        originY = topCoin.position.y;
        flyBG.gameObject.SetActive(false);
        tempCoin = 0;
        Refresh();
    }

    protected override void OnOpen()
    {
        // 被动出发，窗口打开后触发，须先调用 ShowFlyCoinAfterOpen()
        if (flyAfterOpen)
        {
            tempCoin = CurrencyMgr.Instance.Gold - flyCoinNum;
            txtCoin.text = $"{tempCoin}";
            // 这边延迟0.5秒是为了保证窗口已经打开，避免动画播放时窗口已经关闭，可根据实际需求调整
            ToolMgr.Instance.DelayCallBack(() =>
            {
                ShowFlyCoin(new object[] { flyCoinNum });
            }, 0.5f);
            flyAfterOpen = false;
        }

    }

    protected override void OnClose()
    {
        base.OnClose();
    }

    protected override void InitMsg()
    {
        btnGm.onClick.AddListener(OnBtnGmClick);
        btnSetting.onClick.AddListener(OnBtnSettingClick);
        Send.RegisterMsg(SendType.GoldChange, OnChangeGold);
        Send.RegisterMsg(SendType.ShowFlyCoin, ShowFlyCoin);
    }

    protected override void ClearMsg()
    {
        btnGm.onClick.RemoveListener(OnBtnGmClick);
        btnSetting.onClick.RemoveListener(OnBtnSettingClick);
        Send.UnregisterMsg(SendType.GoldChange, OnChangeGold);
        Send.UnregisterMsg(SendType.ShowFlyCoin, ShowFlyCoin);
    }

    private void OnBtnGmClick()
    {
        if (!GameGlobalMgr.Instance.GMMode)
        {
            return;
        }
        WindowMgr.Instance.OpenWindow<GmWindow>();
    }

    private void OnBtnSettingClick()
    {
        WindowMgr.Instance.OpenWindow<SettingWindow>();
    }

    private void OnChangeGold(object[] _objs)
    {
        Refresh();
    }

    private void Refresh()
    {
        txtCoin.text = $"{CurrencyMgr.Instance.Gold}";
    }

    private void ShowFlyCoin(object[] objs)
    {
        int addNum = (int)objs[0];
        int count = 9;
        tempCoin = CurrencyMgr.Instance.Gold - addNum;
        txtCoin.text = $"{tempCoin}";
        Vector3 targetPos = imgGold.transform.position;
        Vector3 pos = rawCoin.transform.position;
        ShowFlyEffect(pos, targetPos, count, addNum);
    }

    private void ShowFlyEffect(Vector3 pos, Vector3 targetPos, int count, int addNum)
    {
        flyBG.gameObject.SetActive(true);
        txtNum.gameObject.SetActive(false);
        StopAllTweens();
        Sequence sequence = DOTween.Sequence();
        // 金币旋转
        coinTR.rotation = Quaternion.identity;
        float rotationDuration = 0.6f;
        Tweener coinTween = coinTR.DORotate(new Vector3(0, 360, 0), rotationDuration, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);
        activeTweens.Add(coinTween);
        // 播放特效
        shimmerFX.transform.position = pos;
        shimmerFX.Play();
        // 计算时间
        Vector3 midpoint = new Vector3((pos.x + targetPos.x) / 2, pos.y - 2f, pos.z);
        List<Vector3> path = ToolMgr.Instance.Bezier(pos, midpoint, targetPos, 20);
        float flyDuration = Vector3.Distance(pos, targetPos) / 8;
        float nextTime = 0.12f;
        float shakeTime = nextTime / 2f;
        float delay = 0.3f;
        //创建金币
        for (int i = 0; i < count; i++)
        {
            int showAdd = (int)((float)(i + 1) / (float)count * (float)addNum);
            int no = i;
            FxCoinView fxCoinView = null;
            if (i >= flyList.Count)
            {
                GameObject go = GameObject.Instantiate(rawCoin, rawCoinRoot);
                fxCoinView = new FxCoinView(go);
                flyList.Add(fxCoinView);
            }
            else
            {
                fxCoinView = flyList[i];
            }
            fxCoinView.SetData(pos, targetPos);
            float insertTime = nextTime * i + delay;
            if (i == count - 1)
            {
                sequence.InsertCallback(insertTime + 0.05f, () =>
                {
                    shimmerFX.Stop();
                });
            }
            sequence.Insert(insertTime, fxCoinView.Trans.DOScale(0.8f, flyDuration).SetEase(Ease.InQuart)); // 缩小 
            /* 飞行  */
            sequence.Insert(insertTime, fxCoinView.Trans.DOPath(path.ToArray(), flyDuration).SetEase(Ease.InQuart).OnComplete(() =>
            {
                fxCoinView.ClearData();
            }));
            sequence.Insert(insertTime + flyDuration, topCoin.DOMoveY(originY + 0.05f, shakeTime).OnStart(() =>
            {
                shimmerTopFX.Play();
                SoundMgr.Instance.PlaySound("金币");
                txtCoin.text = $"{tempCoin + showAdd}";
            }));
            sequence.Insert(insertTime + flyDuration + shakeTime, topCoin.DOMoveY(originY, shakeTime));
            if (i == count / 2)
            {
                // 数字
                RectTransform rect = txtNum.GetComponent<RectTransform>();
                rect.localPosition = Vector3.zero;
                txtNum.text = $"+{addNum}";
                sequence.Insert(insertTime, txtNum.transform.DOLocalMoveY(80f, 0.5f).OnStart(() =>
                {
                    txtNum.gameObject.SetActive(true);
                }));
                sequence.InsertCallback(insertTime + 1f, () =>
                {
                    txtNum.gameObject.SetActive(false);
                });
            }
        }
        sequence.OnComplete(() =>
        {
            flyBG.gameObject.SetActive(false);
            tempCoin = 0;
            ToolMgr.Instance.DelayCallBack(() =>
            {
                OnFlyOverEvent?.Invoke();
                OnFlyOverEvent = null;
            }, 0.5f);
        });
        activeTweens.Add(sequence);
    }

    // 终止所有活动的 Tween 和 Sequence
    private void StopAllTweens()
    {
        foreach (var tween in activeTweens)
        {
            if (tween.IsActive())
            {
                tween.Kill();
            }
        }
        activeTweens.Clear();
    }

    /// <summary>
    /// 被动触发金币获得动画，用于打开窗口后触发
    /// </summary>
    /// <param name="addNum">飞行金币数量</param>
    /// <param name="onFlyOverEvent">飞行结束回调</param>
    public void ShowFlyCoinAfterOpen(int addNum, Action onFlyOverEvent = null)
    {
        flyAfterOpen = true;
        flyCoinNum = addNum;
        OnFlyOverEvent = onFlyOverEvent;
    }
}

public class FxCoinView
{
    public GameObject ItemGO { get; }
    public Transform Trans { get; }
    private Vector3 targetPos;

    public FxCoinView(GameObject go)
    {
        ItemGO = go;
        Trans = ItemGO.transform;
    }

    public void SetData(Vector3 pos, Vector3 _targetPos)
    {
        Trans.localScale = Vector3.one;
        Trans.position = pos;
        targetPos = _targetPos;
        ItemGO.SetActive(true);
    }

    public void ClearData()
    {
        targetPos = Vector3.zero;
        Trans.localScale = Vector3.one;
        ItemGO.SetActive(false);
    }
}
