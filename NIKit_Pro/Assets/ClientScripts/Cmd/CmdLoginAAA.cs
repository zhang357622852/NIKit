using System;
using LPC;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using LitJson;

public partial class Operation
{
    /// <summary>
    /// 登录AAA指令
    /// </summary>
    public class CmdLoginAAA : CmdHandler
    {
        public string GetName()
        {
            return "cmd_auth_account";
        }

        public static bool Go(string account, string password, string ip, int port, string version, LPCValue extra)
        {
            Coroutine.DispatchService(_Login(account, password, ip, port, version, extra));
            return true;
        }

        private static IEnumerator _Login(string account, string password, string ip, int port, string version, LPCValue extra)
        {
            // 检测帐号是否合法
#if UNITY_ANDROID || UNITY_IPHONE
            string device = UnityEngine.SystemInfo.deviceModel;
            device = device.Replace(" ", "_");
            extra.AsMapping["device"] = LPCValue.Create(device);
#endif
            LogMgr.Trace("帐号({0}/{4}登录AAA服务器({1}:{2})，额外信息：{3}", account, ip, port, extra.ToString(), password);

            // 连接目标服务器
            if (Communicate.IsConnectedAAA())
                yield break;

            // 先将旧的数据清除掉
            // ME.user = null;
            Communicate.AccountInfo.Clear();

            // 等待连接成功的消息
            LPCValue loginInfo = PackArgs("account", account,
                                     "password", password, "version", version, "extra", extra, "ip", ip, "port", port);

            // 设置AAA连接等到回调
            Communicate.AAAConnector.WaitMsgArrival("connect_to_aaa", 10f,
                new CallBack(OnConnect, loginInfo),
                new CallBack(OnConnect, loginInfo));

            // 连接到AAA
            Communicate.Connect2AAA(ip, port);
        }

        // 连接成功/失败后的处理
        private static void OnConnect(object loginInfo, object[] para)
        {
            LPCMapping m = (loginInfo as LPCValue).AsMapping;

            if (Communicate.IsConnectedAAA())
            {
                // 连接成功了
                LogMgr.Trace("[CmdLoginAAA.cs] 连接到AAA成功了。");

                // 发送AUTH请求
                LPCValue v = PackArgs("server_type", CfgType.CFG_TYPE_CONSOLE,
                                 "server_name", m["account"],
                                 "request_service", CfgType.CFG_AAA_LOGIN,
                                 "cookie", new System.Random().Next(1, 10000));
                Communicate.Send2AAA("CMD_INTERNAL_AUTH", v);

                // 发送登录命令
                LogMgr.Trace("[CmdLoginAAA.cs] 发送登录消息：{0}", v.ToString());
                Communicate.AAAConnector.RemoveWaitMsg("MSG_AUTH_ACCOUNT_RESULT");

                v = PackArgs("account", m["account"],
                    "password", m["password"],
                    "version", m["version"],
                    "extra_para", m["extra"],
                    "client_data", "");
                Communicate.Send2AAA("CMD_AUTH_ACCOUNT", v);

                // 等待验证结果
                Communicate.AAAConnector.WaitMsgArrival("MSG_AUTH_ACCOUNT_RESULT",
                    10f,
                    new CallBack(OnAuthSucc, loginInfo),
                    new CallBack(OnAuthFail, loginInfo));
            }
            else
            {
                DialogMgr.Notify(LocalizationMgr.Get("LoginMgr_9"));

                // 标识账号是否已经验证
                AccountMgr.Ischecked = false;

                // 抛出网络链接失败
                EventMgr.FireEvent(EventMgrEventType.EVENT_LOGIN_FAILED, MixedValue.NewMixedValue<bool>(false));
            }
        }

        // 帐号验证成功了
        private static void OnAuthSucc(object loginInfo, object[] para)
        {
            Dbase data = Communicate.AccountInfo;
            if (data.Query("account_result") == null ||
                data.Query("account_result").AsInt != 1)
            {
                // 验证失败了
                Communicate.Disconnect();
                return;
            }

            // 认证通过了，记录帐号的信息
            LPCMapping m = (loginInfo as LPCValue).AsMapping;
            LPCValue v = LPCValue.CreateMapping();

            // 如果已经记录有数据了不能覆盖原来数据
            if (data.Query("account") != null)
                v = PackArgs("aaa_ip", m["ip"], "aaa_port", m["port"], "password", m["password"]);
            else
                v = PackArgs("account", m["account"], "aaa_ip", m["ip"], "aaa_port", m["port"], "password", m["password"]);

            // 记录数据
            data.Absorb(v.AsMapping);

            // 获取服务器列表
            LogMgr.Trace("[CmdLoginAAA.cs] 请求服务器列表。");
            Communicate.Send2AAA("CMD_L_GET_SERVER_LIST", PackArgs());

            // 标识账号是否已经验证
            AccountMgr.Ischecked = true;
        }

        // 帐号验证失败了(超时)
        private static void OnAuthFail(object loginInfo, object[] para)
        {
            // 关闭连接
            Communicate.Disconnect();

            // 给出提示信息
            DialogMgr.Notify(LocalizationMgr.Get("LoginMgr_10"));

            // 标识账号是否已经验证
            AccountMgr.Ischecked = false;

            // 抛出网络链接失败
            EventMgr.FireEvent(EventMgrEventType.EVENT_LOGIN_FAILED, MixedValue.NewMixedValue<bool>(false));
        }
    }
}
