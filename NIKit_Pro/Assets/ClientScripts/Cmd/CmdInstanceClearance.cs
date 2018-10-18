/// <summary>
/// Created bu xuhd Jan/23/2015
/// 副本通关
/// </summary>
using System;
using LPC;

public partial class Operation
{
    public class CmdInstanceClearance : CmdHandler
    {
        public string GetName()
        {
            return "cmd_instance_clearance";
        }

        /// <summary>
        /// 消息入口
        /// </summary>
        /// <param name="rid">Rid.</param>
        /// <param name="result">If set to <c>true</c> result.</param>
        /// <param name="dropBonus">Drop bonus.</param>
        /// <param name="levelActions">Level actions.</param>
        /// <param name="aliveAmount">If set to <c>true</c> is all alive.</param>
        /// <param name="killAmount">Kill amount.</param>
        /// <param name="clearanceTime">Clearance time.</param>
        public static bool Go(string rid, bool result, LPCMapping dropBonus, LPCMapping levelActions, int aliveAmount,
            int killAmount, int crossTimes, int remainAmount, int clearanceTime, bool autoCombat, int roundTimes)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 正在等待服务器的回执消息
            if (VerifyCmdMgr.IsVerifyCmd("CMD_INSTANCE_CLEARANCE"))
                return false;

            // 发送玩家缓存的任务进度
            TaskMgr.DoSendCacheTaskProgress(ME.user);

            // 将levelActions转换为byte
            byte[] mActionBuffer = System.Text.Encoding.UTF8.GetBytes(LPCSaveString.SaveToString(LPCValue.Create(levelActions)));

            // 构建消息参数
            LPCValue cmdArgs = PackArgs(
                "rid", rid,
                "result", result ? 1 : 0,
                "drop_bonus", dropBonus,
                "level_actions", LPCValue.Create(Zlib.Compress(mActionBuffer)),
                "cookie", Rid.New(),
                "alive_amount", aliveAmount,
                "kill_amount", killAmount,
                "cross_times", crossTimes,
                "remain_amount", remainAmount,
                "clearance_time", clearanceTime,
                "auto_combat", autoCombat ? 1 : 0,
                "round_times", roundTimes
            );

            // 添加缓存等待消息
            VerifyCmdMgr.AddVerifyCmd("CMD_INSTANCE_CLEARANCE", "MSG_INSTANCE_CLEARANCE", cmdArgs);

            // 通知服务器副本通关
            Communicate.Send2GS("CMD_INSTANCE_CLEARANCE", cmdArgs);

            // 返回成功
            return true;
        }
    }
}