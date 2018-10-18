/// <summary>
/// CmdAdminExpressSend.cs
/// Created by cql 2015/04/22
/// 屏蔽上排行榜
/// </summary>

using LPC;
using System.Collections;

public partial class Operation
{
    public class CmdAdminExpressSend : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_express_send";
        }

        /// <summary>
        /// 通知服务器
        /// </summary>
        /// <param name="addressee">Addressee.</param>
        /// <param name="expire">失效时间</param>
        /// <param name="title">标题</param>
        /// <param name="body">邮件内容（message）</param>
        /// <param name="items">附件列表</param>
        public static bool Go(string addressee, int expire, string title, string body, LPCArray items)
        {
            Communicate.Send2GS("CMD_ADMIN_EXPRESS_SEND",
                PackArgs("addressee", addressee, "expire", expire, "express_items", items, "title", title, "body", body));
            return true;
        }
    }
}
