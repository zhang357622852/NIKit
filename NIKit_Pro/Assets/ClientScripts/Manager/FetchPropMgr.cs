/// <summary>
/// FetchPropMgr.cs
/// Create by fengsc 2016/08/22
/// 附加属性管理类
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class FetchPropMgr
{
    #region 变量

    // 装备主属性配置表;
    private static CsvFile mMainPropCsv;

    // 主属性
    public static Dictionary<int, Dictionary<int, CsvRow>> mMainPropsMap = new Dictionary<int, Dictionary<int, CsvRow>>();

    #endregion

    #region 属性

    // 装备主属性配置表信息
    public static CsvFile MainPropCsv { get { return mMainPropCsv; } }

    #endregion

    #region 内部接口

    /// <summary>
    /// 载入主属性配置表
    /// </summary>
    /// <param name="fileName">File name.</param>
    private static void LoadMainPropFile()
    {
        mMainPropsMap.Clear();

        // 载入装备主属性配置信息
        mMainPropCsv = CsvFileMgr.Load("main_prop");

        foreach (CsvRow row in mMainPropCsv.rows)
        {
            int star = row.Query<int>("star");
            int propId = row.Query<int>("prop_id");

            if (mMainPropsMap.ContainsKey(star))
            {
                if (mMainPropsMap[star].ContainsKey(propId))
                {
                    LogMgr.Trace("main_prop表prop_id{0}星级{1}配置重复", propId, star);
                    continue;
                }

                mMainPropsMap[star].Add(propId, row);
            }
            else
            {
                mMainPropsMap.Add(star, new Dictionary<int, CsvRow>()
                    {
                        { propId, row },
                    });
            }
        }
    }

    /// <summary>
    /// 获取属性类型
    /// </summary>
    /// <returns>The properties map.</returns>
    /// <param name="propType">Property type.</param>
    private static Dictionary<int, Dictionary<int, CsvRow>> GetPropsMap(int propType)
    {
        // 根据不同的属性类型选择不同的配置信息
        switch (propType)
        {
            case EquipConst.MAIN_PROP:
                return mMainPropsMap;
            case EquipConst.PREFIX_PROP:
            case EquipConst.MINOR_PROP:

            default:
                LogMgr.Trace("prop_type错误，未找到该属性的配置信息");
                return new Dictionary<int, Dictionary<int, CsvRow>>();
        }
    }

    /// <summary>
    /// 获取属性信息
    /// </summary>
    /// <returns>The property info.</returns>
    /// <param name="prop_type">Property type.</param>
    /// <param name="star">Star.</param>
    /// <param name="prop_id">Property identifier.</param>
    private static CsvRow GetPropInfo(int prop_type, int star, int prop_id)
    {
        // 获取配置信息
        Dictionary<int, Dictionary<int, CsvRow>> propsMap = GetPropsMap(prop_type);

        // 没有找到配置信息
        if (propsMap == null || propsMap.Count == 0)
            return null;

        if (!propsMap.ContainsKey(star))
            return null;

        Dictionary<int, CsvRow> props = propsMap[star];

        if (!props.ContainsKey(prop_id))
            return null;

        return props[prop_id];
    }

    /// <summary>
    /// 计算数据值
    /// </summary>
    /// <returns>The random value.</returns>
    /// <param name="value">Value.</param>
    /// <param name="intensifyLevel">Intensify level.</param>
    /// <param name="type">Type.</param>
    /// <param name="step">Step.</param>
    private static int calcRandomValue(LPCValue value, int intensifyLevel, int type, LPCValue step)
    {
        if (value == null)
            return 0;

        // array格式
        if (value.IsArray)
        {
            LPCArray valueArr = value.AsArray;
            switch (valueArr.Count)
            {
                case 1:
                    // 固定数值
                    return valueArr[0].AsInt;
                case 2:
                    switch (type)
                    {
                        case PropType.PROP_INIT_MAX:
                            return valueArr.MaxInt();
                        case PropType.PROP_INIT_MIN:
                            return valueArr.MinInt();

                        default:
                            {
                                if (step == null || !step.IsInt || step.AsInt == 1)
                                    return UnityEngine.Random.Range(valueArr[0].AsInt, valueArr[1].AsInt);
                                else
                                    return UnityEngine.Random.Range(valueArr[0].AsInt / step.AsInt, valueArr[1].AsInt / step.AsInt) * step.AsInt;
                            }
                    }

                default:
                    // 表示范围min~max
                    return 0;
            }
        }

        // mapping格式
        if (value.IsMapping)
        {
            // 直接配置的数值
            LPCMapping valueMap = value.AsMapping;

            if (valueMap.ContainsKey(intensifyLevel))
                return valueMap[intensifyLevel].AsInt;

            foreach (object key in valueMap.Keys)
            {
                // 这个地方只是解析string
                if (!(key is string))
                    continue;

                string[] strs = (key as string).Split('~');

                // 格式不正确
                if (strs.Length != 2)
                    continue;

                // 不在范围内
                if (intensifyLevel < int.Parse(strs[0]) || intensifyLevel > int.Parse(strs[1]))
                    continue;

                // 返回数据
                return valueMap[(key as string)].AsInt;
            }
        }

        return value.IsInt ? value.AsInt : 0;
    }

    // 获取相同属性id的强化范围和星级
    private static LPCMapping GetIntensifyRange(int propId, int equipType)
    {
        LPCMapping mIntensifyRangeList = new LPCMapping();

        if (mMainPropCsv == null)
            return new LPCMapping();

        foreach (CsvRow data in mMainPropCsv.rows)
        {
            if (propId != data.Query<int>("prop_id"))
                continue;

            int star = data.Query<int>("star");

            if (mIntensifyRangeList == null)
                mIntensifyRangeList = new LPCMapping();

            mIntensifyRangeList.Add(star, data.Query<LPCValue>("intensify_" + equipType));
        }

        return mIntensifyRangeList;
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        LoadMainPropFile();
    }

    /// <summary>
    /// 获取装备主属性的强化范围
    /// </summary>
    /// <param name="equipOb"></param>
    /// <param name="propId"></param>
    /// <param name="offect">强化范围等级偏移量: 0-当前强化范围值 -1=上一级强化范围 1-下一级强化范围</param>
    /// <returns></returns>
    public static int GetMainPropIntensifyValue(Property equipOb, int propId, int offect)
    {
        if (equipOb == null)
            return 0;

        int equipType = equipOb.Query<int>("equip_type");

        int rank = equipOb.Query<int>("rank");

        int star = equipOb.Query<int>("star");

        // 获取强化范围
        LPCMapping map = GetIntensifyRange(propId, equipType);

        if (map == null)
            return 0;

        // 获取强化等级限制
        int limitLevel = GameSettingMgr.GetSettingInt("equip_intensify_limit_level");

        // 不存在没有星级的装备
        if (star < 1)
            return 0;

        if (map[star].IsMapping)
        {
            LPCMapping intensifyMap = map[star].AsMapping;

            foreach (var key in intensifyMap.Keys)
            {
                if (rank + offect < limitLevel && key is string)
                    return intensifyMap[(string)key].AsInt;
                if (rank + offect == limitLevel && key is int)
                    return intensifyMap[(int)key].AsInt;
            }
        }
        if (map[star].IsInt)
            return map[star].AsInt;

        return 0;
    }

    /// <summary>
    /// 计算属性等级数值
    /// </summary>
    /// <returns>The property value.</returns>
    /// <param name="prop_id">Property identifier.</param>
    /// <param name="equip_type">Equip type.</param>
    /// <param name="star">Star.</param>
    /// <param name="prop_type">Property type.</param>
    /// <param name="level">Level.</param>
    /// <param name="random_type">Random type.</param>
    /// <param name="rand_mode">Rand mode.</param>
    public static int CalcPropValue(int propId, int equipType, int star, int propType, int level, int randomType = PropType.PROP_INIT_RANDOM, int randMode = 0)
    {
        // 获取propid
        CsvRow propInfo = GetPropInfo(propType, star, propId);

        if (propInfo == null)
            return 0;

        LPCValue step = new LPCValue();

        if (propInfo.Contains("step"))
            step = propInfo.Query<LPCValue>("step");

        // 获取初始化属性配置信息
        LPCValue init = propInfo.Query<LPCValue>(string.Format("init_{0}", equipType));

        int value = calcRandomValue(init, 0, randomType, step);

        // 获取强化相关数据
        LPCValue intensify = propInfo.Query<LPCValue>(string.Format("intensify_{0}", equipType));

        // 计算强化等级带来的数值
        for (int i = 1; i <= level; i++)
            value += calcRandomValue(intensify, i, randomType, step);

        return value;
    }

    #endregion
}
