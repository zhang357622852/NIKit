using UnityEngine;
using System.Collections;

public partial class Operation
{
    public class CmdUpgradeBaggage : CmdHandler
    {
        public string GetName()
        {
            return "cmd_upgrade_baggage";
        }

        // 通知服务器升级仓库
        public static bool Go(int payType, int page)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 通知服务器升级仓库
            Communicate.Send2GS("CMD_UPGRADE_BAGGAGE", PackArgs("pay_type", payType, "page", page));
            return true;
        }
    }
}
