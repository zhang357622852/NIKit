/// <summary>
/// GameRoot.cs
/// Created by wangxw 2014-10-22
/// 游戏根对象，一切从这儿开始
/// </summary>

using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Collections;
using QCCommonSDK;
using System.Threading;
using LPC;

public static class GameRoot
{
    #region 属性

    // 根物体
    public static GameObject RootGameObject { get; private set; }

    // 闪屏是否完成
    private static bool isStartAniOver = false;

    /// <summary>
    /// 初始化进度
    /// </summary>
    /// <value>The init progress.</value>
    private static float InitProgress { get; set; }

    // 是否初始化完成
    public static bool IsInit { get; private set; }

    #endregion

    /// <summary>
    /// 初始化游戏根对象
    /// </summary>
    public static void Init()
    {
        // 创建根对象
        CreateRootGameObject();

        // 创建游戏根目录存在
        CreateResourceDirectory();

        // 不休眠
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        // 逻辑+渲染，Unity编辑器中的state帧率统计不准确
        Application.targetFrameRate = 45;

        // 标识isSplashOk为false
        isStartAniOver = false;

        // 监听更新资源更新界面进度
        EventMgr.RegisterEvent("GameRoot", EventMgrEventType.EVENT_MSG_START_ANIMATION_FINISH, OnStartAniOver);

        IsInit = false;

        // 监听更新资源更新界面进度
        EventMgr.RegisterEvent("GameRoot", EventMgrEventType.EVENT_LOADING_END, OnLoadingEnd);

        // 协程中初始化
        Coroutine.DispatchService(DoInit());
    }

    /// <summary>
    /// Dos the init.
    /// </summary>
    /// <returns>The init.</returns>
    static IEnumerator DoInit()
    {
        // 加载最基础的配置Config配置
        // 这个配置信息只能在游戏启动的时候就需要加载
        yield return Coroutine.DispatchService(ConfigMgr.Init());
        if (! ConfigMgr.InitSuccessed)
        {
            LogMgr.Trace("ConfigMgr初始化失败");
            yield break;
        }

        // 战斗客户端初始化
        if (AuthClientMgr.IsAuthClient)
        {
            // 协程中初始化
            Coroutine.DispatchService(DoAuthClientInit());
        }
        else
        {
            // 协程中初始化
            Coroutine.DispatchService(DoNormalClientInit());
        }
    }

    /// <summary>
    /// 创建GameObject根对象
    /// </summary>
    private static void CreateRootGameObject()
    {
        // 如果RootGameObject对象已经存在
        if (RootGameObject != null)
            return;

        // 创建一个永远存在的根对象
        // 某些全局的组件可以放在上面
        // GameRoot.cs 是脚本层的逻辑根对象
        // RootGameObject 是GameObject的根对象
        RootGameObject = new GameObject("GameRoot");
        GameObject.DontDestroyOnLoad(RootGameObject);

        // 添加音效组件，用于bgm唯一播放
        // 两个音效之间切换需要叠加淡入淡出（所以这个地方增加两个AudioSource）
        RootGameObject.AddComponent<AudioSource>();
        RootGameObject.AddComponent<AudioSource>();
        RootGameObject.AddComponent<AudioSource>();
        RootGameObject.AddComponent<AudioSource>();
        RootGameObject.AddComponent<AudioSource>();
        RootGameObject.AddComponent<AudioListener>();

        // 逻辑驱动Scheduler
        RootGameObject.AddComponent<Scheduler>();

        // 添加log组件
        RootGameObject.AddComponent<Log>();
    }

    /// <summary>
    /// 确保必须的目录存在
    /// </summary>
    private static void CreateResourceDirectory()
    {
#if UNITY_EDITOR

        // 应用没有运行不存在
        if (! Application.isPlaying)
            return;

#endif

        // 确保目录LOCAL_ROOT_PATH存在
        if (! Directory.Exists(ConfigMgr.LOCAL_ROOT_PATH))
            Directory.CreateDirectory(ConfigMgr.LOCAL_ROOT_PATH);

        // 确保目录AssetBundles位置
        if (! Directory.Exists(ConfigMgr.ASSETBUNDLES_PATH))
            Directory.CreateDirectory(ConfigMgr.ASSETBUNDLES_PATH);
    }

