using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// GD复制装备
    /// </summary>
    public class CmdAdminClone : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_clone";
        }

        public static bool Go(string class_id_name, int amount, LPCValue para)
        {
            Communicate.Send2GS("CMD_ADMIN_CLONE", PackArgs(
                    "class_id_name", class_id_name,
                    "amount", amount,
                    "init_args", para));
            return true;
        }
    }
}