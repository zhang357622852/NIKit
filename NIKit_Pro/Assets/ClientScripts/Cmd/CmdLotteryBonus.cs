/// <summary>
/// CmdLotteryBonus.cs
/// Created by zhaozy 2016/11/17
/// 抽奖
/// </summary>

using System;
using LPC;

public partial class Operation
{
    public class CmdLotteryBonus : CmdHandler
    {
        public string GetName()
        {
            return "cmd_lottery_bonus";
        }

        public static bool Go()
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 正在等待服务器的回执消息
            if (VerifyCmdMgr.IsVerifyCmd("CMD_LOTTERY_BONUS"))
                return false;

            // 构建消息参数
            LPCValue cmdArgs = PackArgs("cookie", Rid.New());

            // 添加缓存等待消息
            VerifyCmdMgr.AddVerifyCmd("CMD_LOTTERY_BONUS", "MSG_LOTTERY_BONUS", cmdArgs);

            // 通知服务器CMD_LOTTERY_BONUS
            Communicate.Send2GS("CMD_LOTTERY_BONUS", cmdArgs);
            return true;
        }
    }
}