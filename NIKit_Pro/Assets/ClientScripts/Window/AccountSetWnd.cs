/// <summary>
/// AccountSetWnd.cs
/// Created by fengsc 2016/07/05
/// 账号设置窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;
using QCCommonSDK.Addition;
using QCCommonSDK;


public class AccountSetWnd : MonoBehaviour
{
    #region 成员变量

    /// <summary>
    ///玩家名称
    /// </summary>
    public UILabel mPlayerName;

    /// <summary>
    /// 玩家等级
    /// </summary>
    public UILabel mLevel;

    /// <summary>
    ///玩家头像
    /// </summary>
    public UITexture mPlayerIcon;

    /// <summary>
    /// 玩家ID
    /// </summary>
    public UILabel mRid;

    /// <summary>
    ///退出登录按钮
    /// </summary>
    public GameObject mExitLogin;

    public UILabel mExitLoginLabel;

    public UILabel mChangeIconLabel;

    /// <summary>
    ///讨论专区按钮
    /// </summary>
    public GameObject mDiscuss;
    public UILabel mDiscussLabel;

    /// <summary>
    ///游戏初始化按钮
    /// </summary>
    public GameObject mGameInit;
    public UILabel mGameInitLabel;

    // 游戏历程按钮
    public GameObject mGameCourseBtn;
    public UILabel mGameCourseBtnLb;
    public GameObject mGameCourseTips;

    public UILabel mCountDownLb;

    public UILabel mTips;

    public GameObject mConvertCodeBtn;
    public UILabel mConvertCodeBtnLb;

    public GameObject mHelpshiftBtn;
    public UILabel mHelpshiftLb;
    public GameObject mHelpshiftRedBtn;

    public GameObject[] mBindItems;

    public GameObject mBetaTips;

    // 复制名字按钮
    public GameObject mCopyNameBtn;

    // 复制玩家ID按钮
    public GameObject mCopyRidBtn;

    bool mIsCountDown = false;

    float mLastTime = 0;

    int mRemainTime = 0;

    #endregion

    // Use this for initialization
    void Start () 
    {
        RegisterEvent();

        InitLabel();

        // 显示初始化按钮
        ShowInitBtn();

        ShowAccountSetUIInfo();

        // 显示激活码按钮
        ShowConvertCodeButton();

        // 设置绑定信息
        SetBindAccount();

        // 初始化按钮位置
        InitButtonPos();

        // 显示游戏历程提示
        mGameCourseTips.SetActive(GameCourseMgr.GetNewUnlockCourses(ME.user).Count != 0);
    }

    void Update()
    {
        if (mIsCountDown)
        {
            if ((Time.realtimeSinceStartup > mLastTime + 1.0f))
            {
                mLastTime = Time.realtimeSinceStartup;
                CountDown();
            }
        }
    }

    void OnDestroy()
    {
        // 移除消息关注
        MsgMgr.RemoveDoneHook("MSG_UNIVERSE_UPDATED", "AccountSetWnd");

        // 反注册
        EventMgr.UnregisterEvent("AccountSetWnd");

#if !UNITY_EDITOR

        NativeSupport.OnNewMessageResult -= NewMessageHook;

#endif

        if (ME.user == null)
            return;

        ME.user.tempdbase.RemoveTriggerField("AccountSetWnd");
    }

    /// <summary>
    ///注册事件
    /// </summary>
    void RegisterEvent()
    {
        //账号设置界面添加的点击事件;
        UIEventListener.Get(mExitLogin).onClick = OnClickExitLogin;
        UIEventListener.Get(mGameInit).onClick = OnClickGameInit;
        UIEventListener.Get(mDiscuss).onClick = OnClickDiscuss;
        UIEventListener.Get(mConvertCodeBtn).onClick = OnClickConvertBtn;
        UIEventListener.Get(mHelpshiftBtn).onClick = OnClickHelpshiftBtn;
        UIEventListener.Get(mCopyNameBtn).onClick = OnClickCopyNameBtn;
        UIEventListener.Get(mCopyRidBtn).onClick = OnClickCopyIDBtn;
        UIEventListener.Get(mGameCourseBtn).onClick = OnClickGameCourseBtn;

        // 关注MSG_UNIVERSE_UPDATED消息
        MsgMgr.RegisterDoneHook("MSG_UNIVERSE_UPDATED", "AccountSetWnd", OnUniverseUpdateMsg);

        // 注册账号成功
        EventMgr.RegisterEvent("AccountSetWnd", EventMgrEventType.EVENT_REGISTER_ACCOUNT_SUCC, RegisterAndBindAccountSucc);

#if !UNITY_EDITOR

        NativeSupport.OnNewMessageResult += NewMessageHook;

        NativeSupport.GetHSNewMessage();
#endif

        if (ME.user == null)
            return;

        ME.user.tempdbase.RemoveTriggerField("AccountSetWnd");
        ME.user.tempdbase.RegisterTriggerField("AccountSetWnd", new string[]{ "unlock_new_course" }, new CallBack(OnFieldsChange));
    }

