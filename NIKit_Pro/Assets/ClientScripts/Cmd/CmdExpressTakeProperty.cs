using System;
using LPC;

public partial class Operation
{
    public class CmdExpressTakeProperty : CmdHandler
    {
        public string GetName()
        {
            return "cmd_express_take_property";
        }

        // 则将邮件中的所有物品提取
        public static bool Go(string express_rid, LPCMapping extra_para)
        {
            // 发送请求给服务器
            Communicate.Send2GS("CMD_EXPRESS_TAKE_PROPERTY", PackArgs(
                "express_rid", express_rid, "extra_para", extra_para));
            return true;
        }
    }
}
