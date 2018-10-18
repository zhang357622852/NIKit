/// <summary>
/// CmdCancelGangRequest.cs
/// 取消公会请求
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdCancelGangRequest
    {
        public string GetName()
        {
            return "cmd_cancel_gang_request";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go(string requestId)
        {
            // 通知服务器取消公会请求
            Communicate.Send2GS("CMD_CANCEL_GANG_REQUEST", PackArgs("request_id", requestId));

            return true;
        }
    }
}
