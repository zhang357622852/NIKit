/// <summary>
/// MsgResetEquipRevert.cs
/// Created by cql 2015/07/17
/// 回退重置装备结果成功返回的消息
/// </summary>
using LPC;

public class MsgResetEquipRevert : MsgHandler
{
    public string GetName()
    {
        return "msg_reset_equip_revert";
    }

    public void Go(LPCValue para)
    {
    }
}
