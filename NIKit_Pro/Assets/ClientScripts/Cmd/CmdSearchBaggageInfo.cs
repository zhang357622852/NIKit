/// <summary>
/// CmdSearchBaggageInfo.cs
/// Created by fengsc 2016/12/16
/// 查询玩家包裹的宠物信息
/// </summary>
using UnityEngine;
using LPC;

public partial class Operation
{
    public class CmdSearchBaggageInfo : CmdHandler
    {
        public string GetName()
        {
            return "cmd_search_baggage_info";
        }

        // 消息入口
        public static bool Go(string target_address)
        {
            // 向服务器发送消息
            Communicate.Send2GS("CMD_SEARCH_BAGGAGE_INFO", PackArgs("target_address", target_address));
            return true;
        }
    }
}
