/// <summary>
/// CmdGetGangInfo.cs
/// 获取公会信息
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdGetGangInfo
    {
        public string GetName()
        {
            return "cmd_get_gang_info";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go()
        {
            // 通知服务器获取公会信息
            Communicate.Send2GS("CMD_GET_GANG_INFO", PackArgs());

            return true;
        }
    }
}
