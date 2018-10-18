using System;
using LPC;

public partial class Operation
{
    public class CmdReceiveBonus : CmdHandler
    {
        public string GetName()
        {
            return "cmd_receive_bonus";
        }

        /// <summary>
        /// 商店刷新
        /// </summary>
        public static bool Go(int task_id)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;
            
            Communicate.Send2GS("CMD_RECEIVE_BONUS", PackArgs("task_id", task_id));
            return true;
        }
    }
}
