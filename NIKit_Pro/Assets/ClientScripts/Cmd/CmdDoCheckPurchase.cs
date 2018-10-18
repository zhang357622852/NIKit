using System;
using LPC;

public partial class Operation
{
    /// <summary>
    /// 获取账户购买信息
    /// </summary>
    public class CmdDoCheckPurchase : CmdHandler
    {
        public string GetName()
        {
            return "cmd_do_check_purchase";
        }

        public static bool Go(string orderId)
        {
            LogMgr.Trace("服务器验证充值账单:{0}", orderId);
            Communicate.Send2GS("CMD_DO_CHECK_PURCHASE", PackArgs("order_id", orderId));
            return true;
        }
    }
}