    /// <summary>
    /// 闪屏完成
    /// </summary>
    private static void OnStartAniOver(int eventId, MixedValue para)
    {
        isStartAniOver = true;
    }

    /// <summary>
    /// LoadingWnd完成
    /// </summary>
    private static void OnLoadingEnd(int eventId, MixedValue para)
    {
        // 参数不存在
        LPCMapping args = para.GetValue<LPCMapping>();
        if (args == null)
            return;

        // 不是资源更新类型
        if (args.GetValue<int>("type") != LoadingType.LOAD_TYPE_UPDATE_RES)
            return;

        IsInit = true;
    }

    /// <summary>
    /// 支付初始化
    /// </summary>
    private static IEnumerator InitSDK()
    {
        LogMgr.Trace("SDK初始化开始");
        QCCommonSDK.QCCommonSDK.InitSdk("smjh");

        // 直到SDK初始化完成
        while (!QCCommonSDK.QCCommonSDK.IsInitFinished())
            yield return TimeMgr.WaitForRealSeconds(0.1f);

        // 打印sdk初始化结束
        LogMgr.Trace("SDK初始化完成");
    }

    /// <summary>
    /// 载入本地etc资源线程
    /// </summary>
    /// <param name="_files">Files.</param>
    private static void DoInitMgrThread(object para)
    {
        try
        {
            // 初始化WindowMgr信息
            WindowMgr.Init();
            InitProgress += 0.02f;

            // 本地资源初始化
            LocalizationMgr.Init();
            InitProgress += 0.02f;

            // 初始化SystemTipsMgr
            SystemTipsMgr.Init();
            InitProgress += 0.02f;

            // 初始化EquipMgr
            EquipMgr.Init();
            InitProgress += 0.02f;

            // 初始化ItemMgr
            ItemMgr.Init();
            InitProgress += 0.02f;

            // 初始化SkillMgr
            SkillMgr.Init();
            InitProgress += 0.02f;

            // 初始化PropMgr
            PropMgr.Init();
            InitProgress += 0.02f;

            // 初始化MonsterMgr
            MonsterMgr.Init();
            InitProgress += 0.02f;

            // 初始化InstanceMgr
            InstanceMgr.Init();
            InitProgress += 0.02f;

            // 初始化TowerMgr
            TowerMgr.Init();
            InitProgress += 0.02f;

            // 初始化MapMgr
            MapMgr.Init();
            InitProgress += 0.02f;

            // 初始化TriggerPropMgr
            TriggerPropMgr.Init();
            InitProgress += 0.02f;

            // 初始化FieldsMgr
            FieldsMgr.Init();
            InitProgress += 0.02f;

            // 初始化BonusMgr奖励模块
            BonusMgr.Init();
            InitProgress += 0.02f;

            // 初始化RandomNameMgr信息
            RandomNameMgr.Init();
            InitProgress += 0.02f;

            // 初始化OptionMgr信息
            OptionMgr.Init();
            InitProgress += 0.02f;

            //初始化HelpInfoMgr
            HelpInfoMgr.Init();
            InitProgress += 0.02f;

            //初始化StdMgr
            StdMgr.Init();
            InitProgress += 0.02f;

            //初始化ShareMgr
            ShareMgr.Init();
            InitProgress += 0.02f;

            // 初始化SummonMgr信息
            SummonMgr.InIt();
            InitProgress += 0.02f;

            // 初始化FetchPropMgr信息
            FetchPropMgr.Init();
            InitProgress += 0.02f;

            // 初始化FormationMgr信息
            FormationMgr.Init();
            InitProgress += 0.02f;

            // 初始化GameSettingMgr信息
            GameSettingMgr.Init();
            InitProgress += 0.02f;

            // 初始化MarketMgr信息
            MarketMgr.Init();
            InitProgress += 0.02f;

            // 初始化StatusMgr信息
            StatusMgr.Init();
            InitProgress += 0.02f;

            // 初始化CommonBonusMgr信息
            CommonBonusMgr.Init();
            InitProgress += 0.02f;

            // 初始化HaloMgr信息
            HaloMgr.Init();
            InitProgress += 0.02f;

            // 初始化ArenaMgr信息
            ArenaMgr.Init();
            InitProgress += 0.02f;

            // 初始化TaskMgr信息
            TaskMgr.Init();
            InitProgress += 0.02f;

            // 初始化LotteryBonusMgr信息
            LotteryBonusMgr.Init();
            InitProgress += 0.02f;

            // 通讯文件
            CommInit.Init();
            InitProgress += 0.02f;

            // 屏蔽词文件
            BanWordMgr.Init();
            InitProgress += 0.02f;

            // 初始化登陆模块
            LoginMgr.Init();
            InitProgress += 0.02f;

            // 初始化账户模块
            AccountMgr.Init();
            InitProgress += 0.02f;

            // 战斗系统
            CombatRootMgr.Init();
            InitProgress += 0.02f;

            // 策略模块初始化
            TacticsMgr.Init();
            InitProgress += 0.02f;

            // 工坊初始化
            BlacksmithMgr.Init();
            InitProgress += 0.02f;

            // 宠物工坊初始化
            PetsmithMgr.Init();
            InitProgress += 0.02f;

            // CombatMgr初始化
            CombatMgr.Init();
            InitProgress += 0.02f;

            // GameSoundMgr初始化
            GameSoundMgr.Init();
            InitProgress += 0.02f;

            // 初始化PurchaseMgr信息
            PurchaseMgr.Init();
            InitProgress += 0.02f;

            // 初始化ChatRoomMgr信息
            ChatRoomMgr.Init();
            InitProgress += 0.02f;

            // 初始化VerifyCmdMgr信息
            VerifyCmdMgr.Init();
            InitProgress += 0.02f;

            // 初始化MailMgr信息
            MailMgr.Init();
            InitProgress += 0.02f;

            // 初始化CombatSummonMgr信息
            CombatSummonMgr.InIt();
            InitProgress += 0.02f;

            // 初始化PreloadMgr信息
            PreloadMgr.Init();
            InitProgress += 0.02f;

            // 初始化AutoCombatSelectTypeMgr信息
            AutoCombatSelectTypeMgr.Init();
            InitProgress += 0.02f;

            // 初始化ActivityMgr信息
            ActivityMgr.Init();
            InitProgress += 0.02f;

            // 初始化EffectMgr信息
            EffectMgr.Init();
            InitProgress += 0.02f;

            // 初始化SystemFunctionMgr信息
            SystemFunctionMgr.Init();
            InitProgress += 0.02f;

            // 初始化PushMgr信息
            PushMgr.Init();
            InitProgress += 0.02f;

            // 初始化指引模块
            GuideMgr.Init();
            InitProgress += 0.02f;

            // 初始化ManualMgr信息
            ManualMgr.Init();
            InitProgress += 0.02f;

            // 初始化GangMgr信息
            GangMgr.Init();
            InitProgress += 0.02f;

            // 初始化GameCourseMgr信息
            GameCourseMgr.Init();
            InitProgress += 0.02f;
        }
        catch(Exception e)
        {
            // 抛出异常信息
            LogMgr.Exception(e);
        }
    }

