using System;
using LPC;

public partial class Operation
{
    public class CmdExpressRead : CmdHandler
    {
        public string GetName()
        {
            return "cmd_express_read";
        }

        // 移除一条临时仓库记录
        public static bool Go(string express_rid)
        {
            // 发送请求给服务器
            Communicate.Send2GS("CMD_EXPRESS_READ", PackArgs(
                    "express_rid", express_rid));
            return true;
        }
    }
}
