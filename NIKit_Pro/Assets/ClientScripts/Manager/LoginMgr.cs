/// <summary>
/// LoginMgr.cs
/// Create by zhaozy 2014-11-3
/// 登陆管理模块
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms;
using LPC;
using QCCommonSDK;
using QCCommonSDK.Util;

/// <summary>
/// LoginMgr登陆管理模块
/// </summary>
public static class LoginMgr
{
    #region 变量

    /// <summary>
    /// 是否是在维护状态
    /// </summary>
    public static bool isFixedStatus = false;

    /// <summary>
    /// 恢复登陆相关数据
    /// </summary>
    private static LPCMapping mRecoverMap = LPCMapping.Empty;

    // 上次检测服务器配置的时间
    private static int lastCheckTime = 0;

    // gs直连登陆最大尝试次数
    public static int MAX_RECOVER_TIMES = 3;

    #endregion

    #region 属性

    /// <summary>
    /// 当前账号恢复登陆验证数据 
    /// </summary>
    public static LPCMapping RecoverMap
    {
        get
        {
            return mRecoverMap;
        }

        set
        {
            mRecoverMap = value;
        }
    }

    #endregion

    #region 内部接口

    /// <summary>
    /// 延时处理
    /// </summary>
    private static void OnNetDelay()
    {
        // 如果玩家没有登陆成功的时候不处理延迟处理
        if (! ME.isLoginOk)
            return;

        // 添加网络延迟等待窗口, 默认网络延迟需要等待6s
        WaitWndMgr.AddWaitWnd(
            "network",
            WaitWndType.WAITWND_TYPE_NETWORK,
            new CallBack(OnNetDelayOvertime),
            6f,
            LPCMapping.Empty);
    }

    /// <summary>
    /// 网络延迟等待超时
    /// </summary>
    private static void OnNetDelayOvertime(object para, object[] param)
    {
        // 断开当前连接
        Communicate.Disconnect();

        // 提示确认窗口
        DialogMgr.ShowDailog(
            new CallBack(OnRecoverLoginConfirmed, param[0]),
            LocalizationMgr.Get("LoginMgr_3"),
            LocalizationMgr.Get("LoginMgr_4"),
            LocalizationMgr.Get("LoginMgr_5"),
            LocalizationMgr.Get("LoginMgr_6"),
            false);

        // 网络恢复移除网络等待窗口
        WaitWndMgr.RemoveWaitWnd("network", WaitWndType.WAITWND_TYPE_NETWORK);
    }

    /// <summary>
    /// 恢复登陆超时
    /// </summary>
    private static void OnRecoverLoginOvertime(object para, object[] param)
    {
        // 断开当前连接
        Communicate.Disconnect();

        // 提示确认窗口
        DialogMgr.ShowDailog(
            new CallBack(OnRecoverLoginConfirmed, param[0]),
            LocalizationMgr.Get("LoginMgr_3"),
            LocalizationMgr.Get("LoginMgr_4"),
            LocalizationMgr.Get("LoginMgr_5"),
            LocalizationMgr.Get("LoginMgr_6"),
            false);

        // 网络恢复移除网络等待窗口
        WaitWndMgr.RemoveWaitWnd("recover_login", WaitWndType.WAITWND_TYPE_RELOGIN);
    }

    /// <summary>
    /// 网络延迟确认结果
    /// </summary>
    private static void OnRecoverLoginConfirmed(object para, object[] param)
    {
        // 获取确认结果
        bool confirmRet = (bool) param[0];

        // 如果玩家取消重连
        if (! confirmRet)
        {
            // 玩家确认返回登陆场景
            LoginMgr.ExitGame();
            return;
        }

        // 转换数据格式
        LPCMapping paraMap = para as LPCMapping;

        // 尝试链接gs, 最大直连gs次数默认为3次
        int recoverTimes = paraMap.GetValue<int>("recover_times");
        if (recoverTimes < LoginMgr.MAX_RECOVER_TIMES)
        {
            // 增加重连次数
            paraMap.Add("recover_times", recoverTimes + 1);

            // 添加恢复登陆等待窗口, 默认网络延迟需要等待10s
            WaitWndMgr.AddWaitWnd(
                "recover_login",
                WaitWndType.WAITWND_TYPE_RELOGIN,
                new CallBack(OnRecoverLoginOvertime),
                10f,
                paraMap);

            // 尝试直连gs
            Operation.CmdRecoverLogin.Go();
            return;
        }

        // 添加恢复登陆等待窗口, 默认网络延迟需要等待30s
        WaitWndMgr.AddWaitWnd(
            "recover_login",
            WaitWndType.WAITWND_TYPE_RELOGIN,
            new CallBack(OnRecoverLoginOvertime),
            30f,
            paraMap);

        // 发起恢复登陆
        DoLoginAAA(true);
    }

