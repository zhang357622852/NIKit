using System;
using LPC;

public partial class Operation
{
    public class CmdTake : CmdHandler
    {
        public string GetName()
        {
            return "cmd_take";
        }

        // 指定仓库存储
        // propertyList单元为(["rid":xxxx,"pos":xxxxx])
        public static bool Go(LPCArray propertyList)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            Communicate.Send2GS("CMD_TAKE", PackArgs("property_list", propertyList));
            return true;
        }
    }
}
