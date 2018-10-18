using LPC;

public partial class Operation
{
    public class CmdBuyMarketItem : CmdHandler
    {
        public string GetName()
        {
            return "cmd_buy_market_item";
        }

        /// <summary>
        /// 购买商城道具
        /// </summary>
        public static bool Go(int classId, int amount)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            // 通知服务器购买道具
            Communicate.Send2GS("CMD_BUY_MARKET_ITEM", PackArgs(
                "class_id", classId,
                "amount", amount
            ));
            return true;
        }
    }
}
