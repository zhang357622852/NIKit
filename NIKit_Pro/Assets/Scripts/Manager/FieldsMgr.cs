/// <summary>
/// FieldsMgr.cs
/// Created by zhaozy 2014-12-12
/// 字段表管理
/// </summary>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using LPC;

/// <summary>
/// 字段表管理
/// </summary>
public class FieldsMgr
{
    #region 变量

    // 字段配置
    private static CsvFile mFieldsCsv;

    private static LPCMapping mAttrib2Item = LPCMapping.Empty;
    private static LPCMapping mItem2Attrib = LPCMapping.Empty;

    #endregion

    #region 属性

    // 字段配置信息
    public static CsvFile FieldsCsv { get { return mFieldsCsv; } }

    #endregion

    #region  公共接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        mAttrib2Item = LPCMapping.Empty;
        mItem2Attrib = LPCMapping.Empty;

        // 载入字段配置表
        mFieldsCsv = CsvFileMgr.Load("fields");
        int itemClassId;

        foreach (CsvRow data in mFieldsCsv.rows)
        {
            // 获取属性对应的item_class_id
            itemClassId = data.Query<LPCValue>("item_class_id").AsInt;

            // 该属性没有绑定道具
            if (itemClassId == 0)
                continue;

            // 记录数据
            mAttrib2Item.Add(data.Query<string>("field"), itemClassId);
            mItem2Attrib.Add(itemClassId, data.Query<string>("field"));
        }
    }

    /// <summary>
    /// Gets the class identifier by attrib.
    /// </summary>
    public static int GetClassIdByAttrib(string attrib)
    {
        return mAttrib2Item.GetValue<int>(attrib);
    }

    /// <summary>
    /// Gets the attrib by class identifier.
    /// </summary>
    public static string GetAttribByClassId(int classId)
    {
        return mItem2Attrib.GetValue<string>(classId);
    }

    /// <summary>
    /// 获取属性字段描述
    /// </summary>
    public static string GetFieldDesc(string field, string attrib)
    {
        CsvRow data = FieldsCsv.FindByKey(field);

        // 没有配置的字段
        if (data == null)
        {
            NIDebug.Log("fields.csv未配置字段{0}", field);
            return string.Empty;
        }

        // 返回数据
        return data.Query<string>(attrib);
    }

    /// <summary>
    /// 取得配置成LPCMAPPING属性的字段
    /// </summary>
    /// <returns>The field in mapping.</returns>
    /// <param name="field_map">Field_map.</param>
    public static string GetFieldInMapping(LPCMapping field_map)
    {
        if (field_map == null)
            return string.Empty;

        foreach (string field in field_map.Keys)
        {
            if (!string.IsNullOrEmpty(field))
                return field;
        }

        return string.Empty;
    }

    /// <summary>
    /// 获取字段名称
    /// </summary>
    /// <returns>The field name.</returns>
    /// <param name="field">Field.</param>
    public static string GetFieldName(string field)
    {
        CsvRow data = FieldsCsv.FindByKey(field);

        // 没有配置的字段
        if (data == null)
        {
            NIDebug.Log("fields.csv未配置字段{0}", field);
            return string.Empty;
        }

        // 返回数据
        return string.Empty;//LocalizationMgr.Get(data.Query<string>("name"));
    }

    /// <summary>
    /// 获取字段icon
    /// </summary>
    /// <returns>The field icon.</returns>
    /// <param name="field">Field.</param>
    public static string GetFieldIcon(string field)
    {
        CsvRow data = FieldsCsv.FindByKey(field);

        // 没有配置的字段
        if (data == null)
        {
            NIDebug.Log("fields.csv未配置字段{0}", field);
            return string.Empty;
        }

        // 返回数据
        return data.Query<string>("icon");
    }

    /// <summary>
    /// 获取字段icon
    /// </summary>
    /// <returns>The field icon.</returns>
    /// <param name="field">Field.</param>
    public static string GetFieldTexture(string field)
    {
        CsvRow data = FieldsCsv.FindByKey(field);

        // 没有配置的字段
        if (data == null)
        {
            NIDebug.Log("fields.csv未配置字段{0}", field);
            return string.Empty;
        }

        // 返回数据
        return data.Query<string>("texture");
    }

    /// <summary>
    /// 获取字段单位
    /// </summary>
    public static string GetFieldUnit(string field)
    {
        CsvRow data = FieldsCsv.FindByKey(field);

        // 没有配置的字段
        if (data == null)
        {
            NIDebug.Log("fields.csv未配置字段{0}", field);
            return string.Empty;
        }

        // 返回数据
        return data.Query<string>("unit");
    }

    /// <summary>
    /// 获取字段对应的道具
    /// </summary>
    public static int GetFieldItemClassId(string field)
    {
        CsvRow data = FieldsCsv.FindByKey(field);

        // 没有配置的字段
        if (data == null)
        {
            NIDebug.Log("fields.csv未配置字段{0}", field);
            return -1;
        }

        // 返回数据
        return data.Query<int>("item_class_id");
    }

    #endregion
}
