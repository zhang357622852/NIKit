using System.Collections;
using LPC;

/// <summary>
/// 通知转接服务器
/// </summary>
public class MsgSwitchServer : MsgHandler
{
    public string GetName()
    {
        return "msg_switch_server";
    }

    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
        LogMgr.Trace("通知转接服务器：" + para.ToString());
        Switch(para);
    }

    /// <summary>
    /// 换gs
    /// </summary>
    public void Switch(LPCValue para)
    {
        Coroutine.DispatchService(_Switch(para));
    }

    /// <summary>
    /// _Switch
    /// </summary>
    IEnumerator _Switch(LPCValue para)
    {
        string account = Communicate.AccountInfo.Query("account", string.Empty);
        string ip = para.AsMapping["ip"].AsString;
        int port = para.AsMapping["port"].AsInt;
        LogMgr.Trace("转移帐号({0})的连接至：{1}:{2} cookie={3}", account, para.AsMapping["ip"],
            para.AsMapping["port"], para.AsMapping["cookie"]);

        LogMgr.Trace("转接中，先单独切断和原服务器的连接");
        Communicate.GSConnector.DisconnectFromServer();
        yield return TimeMgr.WaitForRealSeconds(0.1f);

        LogMgr.Trace("连接到新的服务器");
        Communicate.GSConnector.WaitMsgArrival("connect_to_gs", 10f,
            new CallBack(OnConnect, para),
            new CallBack(OnConnect, para));

        // 连接GS
        Communicate.Connect2GS(ip, port);
    }

    /// <summary>
    /// 连接回调
    /// </summary>
    private static void OnConnect(object loginInfo, object[] para)
    {
        LPCMapping m = (loginInfo as LPCValue).AsMapping;
        if (Communicate.IsConnectedGS())
        {
            // 连接成功了
            LogMgr.Trace("SwitchServerCtr", "连接到GS成功了。");

            // 发送AUTH请求
            LPCValue v = Operation.PackArgs("server_type", CfgType.CFG_TYPE_CONSOLE,
                             "server_name", "unknown",
                             "request_service", CfgType.CFG_SERVICE_GS,
                             "cookie", new System.Random().Next(1, 10000));

            // 发送认证消息
            Communicate.Send2GS("CMD_INTERNAL_AUTH", v);

            // 连接成功以后发送COOKIE
            LogMgr.Trace("身份说明完毕后发生COOKIE，说明是中继连接");
            v = Operation.PackArgs("account", Communicate.AccountInfo.Query("account", string.Empty),
                "domain", m["domain"].AsString,
                "thread", m["thread"].AsInt,
                "cookie", m["cookie"].AsInt,
                "auth_key", Communicate.AccountInfo.Query("auth_key", 0),
                "seed", Communicate.AccountInfo.Query("seed", 0),
                "extra_info", m["extra_info"]);

            // CMD_RELAY_TO_SERVER
            Communicate.Send2GS("CMD_RELAY_TO_SERVER", v);

            // 等待应答结果
            Communicate.GSConnector.RemoveWaitMsg("MSG_RELAY_TO_SERVER");
            Communicate.GSConnector.WaitMsgArrival("MSG_RELAY_TO_SERVER", 15f, null, null);
        }
        else
        {
            // 连接失败了
            DialogMgr.Notify("转接服务器失败。");
        }
    }
}
