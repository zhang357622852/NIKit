/// <summary>
/// CmdSendGangSlogan.cs
/// 发送公会宣传
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdSendGangSlogan
    {
        public string GetName()
        {
            return "cmd_send_gang_slogan";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go(LPCArray message)
        {
            // 通知服务器发送公会宣传
            Communicate.Send2GS("CMD_SEND_GANG_SLOGAN", PackArgs("message", message));

            return true;
        }
    }
}
