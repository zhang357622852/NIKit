/// <summary>
/// VerifyCmdMgr.cs
/// Create by zhaozy 2016-12-02
/// 消息到达服务器确认管理模块
/// </summary>

using System;
using System.Diagnostics;
using System.Collections.Generic;
using LPC;

/// 道具管理器
public static class VerifyCmdMgr
{
    /// <summary>
    /// 消息确认缓存列表
    /// </summary>
    private static Dictionary<string, List<LPCMapping>> VerifyCmdMap = new Dictionary<string, List<LPCMapping>>();

    /// <summary>
    /// 等待消息时间不能操作6s否则认为延迟
    /// 由于战斗验证时间为最大时间为6s，所以这个地方最大等待6s延迟
    /// </summary>
    private static int echoInterval = 6 * 1000;

    #region 功能接口

    #endregion

    /// <summary>
    /// 模块初始化
    /// </summary>
    public static void Init()
    {
        // 消息达到回调处理
        MsgMgr.eventMsgArrived -= OnMsgArrived;
        MsgMgr.eventMsgArrived += OnMsgArrived;

        // 关注MSG_LOGIN_NOTIFY_OK消息
        MsgMgr.RemoveDoneHook("MSG_LOGIN_NOTIFY_OK", "VerifyCmdMgr");
        MsgMgr.RegisterDoneHook("MSG_LOGIN_NOTIFY_OK", "VerifyCmdMgr", OnMsgLoginNotifyOK);
    }

    /// <summary>
    /// 清空数据
    /// </summary>
    public static void DoResetAll()
    {
        // 清除数据
        VerifyCmdMap.Clear();
    }

    /// <summary>
    /// 添加等到消息
    /// </summary>
    public static void AddVerifyCmd(string cmd, string waitMsg, LPCValue args)
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 构建消息结构
        LPCMapping data = LPCMapping.Empty;
        data.Add("cmd", cmd);
        data.Add("wait_msg", waitMsg);
        data.Add("args", args);
        data.Add("timeout", TimeMgr.RealTick + echoInterval);

        // 获取当前玩家角色的rid
        string rid = ME.GetRid();

        // 不包含该rid的key
        if (! VerifyCmdMap.ContainsKey(rid))
            VerifyCmdMap.Add(rid, new List<LPCMapping>() { data });
        else
            VerifyCmdMap[rid].Add(data);
    }

    /// <summary>
    /// 判断消息等待是否超时
    /// </summary>
    public static bool IsDelay()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return false;

        // 获取当前玩家角色的rid
        string rid = ME.GetRid();

        // 没有该角色需要等待的消息
        if (string.IsNullOrEmpty(rid) ||
            ! VerifyCmdMap.ContainsKey(rid) ||
            VerifyCmdMap[rid].Count == 0)
            return false;

        // 获取等待消息队列中的第一个消息
        LPCMapping catchCmd = VerifyCmdMap[rid][0];
        if (catchCmd == null)
            return false;

        // 返回确认等待消息数量
        return (TimeMgr.RealTick > catchCmd.GetValue<int>("timeout"));
    }

    /// <summary>
    /// 是否正在等待确认消息
    /// </summary>
    public static bool IsVerifyCmd()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return false;

        // 获取当前玩家角色的rid
        string rid = ME.GetRid();

        // 不包含该rid的key
        if (string.IsNullOrEmpty(rid) ||
            ! VerifyCmdMap.ContainsKey(rid))
            return false;

        // 返回确认等待消息数量
        return VerifyCmdMap[rid].Count > 0;
    }

    /// <summary>
    /// 是否正在等待确认消息
    /// </summary>
    public static bool IsVerifyCmd(string cmd)
    {
        // 玩家对象不存在
        if (ME.user == null)
            return false;

        // 获取当前玩家角色的rid
        string rid = ME.GetRid();

        // 不包含该rid的key
        if (! VerifyCmdMap.ContainsKey(rid))
            return false;

        // 获取等待消息队列中的是否有指定消息
        LPCMapping catchCmd;
        for (int i = 0; i < VerifyCmdMap[rid].Count; i++)
        {
            // 获取缓存消息
            catchCmd = VerifyCmdMap[rid][i];

            // 不是需要查找的消息
            if (!string.Equals(catchCmd.GetValue<string>("cmd"), cmd))
                continue;

            // 返回true
            return true;
        }

        // 返回确认等待消息数量
        return false;
    }

    #region 内部方法

    /// <summary>
    /// 确认消息回调
    /// </summary>
    private static void OnMsgArrived(string cmd)
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 获取玩家的rid
        string rid = ME.GetRid();

        // 没有该角色需要等待的消息
        if (string.IsNullOrEmpty(rid) ||
            ! VerifyCmdMap.ContainsKey(rid) ||
            VerifyCmdMap[rid].Count == 0)
            return;

        // 获取等待消息队列中的第一个消息
        LPCMapping catchCmd = VerifyCmdMap[rid][0];

        // 缓存等待确认消息和收到服务器的回执消息不一致
        if (! string.Equals(catchCmd.GetValue<string>("wait_msg"), cmd.ToUpper()))
            return;

        // 移除第一个等待消息
        VerifyCmdMap[rid].RemoveAt(0);
    }

    /// <summary>
    /// 登陆成功回调
    /// </summary>
    private static void OnMsgLoginNotifyOK(string cmd, LPCValue para)
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 获取玩家的rid
        string rid = ME.GetRid();

        // 没有该角色需要等待的消息
        if (!VerifyCmdMap.ContainsKey(rid) ||
            VerifyCmdMap[rid].Count == 0)
        {
            VerifyCmdMap.Clear();
            return;
        }

        // 遍历消息逐个重发
        foreach (LPCMapping data in VerifyCmdMap[rid])
        {
            // 向服务器发送消息
            Communicate.Send2GS(data.GetValue<string>("cmd"),
                data.GetValue<LPCValue>("args"));
        }
    }

    #endregion
}
