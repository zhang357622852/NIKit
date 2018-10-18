/// <summary>
/// CmdCreateGang.cs
/// 退出公会
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdLeftGang
    {
        public string GetName()
        {
            return "cmd_left_gang";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go()
        {
            // 通知服务器退出公会
            Communicate.Send2GS("CMD_LEFT_GANG", PackArgs());

            return true;
        }
    }
}
