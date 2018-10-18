/// <summary>
/// CmdSetGangFlag.cs
/// 设置公会旗帜
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdSetGangFlag
    {
        public string GetName()
        {
            return "cmd_set_gang_flag";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go(LPCArray flag)
        {
            // 通知服务器设置公会旗帜
            Communicate.Send2GS("CMD_SET_GANG_FLAG", PackArgs("flag", flag));

            return true;
        }
    }
}
