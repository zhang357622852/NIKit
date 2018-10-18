using System;
using LPC;

public partial class Operation
{
    public class CmdChatSay : CmdHandler
    {
        public string GetName()
        {
            return "cmd_chat_say";
        }

        // 消息入口
        public static bool Go(string type, string to_rid, LPCArray message)
        {
            // 向服务器发送消息
            Communicate.Send2GS("CMD_CHAT_SAY", PackArgs("type", type, "to_rid", to_rid, "message", message));
            return true;
        }
    }
}
