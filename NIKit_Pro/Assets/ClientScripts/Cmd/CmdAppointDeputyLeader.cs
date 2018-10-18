/// <summary>
/// CmdAppointDeputyLeader.cs
/// 任命工会副会长
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdAppointDeputyLeader
    {
        public string GetName()
        {
            return "cmd_appoint_deputy_leader";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go(string targetRid, bool status)
        {
            // 通知服务器任命工会副会长
            Communicate.Send2GS("CMD_APPOINT_DEPUTY_LEADER", PackArgs(
                "target_rid", targetRid,
                "status", status ? 1 : 0
            ));

            return true;
        }
    }
}
