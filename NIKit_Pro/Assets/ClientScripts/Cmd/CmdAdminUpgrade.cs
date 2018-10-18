/// <summary>
/// CmdAdminUpgrade.cs
/// Created by fengsc 2017/05/25
/// GM功能玩家升级
/// </summary>
using LPC;


public partial class Operation
{
    public class CmdAdminUpgrade : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_upgrade";
        }

        public static bool Go(int cost_exp)
        {
            Communicate.Send2GS("CMD_ADMIN_UPGRADE", PackArgs("cost_exp", cost_exp));
            return true;
        }
    }
}
