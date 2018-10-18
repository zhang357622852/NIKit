using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// 登录到GS指令
    /// </summary>
    /// <author>weism</author>
    public class CmdLogin : CmdHandler
    {
        public string GetName()
        {
            return "cmd_login";
        }

        /// <summary>
        /// 指令入口
        /// </summary>
        /// <param name="account">帐号名</param>
        /// <param name="ip">服务器的IP，如果没有指定为快捷登录</param>
        /// <param name="port">服务器的端口号</param>
        public static bool Go(string account, string ip, int port)
        {
            // TODO: 验证帐号的合法性

            // 取得帐号信息
            Dbase accountInfo = Communicate.AccountInfo;

            // 取得区组信息
            string dist = accountInfo.Query("dist", "unknow");

            // 取得服务器名字
            string server = accountInfo.Query("server", "unknow");

            // authkey和seed
            int authKey, seed;

            // 获取登陆信息中的认证关键字
            authKey = accountInfo.Query("auth_key", 0);

            if (authKey == 0)
            {
                // 快捷登录，不会有authKey和seed
                authKey = 0;
                seed = 0;
            }
            else
            {
                // 正常的登录流程，一定会有authKey和seed
                authKey = accountInfo.Query("auth_key", 0);
                seed = accountInfo.Query("seed", 0);
                if (authKey == 0)
                {
                    LogMgr.Trace("[CmdLogin.cs] 没有取得 auth_key不能登录");
                    return false;
                }

                // 取得服务器的ip和端口号
                ip = accountInfo.Query("gs_ip", string.Empty);
                port = accountInfo.Query("gs_port", 0);
                if (string.IsNullOrEmpty(ip) || port == 0)
                {
                    LogMgr.Trace("[CmdLogin.cs] aaa服务器没有返回gs服务器的ip 和 port 不能登录");
                    return false;
                }
            }

            LogMgr.Trace("[CmdLogin.cs] 帐号({0})登录进入游戏：区组({1})/服务器({2})", account, dist, server);

            // 1. 连接GS服务器
            if (Communicate.IsConnectedGS())
                return false;

            // 等待连接成功的消息
            // ME.user = null;
            LPCValue loginInfo = PackArgs("account", account,
                                     "auth_key", authKey,
                                     "seed", seed);
            Communicate.GSConnector.WaitMsgArrival("connect_to_gs", 10f,
                new CallBack(OnConnect, loginInfo),
                new CallBack(OnConnect, loginInfo));
            Communicate.Connect2GS(ip, port);
            return true;
        }

        // 连接成功/失败后的处理
        private static void OnConnect(object loginInfo, object[] para)
        {
            LPCMapping m = (loginInfo as LPCValue).AsMapping;

            if (Communicate.IsConnectedGS())
            {
                // 连接成功了
                LogMgr.Trace("[CmdLogin.cs] 连接到GS成功了。");

                // 发送AUTH请求
                LPCValue v = PackArgs("server_type", CfgType.CFG_TYPE_CONSOLE,
                                 "server_name", "unknown",
                                 "request_service", CfgType.CFG_SERVICE_GS,
                                 "cookie", new Random().Next(1, 10000));

                Communicate.Send2GS("CMD_INTERNAL_AUTH", v);
                LogMgr.Trace("[CmdLogin.cs] 发送验证消息：{0}", v.GetDescription());

                // 发送登录命令
                LPCValue args = PackArgs("account", m["account"],
                                    "auth_key", m["auth_key"],
                                    "seed", m["seed"]);

                Communicate.Send2GS("CMD_LOGIN", args);
                LogMgr.Trace("[CmdLogin.cs] 发送登录消息：{0}", args.GetDescription());

                // 等待应答结果
                Communicate.GSConnector.RemoveWaitMsg("MSG_LOGIN");
                Communicate.GSConnector.WaitMsgArrival("MSG_LOGIN", 15f, null, new CallBack(OnLoginTimeout));
            }
            else
            {
                // 连接失败了
                LogMgr.Trace("[CmdLogin.cs] 连接GS失败了。");

                // 返回登陆场景
                EventMgr.FireEvent(EventMgrEventType.EVENT_LOGIN_FAILED, MixedValue.NewMixedValue<bool>(false));
            }
        }

        // 登录超时的处理
        private static void OnLoginTimeout(object data, object[] para)
        {
            // 关闭连接
            // Alart.Show(L.CmdLogin_3);
            Communicate.Disconnect();

            // 返回登陆场景
            EventMgr.FireEvent(EventMgrEventType.EVENT_LOGIN_FAILED, MixedValue.NewMixedValue<bool>(false));
        }
    }
}
