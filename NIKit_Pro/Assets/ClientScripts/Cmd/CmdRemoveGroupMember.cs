/// <summary>
/// CmdRemoveGroupMember.cs
/// 踢出公会成员
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdRemoveGroupMember
    {
        public string GetName()
        {
            return "cmd_remove_gang_member";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go(string targetRid)
        {
            // 通知服务器踢出公会成员
            Communicate.Send2GS("CMD_REMOVE_GANG_MEMBER", PackArgs("target_rid", targetRid));

            return true;
        }
    }
}
