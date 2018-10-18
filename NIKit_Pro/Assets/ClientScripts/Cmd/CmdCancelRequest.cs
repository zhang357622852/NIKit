/// <summary>
/// CmdCancelRequest.cs
/// Created by fengsc 2017/02/08
/// 取消好友申请
/// </summary>
using UnityEngine;

public partial class Operation
{
    public class CmdCancelRequest : CmdHandler
    {
        public string GetName()
        {
            return "cmd_cancel_request";
        }

        // 消息入口
        public static bool Go(string target)
        {
            // 向服务器发送消息
            Communicate.Send2GS("CMD_CANCEL_REQUEST", PackArgs("target", target));
            return true;
        }
    }
}