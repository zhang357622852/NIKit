/// <summary>
/// LoginWnd.cs
/// Created by xuhd Jan/31/2015
/// 登陆窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;
using QCCommonSDK;
using QCCommonSDK.Util;

public class LoginWnd : WindowBase<LoginWnd>
{
    #region 成员变量

    // 版本号提示
    public UILabel mStartGame;

    // 版本号提示
    public GameObject mVersionTips;

    // 输入账号密码窗口
    public GameObject mAccountWnd;

    // 创建角色
    public GameObject mCreatePlayer;

    public GameObject mSlectLoginWnd;

    public GameObject mLogout;
    public GameObject mLogoutBtn;
    public UILabel mLogTypeLb;
    public UILabel mLogoutLb;

    // 账号切换按钮
    public GameObject mChangeAccount;
    public GameObject mChangeAccountBtn;
    public UILabel mChangeAccountLb;

    public UILabel mBsTips;

    // 渠道
    private string mChannel;

    #endregion

    #region 内部接口

    // Use this for initialization
    void Start()
    {
        // 设置版本号信息
        mVersionTips.GetComponent<UILabel>().text = "Ver " + ConfigMgr.ClientVersion;

#if UNITY_ANDROID && ! UNITY_EDITOR

        if(string.Equals(ConfigMgr.GameRunMode, ConfigMgr.MODE_PUBLISH))
            mVersionTips.GetComponent<UILabel>().text = "「"+ QCCommonSDK.QCCommonSDK.FindNativeSetting("Client_Name") +"」"+ "Ver " + ConfigMgr.ClientVersion;
        else
            mVersionTips.GetComponent<UILabel>().text = "Ver " + ConfigMgr.ClientVersion;

#else

        mVersionTips.GetComponent<UILabel>().text = "Ver " + ConfigMgr.ClientVersion;

#endif

        // 游戏版号版权提示
        // 防沉迷提示
        string copyRightTip = LocalizationMgr.Get("ResourceLoadingWnd_10", LocalizationConst.START);
        string preventAddictionTip = LocalizationMgr.Get("ResourceLoadingWnd_100", LocalizationConst.START);
        string bsTipsText = string.Empty;

        // 防沉迷提示
        if (! string.IsNullOrEmpty(preventAddictionTip))
            bsTipsText = preventAddictionTip;

        // 游戏版号版权提示
        if (! string.IsNullOrEmpty(copyRightTip))
            bsTipsText = string.Format("{0}\n{1}", copyRightTip, bsTipsText);

        // 设置文本显示
        mBsTips.text = bsTipsText;

        // 显示客户端版本
        mVersionTips.SetActive(true);

#if !UNITY_EDITOR

        // 内测版本直接打开账号界面
        if(string.Equals(ConfigMgr.GameRunMode, ConfigMgr.MODE_LOCAL_HOST))
        {
            mStartGame.gameObject.SetActive(false);
            UIEventListener.Get(mStartGame.gameObject).onClick = OnStartGame;
            mStartGame.text = LocalizationMgr.Get("LoginWnd_6");
        }
        else if(string.Equals(ConfigMgr.GameRunMode, ConfigMgr.MODE_PUBLISH))
        {
            mChannel = QCCommonSDK.QCCommonSDK.FindNativeSetting("CHANNEL");

            if (mChannel.EndsWith("en") || mChannel.EndsWith("tw"))
            {
                mStartGame.gameObject.SetActive(false);
                UIEventListener.Get(mStartGame.gameObject).onClick = OnStartGame;
                mStartGame.text = LocalizationMgr.Get("LoginWnd_6");
            }
            else
            { 
                // 激活开始游戏界面
                mStartGame.gameObject.SetActive(true);

                UIEventListener.Get(mStartGame.gameObject).onClick = OnClickStartGameShowLogin;
                mStartGame.text = LocalizationMgr.Get("LoginWnd_26");
            }
        }
#else
        mStartGame.gameObject.SetActive(false);
        mStartGame.text = LocalizationMgr.Get("LoginWnd_6");
        UIEventListener.Get(mStartGame.gameObject).onClick = OnStartGame;
#endif

        mLogoutLb.text = LocalizationMgr.Get("LoginWnd_16");

        // 注册按钮点击事件
        UIEventListener.Get(mLogoutBtn).onClick += OnLogoutBtn;
        UIEventListener.Get(mChangeAccountBtn).onClick = OnClickChangeAccount;

        mChangeAccountLb.text = LocalizationMgr.Get("LoginWnd_25");

        InitWnd();
    }

