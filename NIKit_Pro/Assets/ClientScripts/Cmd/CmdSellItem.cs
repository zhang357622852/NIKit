using LPC;

public partial class Operation
{
    public class CmdSellItem : CmdHandler
    {
        public string GetName()
        {
            return "cmd_sell_item";
        }

        /// <summary>
        /// 出售道具
        /// </summary>
        public static bool Go(LPCMapping extra_para)
        {
            // 玩家不在游戏中，或者正在登出游戏
            if (!ME.isInGame || ME.isLogouting)
                return false;

            Communicate.Send2GS("CMD_SELL_ITEM", PackArgs("extra_para", extra_para));
            return true;
        }
    }
}
