using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// 添加邀请人
    /// </summary>
    public class CmdAddInvite : CmdHandler
    {
        public string GetName()
        {
            return "cmd_add_invite";
        }

        public static bool Go(string rid)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 发送消息
            Communicate.Send2GS("CMD_ADD_INVITE", PackArgs("rid", rid));
            return true;
        }
    }
}
