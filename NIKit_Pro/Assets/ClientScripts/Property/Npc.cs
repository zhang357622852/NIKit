/// <summary>
/// NPC.cs
/// Copy from zhangyg 2014-10-22
/// NPC对象
/// </summary>

using LPC;
 
/// <summary>
/// NPC对象 
/// </summary>
public class NPC : Monster
{
    public NPC(LPCMapping data) : base(data)
    {
        // 设置类型为NPC
        this.objectType = ObjectType.OBJECT_TYPE_NPC;
    }

    /// <summary>
    /// 是否是NPC对象
    /// </summary>
    public bool IsNPC()
    {
        return true;
    }
}
