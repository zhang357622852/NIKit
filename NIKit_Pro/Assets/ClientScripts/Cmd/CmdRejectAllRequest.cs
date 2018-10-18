/// <summary>
/// CmdRejectAllRequest.cs
/// Created by fengsc 2017/02/07
/// 拒绝所有好友请求
/// </summary>
using UnityEngine;

public partial class Operation
{
    public class CmdRejectAllRequest : CmdHandler
    {
        public string GetName()
        {
            return "cmd_reject_all_request";
        }

        // 消息入口
        public static bool Go()
        {
            // 向服务器发送消息
            Communicate.Send2GS("CMD_REJECT_ALL_REQUEST", PackArgs());
            return true;
        }
    }
}
