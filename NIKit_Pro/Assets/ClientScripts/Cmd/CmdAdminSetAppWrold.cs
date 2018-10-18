using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// GD设置版署世界
    /// </summary>
    public class CmdAdminSetGappWrold : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_set_gapp_world";
        }

        public static bool Go(bool gapp_world)
        {
            Communicate.Send2GS("CMD_ADMIN_SET_GAPP_WORLD", PackArgs("gapp_world", gapp_world ? 1 : 0));
            return true;
        }
    }
}