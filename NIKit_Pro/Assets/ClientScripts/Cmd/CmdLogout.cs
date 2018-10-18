using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// 重新登录
    /// </summary>
    public class CmdLogout : CmdHandler
    {
        public string GetName()
        {
            return "cmd_logout";
        }

        public static bool Go()
        {
            Communicate.Send2GS("CMD_LOGOUT", PackArgs());
            return true;
        }
    }
}
