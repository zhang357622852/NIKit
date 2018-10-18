using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class Operation
{
    public class CmdReceiveManualBonus : CmdHandler
    {
        public string GetName()
        {
            return "cmd_receive_manual_bonus";
        }

        public static bool Go(int classId, int rank)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 向服务器发出请求
            Communicate.Send2GS("CMD_RECEIVE_MANUAL_BONUS", PackArgs("class_id", classId, "rank", rank));
            return true;
        }
    }
}
