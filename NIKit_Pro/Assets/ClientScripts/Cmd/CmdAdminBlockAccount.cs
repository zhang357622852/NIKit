using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// GD复制装备
    /// </summary>
    public class CmdAdminBlockAccount : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_block_account";
        }

        public static bool Go(string account, int block, string reason)
        {
            Communicate.Send2GS("CMD_ADMIN_BLOCK_ACCOUNT",
                PackArgs("account", account, "block", block, "reason", reason, "last_time", -16));
            return true;
        }
    }
}