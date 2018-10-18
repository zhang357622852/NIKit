/// <summary>
/// MsgShopRefresh.cs
/// Created by xuhd Mar/05/2015
/// 市场刷新返回的消息
/// </summary>

using LPC;

/// <summary>
/// 市场刷新返回的消息
/// </summary>
public class MsgShopRefresh : MsgHandler
{
    public string GetName()
    {
        return "msg_shop_refresh";
    }

    /// <summary>
    /// 入口
    /// </summary>
    public void Go(LPCValue para)
    {
    }
}