    void OnFieldsChange(object para, params object[] param)
    {
        // 显示游戏历程提示
        mGameCourseTips.SetActive(GameCourseMgr.GetNewUnlockCourses(ME.user).Count != 0);
    }

    /// <summary>
    /// MSG_UNIVERSE_UPDATED消息回调
    /// </summary>
    void OnUniverseUpdateMsg(string cmd, LPCValue para)
    {
        if (!para.IsMapping)
            return;

        LPCMapping dbase = para.AsMapping.GetValue<LPCMapping>("dbase");
        if (dbase == null)
            return;

        if (!dbase.ContainsKey("is_redeem_key_on"))
            return;

        // 显示激活码按钮
        ShowConvertCodeButton();
    }

    /// <summary>
    /// 账号创建并绑定成功
    /// </summary>
    void RegisterAndBindAccountSucc(int eventId, MixedValue para)
    {
        // 关闭窗口
        SetBindAccount();
    }

    /// <summary>
    ///初始化文本
    /// </summary>
    void InitLabel()
    {
        mChangeIconLabel.text = LocalizationMgr.Get("SystemWnd_15");
        mDiscussLabel.text = LocalizationMgr.Get("SystemWnd_16");
        mGameInitLabel.text = LocalizationMgr.Get("SystemWnd_17");
        mExitLoginLabel.text = LocalizationMgr.Get("SystemWnd_18");
        mTips.text = LocalizationMgr.Get("SystemWnd_29");
        mConvertCodeBtnLb.text = LocalizationMgr.Get("SystemWnd_30");
        mHelpshiftLb.text = LocalizationMgr.Get("SystemWnd_37");
        mCopyNameBtn.GetComponent<UILabel>().text = LocalizationMgr.Get("SystemWnd_38");
        mCopyRidBtn.GetComponent<UILabel>().text = LocalizationMgr.Get("SystemWnd_38");
        mGameCourseBtnLb.text = LocalizationMgr.Get("SystemWnd_43");
    }

    /// <summary>
    /// 获取sku消息的回调
    /// </summary>
    private void NewMessageHook(QCEventResult result)
    {
        if(!result.result.ContainsKey("number"))
            return;

        string number = result.result["number"];

        mHelpshiftRedBtn.SetActive(int.Parse(number) > 0 ? true : false);
    }

    /// <summary>
    /// 倒计时
    /// </summary>
    void CountDown()
    {
        if (mRemainTime < 0)
        {
            // 刷新初始化按钮
            ShowInitBtn();

            return;
        }

        mRemainTime--;

        // 剩余多少小时
        mCountDownLb.text = string.Format(LocalizationMgr.Get("SystemWnd_31"), mRemainTime / 3600);
    }

    /// <summary>
    /// 显示激活码按钮
    /// </summary>
    void ShowConvertCodeButton()
    {
        if (ME.user == null)
            return;

        // 是否开启兑换码功能
        if (ME.user.QueryTemp<int>("is_redeem_key_on") == 1)
            mConvertCodeBtn.SetActive(true);
        else
            mConvertCodeBtn.SetActive(false);
    }

    // 显示初始化按钮
    void ShowInitBtn()
    {
        // 上次账号重置的时间
        int resetTime = 0;

        LPCValue v = ME.user.Query<LPCValue>("reset_time");
        if (v != null && v.IsInt)
            resetTime = v.AsInt;

        // 默认重置间隔
        int defaultResetInterval = GameSettingMgr.GetSettingInt("default_reset_interval");

        // 多久后可以刷新
        mRemainTime = Mathf.Max(defaultResetInterval - (TimeMgr.GetServerTime() - resetTime), 0);
        if (mRemainTime <= 0)
        {
            mCountDownLb.gameObject.SetActive(false);

            mGameInitLabel.gameObject.SetActive(true);

            mIsCountDown = false;

            // 激活碰撞盒
            mGameInit.GetComponent<BoxCollider>().enabled = true;

            mGameInit.GetComponent<UISprite>().alpha = 1f;

            return;
        }

        mCountDownLb.gameObject.SetActive(true);
        mGameInitLabel.gameObject.SetActive(false);

        // 剩余多少小时
        mCountDownLb.text = string.Format(LocalizationMgr.Get("SystemWnd_31"), mRemainTime / 3600);

        mGameInitLabel.gameObject.SetActive(false);

        mGameInit.GetComponent<BoxCollider>().enabled = false;

        mGameInit.GetComponent<UISprite>().alpha = 0.59f;

        mIsCountDown = true;
    }

