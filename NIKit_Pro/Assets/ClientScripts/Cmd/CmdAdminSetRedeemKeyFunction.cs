using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// GD开关兑换码功能
    /// </summary>
    public class CmdAdminSetRedeemKeyFunction : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_set_redeem_key_function";
        }

        public static bool Go(bool is_redeem_key_on)
        {
            Communicate.Send2GS("CMD_ADMIN_SET_REDEEM_KEY_FUNCTION", PackArgs("is_redeem_key_on", is_redeem_key_on ? 1 : 0));
            return true;
        }
    }
}