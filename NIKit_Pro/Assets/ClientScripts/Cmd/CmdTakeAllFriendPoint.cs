using System;
using LPC;

public partial class Operation
{
    public class CmdTakeAllFriendPoint : CmdHandler
    {
        public string GetName()
        {
            return "cmd_take_all_friend_point";
        }

        // 则将邮件中的所有物品提取
        public static bool Go()
        {
            // 发送请求给服务器
            Communicate.Send2GS("CMD_TAKE_ALL_FRIEND_POINT", PackArgs());
            return true;
        }
    }
}
