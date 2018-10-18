/// <summary>
/// CmdUnlockSpecifyMap.cs
/// Created by cql 2015/06/01
/// 解锁指定地图
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdUnlockSpecifyMap : CmdHandler
    {
        public string GetName()
        {
            return "cmd_unlock_specify_map";
        }

        public static bool Go(int level, int cost)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;
            
            Communicate.Send2GS("CMD_UNLOCK_SPECIFY_MAP", PackArgs("level", level, "cost", cost));
            return true;
        }
    }
}