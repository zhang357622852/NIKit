/// <summary>
/// AccountMgr.cs
/// Create by zhaozy 2014-11-3
/// 维护管理模块
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using LPC;

/// <summary>
/// AccountMgr账户管理模块
/// </summary>
public static class MaintainMgr
{
    #region 变量

    /// <summary>
    /// 维护方式
    /// </summary>
    public const int MAINTAIN_TYPE_UPDATE   = 0;
    public const int MAINTAIN_TYPE_SHUTDOWN = 1;

    #endregion

    #region 内部接口

    #endregion

    #region 公共接口

    /// <summary>
    /// 模块初始化
    /// </summary>
    public static void Init()
    {
    }

    /// <summary>
    /// 提示维护
    /// </summary>
    /// <param name="data">Data.</param>
    public static void NoticePrepareMaintain(int leftTime)
    {
        // 给出系统提示
        LPCValue notifyTip = LPCValue.Create(string.Format(LocalizationMgr.Get("MaintainMgr_1"),
            TimeMgr.ConvertTimeToChineseIgnoreZero(leftTime)));
        DialogMgr.Notify(LocalizationMgr.GetServerDesc(notifyTip));

        // 2.显示聊天系统消息
        LPCArray messageArray = LPCArray.Empty;
        messageArray.Add(LPCSaveString.SaveToString(notifyTip));
        LPCMapping data = new LPCMapping();
        data.Add("name", LocalizationMgr.Get("ChatWnd_18"));
        data.Add("message", messageArray);
        data.Add("chat_type", ChatConfig.SYSTEM_CHAT);
        data.Add("type", ChatConfig.WORLD_CHANNEL);
        data.Add("rid", "system");

        // 模拟服务器下发消息
        LPCMapping msgArgs = new LPCMapping();
        msgArgs.Add("type", ChatConfig.WORLD_CHANNEL);
        msgArgs.Add("message_list", new LPCArray(data));

        // 模拟服务器下发MSG_CHAT_MESSAGE消息
        MsgMgr.Execute("MSG_CHAT_MESSAGE", LPCValue.Create(msgArgs));
    }

    /// <summary>
    /// 开始维护
    /// </summary>
    /// <param name="data">Data.</param>
    public static void StartMaintain(int maintainType)
    {
        // 清空所有等待消息
        VerifyCmdMgr.DoResetAll();

        // 断开原来的连接;
        Communicate.Disconnect();

        // 网络恢复移除网络等待窗口
        WaitWndMgr.RemoveWaitWnd("network", WaitWndType.WAITWND_TYPE_NETWORK);

        // 如果是客户端更新不停服维护
        if (maintainType == MAINTAIN_TYPE_UPDATE)
        {
            // 停服维护
            DialogMgr.ShowSingleBtnDailog(new CallBack((para, obj) =>
            {
                QuitGame();
                }), LocalizationMgr.Get("MaintainMgr_7"), LocalizationMgr.Get("MaintainMgr_6"), string.Empty, false);

            // 返回
            return;
        }

        // 停服维护
        DialogMgr.ShowSingleBtnDailog(new CallBack((para, obj) =>
        {
            QuitGame();
        }), LocalizationMgr.Get("MaintainMgr_3"), LocalizationMgr.Get("MaintainMgr_2"), string.Empty, false);
    }

    /// <summary>
    /// 离开游戏
    /// </summary>
    public static void QuitGame()
    {
        // 如果角色不在游戏中不处理
        if (! ME.isInGame)
            return;

        //加载登录界面;
        LoginMgr.ExitGame();
    }

    #endregion
}
