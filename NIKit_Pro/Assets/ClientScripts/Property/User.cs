/// <summary>
/// User.cs
/// Copy from zhangyg 2014-10-22
/// 玩家对象
/// </summary>

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 玩家对象 
/// </summary>
public class User : Container
{
    #region 内部接口

    /// <summary>
    /// 获取配置表信息
    /// </summary>
    private List<CsvRow> GetBasicAttrib(int style)
    {
        return new List<CsvRow>();
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="data">Data.</param>
    public User(LPCMapping data)
        : base(data)
    {
        // 设置类型
        this.objectType = ObjectType.OBJECT_TYPE_USER;

        // 如果指明了dbase数据，需要吸收进来
        if (data != null && data["dbase"] != null && data["dbase"].IsMapping)
        {
            // 吸入dbase数据
            this.dbase.Absorb(data["dbase"].AsMapping);

            // 设置名称
            if (data["dbase"].AsMapping["name"] != null)
                SetName(data["dbase"].AsMapping["name"].AsString);

            // 初始化实体的csv表格行
            if (data["dbase"].AsMapping.ContainsKey("style"))
            {
                int style = data["dbase"].AsMapping["style"].AsInt;
                SetBasicAttrib(GetBasicAttrib(style));
            }
        }

        // 创建actor对象
        CreateCombatActor(data);
    }

    /// <summary>
    /// 重载ToString接口 
    /// </summary>
    public override string ToString()
    {
        return string.Format("用户：{0}\nRID: {1}\n位置：{2}\n", GetName(), GetRid(), this.move.GetPos());
    }

    /// <summary>
    /// 取得classID
    /// </summary>
    public override int GetClassID()
    {
        return Query<int>("style");
    }

    /// 获取Vip等级
    public int GetVip()
    {
        return dbase.Query("charge_vip/level", 0);
    }

    /// <summary>
    /// 返回玩家某个道具的数量
    /// </summary>
    public int GetItemCountByClassID(int classID)
    {
        int count = 0;
        Dictionary<string, Property> items = baggage.GetPageCarry(ContainerConfig.POS_ITEM_GROUP);
        foreach (Property item in items.Values)
        {
            if (item != null && item.QueryAttrib("class_id") == classID)
                count += item.GetAmount();
        }
        
        return count;
    }

    /// <summary>
    /// 获得某个ID的所有物品
    /// </summary>
    public List<Property> GetItemsByClassID(int classID)
    {
        List<Property> result = new List<Property>();
        Dictionary<string, Property> items = baggage.GetPageCarry(ContainerConfig.POS_ITEM_GROUP);
        foreach (Property item in items.Values)
        {
            if (item != null && item.QueryAttrib("class_id") == classID)
                result.Add(item);
        }
        
        return result;
    }

    #endregion
}
