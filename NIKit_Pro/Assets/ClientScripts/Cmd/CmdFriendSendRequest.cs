/// <summary>
/// CmdFriendSendRequest.cs
/// Created by fengsc 2017/02/06
/// 发送好友请求信息
/// </summary>
using UnityEngine;

public partial class Operation
{
    public class CmdFriendSendRequest : CmdHandler
    {

        public string GetName()
        {
            return "cmd_friend_send_request";
        }

        // 消息入口
        public static bool Go(string target)
        {
            // 向服务器发送消息
            Communicate.Send2GS("CMD_FRIEND_SEND_REQUEST", PackArgs("target", target));
            return true;
        }
    }
}
