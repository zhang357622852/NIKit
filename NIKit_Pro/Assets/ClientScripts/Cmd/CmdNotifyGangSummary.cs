/// <summary>
/// CmdNotifyGangSummary.cs
/// 获取公会列表
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdNotifyGangSummary
    {
        public string GetName()
        {
            return "cmd_notify_gang_summary";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        public static bool Go(int step, int checkFlag, int startIndex)
        {
            // 通知服务器获取公会信息
            Communicate.Send2GS("CMD_NOTIFY_GANG_SUMMARY", PackArgs(
                "step", step,
                "check_flag", checkFlag,
                "start_index", startIndex
            ));

            return true;
        }
    }
}
