/// <summary>
/// CmdDismissGang.cs
/// 解散公会
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdDismissGang
    {
        public string GetName()
        {
            return "cmd_dismiss_gang";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go()
        {
            // 通知服务器退出公会
            Communicate.Send2GS("CMD_DISMISS_GANG", PackArgs());

            return true;
        }
    }
}
