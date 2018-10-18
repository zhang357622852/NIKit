using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// GD清除包裹
    /// </summary>
    public class CmdAdminClearBaggage : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_clear_baggage";
        }

        public static bool Go(int page)
        {
            Communicate.Send2GS("CMD_ADMIN_CLEAR_BAGGAGE", PackArgs(
                    "page", page));
            return true;
        }
    }
}