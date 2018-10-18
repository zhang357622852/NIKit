/// <summary>
/// Item.cs
/// Copy from zhangyg 2014-10-22
/// 物品对象
/// </summary>

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 物品对象
/// </summary>
public class Item : Property
{
    #region 内部接口

    /// <summary>
    /// 获取配置表信息
    /// </summary>
    private List<CsvRow> GetBasicAttrib(int class_id)
    {
        List<CsvRow> rows = new List<CsvRow>();

        // 查询item配置表
        if (ItemMgr.IsItem (class_id))
        {
            rows.Add (ItemMgr.ItemCsv.FindByKey (class_id));
        }

        // 查询equipment配置表
        if (EquipMgr.IsEquipment (class_id))
        {
            rows.Add (EquipMgr.EquipCsv.FindByKey (class_id));
        }

        // 返回数据
        return rows;
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 构建函数
    /// </summary>
    public Item(LPCMapping data)
        : base(data)
    {
        // 设置类别为道具
        this.objectType = ObjectType.OBJECT_TYPE_ITEM;

        // 设置数量为1
        SetAmount(1);

        // 如果指明了dbase数据，需要吸收进来
        if (data != null && data["dbase"] != null && data["dbase"].IsMapping)
        {
            // 吸入dbase数据
            this.dbase.Absorb(data["dbase"].AsMapping);

            // 设置名称
            if (data["dbase"].AsMapping["name"] != null)
                SetName(data["dbase"].AsMapping["name"].AsString);

            // 初始化实体的csv表格行
            if (data["dbase"].AsMapping.ContainsKey("class_id"))
            {
                int classId = data["dbase"].AsMapping["class_id"].AsInt;
                SetBasicAttrib(GetBasicAttrib(classId));
            }
        }
    }

    /// <summary>
    /// 消耗数量
    /// </summary>
    public void CostAmount(int amount)
    {
        Debug.Assert(amount > 0);

        int curr = GetAmount();

        SetAmount(curr - amount);

        // 数量为0，干掉本对象
        if (curr - amount <= 0)
            Destroy();
    }

    /// <summary>
    /// 增加道具数量
    /// </summary>
    public void AddAmount(int amount)
    {
        Debug.Assert(amount > 0);

        // 获取道具当前的数量
        int curr = GetAmount();

        // 增加数量
        SetAmount(curr + amount);
    }

    /// <summary>
    /// 判断本道具是否是药物
    /// </summary>
    public bool IsDrug()
    {
        LPCValue v = Query("drug_type");
        if (v == null || !v.IsInt || v.AsInt == 0)
            return false;

        return true;
    }

    /// <summary>
    /// 判断和另外一个道具是否相同
    /// </summary>
    public bool IsSameItem(Item item)
    {
        // class_id不一致
        if (!dbase.Query("class_id").Equals(item.dbase.Query("class_id")))
            return false;

        // pet_id不一致
        if (!dbase.Query("pet_id").Equals(item.dbase.Query("pet_id")))
            return false;

        // 相同的道具
        return true;
    }

    /// <summary>
    /// 通知本对象数据更新了
    /// </summary>
    public void updated(LPCMapping dbase)
    {
        // TODO
        return;
    }

    #endregion
}
