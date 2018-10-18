using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// 创建角色指令
    /// </summary>
    public class CmdCreateNewChar : CmdHandler
    {
        public string GetName()
        {
            return "cmd_create_new_char";
        }

        /// <summary>
        /// Go the specified name, gender and extraInfo.
        /// </summary>
        /// <param name="name">Name.</param>
        /// <param name="gender">Gender.</param>
        /// <param name="extraInfo">Extra info.</param>
        public static bool Go(string name, int gender, LPCValue extraInfo)
        {
            // 如果有连接到gs，则向服务器发送创建角色消息
            if (Communicate.IsConnectedGS())
            {
                // 通知服务器创建新角色
                Communicate.Send2GS("CMD_CREATE_NEW_CHAR",
                    PackArgs("char_name", name,
                        "gender", gender,
                        "extra_info", extraInfo
                    ));

                // 等待应答结果
                Communicate.GSConnector.RemoveWaitMsg("MSG_CREATE_NEW_CHAR_RESULT");
                Communicate.GSConnector.WaitMsgArrival("MSG_CREATE_NEW_CHAR_RESULT", 10f, null, new CallBack(OnCreateTimeout));
            }
            else
            {
                // 连接失败了
                LogMgr.Trace("[CmdCreateNewChar.cs] GS已经连接断开，创建角色失败。");

                // 网络已经断开，创建角色失败
                DialogMgr.Notify(LocalizationMgr.Get("LoginMgr_11"));

                // 创建角色失败
                EventMgr.FireEvent(EventMgrEventType.EVENT_LOGIN_FAILED, MixedValue.NewMixedValue<bool>(false));
            }

            // 返回成功
            return true;
        }

        // 登录超时的处理
        private static void OnCreateTimeout(object data, object[] para)
        {
            // 关闭连接
            // Alart.Show(L.CmdLogin_3);
            Communicate.Disconnect();

            // 创建角色失败，等待超时请确认网络
            DialogMgr.Notify(LocalizationMgr.Get("LoginMgr_11"));

            // 返回登陆场景
            EventMgr.FireEvent(EventMgrEventType.EVENT_LOGIN_FAILED, MixedValue.NewMixedValue<bool>(false));
        }
    }
}