    /// <summary>
    /// 网络恢复
    /// </summary>
    private static void OnNetUnobstructed()
    {
        // 如果玩家不延迟了，则需要判断VerifyCmd是否已经全部到达
        if (VerifyCmdMgr.IsVerifyCmd())
            return;

        // 网络恢复移除网络等待窗口
        WaitWndMgr.RemoveWaitWnd("network", WaitWndType.WAITWND_TYPE_NETWORK);
    }

    private static void OnMsgCreateNewCharResult(string cmd, LPCValue para)
    {
        // 转换数据格式
        LPCMapping args = para.AsMapping;

        // 如果是创建玩家角色失败
        if (args.GetValue<int>("result") == 0)
            return;

#if  !UNITY_EDITOR

        // 生成渠道account
        string channelAccount = string.Format("{0}{1}",
        QCCommonSDK.QCCommonSDK.FindNativeSetting("CHANNEL"),
        Communicate.AccountInfo.Query("account", ""));

        // 通知sdk记录玩家第一次登录游戏数据
        QCCommonSDK.Addition.DataAnalyzeSupport.RecordEvent("register",new Dictionary<string, string>() {
            {"account", channelAccount},
            {"server", Communicate.AccountInfo.Query("server_id", "")},
        });

#endif
    }

    /// <summary>
    /// MSG_EXISTED_CHAR_LIST的回调
    /// </summary>
    private static void OnMsgExistedCharList(string cmd, LPCValue para)
    {
        // 获取全部角色列表
        List<LPCValue> users = LoginMgr.GetAllUser();

        // 没有玩家列表, 这个时候需要弹出角色创建界面
        if (users.Count == 0)
            return;

        // 登陆角色游戏
        LoginMgr.DoLoginUser(users[0].AsMapping["rid"].AsString);
    }

    /// <summary>
    /// 切换账号回调
    /// </summary>
    /// <param name="result">Result.</param>
    private static void ChangeAccountEventHandle(QCEventResult result)
    {
        if (result == null)
        {
            LogMgr.Trace("ChangeAccountEventHandle erro");
            return;
        }

        // Log.Normal(string.Format("AuthEventHandle = {0}", DictionaryHelp.ToJson(result.result)));
        switch (result.error)
        {
            case QCErrorCode.SUCCESS:

                // 直接退出游戏，重新发起登陆
                ExitGame();

                break;
            default:
                // 切换账号失败
                DialogMgr.Notify(string.Format(LocalizationMgr.Get("LoginMgr_8"), result.error));

                break;
        }
    }

    /// <summary>
    /// Binds the account result.
    /// </summary>
    /// <param name="result">Result.</param>
    private static void BindAccountResult(QCEventResult result)
    {
        if (result == null)
        {
            LogMgr.Trace("BindAccountResult error");
            return;
        }

        switch (result.error)
        {
            case QCErrorCode.SUCCESS:

                // 抛出账号绑定成功事件
                EventMgr.FireEvent(EventMgrEventType.EVENT_REGISTER_ACCOUNT_SUCC, null);

                // 给出提示信息
                DialogMgr.Notify(LocalizationMgr.Get("LoginMgr_1"));
                break;
            default:
                // 绑定失败
                LPCMapping extraData = new LPCMapping();

                // 填充数据
                foreach (var key in result.result.Keys)
                    extraData.Add(key, result.result[key]);

                // 提示错误代码
                DialogMgr.Notify(string.Format(LocalizationMgr.Get("LoginMgr_2"),
                    LocalizationMgr.Get(extraData.GetValue<string>("code", ""))));

                break;
        }
    }