    /// <summary>
    ///显示玩家账号设置界面信息
    /// </summary>
    void ShowAccountSetUIInfo()
    {
        //显示玩家名称;
        mPlayerName.text = LocalizationMgr.Get("SystemWnd_14") + ME.user.GetName();

        mLevel.text = string.Format( LocalizationMgr.Get("SystemWnd_36") , ME.user.Query<LPCValue>("level").AsString);

        mRid.text = string.Format( LocalizationMgr.Get("SystemWnd_39") , ME.user.GetRid());

        LPCValue v = ME.user.Query<LPCValue>("icon");

        if (v == null || ! v.IsString)
        {
            mPlayerIcon.mainTexture = null;
        }
        else
        {
            //获取玩家头像名字;
            //加载玩家头像;
            string resPath = string.Format("Assets/Art/UI/Icon/monster/{0}.png", v.AsString);
            Texture2D iconRes = ResourceMgr.LoadTexture(resPath);

            mPlayerIcon.mainTexture = iconRes;
        }
    }

    /// <summary>
    /// 初始化按钮位置
    /// </summary>
    void InitButtonPos()
    {
        // 是否开启初始化功能
        string closeInitFunc = ConfigMgr.Get<string>("close_init_func");

        // 初始化按钮位置
        mGameCourseBtn.transform.localPosition = new Vector3(-218, 26f, 0);
        mGameInit.transform.localPosition = new Vector3(-65, 26, 0);
        mHelpshiftBtn.transform.localPosition = new Vector3(-65, 26, 0);
        mDiscuss.transform.localPosition = new Vector3(87f, 26.1f, 0f);
        mExitLogin.transform.localPosition = new Vector3(239.2f, 26, 0);

        mHelpshiftRedBtn.SetActive(false);

        if(QCCommonSDK.Native.NativeCall.CheckFunctionSupport("showHelpshift"))
        {
            mHelpshiftBtn.SetActive(true);

            mGameInit.SetActive(false);
        }
        else
        {
            mHelpshiftBtn.SetActive(false);

            if (string.IsNullOrEmpty(closeInitFunc))
            {
                mGameInit.SetActive(true);
            }
            else
            {
                mDiscuss.transform.localPosition = new Vector3(8, 26.5f, 0);

                mGameInit.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 设定绑定账号状态
    /// </summary>
    /// <param name="isNeedBind">If set to <c>true</c> is need bind.</param>
    void SetBindAccount()
    {
#if UNITY_EDITOR
        foreach (GameObject item in mBindItems)
            item.SetActive(false);

        mBetaTips.SetActive(true);
#else

        if(! string.Equals(ConfigMgr.GameRunMode, ConfigMgr.MODE_PUBLISH))
        {
            foreach (GameObject item in mBindItems)
                item.SetActive(false);

            mBetaTips.SetActive(true);

            return;
        }

        mBetaTips.SetActive(false);

        // TODO 此处做了特殊处理，英文版与中文版做不同处理
        string clientVersion = QCCommonSDK.QCCommonSDK.FindNativeSetting("CHANNEL");

        if(clientVersion.EndsWith("en") || clientVersion.EndsWith("tw"))
        {
            // 设置账号绑定区域
            Dictionary<string, string> bindData = QCCommonSDK.Addition.AuthSupport.GetBindAccount();

            mBindItems[0].SetActive(true);
            mBindItems[1].SetActive(true);

            mBindItems[0].GetComponent<BindAccountItem>().SetData("facebook", "login_fb", "Facebook",
                bindData.ContainsKey("facebook") ? bindData["facebook"] : "" );

            mBindItems[1].GetComponent<BindAccountItem>().SetData("google", "login_google", "Google",
                bindData.ContainsKey("google") ? bindData["google"] : "" );

            mBindItems[0].transform.localPosition = new Vector3(0f, -55f, 0f);

            mBindItems[1].transform.localPosition = new Vector3(0f, -117f, 0f);

            return;
        }

        bool needBindAccount = QCCommonSDK.Addition.AuthSupport.needBindAccount();

        mBindItems[0].SetActive(true);
        mBindItems[1].SetActive(false);

        mBindItems[0].GetComponent<BindAccountItem>().SetData("qc", "login_qc", "青瓷数码",
            needBindAccount ? "" : LocalizationMgr.Get("SystemWnd_34"));

        mBindItems[0].transform.localPosition = new Vector3(0f,-85f, 0f);

#endif
    }

    /// <summary>
    /// 兑换码按钮点击事件
    /// </summary>
    void OnClickConvertBtn(GameObject go)
    {
        // 打开兑换码窗口
        WindowMgr.OpenWnd(ConvertCodeWnd.WndType);
    }

    /// <summary>
    /// 复制玩家名字至系统剪切板
    /// </summary>
    void OnClickCopyNameBtn(GameObject go)
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 给出提示信息
        DialogMgr.Notify(string.Format(LocalizationMgr.Get("SystemWnd_40")));

        // 调用平台接口复制名称至剪切板
        QCCommonSDK.Native.NativeCall.CopyToNativeClipboard(ME.user.GetName());
    }

    /// <summary>
    /// 复制玩家ID至系统剪切板
    /// </summary>
    void OnClickCopyIDBtn(GameObject go)
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 给出提示信息
        DialogMgr.Notify(string.Format(LocalizationMgr.Get("SystemWnd_41")));

        // 调用平台接口复制名称至剪切板
        QCCommonSDK.Native.NativeCall.CopyToNativeClipboard(ME.user.GetRid());
    }

    /// <summary>
    /// 兑换客服系统
    /// </summary>
    void OnClickHelpshiftBtn(GameObject go)
    {
#if !UNITY_EDITOR

        if(ME.user == null)
            return;

        // 获取玩家使魔数量
        int petAmount = BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_PET_GROUP).Count
            + BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_STORE_GROUP).Count;

