using System;
using LPC;

public partial class Operation
{
    public class CmdShopRefresh : CmdHandler
    {
        public string GetName()
        {
            return "cmd_shop_refresh";
        }

        /// <summary>
        /// 商店刷新
        /// </summary>
        public static bool Go(int is_force)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            LogMgr.Trace("[CmdShopRefresh.cs] 发送商店刷新消息 is_force = {0}", is_force);
            Communicate.Send2GS("CMD_SHOP_REFRESH", PackArgs("is_force", is_force));
            return true;
        }
    }
}
