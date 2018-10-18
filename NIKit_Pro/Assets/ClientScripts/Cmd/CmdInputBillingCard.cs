/// <summary>
/// CmdInputBillingCard.cs
/// Created by fengxl 2015-3-30
/// 验证激活码
/// </summary>
using System;
using LPC;

public partial class Operation
{
    public class CmdInputBillingCard:CmdHandler
    {
        public string GetName()
        {
            return "cmd_input_billing_card";
        }

        public static bool Go(string cardNo)
        {
            LogMgr.Trace("[CmdInputBillingCard.cs] 验证激活码");
            Communicate.Send2GS("CMD_INPUT_BILLING_CARD", PackArgs("card_no", cardNo));
            return true;
        }
    }
}