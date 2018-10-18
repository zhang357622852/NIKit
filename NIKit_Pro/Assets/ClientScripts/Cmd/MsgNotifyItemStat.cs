using LPC;
using UnityEngine;

/// 物品改变
public class MsgNotifyItemStat : MsgHandler
{
    public string GetName()
    {
        return "MSG_NOTIFY_ITEM_STAT";
    }

    public void Go(LPCValue para)
    {
    }
}