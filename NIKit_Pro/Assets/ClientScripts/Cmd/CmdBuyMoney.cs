/// <summary>
/// CmdBuyMoney.cs
/// Created by xuhd Fab/12/2015
/// 购买金币
/// </summary>
using System;
using LPC;

public partial class Operation
{
    public class CmdBuyMoney : CmdHandler
    {
        public string GetName()
        {
            return "cmd_buy_money";
        }

        public static bool Go(int buyType)
        {
            LogMgr.Trace("[CmdBuyMoney.cs] 购买金币，buyType = {0}。", buyType);
            Communicate.Send2GS("CMD_BUY_MONEY", PackArgs("buy_type", buyType));
            return true;
        }
    }
}