/// <summary>
/// CmdInviteJoinGang.cs
/// 邀请玩家加入公会
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdInviteJoinGang
    {
        public string GetName()
        {
            return "cmd_invite_join_gang";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go(string userName)
        {
            // 通知服务器邀请玩家加入公会
            Communicate.Send2GS("CMD_INVITE_JOIN_GANG", PackArgs("user_name", userName));

            return true;
        }
    }
}