    /// <summary>
    /// OnDisable
    /// </summary>
    void OnDisable()
    {
        // 注销消息回调
        MsgMgr.RemoveDoneHook("MSG_EXISTED_CHAR_LIST", "LoginWnd");

        // 关注账户认证结果消息
        MsgMgr.RemoveDoneHook("MSG_AUTH_ACCOUNT_RESULT", "LoginWnd");

        MsgMgr.RemoveDoneHook("MSG_CREATE_NEW_CHAR_RESULT", LoginWnd.WndType);

        // 注销事件监听
        EventMgr.UnregisterEvent("LoginWnd");

#if ! UNITY_EDITOR
            QCCommonSDK.Addition.AuthSupport.OnAuthResult -= AuthEventHandle;
            QCCommonSDK.Addition.AuthSupport.OnCancelAuthResult -= CancelAuthEventHandle;
#endif
    }

    /// <summary>
    /// OnEnable
    /// </summary>
    void OnEnable()
    {
        // 关注角色列表数据
        MsgMgr.RemoveDoneHook("MSG_EXISTED_CHAR_LIST", "LoginWnd");
        MsgMgr.RegisterDoneHook("MSG_EXISTED_CHAR_LIST", "LoginWnd", OnMsgExistedCharList);

        // 关注账户认证结果消息
        MsgMgr.RemoveDoneHook("MSG_AUTH_ACCOUNT_RESULT", "LoginWnd");
        MsgMgr.RegisterDoneHook("MSG_AUTH_ACCOUNT_RESULT", "LoginWnd", OnAuthAccountResult);

        // 关注创建角色失败消息
        MsgMgr.RemoveDoneHook("MSG_CREATE_NEW_CHAR_RESULT", LoginWnd.WndType);
        MsgMgr.RegisterDoneHook("MSG_CREATE_NEW_CHAR_RESULT", LoginWnd.WndType, OnMsgCreateNewCharResult);

        // 网络连接失败
        EventMgr.UnregisterEvent("LoginWnd");
        EventMgr.RegisterEvent("LoginWnd", EventMgrEventType.EVENT_LOGIN_FAILED, OnLoginFailed);

#if ! UNITY_EDITOR
            QCCommonSDK.Addition.AuthSupport.OnAuthResult += AuthEventHandle;

            // 注册取消登陆回调
            QCCommonSDK.Addition.AuthSupport.OnCancelAuthResult += CancelAuthEventHandle;
#endif
    }

    /// <summary>
    /// MSG_CREATE_NEW_CHAR_RESULT消息回调
    /// </summary>
    void OnMsgCreateNewCharResult(string cmd, LPCValue para)
    {
        // 转换数据格式
        LPCMapping args = para.AsMapping;

        // 如果是创建玩家角色成功
        if (args.GetValue<int>("result") != 0)
            return;

        // 隐藏等待窗口
        WaitWndMgr.RemoveWaitWnd("LogIn", WaitWndType.WAITWND_TYPE_LOGIN);

        // 如果已经有玩家列表
        List<LPCValue> users = LoginMgr.GetAllUser();
        if (users.Count != 0)
            return;

        // 显示角色创建窗口
        if (!mCreatePlayer.activeSelf)
            mCreatePlayer.SetActive(true);
    }


    /// <summary>
    /// MSG_EXISTED_CHAR_LIST的回调.
    /// </summary>
    private void OnMsgExistedCharList(string cmd, LPCValue para)
    {
        // 隐藏等待窗口
        WaitWndMgr.RemoveWaitWnd("LogIn", WaitWndType.WAITWND_TYPE_LOGIN);

        // 获取全部角色列表
        List<LPCValue> users = LoginMgr.GetAllUser();

        // 没有玩家列表, 这个时候需要弹出角色创建界面
        if (users.Count == 0)
        {
            // 该指引已经结束或者没有角色列表主动触发指引
            if (!GuideMgr.DoGuide(GuideConst.SHOW_CREATE_CHAR_BEFORE, new CallBack(OnGuideCallBack)))
            {
                // 显示创建角色界面
                mCreatePlayer.SetActive(true);

                // 隐藏登录界面;
                mAccountWnd.SetActive(false);
            }

            return;
        }

        //添加游戏登陆等待消息
        WaitWndMgr.AddWaitWnd("LogIn", WaitWndType.WAITWND_TYPE_LOGIN);

        // 隐藏创建角色界面
        mCreatePlayer.SetActive(false);
    }

    /// <summary>
    /// 创建角色指引完成回调
    /// </summary>
    void OnGuideCallBack(object para, params object[] param)
    {
        // 显示创建角色界面
        mCreatePlayer.SetActive(true);

        // 隐藏登录界面;
        mAccountWnd.SetActive(false);
    }

