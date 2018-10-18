/// <summary>
/// CmdApply.cs
/// 使用道具消息
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdApply
    {
        public string GetName()
        {
            return "cmd_apply";
        }

        public static bool Go(LPCMapping items, string targetRid)
        {
            // 通知服务使用道具
            Communicate.Send2GS("CMD_APPLY", PackArgs("items", items, "target_rid", targetRid));
            return true;
        }
    }
}
