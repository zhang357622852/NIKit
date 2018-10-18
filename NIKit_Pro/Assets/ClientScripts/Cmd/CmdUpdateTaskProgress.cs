using System;
using LPC;

public partial class Operation
{
    public class CmdUpdateTaskProgress : CmdHandler
    {
        public string GetName()
        {
            return "cmd_update_task_progress";
        }

        // 通知服务器更新任务进度
        public static bool Go(LPCMapping progressMap)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (! ME.isInGame || ME.isLogouting)
                return false;

            // 通知服务器更新任务进度
            Communicate.Send2GS("CMD_UPDATE_TASK_PROGRESS", PackArgs("progress_map", progressMap));
            return true;
        }
    }
}
