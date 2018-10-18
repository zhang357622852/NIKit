/// <summary>
/// AccountMgr.cs
/// Create by zhaozy 2014-11-3
/// 账户管理模块
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections;
using System.Net.NetworkInformation;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using LPC;
using System.Text;
using System.Text.RegularExpressions;
using LitJson;
using QCCommonSDK.Addition;

/// <summary>
/// AccountMgr账户管理模块
/// </summary>
public static class AccountMgr
{
    #region 变量

    public static bool Ischecked = false;
    private static LPCMapping AccountData = new LPCMapping();

    // 缓存账号密码
    public static string Account { get; set; }

    public static string Secret { get; set; }

    #endregion

    #region 内部接口

    /// <summary>
    /// 登陆成功回调
    /// </summary>
    private static void WhenLoginOk(int eventId, MixedValue para)
    {
        // 如果玩家没有开启接受该类型的聊天信息
        LPCValue confirmedLogin = OptionMgr.GetOption(ME.user, "confirmed_login");

        // 通知服务器该玩家已经登陆过(为null表示没有配置该option表没有配置该参数)
        if (confirmedLogin != null && confirmedLogin.AsInt != 1)
            OptionMgr.SetOption(ME.user, "confirmed_login", LPCValue.Create(1));

#if ! UNITY_EDITOR

        // 生成渠道account
        string channelAccount = string.Format("{0}{1}",
            QCCommonSDK.QCCommonSDK.FindNativeSetting("CHANNEL"),
            Communicate.AccountInfo.Query("account", ""));

        // 注册SDK OnLoginOK
        DataAnalyzeSupport.RecordEvent("login", new Dictionary<string, string>() {
            {"account", channelAccount},
            {"role", ME.user.GetRid()},
            {"role_name", ME.user.GetName()},
            {"server", Communicate.AccountInfo.Query("server_id", "")},
            {"grade", ME.user.GetLevel().ToString()},
            {"gold_coin", ME.user.Query<int>("gold_coin").ToString()},
            {"money", ME.user.Query<int>("money").ToString()},
            {"create_time",  ME.user.Query<int>("create_time").ToString()}
        });

#endif
    }

    /// <summary>
    /// 登陆游戏回调
    /// </summary>
    private static void OnMsgLogin(string cmd, LPCValue para)
    {

    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 模块初始化
    /// </summary>
    public static void Init()
    {
        // 关注MSG_LOGIN消息
        MsgMgr.RemoveDoneHook("MSG_LOGIN", "AccountMgr");
        MsgMgr.RegisterDoneHook("MSG_LOGIN", "AccountMgr", OnMsgLogin);

        // 注册登陆成功回调
        EventMgr.UnregisterEvent("AccountMgr");
        EventMgr.RegisterEvent("AccountMgr", EventMgrEventType.EVENT_LOGIN_OK, WhenLoginOk);
    }

    /// <summary>
    /// 获取账号数据
    /// </summary>
    public static LPCMapping GetAccountData()
    {
        // 返回账户信息
        return AccountData;
    }

    /// <summary>
    /// 设置账号数据
    /// </summary>
    /// <param name="data">Data.</param>
    public static void SetAccountData(LPCMapping data)
    {
        AccountData = data;
    }

    /// <summary>
    /// 验证账号
    /// </summary>
    public static void CheckAccount(string account, string password)
    {
        string re_account = account.Replace("@", "#");

        // 对接平台的规则（如需要改动需要咨询账户平台）
        AccountData.Add("account", re_account);

        // string serverPassword = Encrypt.Md5("qcplay#com" + re_account.ToLower() + Encrypt.Md5(password).ToLower()).ToLower();
        AccountData.Add("password", password);

        if (string.Equals(ConfigMgr.GameRunMode, ConfigMgr.MODE_LOCAL_HOST))
            AccountData.Add("extra_data", Operation.PackArgs("client", string.Empty,
                    "3rdplatform", "local_test"));
        else
            AccountData.Add("extra_data", Operation.PackArgs("client", string.Empty,
                    "3rdplatform", "qc"));

        // 记录最后登陆成功的账户密码
        OptionMgr.SetPublicOption("account", LPCValue.Create(account));
        OptionMgr.SetPublicOption("password", LPCValue.Create(password));
    }

    #endregion
}
