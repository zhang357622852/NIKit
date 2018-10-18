/// <summary>
/// CmdGetTopList.cs
/// Created by cql 2015/03/25
/// 获取排行榜列表
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdGetArenaTopList : CmdHandler
    {
        public string GetName()
        {
            return "cmd_get_arena_top_list";
        }

        public static bool Go(int start_index, int end_index)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            LPCValue args = PackArgs("start_index", start_index, "end_index", end_index);
            Communicate.Send2GS("CMD_GET_ARENA_TOP_LIST", args);
            return true;
        }
    }
}
