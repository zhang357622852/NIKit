/// <summary>
/// Item.cs
/// Copy from zhangyg 2014-10-22
/// 物品对象
/// </summary>

using System.Diagnostics;
using LPC;

/// <summary>
/// 装备对象
/// </summary>
public class Equipment : Item
{
    #region 公共接口

    // 构造函数
    public Equipment(LPCMapping data)
        : base(data)
    {
        // 暂时什么都不做
    }

    /// <summary>
    /// 是否是Equipment对象
    /// </summary>
    public bool IsEquipment()
    {
        return true;
    }

    /// <summary>
    /// 判断是否能够卸载物品到指定位置
    /// </summary>
    public bool CanBeUnequipped(Property who)
    {
        return EquipMgr.CanBeUnequipped(who, this);
    }

    /// <summary>
    /// 装备到指定位置
    /// </summary>
    public bool Equip(Property who)
    {
        return EquipMgr.Equip(who, this);
    }

    /// <summary>
    /// 本道具是否已经装备
    /// </summary>
    public bool IsEquipped(Property who)
    {
        // 获取道具的位置
        string pos = this.move.GetPos();

        // 没有位置信息
        if (pos == null)
            return false;

        // 本道具是否已经装备
        return EquipMgr.IsEquippedPos(who, pos);
    }

    /// <summary>
    /// 解除物品，放到指定位置
    /// </summary>
    public bool UnEquip(Property who)
    {
        return EquipMgr.UnEquip(who, this);
    }

    #endregion
}