        // 生成渠道account
        string channelAccount = string.Format("{0}{1}",
            QCCommonSDK.QCCommonSDK.FindNativeSetting("CHANNEL"),
            Communicate.AccountInfo.Query("account", ""));

        NativeSupport.ShowHelpshift(new Dictionary<string, string>{
            {"uid" , ME.user.GetRid()},
            {"account" ,channelAccount},
            {"level" , ME.user.GetLevel().ToString()},
            {"pets" , petAmount.ToString()},
            {"version" , ConfigMgr.ClientVersion},
            {"server_id" , Communicate.AccountInfo.Query("server_id", "")},
            {"reg_time" , TimeMgr.ConvertIntDateTime((double)ME.user.Query<int>("create_time")).ToString()},
            {"gem" , ME.user.Query<int>("gold_coin").ToString()},
            {"money",  ME.user.Query<int>("money").ToString()}
        });

#endif
    }

    /// <summary>
    ///讨论专区点击事件
    /// </summary>
    void OnClickDiscuss(GameObject go)
    {
        string url = ConfigMgr.Get<string>("bbs_url", string.Empty);

        // 没有论坛地址不显示
        if (string.IsNullOrEmpty(url))
            return;

        GameObject wnd = WindowMgr.OpenWnd("WebViewWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        wnd.GetComponent<WebViewWnd>().BindData(url);
    }

    /// <summary>
    ///游戏初始化点击事件 
    /// </summary>
    void OnClickGameInit(GameObject go)
    {
        //获取账号初始化窗口;
        GameObject wnd = WindowMgr.OpenWnd("AccountInitWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);

        //创建窗口失败;
        if(wnd == null)
        {
            LogMgr.Trace("打开AccountInitWnd窗口失败!");
            return;
        }
    }

    /// <summary>
    /// 游戏历程点击回调
    /// </summary>
    void OnClickGameCourseBtn(GameObject go)
    {
        // 打开游戏历程窗口
        GameObject wnd = WindowMgr.OpenWnd(GameCourseWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<GameCourseWnd>().Bind(ME.user);
    }

    /// <summary>
    /// 退出登录点击事件
    /// </summary>
    void OnClickExitLogin(GameObject go)
    {
        // 通知服务器退出游戏
        Operation.CmdLogout.Go();

        // 等待退出登陆消息到达服务器
        // 消息线程处理时间是间隔是0.02s，故这个地方等待时间必须要大于这个时间
        Coroutine.DispatchService(DenyBackToLogin(), "DenyBackToLogin");
    }

    /// <summary>
    /// 延迟退出游戏
    /// </summary>
    /// <returns>The wait sever.</returns>
    IEnumerator DenyBackToLogin()
    {
        // 延迟0.1s返回到登陆界面
        yield return new WaitForSeconds(0.1f);

        // 返回到登陆界面
        LoginMgr.ExitGame();
    }
}
