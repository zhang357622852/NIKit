/// <summary>
/// CmdAcceptGangRequest.cs
/// 接受公会请求
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdAcceptGangRequest
    {
        public string GetName()
        {
            return "cmd_accept_gang_request";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go(string requestId)
        {
            // 通知服务器接受公会请求
            Communicate.Send2GS("CMD_ACCEPT_GANG_REQUEST", PackArgs("request_id",requestId));

            return true;
        }
    }
}
