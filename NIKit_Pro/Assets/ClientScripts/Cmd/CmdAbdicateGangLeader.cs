/// <summary>
/// CmdAbdicateGangLeader.cs
/// 转让工会长
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdAbdicateGangLeader
    {
        public string GetName()
        {
            return "cmd_abdicate_gang_leader";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go(string targetRid)
        {
            // 通知服务器转让工会长
            Communicate.Send2GS("CMD_ABDICATE_GANG_LEADER", PackArgs("target_rid", targetRid));

            return true;
        }
    }
}
