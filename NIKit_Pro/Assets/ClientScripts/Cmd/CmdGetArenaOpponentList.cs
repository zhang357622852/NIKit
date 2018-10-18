/// <summary>
/// CmdGetArenaOpponentList.cs
/// Created by fengsc 2016/09/26
/// 请求服务器获取竞技场排位对战列表
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public partial class Operation
{
    public class CmdGetArenaOpponentList : CmdHandler
    {
        public string GetName()
        {
            return "cmd_get_arena_opponent_list";
        }

        public static bool Go(int type)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            LPCValue args = PackArgs("type", type);
            Communicate.Send2GS("CMD_GET_ARENA_OPPONENT_LIST", args);
            return true;
        }
    }
}
