using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// GD开关社交功能
    /// </summary>
    public class CmdAdminSetSocialFunction : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_set_social_function";
        }

        public static bool Go(bool is_social_on)
        {
            Communicate.Send2GS("CMD_ADMIN_SET_SOCIAL_FUNCTION", PackArgs("is_social_on", is_social_on ? 1 : 0));
            return true;
        }
    }
}