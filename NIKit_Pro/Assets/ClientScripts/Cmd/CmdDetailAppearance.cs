/// <summary>
/// CmdDetailAppearance.cs
/// Created by fengxl 2015-3-25
/// 通过名字查询玩家信息
/// </summary>
using System;
using LPC;

public partial class Operation
{
    public class CmdDetailAppearance:CmdHandler
    {
        public string GetName()
        {
            return "cmd_detail_appearance";
        }

        /// <summary>
        /// 向服务端查询玩家信息
        /// </summary>
        /// <param name="target_address">所查询的玩家域名</param>
        public static bool Go(string target_address, string target = "")
        {
            LogMgr.Trace("[CmdDetailAppearance.cs] 查询玩家");
            Communicate.Send2GS("CMD_DETAIL_APPEARANCE", PackArgs("target_address", target_address, "target", target));
            return true;
        }
    }
}
