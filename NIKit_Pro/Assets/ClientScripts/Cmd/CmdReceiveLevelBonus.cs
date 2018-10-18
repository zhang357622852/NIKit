/// <summary>
/// Created by cql 2015/04/27
/// 签到
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdReceiveLevelBonus : CmdHandler
    {
        public string GetName()
        {
            return "cmd_receive_level_bonus";
        }

        public static bool Go(int level)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 向服务器发出请求
            Communicate.Send2GS("CMD_RECEIVE_LEVEL_BONUS", PackArgs("level", level));
            return true;
        }
    }
}
