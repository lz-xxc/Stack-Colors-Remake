using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GmWindow : BaseWindowWrapper<GmWindow>
{

    private Button btnClose;
    private Button btnAddCoin;
    private Button btnSkip;
    private Button btnClear;
    private Button btnChangeLv;
    private Text txtLv;
    private Button btnOpenWin;
    private Button btnOpenLose;

    protected override void InitCtrl()
    {
        btnClose = gameObject.GetChildControl<Button>("btnClose");
        btnAddCoin = gameObject.GetChildControl<Button>("btnAddCoin");
        btnSkip = gameObject.GetChildControl<Button>("btnSkip");
        btnClear = gameObject.GetChildControl<Button>("btnClear");
        btnChangeLv = gameObject.GetChildControl<Button>("ChangeLv/btnChangeLv");
        txtLv = gameObject.GetChildControl<Text>("ChangeLv/InputLv/Text");
        btnOpenWin = gameObject.GetChildControl<Button>("btnOpenWin");
        btnOpenLose = gameObject.GetChildControl<Button>("btnOpenLose");
    }

    protected override void OnOpen()
    {

    }

    protected override void OnClose()
    {
        base.OnClose();
    }

    protected override void InitMsg()
    {
        btnClose.onClick.AddListener(OnCloseClick);
        btnAddCoin.onClick.AddListener(OnAddCoin);
        btnClear.onClick.AddListener(OnDeleteKeys);
        btnSkip.onClick.AddListener(OnPassLevel);
        btnChangeLv.onClick.AddListener(OnChangeLv);
        btnOpenWin.onClick.AddListener(OnOpenWin);
        btnOpenLose.onClick.AddListener(OnOpenLose);
    }

    protected override void ClearMsg()
    {
        btnClose.onClick.RemoveListener(OnCloseClick);
        btnAddCoin.onClick.RemoveListener(OnAddCoin);
        btnClear.onClick.RemoveListener(OnDeleteKeys);
        btnSkip.onClick.RemoveListener(OnPassLevel);
        btnChangeLv.onClick.RemoveListener(OnChangeLv);
        btnOpenWin.onClick.RemoveListener(OnOpenWin);
        btnOpenLose.onClick.RemoveListener(OnOpenLose);
    }

    private void OnCloseClick()
    {
        WindowMgr.Instance.CloseWindow<GmWindow>();
    }

    private void OnAddCoin()
    {
        CurrencyMgr.Instance.Gold += 10000;
    }

    private void OnDeleteKeys()
    {
        LocalSave.DeleteAll();
    }

    private void OnPassLevel()
    {
        LevelDataMgr.Instance.LevelPass();
    }

    private void OnChangeLv()
    {
        int levelID = 13;
        int.TryParse(txtLv.text, out levelID);
        LevelDataMgr.Instance.SetLevel(levelID);
    }

    private void OnOpenWin()
    {
    }

    private void OnOpenLose()
    {
    }
}
