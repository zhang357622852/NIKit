using LPC;

/// <summary>
/// 获取公会推荐玩家
/// </summary>
public class MsgGetRecommendUser : MsgHandler
{
    public string GetName() { return "msg_get_recommend_user"; }

    public void Go(LPCValue para)
    {
    }
}