    /// <summary>
    /// 战斗客户端初始化
    /// </summary>
    static IEnumerator DoAuthClientInit()
    {
#if UNITY_EDITOR
        // 设置日志
        UniLogger.Log.Instance.SetConsole(true);
#else
        // 忽略日志
        UniLogger.Log.Instance.SetConsole(false);
#endif

        // 锁帧
        // 逻辑+渲染，Unity编辑器中的state帧率统计不准确
        Application.targetFrameRate = 45;

        LogMgr.Trace("开始游戏初始化。");

        // 开启MsgMgr消息处理线程
        MsgMgr.Start();

        // 脚本和公式初始化
        FormulaMgr.Init();
        ScriptMgr.Init();

        // 初始化EquipMgr
        EquipMgr.Init();

        // 初始化SkillMgr
        SkillMgr.Init();

        // 初始化PropMgr
        PropMgr.Init();

        // 初始化MonsterMgr
        MonsterMgr.Init();

        // 初始化InstanceMgr
        InstanceMgr.Init();

        // 初始化TowerMgr
        TowerMgr.Init();

        // 初始化MapMgr
        MapMgr.Init();

        // 初始化TriggerPropMgr
        TriggerPropMgr.Init();

        //初始化StdMgr
        StdMgr.Init();

        // 初始化FormationMgr信息
        FormationMgr.Init();

        // 初始化GameSettingMgr信息
        GameSettingMgr.Init();

        // 初始化StatusMgr信息
        StatusMgr.Init();

        // 初始化HaloMgr信息
        HaloMgr.Init();

        // 初始化ArenaMgr信息
        ArenaMgr.Init();

        // 通讯文件
        CommInit.Init();

        // 战斗系统
        CombatRootMgr.Init();

        // 策略模块初始化
        TacticsMgr.Init();

        // CombatMgr初始化
        CombatMgr.Init();

        // GameSoundMgr初始化
        GameSoundMgr.Init();

        // 初始化CombatSummonMgr信息
        CombatSummonMgr.InIt();

        // 初始化AutoCombatSelectTypeMgr信息
        AutoCombatSelectTypeMgr.Init();

        // 初始化EffectMgr信息
        EffectMgr.Init();

        // 战斗客户端初始化
        AuthClientMgr.Init();

        IsInit = true;

        // 等待一帧
        yield return null;
    }

