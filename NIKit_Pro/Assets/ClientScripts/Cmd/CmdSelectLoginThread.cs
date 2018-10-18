using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// 选线指令
    /// </summary>
    public class CmdSelectLoginThread : CmdHandler
    {
        public string GetName()
        {
            return "cmd_select_login_thread";
        }

        public static bool Go(int thread)
        {
            Communicate.CurrInfo["thread"] = thread;
            Communicate.Send2GS("CMD_SELECT_LOGIN_THREAD", PackArgs("thread", thread));
            return true;
        }
    }
}