    private static void CancelAuthEventHandle(QCEventResult result)
    {
        if (result == null)
        {
            LogMgr.Trace("CancelAuthEventHandle erro");
            return;
        }
        switch (result.error)
        {
            case QCErrorCode.SUCCESS:

                // 登出成功
                LPCMapping extraData = new LPCMapping();

                // 填充数据
                foreach (var key in result.result.Keys)
                    extraData.Add(key, result.result[key]);

                // 清空账号信息
                AccountMgr.SetAccountData(LPCMapping.Empty);

                // 构造数据
                OptionMgr.SetPublicOption("is_loggedin", LPCValue.Create(1));

                OptionMgr.SetPublicOption("login_type", LPCValue.Create(extraData.GetValue<string>("3rdplatform", "")));

                break;
            default:

                break;
        }
    }

    private static void AuthEventHandle(QCEventResult result)
    {
        if (result == null)
        {
            LogMgr.Trace("AuthEventHandle erro");
            return;
        }
        switch (result.error)
        {
            case QCErrorCode.SUCCESS:

                // 登录成功
                LPCMapping extraData = new LPCMapping();

                // 填充数据
                foreach (var key in result.result.Keys)
                    extraData.Add(key, result.result[key]);

                LPCMapping accountData = new LPCMapping();
                accountData.Add("account", extraData.GetValue<string>("uid", ""));
                accountData.Add("password", "");
                accountData.Add("extra_data", extraData);

                // 设置账号信息
                AccountMgr.SetAccountData(accountData);

                // 构造数据
                OptionMgr.SetPublicOption("is_loggedin", LPCValue.Create(1));

                OptionMgr.SetPublicOption("login_type", LPCValue.Create(extraData.GetValue<string>("3rdplatform", "")));

                break;
            case QCErrorCode.CanelOperation:

                break;
            default:

                break;
        }
    }

    /// <summary>
    /// 打开登陆场景之前
    /// </summary>
    private static void OnLoginSceneBefore(object para, object[] param)
    {
        // 打开闪屏界面
        WindowMgr.OpenWnd("WhiteFlash");
    }

