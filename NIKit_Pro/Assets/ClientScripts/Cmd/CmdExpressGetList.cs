using System;
using LPC;

public partial class Operation
{
    public class CmdExpressGetList : CmdHandler
    {
        public string GetName()
        {
            return "cmd_express_get_list";
        }

        // 获取临时仓库的物件列表
        public static bool Go()
        {
            // 构造一个默认顺序 order = ({ ({ "send_time", "desc" }) });
            LPCValue v = LPCValue.CreateArray();
            v.AsArray.Add(LPCValue.Create("send_time"));
            v.AsArray.Add(LPCValue.Create("desc"));

            LPCValue order = LPCValue.CreateArray();
            order.AsArray.Add(v);

            // 构造查询 cookie
            string cookie = Game.NewCookie("express_get");
            MailMgr.SetCookie(cookie);

            // 发送请求给服务器
            Communicate.Send2GS("CMD_EXPRESS_GET_LIST", PackArgs(
                    "cookie", cookie,
                    "order", order));

            // 返回成功
            return true;
        }
    }
}