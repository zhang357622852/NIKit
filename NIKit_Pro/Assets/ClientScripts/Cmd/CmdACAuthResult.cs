using System;
using System.Collections.Generic;
using LPC;

public partial class Operation
{
    public class CmdACAuthResult : CmdHandler
    {
        public string GetName()
        {
            return "cmd_ac_auth_result";
        }

        // 通知服务器验证结果
        // 指令包格式: string combat_rid, bool result
        public static bool Go(string combat_rid, bool result)
        {
            LPCValue m = PackArgs("combat_rid", combat_rid, "result", (result ? 1 : 0));
            Communicate.Send2GS("CMD_AC_AUTH_RESULT", m);
            return true;
        }
    }
}