    /// <summary>
    /// 打开登陆场景后
    /// </summary>
    private static void OnLoginScene(object para, object[] param)
    {
        // 玩家进入登陆场景清除对账信息
        PurchaseMgr.CleanOrderMap();
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 模块初始化
    /// </summary>
    public static void Init()
    {
        // 网络延时
        MsgMgr.eventNetDelay -= OnNetDelay;
        MsgMgr.eventNetDelay += OnNetDelay;

        // 网络恢复
        MsgMgr.eventNetUnobstructed -= OnNetUnobstructed;
        MsgMgr.eventNetUnobstructed += OnNetUnobstructed;

        // 关注角色列表数据
        MsgMgr.RemoveDoneHook("MSG_EXISTED_CHAR_LIST", "LoginMgr");
        MsgMgr.RegisterDoneHook("MSG_EXISTED_CHAR_LIST", "LoginMgr", OnMsgExistedCharList);

        // 关注角色创建成功数据
        MsgMgr.RemoveDoneHook("MSG_CREATE_NEW_CHAR_RESULT", "LoginMgr");
        MsgMgr.RegisterDoneHook("MSG_CREATE_NEW_CHAR_RESULT", "LoginMgr", OnMsgCreateNewCharResult);

#if ! UNITY_EDITOR
        // 注册账户切换回调
        QCCommonSDK.Addition.AuthSupport.OnChangeAccountResult -= ChangeAccountEventHandle;
        QCCommonSDK.Addition.AuthSupport.OnChangeAccountResult += ChangeAccountEventHandle;

        // 绑定账号成功回调
        QCCommonSDK.Addition.AuthSupport.OnBindAccountResult -= BindAccountResult;
        QCCommonSDK.Addition.AuthSupport.OnBindAccountResult += BindAccountResult;

        // 注册取消登陆回调
        QCCommonSDK.Addition.AuthSupport.OnCancelAuthResult -= CancelAuthEventHandle;
        QCCommonSDK.Addition.AuthSupport.OnCancelAuthResult += CancelAuthEventHandle;


        // 注册登录回调
        QCCommonSDK.Addition.AuthSupport.OnAuthResult -= AuthEventHandle;
        QCCommonSDK.Addition.AuthSupport.OnAuthResult += AuthEventHandle;
#endif
    }

    /// <summary>
    /// 获取所有玩家列表
    /// </summary>
    public static List<LPCValue> GetAllUser()
    {
        // 如果包含角色列表信息
        if (Communicate.CurrInfo.ContainsKey("char_list"))
            return (List<LPCValue>)Communicate.CurrInfo["char_list"];

        // 返回数据
        return new List<LPCValue>();
    }

    /// <summary>
    /// 登录成功回调
    /// </summary>
    public static void OnLoginOK()
    {
        // 记录数据
        RecoverMap.Add("auth_key", Communicate.AccountInfo.Query("auth_key", 0));
        RecoverMap.Add("seed", Communicate.AccountInfo.Query("seed", 0));

        // 提示登陆失败
        LogMgr.Trace("登录成功。");
    }

    /// <summary>
    /// 退出游戏
    /// </summary>
    public static void DoRequestQuitGame(object para, object[] expara)
    {
        // 向SDK请求退出游戏
        // QCCommonSDK.QCCommonSDK.SystemQuit();
        Application.Quit();
    }

    /// <summary>
    /// 尝试登陆AAA
    /// </summary>
    public static bool DoLoginAAA(bool recoverLogin = false)
    {
        // 获取账户信息
        LPCMapping accountMap = AccountMgr.GetAccountData();

        // 没有获取到数据不处理
        if (accountMap.Count == 0)
        {
            LogMgr.Trace("登陆失败，账号信息异常。");
            return false;
        }

        LPCValue extraData = accountMap.GetValue<LPCValue>("extra_data");
        extraData.AsMapping.Add("recover_login", recoverLogin ? 1 : 0);

        // 如果是恢复登陆需要添加恢复登陆验证参数
        if (recoverLogin)
            extraData.AsMapping.Add("recover_map", RecoverMap);
        else
            extraData.AsMapping.Add("recover_map", LPCMapping.Empty);

        // 向服务器申请登陆AAA
        Operation.CmdLoginAAA.Go(accountMap.GetValue<string>("account", ""), 
            accountMap.GetValue<string>("password", ""), 
            ConfigMgr.Get<string>("login_ip", "127.0.0.1"),
            int.Parse(ConfigMgr.Get<string>("login_port", "8001")),
            ConfigMgr.Get<string>("version", "1.0.0"),
            extraData);

        // 非测试发布版本，必须通过服务器
        return true;
    }

    /// <summary>
    /// 本地登陆玩家
    /// </summary>
    public static void DoLoginUser(string userRid)
    {
        // 重置RecoverMap
        RecoverMap = LPCMapping.Empty;

        // 载入角色
        LPCValue extraInfo = LPCValue.CreateMapping();
        extraInfo.AsMapping["client_os"] = LPCValue.Create("mac"); // TODO: 操作系统需要修改
        extraInfo.AsMapping["game_mode"] = LPCValue.Create(1);
        Operation.CmdLoadExistedChar.Go(userRid, extraInfo);
    }

    /// <summary>
    /// 登陆新职业玩家
    /// </summary>
    public static void CreateNewUser(int userStyle, string userName, int gender)
    {
        // 通知服务器创建新角色
        LPCMapping extra = LPCMapping.Empty;

#if UNITY_EDITOR
        extra.Add("client_name", "QCPLAY");
#else
        // 通过sdk获取Client_Name
        string clientName = QCCommonSDK.QCCommonSDK.FindNativeSetting("Client_Name");

        // 如果获取clientName异常，则直接通过ConfigMgr获取本地配置
        if (string.IsNullOrEmpty(clientName))
            clientName = "QCPLAY";

        // 添加extra
        extra.Add("client_name", clientName);
#endif

        // 通知服务器创建新角色
        Operation.CmdCreateNewChar.Go(userName, gender, LPCValue.Create(extra));
    }

    /// <summary>
    /// 踢玩家下线
    /// </summary>
    public static void KickOut()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 通知玩家返回登陆界面
        ExitGame();
    }

