/// <summary>
/// CmdFriendReject.cs
/// Created by fengsc 2017/02/07
/// 拒绝好友请求
/// </summary>
using UnityEngine;

public partial class Operation
{
    public class CmdFriendReject : CmdHandler
    {
        public string GetName()
        {
            return "cmd_friend_reject";
        }

        // 消息入口
        public static bool Go(string target)
        {
            // 向服务器发送消息
            Communicate.Send2GS("CMD_FRIEND_REJECT", PackArgs("target", target));
            return true;
        }
    }
}