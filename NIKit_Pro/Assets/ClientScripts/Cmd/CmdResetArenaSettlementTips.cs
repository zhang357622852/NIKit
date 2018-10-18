using System.Collections;
using System.Collections.Generic;
using LPC;

public partial class Operation
{
    public class CmdResetArenaSettlementTips : CmdHandler
    {
        public string GetName()
        {
            return "cmd_reset_arena_settlement_tips";
        }

        public static bool Go(int is_tips)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 构建消息参数
            LPCValue cmdArgs = PackArgs("is_tips", is_tips);

            Communicate.Send2GS("CMD_RESET_ARENA_SETTLEMENT_TIPS", cmdArgs);
            return true;
        }
    }
}
