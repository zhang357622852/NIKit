/// <summary>
/// CmdGetTowerTopList.cs
/// Created by lic 2017/08/31
/// 获取通天塔排行榜列表
/// </summary>

using LPC;
using System;

public partial class Operation
{
    public class CmdGetTowerTopList : CmdHandler
    {
        public string GetName()
        {
            return "cmd_get_tower_top_list";
        }

        public static bool Go(int difficulty, int start_index, int end_index)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            LPCValue args = PackArgs("difficulty", difficulty,"start_index", start_index, "end_index", end_index);
            Communicate.Send2GS("CMD_GET_TOWER_TOP_LIST", args);
            return true;
        }
    }
}
