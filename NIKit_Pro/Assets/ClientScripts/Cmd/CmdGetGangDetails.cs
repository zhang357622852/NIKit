/// <summary>
/// CmdGetGangDetails.cs
/// 获取公会详情
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdGetGangDetails
    {
        public string GetName()
        {
            return "cmd_get_gang_details";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go(string gangName)
        {
            // 通知服务器获取公会详情
            Communicate.Send2GS("CMD_GET_GANG_DETAILS", PackArgs("gang_name",gangName));

            return true;
        }
    }
}