    /// <summary>
    /// 协程迭代函数，执行具体的初始化处理
    /// </summary>
    /// <returns>The init.</returns>
    static IEnumerator DoNormalClientInit()
    {
        LogMgr.Trace("开始游戏初始化。");

        // 初始化开始场景中的资源,
        // 中文资源和先关配置信息(资源解压和资源更新时界面需要用到相关信息)
        LocalizationMgr.InitStartRes();
        CsvFileMgr.InitStartCsv();

        LoadingMgr.ShowResLoadingWnd("ResourceLoadingWnd", LoadingType.LOAD_TYPE_UPDATE_RES);

        // 等待动画播放完毕
        while(!isStartAniOver)
            yield return null;

        // 检查客户端是否需要更新, 如果客户端更新则退出
        yield return Coroutine.DispatchService(ConfigMgr.CheckClientUpdate());
        if (ConfigMgr.IsNeedUpdateClient)
            yield break;

        // 开启MsgMgr消息处理线程
        MsgMgr.Start();

        // 脚本和公式初始化
        FormulaMgr.Init();
        ScriptMgr.Init();

#if ! UNITY_EDITOR

        if(! QCCommonSDK.QCCommonSDK.IsInitFinished())
            yield return Coroutine.DispatchService(InitSDK());
#endif
        // 资源初始化
        yield return Coroutine.DispatchService(ResourceMgr.Init());

        // 抛出开始解压包体中资源事件
        LoadingMgr.ChangeState(ResourceLoadingConst.LOAD_TYPE_INIT, ResourceLoadingStateConst.LOAD_STATE_CHECK);

        // 抛出开始解压包体中资源事件
        LoadingMgr.ChangeState(ResourceLoadingConst.LOAD_TYPE_INIT, ResourceLoadingStateConst.LOAD_STATE_UPDATE);

        // 载入初始化用到的全部文本资源
        // 载入资源包含Etc目录下的所有资源
        yield return Coroutine.DispatchService(ResourceMgr.LoadEtcText());

        // 设置初始化进度
        LoadingMgr.SetProgress(InitProgress = 0.05f);

        // 在线程中初始化各个mgr模块
        // 载入etc资源线程
        Thread InitMgrThread = new Thread(new ParameterizedThreadStart(DoInitMgrThread));
        InitMgrThread.Start();

        // 等待载入资源完毕
        while (InitMgrThread.IsAlive)
        {
            LoadingMgr.SetProgress(InitProgress*0.5f);
            yield return null;
        }

        // 释放提前载入的文本资源
        ResourceMgr.UnloadEtcText();

        // 管理器初始化结束
        LoadingMgr.SetProgress(InitProgress = 0.5f);

        // 载入一些静态资源
        PreloadMgr.DoPreload("Main");

        // 等待资源载入结束
        while (!PreloadMgr.IsLoadEnd("Main"))
        {
            // 更新资源载入进度
            LoadingMgr.SetProgress(0.5f + PreloadMgr.GetProgress("Main")*0.5f);
            yield return null;
        }

        // 资源预加载结束
        LoadingMgr.SetProgress(InitProgress = 1f);

        // 等待进度条结束
        while(!LoadingMgr.IsLoadingEnd(LoadingType.LOAD_TYPE_UPDATE_RES,
            ResourceLoadingConst.LOAD_TYPE_INIT))
            yield return null;

        LogMgr.Trace("完成游戏初始化");

        // 初始化是否结束
        while(!IsInit)
            yield return null;

        // 打开游戏登陆界面
        WindowMgr.OpenWnd(LoginWnd.WndType);
    }
}
