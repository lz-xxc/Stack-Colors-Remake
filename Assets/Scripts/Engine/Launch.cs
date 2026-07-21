#define DIRECT_READING //需要延迟读表注释这行

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Globalization;

/// <summary>
/// 游戏启动
/// </summary>
public class Launch : SingletonMonoBehavior<Launch> {

    protected override void Awake() {
        Debug.Log(" Launch Awake Begin......");
        Debug.Log(" XSFrameWork Version:0.4.0");
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.CreateSpecificCulture("en-US");
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.CreateSpecificCulture("en-US");
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        gameObject.AddMissingComponent<CoDelegator>();
        gameObject.AddMissingComponent<SoundMgr>();
        DontDestroyOnLoad(gameObject);
    }

    void Start() {
#if DIRECT_READING
        InitClient();
#else
        CoDelegator.Coroutine(InitClient());
#endif
    }

#if DIRECT_READING
    private void InitClient() {
        GameStateMgr.Instance.SwitchState(GameState.Loading);
        //直读表模块
        RefDataMgr.Instance.InitBasic();
        InitLogic();
    }
#else
    //延迟读表模块
    IEnumerator InitClient() {
        GameStateMgr.Instance.SwitchState(GameState.Loading);
        yield return CoDelegator.Coroutine(RefDataMgr.Instance.Init());
        InitLogic();
    }
#endif

    private void InitLogic() {
        //逻辑模块的初始化
        LanguageMgr.Instance.Init();//语言模块要最早初始化
        ObjectPool.Instance.Init();
        BattleMgr.Instance.Init();
        GradeMgr.Instance.Init();
        CurrencyMgr.Instance.Init();
        ScoreMgr.Instance.Init();
        PropMgr.Instance.Init();
        ShopMgr.Instance.Init();
        TaskMgr.Instance.Init();
        SignMgr.Instance.Init();
        SoundMgr.Instance.Init();
        SettingsMgr.Instance.Init();
        LevelDataMgr.Instance.Init();
        ColorProxy.Instance.Init();
        CameraMgr.Instance.Init();
        PlayerMgr.Instance.Init();
        PickUpMgr.Instance.Init();
        RoadMgr.Instance.Init();
        ColorChangerMgr.Instance.Init();
        //初始化完成切换到主界面
        GameStateMgr.Instance.SwitchState(GameState.Main);
        //初始化完成切换到主场景
        //LoadSceneMgr.Instance.LoadScene("Main", GameState.Main);
    }

    private void Update() {
#if UNITY_EDITOR
        //空格暂停游戏功能
        if (Input.GetKeyDown(KeyCode.Space)) {
            EditorApplication.isPaused = true;
        }

        if (BattleMgr.Instance.state == BattleState.Game) {
            PlayerMgr.Instance.OnUpdate();
            EnergyMgr.Instance.OnUpdate();
        }

        if (GameStateMgr.Instance.curState == GameState.GameOver) {
            if (Input.GetMouseButtonDown(0)) {
                GameStateMgr.Instance.SwitchState(GameState.Main);
                PlayerMgr.Instance.Clear();
                BattleMgr.Instance.Clear();
                RoadMgr.Instance.Clear();
                PickUpMgr.Instance.Clear();
                ColorChangerMgr.Instance.Clear();
                RateMgr.Instance.Clear();
                CameraMgr.Instance.Clear();
            }
        }
#endif
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    private void OnApplicationQuit() {
        LocalSave.SaveAll();
    }

    /// <summary>
    /// 游戏焦点
    /// </summary>
    private void OnApplicationFocus(bool focus) {
        if (!focus) {
            LocalSave.SaveAll();
        }
    }
}