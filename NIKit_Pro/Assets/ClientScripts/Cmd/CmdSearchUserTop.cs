/// <summary>
/// CmdSearchUserTop.cs
/// Created by cql 2015/03/26
/// 获取玩家所在位置排行榜列表
/// </summary>

using System;
using LPC;

public partial class Operation
{

    public class CmdSearchUserTop : CmdHandler
    {
        public string GetName()
        {
            return "cmd_search_user_top";
        }

        public static bool Go(string user_name, int rank_type)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            LogMgr.Trace("[CmdSearchUserTop.cs] 获取玩家所在位置排行榜列表: 玩家{0}, 类型:{1}", user_name, rank_type);
            LPCValue args = PackArgs("user_name", user_name, "rank_type", rank_type);
            Communicate.Send2GS("CMD_SEARCH_USER_TOP", args);
            return true;
        }
    }
}