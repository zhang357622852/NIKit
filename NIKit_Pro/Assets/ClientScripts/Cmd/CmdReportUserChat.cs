using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// 举报玩家聊天信息
    /// </summary>
    public class CmdReportUserChat : CmdHandler
    {
        public string GetName()
        {
            return "cmd_report_user_chat";
        }

        public static bool Go(string userName, string chatId)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 发送消息
            Communicate.Send2GS("CMD_REPORT_USER_CHAT", PackArgs("user_name", userName, "chat_id", chatId));
            return true;
        }
    }
}
