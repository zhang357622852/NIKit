/// <summary>
/// CmdSetGangInformation.cs
/// 设置公会信息
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdSetGangInformation
    {
        public string GetName()
        {
            return "cmd_set_gang_information";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go(LPCMapping gangInfo)
        {
            // 通知服务器设置公会信息
            Communicate.Send2GS("CMD_SET_GANG_INFORMATION", PackArgs("gang_info", gangInfo));

            return true;
        }
    }
}
