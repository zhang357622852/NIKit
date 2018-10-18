using System;
using LPC;

public partial class Operation
{
    public class CmdSetShortcuts : CmdHandler
    {
        public string GetName()
        {
            return "cmd_set_shortcuts";
        }

        // 消息入口
        public static bool Go(string rid, LPCMapping shortcuts)
        {
            // 向服务器发送消息
            Communicate.Send2GS("CMD_SET_SHORTCUTS", PackArgs("rid", rid, "shortcuts", shortcuts));
            return true;
        }
    }
}
