using System;
using System.Diagnostics;
using LPC;

public partial class Operation
{
    public class CmdACLogin : CmdHandler
    {
        public string GetName()
        {
            return "cmd_ac_login";
        }

        // 验证客户端发起登陆
        public static bool Go(string owner, string ip, int port, string cookie)
        {
            // 没有服务器的ip 和 port 不能登录
            if (ip == string.Empty || port == 0)
            {
                LogMgr.Trace("没有服务器的ip 和 port 不能登录");
                return false;
            }

            // 连接GS服务器
            if (Communicate.IsConnectedGS())
            {
                LogMgr.Trace("已经登录服务器");
                return false;
            }

            LogMgr.Trace("登录战斗验证服务器 ip = {0}, port = {1}, cookie = {2}, owner = {3}", ip, port, cookie, owner);

            // 构建登陆参数
            LPCValue loginInfo = PackArgs(
                "owner", owner,
                "cookie", cookie);

            Communicate.GSConnector.RemoveWaitMsg("connect_to_gs");
            Communicate.GSConnector.WaitMsgArrival("connect_to_gs", 10f,
                new CallBack(OnConnect, loginInfo),
                new CallBack(OnConnect, loginInfo));
            Communicate.Connect2GS(ip, port);

            return true;
        }

        // 连接成功/失败后的处理
        private static void OnConnect(object loginInfo, object[] para)
        {
            // 转换数据格式
            LPCMapping m = (loginInfo as LPCValue).AsMapping;

            if (Communicate.IsConnectedGS())
            {
                // 连接成功了
                LogMgr.Trace("连接到GS成功了。");

                // 发送AUTH请求
                LPCValue v = PackArgs(
                    "server_type", CfgType.CFG_TYPE_AUTH_CLIENT,
                    "server_name", "unknown",
                    "request_service", CfgType.CFG_AUTH_CLIENT,
                    "cookie", new System.Random().Next(1, 10000));
                Communicate.Send2GS("CMD_INTERNAL_AUTH", v);

                LogMgr.Trace("发送验证消息：{0}", v.GetDescription());

                // 发送标示
                Process process = Process.GetCurrentProcess();

                // 构建消息参数
                v = PackArgs("pid", process.Id, "cookie", m["cookie"], "owner", m["owner"]);
                Communicate.Send2GS("CMD_AC_LOGIN", v);

                LogMgr.Trace("发送身份信息：{0}", v.GetDescription());
            }
        }
    }
}
