using LPC;

public partial class Operation
{
    public class CmdUseItem : CmdHandler
    {
        public string GetName()
        {
            return "cmd_use_item";
        }

        /// <summary>
        /// 使用道具
        /// </summary>
        public static bool Go(LPCMapping itemRids)
        {
            // 通知服务器使用道具
            Communicate.Send2GS("CMD_USE_ITEM", PackArgs("item_rids", itemRids));
            return true;
        }
    }
}
