using System;
using LPC;

public partial class Operation
{
    public class CmdUpgradeStore : CmdHandler
    {
        public string GetName()
        {
            return "cmd_upgrade_store";
        }

        // 通知服务器升级仓库
        public static bool Go(int payType)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 通知服务器升级仓库
            Communicate.Send2GS("CMD_UPGRADE_STORE", PackArgs("pay_type", payType));
            return true;
        }
    }
}
