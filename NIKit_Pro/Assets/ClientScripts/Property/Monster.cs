/// <summary>
/// Monster.cs
/// Copy from zhangyg 2014-10-22
/// 怪物对象
/// </summary>

using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 怪物对象 
/// </summary>
public class Monster : Container
{
    #region 内部接口

    /// <summary>
    /// 获取配置表信息
    /// </summary>
    private List<CsvRow> GetBasicAttrib(int class_id)
    {
        List<CsvRow> rows = new List<CsvRow>();

        // 查询monster配置表
        CsvRow row = MonsterMgr.MonsterCsv.FindByKey(class_id);
        if (row != null)
            rows.Add(row);

        // 返回数据
        return rows;
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="data">Data.</param>
    public Monster(LPCMapping data)
        : base(data)
    {
        // 设置类型为怪物
        this.objectType = ObjectType.OBJECT_TYPE_MONSTER;

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

        // 创建actor对象
        CreateCombatActor(data);
    }

    #endregion
}
