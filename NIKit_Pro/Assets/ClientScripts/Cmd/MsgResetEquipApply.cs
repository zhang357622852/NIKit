/// <summary>
/// MsgResetEquipApply.cs
/// Created by cql 2015/07/17
/// 保留重置装备结果成功返回的消息
/// </summary>
using LPC;

public class MsgResetEquipApply : MsgHandler
{
    public string GetName()
    {
        return "msg_reset_equip_apply";
    }

    public void Go(LPCValue para)
    {
    }
}
