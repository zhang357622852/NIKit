/// <summary>
/// CmdAdminAddActivity.cs
/// Created by fengsc 2017/03/21
/// GM增加活动功能
/// </summary>
using LPC;

public partial class Operation
{
    public class CmdAdminAddActivity : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_add_activity";
        }

        public static bool Go(LPCMapping activity_data)
        {
            Communicate.Send2GS("CMD_ADMIN_ADD_ACTIVITY", PackArgs("activity_data", activity_data));
            return true;
        }
    }
}
