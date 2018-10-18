/// <summary>
/// MsgBuyMoney.cs
/// Created by xuhd Feb/12/2015
/// 购买金币
/// </summary>
using LPC;

/// <summary>
/// 购买金币
/// </summary>
public class MsgBuyMoney : MsgHandler
{
    public string GetName()
    {
        return "msg_buy_money";
    }

    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
        LPCMapping args = para.AsMapping;
        LPCMapping costMap = args.GetValue<LPCMapping>("cost_map");
        LPCMapping gainMap = args.GetValue<LPCMapping>("gain_map");

        if (costMap == null || gainMap == null)
        {
            LogMgr.Trace("costMap或gainMap是null");
            return;
        }

        if (ME.user == null)
            return;

        // 执行购买操作
        ME.user.CostAttrib(costMap);
        ME.user.AddAttrib(gainMap);
    }
}
