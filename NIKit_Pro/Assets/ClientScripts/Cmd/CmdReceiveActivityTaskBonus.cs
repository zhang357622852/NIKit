using System;
using LPC;

public partial class Operation
{
    public class CmdReceiveActivityTaskBonus : CmdHandler
    {
        public string GetName()
        {
            return "cmd_receive_activity_task_bonus";
        }

        /// <summary>
        /// 领取活动任务奖励
        /// </summary>
        public static bool Go(string cookie, int taskId)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            Communicate.Send2GS("CMD_RECEIVE_ACTIVITY_TASK_BONUS", PackArgs("cookie", cookie, "task_id", taskId));
            return true;
        }
    }
}
