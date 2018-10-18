/// <summary>
/// Move.cs
/// Copy from zhangyg 2014-10-22
/// 移动特性
/// </summary>

using System;
using LPC;

/// <summary>
/// 移动特性
/// </summary>
public class Move
{
    /// <summary>
    /// 父容器
    /// </summary>
    public Container father = null;

    // 本属性所属的道具
    private Property owner;

    public Move(Property property)
    {
        this.owner = property;
    }

    public void Destroy()
    {
        // 从父亲容器中移出去
        if (father != null)
            father.UnloadProperty(owner);
    }

    /// <summary>
    /// 取得道具的位置
    /// </summary>
    public string GetPos()
    {
        LPCValue v = owner.dbase.Query("pos");
        if (v == null || !v.IsString)
            return null;

        return v.AsString;
    }

    /// <summary>
    /// 设置道具的坐标
    /// </summary>
    public void SetPos(string pos)
    {
        owner.dbase.Set("pos", pos);
    }
}
