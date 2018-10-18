using System;
using LPC;

public partial class Operation
{
    public class CmdOpenStore : CmdHandler
    {
        public string GetName()
        {
            return "cmd_open_store";
        }

        // 指定仓库存储物品
        // propertyList单元为(["rid":xxxx,"pos":xxxxx])
        public static bool Go()
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            Communicate.Send2GS("CMD_OPEN_STORE", PackArgs("belong", LPCMapping.Empty));
            return true;
        }
    }
}
