using System;
using System.Diagnostics;
using LPC;

public partial class Operation
{
    public class CmdCCPrepareShutdown : CmdHandler
    {
        public string GetName()
        {
            return "cmd_ac_prepare_shutdown";
        }

        // 消息入口
        public static bool Go()
        {
            // 通知服务器准备关闭
            return Communicate.Send2GS("CMD_AC_PREPARE_SHUTDOWN", PackArgs());
        }
    }
}
