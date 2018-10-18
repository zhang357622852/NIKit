/// <summary>
/// CmdGetAllGangRequest.cs
/// 获取公会请求列表
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdGetAllGangRequest
    {
        public string GetName()
        {
            return "cmd_get_all_gang_request";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go()
        {
            // 通知服务器获取公会请求列表
            Communicate.Send2GS("CMD_GET_ALL_GANG_REQUEST", PackArgs());

            return true;
        }
    }
}
