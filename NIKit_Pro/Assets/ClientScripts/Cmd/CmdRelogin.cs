using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// 重新登录
    /// </summary>
    public class CmdRelogin : CmdHandler
    {
        public string GetName()
        {
            return "cmd_relogin";
        }

        public static bool Go()
        {
            Communicate.Send2GS("CMD_RELOGIN", PackArgs());
            return true;
        }
    }
}