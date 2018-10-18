/// <summary>
/// CmdAdminBlockTop.cs
/// Created by cql 2015/04/13
/// 屏蔽上排行榜
/// </summary>

using LPC;
using System.Collections;

public partial class Operation
{
    public class CmdAdminBlockTop : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_block_top";
        }

        public static bool Go(string user_rid, int block, string reason)
        {
            Communicate.Send2GS("CMD_ADMIN_BLOCK_TOP",
                PackArgs("user_rid", user_rid, "block", block, "reason", reason));
            return true;
        }
    }
}
