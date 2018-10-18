using LPC;

/// <summary>
/// 玩家技能更新
/// </summary>
public class MsgUpdateSkill : MsgHandler
{
    public string GetName()
    {
        return "msg_update_skill";
    }

    public void Go(LPCValue para)
    {
        // 老消息 忽略 使用 Object_Update
    }
}