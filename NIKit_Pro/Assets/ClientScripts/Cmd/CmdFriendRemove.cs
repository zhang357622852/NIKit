/// <summary>
/// CmdFriendRemove.cs
/// Created by fengsc 2017/02/07
/// 删除好友
/// </summary>
using UnityEngine;

public partial class Operation
{
    public class CmdFriendRemove : CmdHandler
    {
        public string GetName()
        {
            return "cmd_friend_remove";
        }

        // 消息入口
        public static bool Go(string target)
        {
            // 向服务器发送消息
            Communicate.Send2GS("CMD_FRIEND_REMOVE", PackArgs("target", target));
            return true;
        }
    }
}
