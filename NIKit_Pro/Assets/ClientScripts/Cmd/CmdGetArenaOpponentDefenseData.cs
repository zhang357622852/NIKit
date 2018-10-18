/// <summary>
/// CmdGetOpponentDefenseData.cs
/// Created by zhaozy 2016/10/11
/// 获取对战角色的详细信息
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdGetArenaOpponentDefenseData : CmdHandler
    {
        public string GetName()
        {
            return "cmd_get_arena_opponent_defense_data";
        }

        public static bool Go(string opponent_id, bool is_revenge = false)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            Communicate.Send2GS("CMD_GET_ARENA_OPPONENT_DEFENSE_DATA", PackArgs("opponent_id", opponent_id, "is_revenge", is_revenge ? 1:0));
            return true;
        }
    }
}
