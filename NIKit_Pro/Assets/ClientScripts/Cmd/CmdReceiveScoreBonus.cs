/// <summary>
/// CmdReceiveScoreBonus.cs
/// Created by fengsc 2017/06/08
/// 领取积分奖励
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public partial class Operation
{
    public class CmdReceiveScoreBonus : CmdHandler
    {
        public string GetName()
        {
            return "cmd_receive_score_bonus";
        }

        public static bool Go(string cookie, LPCValue para)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            Communicate.Send2GS("CMD_RECEIVE_SCORE_BONUS", PackArgs("cookie", cookie, "para", para));
            return true;
        }
    }
}