    /// <summary>
    /// 开始游戏按钮点击事件
    /// </summary>
    private void OnStartGame(GameObject ob)
    {
        mStartGame.gameObject.SetActive(false);

        mLogout.SetActive(false);

        mChangeAccount.SetActive(false);

        Coroutine.DispatchService(TryLoginAAA());
    }

    /// <summary>
    /// StartGame按钮点击回调
    /// </summary>
    private void OnClickStartGameShowLogin(GameObject go)
    {
        TryLogin();
    }

    private IEnumerator TryLoginAAA()
    {
#if ! UNITY_EDITOR

        yield return Coroutine.DispatchService(LoginMgr.CheckConfig(
        new CallBack((para, obj) =>
            {
                SetLoginState();
            })
        ));

        // 还在维护状态
        if (LoginMgr.isFixedStatus)
            yield break;

#endif

        // 执行登陆操作
        LoginMgr.DoLoginAAA();

        yield break;
    }

    /// <summary>
    /// 开始游戏按钮点击事件
    /// </summary>
    private void OnLogoutBtn(GameObject ob)
    {
        mStartGame.gameObject.SetActive(false);

        mLogout.SetActive(false);

        mChangeAccount.SetActive(false);

#if !UNITY_EDITOR

        // 登出sdk账号
        if(string.Equals(ConfigMgr.GameRunMode, ConfigMgr.MODE_PUBLISH))
            QCCommonSDK.Addition.AuthSupport.CancelAuth();

#endif
    }

    /// <summary>
    /// 账号切换按钮点击回调
    /// </summary>
    private void OnClickChangeAccount(GameObject go)
    {
#if !UNITY_EDITOR

        // 显示用户中心
        if(string.Equals(ConfigMgr.GameRunMode, ConfigMgr.MODE_PUBLISH))
            QCCommonSDK.Addition.AuthSupport.ShowUserCenter(false);

#endif
    }

    /// <summary>
    /// 检测配置
    /// </summary>
    /// <returns>The init.</returns>
    private void TryLogin()
    {
#if UNITY_EDITOR

        mAccountWnd.SetActive(true);
#else

        // 内测版本直接打开账号界面
        if(string.Equals(ConfigMgr.GameRunMode, ConfigMgr.MODE_LOCAL_HOST))
        {
            mAccountWnd.SetActive(true);
            return;
        }

        // 取得上次登陆的方式
        string type = OptionMgr.GetPublicOption("login_type").AsString;

        // 取得上次登陆的方式
        int isLogedin = OptionMgr.GetPublicOption("is_loggedin").AsInt;

        if(isLogedin == 0)
        {
            QCCommonSDK.Addition.AuthSupport.Auth("guest");
            return;
        }

        if(string.IsNullOrEmpty(type))
        {
            mSlectLoginWnd.SetActive(true);
            return;
        }

        QCCommonSDK.Addition.AuthSupport.Auth(type);
#endif
    }

    private void ShowLoginWnd()
    {
#if ! UNITY_EDITOR

        // 内测版本直接打开账号界面
        if(string.Equals(ConfigMgr.GameRunMode, ConfigMgr.MODE_LOCAL_HOST))
        {
            mAccountWnd.SetActive(true);
            return;
        }

        if (mChannel.EndsWith("en") || mChannel.EndsWith("tw"))
        {
            OptionMgr.SetPublicOption("login_type", LPCValue.Create(string.Empty));

            // 打开选择登录界面
            mSlectLoginWnd.SetActive(true);
        }
        else
        {
            // 构造数据
            OptionMgr.SetPublicOption("is_loggedin", LPCValue.Create(1));

            OptionMgr.SetPublicOption("login_type", LPCValue.Create(mChannel));

            // 尝试登录
            TryLogin();

            mStartGame.gameObject.SetActive(true);
        }
#else
        mAccountWnd.SetActive(true);

#endif
    }

    /// <summary>
    /// Auths the event handle.
    /// </summary>
    private void AuthEventHandle(QCEventResult result)
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

                // 设置成已登录状态
                SetLoginState();

                if (mChannel.EndsWith("en") || mChannel.EndsWith("tw"))
                {
                    // 隐藏选择登录窗口
                    mSlectLoginWnd.SetActive(false);
                }
                else
                {
                    mStartGame.text = LocalizationMgr.Get("LoginWnd_6");

                    mChangeAccount.SetActive(true);

                    UIEventListener.Get(mStartGame.gameObject).onClick -= OnClickStartGameShowLogin;
                    UIEventListener.Get(mStartGame.gameObject).onClick = OnStartGame;
                }

