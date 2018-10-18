using System;
using LPC;
using UnityEngine;
using System.Text;

public partial class Operation
{
    /// <summary>
    /// 恢复登录 
    /// </summary>
    public class CmdRecoverLogin
    {
        /// <summary>
        /// 指令入口 
        /// </summary>
        public static bool Go()
        {
            // 如果玩家角色不在游戏中不允许恢复登陆
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 取得帐号信息
            Dbase accountInfo = Communicate.AccountInfo;

            // 取得服务器的ip和端口号
            string ip = accountInfo.Query("gs_ip", string.Empty);
            int port = accountInfo.Query("gs_port", 0);
            if (string.IsNullOrEmpty(ip) || port == 0)
            {
                LogMgr.Trace("[CmdLogin.cs] aaa服务器没有返回gs服务器的ip 和 port 不能登录");
                return false;
            }

            // 正常的登录流程，一定会有authKey和seed
            int authKey = accountInfo.Query("auth_key", 0);
            int seed = accountInfo.Query("seed", 0);

            // 等待连接成功的消息
            LPCValue loginInfo = PackArgs("rid", ME.GetRid(),
                                     "auth_key", authKey, "seed", seed);

            Communicate.GSConnector.RemoveWaitMsg("connect_to_gs");
            Communicate.GSConnector.WaitMsgArrival("connect_to_gs", 30f,
                new CallBack(OnConnect, loginInfo),
                new CallBack(OnConnect, loginInfo));

            // 连接GS服务
            Communicate.Connect2GS(ip, port);
            return true;
        }

        /// <summary>
        /// 连接成功/失败后的处理
        /// </summary>
        private static void OnConnect(object loginInfo, object[] para)
        {
            // 连接GS失败
            if (!Communicate.IsConnectedGS())
            {
                // 通知恢复登陆链接失败
                EventMgr.FireEvent(EventMgrEventType.EVENT_LOGIN_FAILED, MixedValue.NewMixedValue<bool>(false));
                return;
            }

            // 发送AUTH请求
            LPCValue v = PackArgs("server_type", CfgType.CFG_TYPE_CONSOLE,
                             "server_name", "unknown",
                             "request_service", CfgType.CFG_SERVICE_GS,
                             "cookie", new System.Random().Next(1, 10000));
            Communicate.Send2GS("CMD_INTERNAL_AUTH", v);
            LogMgr.Trace("[CmdLogin.cs] 发送验证消息：{0}", v.GetDescription());

            // 向服务器请求恢复登陆
            Communicate.Send2GS("CMD_RECOVER_LOGIN", loginInfo);
        }
    }
}
