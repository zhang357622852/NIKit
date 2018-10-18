using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// GD设置版署世界
    /// </summary>
    public class CmdAdminClosePay : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_close_pay";
        }

        public static bool Go(int close_pay)
        {
            Communicate.Send2GS("CMD_ADMIN_CLOSE_PAY", PackArgs("close_pay", close_pay));
            return true;
        }
    }
}