                break;
            case QCErrorCode.CanelOperation:
                
                // 登陆失败
                DialogMgr.Notify(string.Format(LocalizationMgr.Get("LoginWnd_23"), result.error));

                ShowLoginWnd();

                break;
            default:

                // 登录失败
                DialogMgr.Notify(string.Format(LocalizationMgr.Get("LoginWnd_23"), result.error));

                ShowLoginWnd();

                break;
        }
    }

    /// <summary>
    /// Auths the event handle.
    /// </summary>
    private void CancelAuthEventHandle(QCEventResult result)
    {
        if (result == null)
        {
            LogMgr.Trace("CancelAuthEventHandle erro");
            return;
        }
        switch (result.error)
        {
            case QCErrorCode.SUCCESS:

                if (mChannel.EndsWith("en") || mChannel.EndsWith("tw"))
                {
                    // 显示登录界面
                    mSlectLoginWnd.SetActive(true);
                }
                else
                {
                    mStartGame.text = LocalizationMgr.Get("LoginWnd_26");

                    // 隐藏账号切换按钮
                    mChangeAccount.SetActive(false);

                    UIEventListener.Get(mStartGame.gameObject).onClick -= OnStartGame;
                    UIEventListener.Get(mStartGame.gameObject).onClick = OnClickStartGameShowLogin;
                }

                break;
            default:
                // 登出失败
                DialogMgr.Notify(string.Format(LocalizationMgr.Get("LoginWnd_24"), result.error));

                // 继续退出
                QCCommonSDK.Addition.AuthSupport.CancelAuth();

                break;
        }
    }

    /// <summary>
    /// 关注账户认证结果的消息.
    /// </summary>
    private void OnAuthAccountResult(string cmd, LPCValue para)
    {
        // 验证结果码
        int result = para.AsMapping["result"].AsInt;

        // 连接成功
        if (result == 1)
            return;
        
#if !UNITY_EDITOR

        if (mChannel.EndsWith("en") || mChannel.EndsWith("tw"))
        {
            OptionMgr.SetPublicOption("login_type", LPCValue.Create(string.Empty));
        }

        // 设置登录状态
        SetLoginState();

#else

        mAccountWnd.SetActive(true);
        mStartGame.gameObject.SetActive(false);

#endif
    }

    /// <summary>
    /// 登陆失败.
    /// </summary>
    private void OnLoginFailed(int eventId, MixedValue para)
    {
        // 隐藏创建角色界面
        mCreatePlayer.SetActive(false);

#if ! UNITY_EDITOR

        // 非内测版本, 退出当前sdk账号
        if(string.Equals(ConfigMgr.GameRunMode, ConfigMgr.MODE_PUBLISH))
            QCCommonSDK.Addition.AuthSupport.CancelAuth();

        // 显示账号界面
        ShowLoginWnd();

#else

        // 显示账号界面
        mAccountWnd.SetActive(true);

#endif
    }

    /// <summary>
    /// 设置登出按钮状态
    /// </summary>
    private void SetLogOutButtonState()
    {
        // 设置账号绑定区域
        bool needBindAccount = QCCommonSDK.Addition.AuthSupport.needBindAccount();
        if (needBindAccount)
            mLogTypeLb.text = LocalizationMgr.Get("LoginWnd_18");
        else
            mLogTypeLb.text = LocalizationMgr.Get("LoginWnd_17");
    }

    #endregion


    #region 外部接口

    /// <summary>
    /// 初始化窗口
    /// </summary>
    public void InitWnd()
    {
        // 隐藏角色创建界面
        mCreatePlayer.SetActive(false);

        // 隐藏登陆界面
        mAccountWnd.SetActive(false);

        // 隐藏登陆界面
        mLogout.SetActive(false);

        mChangeAccount.SetActive(false);

        TryLogin();
    }

    /// <summary>
    /// 设置登录状态
    /// </summary>
    public void SetLoginState()
    {
        // 显示开始游戏按钮
        mStartGame.gameObject.SetActive(true);

#if ! UNITY_EDITOR

        // 内测版本直接打开账号界面
        if(string.Equals(ConfigMgr.GameRunMode, ConfigMgr.MODE_LOCAL_HOST))
            return;

        // 海外版本特殊处理
        if (mChannel.EndsWith("en") || mChannel.EndsWith("tw"))
        {
            mLogout.SetActive(true);
            SetLogOutButtonState();
        }
#endif
    }

    #endregion
}