    /// <summary>
    /// 退出服务器
    /// </summary>
    public static void ExitGame()
    {
        // 断开原来的连接;
        Communicate.Disconnect();

#if  !UNITY_EDITOR

        // 如果玩家对象存在，则通知sdk记录玩家离开游戏数据
        if (ME.user != null)
        {
            // 生成渠道account
            string channelAccount = string.Format("{0}{1}",
                QCCommonSDK.QCCommonSDK.FindNativeSetting("CHANNEL"),
                Communicate.AccountInfo.Query("account", ""));

            // 通知退出游戏
            QCCommonSDK.Addition.DataAnalyzeSupport.RecordEvent("exitServer",new Dictionary<string, string>() {
                {"account", channelAccount},
                {"role", ME.user.GetRid()},
                {"role_name", ME.user.GetName()},
                {"server", Communicate.AccountInfo.Query("server_id", "")},
                {"grade", ME.user.GetLevel().ToString()},
                {"gold_coin", ME.user.Query<int>("gold_coin").ToString()},
                {"money", ME.user.Query<int>("money").ToString()},
                {"create_time",  ME.user.Query<int>("create_time").ToString()}
            });
        }

        // sdk账号登出
        QCCommonSDK.Addition.AuthSupport.CancelAuth();
#endif

        // 网络恢复移除网络等待窗口
        WaitWndMgr.RemoveWaitWnd("network", WaitWndType.WAITWND_TYPE_NETWORK);

        // 停止游戏
        ME.StopGame();

        // 如果当前场景就是登陆场景, 直接Init LoginWnd，否则需要重新载入Start场景
        if(string.Equals(SceneManager.GetActiveScene().name, "Start"))
        {
            // 获取LoginWnd窗口
            GameObject wnd = WindowMgr.GetWindow(LoginWnd.WndType);

            // 重新初始化登陆窗口
            if (wnd != null)
                wnd.GetComponent<LoginWnd>().InitWnd();

            return;
        }

        // 加载登陆场景
        SceneMgr.LoadScene("Start", new CallBack(OnLoginScene));
    }

    /// <summary>
    /// 检查配置
    /// </summary>
    /// <returns>The config.</returns>
    /// <param name="cb">Cb.</param>
    public static IEnumerator CheckConfig(CallBack cb)
    {
        // 10s内不用再检测
        int nowTime = TimeMgr.GetTime();

        // 10s内不做检测
        if (lastCheckTime != 0 && (nowTime - lastCheckTime) < 10)
        {
            // 维护状态
            if (isFixedStatus)
            {
                // 打开服务器维护提示窗口
                GameObject noticeWnd = WindowMgr.OpenWnd(NoticeWnd.WndType);

                // 设置维护窗口点击回调
                noticeWnd.GetComponent<NoticeWnd>().SetCallBack(cb);
            }

            yield break;
        }

        lastCheckTime = nowTime;

        // 重新设置维护状态
        isFixedStatus = false;

        // 加载服务器配置
        yield return Coroutine.DispatchService(ConfigMgr.Init());
        while (! ConfigMgr.InitSuccessed)
            yield return null;

        // 检查客户端是否需要更新, 如果需要更新则退出
        yield return Coroutine.DispatchService(ConfigMgr.CheckClientUpdate());
        if (ConfigMgr.IsNeedUpdateClient)
            yield break;

        // 检测资源更新, 如果需要资源更新则会到资源更新界面
        yield return Coroutine.DispatchService(VersionMgr.CompareOnlineVersion());
        if(VersionMgr.UpdateResDict.Count != 0)
        {
            // 确认操作标识
            bool isConfirmed = false;
            DialogMgr.ShowSingleBtnDailog(new CallBack((para, obj) =>
            {
                // 加载start场景
                SceneMgr.LoadScene("Start", new CallBack(OnLoginScene));
            }), LocalizationMgr.Get("AccountWnd_2"), LocalizationMgr.Get("AccountWnd_1"));

            // 等到玩家确认
            while(!isConfirmed)
                yield return null;
        }

        int status = 0;
        string serverStatus = ConfigMgr.Get<string>("ServerStatus");
        if(!string.IsNullOrEmpty(serverStatus))
        {
            int.TryParse(serverStatus, out status);

            // 服务器维护中
            if(status.Equals(ServerStatus.SERVER_STATUS_FIX))
            {
                // 打开服务器维护提示窗口
                GameObject noticeWnd = WindowMgr.OpenWnd(NoticeWnd.WndType);

                isFixedStatus = true;

                // 设置维护窗口点击回调
                noticeWnd.GetComponent<NoticeWnd>().SetCallBack(cb);
            }
        }

        yield break;
    }

    #endregion
}
