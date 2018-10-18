/// <summary>
/// CmdFriendRequestAgree.cs
/// Created by fengsc 2017/02/07
/// 同意添加好友
/// </summary>
using UnityEngine;

public partial class Operation
{
    public class CmdFriendRequestAgree : CmdHandler
    {
        public string GetName()
        {
            return "cmd_friend_request_agree";
        }

        // 消息入口
        public static bool Go(string target)
        {
            // 向服务器发送消息
            Communicate.Send2GS("CMD_FRIEND_REQUEST_AGREE", PackArgs("target", target));
            return true;
        }
    }
}
