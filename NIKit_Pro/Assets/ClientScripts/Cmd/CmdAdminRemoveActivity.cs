/// <summary>
/// CmdAdminRemoveActivity.cs
/// Created by fengsc 2017/03/21
/// GM 移除活动功能
/// </summary>
using LPC;

public partial class Operation
{
    public class CmdAdminRemoveActivity : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_remove_activity";
        }

        public static bool Go(string activity_cookie)
        {
            Communicate.Send2GS("CMD_ADMIN_REMOVE_ACTIVITY", PackArgs("activity_cookie", activity_cookie));
            return true;
        }
    }
}
