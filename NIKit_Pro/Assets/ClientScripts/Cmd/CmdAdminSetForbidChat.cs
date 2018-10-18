using System;
using LPC;

public partial class Operation
{
    public class CmdAdminSetForbidChat : CmdHandler
    {
        public string GetName()
        {
            return "cmd_admin_set_forbid_chat";
        }

        // 设置版本更新提示信息
        public static bool Go(string user_rid, int forbid_time)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 向服务器发送消息
            Communicate.Send2GS("CMD_ADMIN_SET_FORBID_CHAT", PackArgs("user_rid", user_rid, "forbid_time", forbid_time));
            return true;
        }
    }
}
