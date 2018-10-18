/// <summary>
/// PropMgr.cs
/// Copy from zhangyg 2014-10-22
/// 附加属性计算管理
/// </summary>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using LPC;

/// <summary>
/// 附加属性计算管理
/// </summary>
public class PropMgr
{
    #region 变量

    /// <summary>
    /// 刷新附加属性的回调
    /// </summary>
    public delegate void RefreshAffectHook(Property property);

    private static Dictionary<string, RefreshAffectHook> refreshHooks = new Dictionary<string, RefreshAffectHook>();

    const int PROP_TYPE_WEIGHT = 2;
    const int PROP_TYPE_BOOL = 1;
    const int PROP_TYPE_NO_MERGE = 0;

    const string PROP_TYPE_WEIGHT_STR = "2";
    const string PROP_TYPE_BOOL_STR = "1";
    const string PROP_TYPE_NO_MERGE_STR = "0";

    // 属性配置表信息
    private static CsvFile mPropCsv;

    // 主性配置表信息
    private static CsvFile mMainPropCsv;

    // 次要属性配置表信息
    private static CsvFile mMinorPropCsv;

    // 属性刷新脚本配置信息
    private static CsvFile mUpdateScriptCsv;

    // 属性价值配置信息
    private static CsvFile mPropValueCsv;

    // 属性别名对应id
    private static Dictionary<string, int> mNameToPropId = new Dictionary<string, int>();
    private static Dictionary<string, string> mNameToPath = new Dictionary<string, string>();

    // 主属性列表
    private static List<int> mMainPropList = new List<int>();

    // 次要属性列表
    private static List<int> mMinorPropList = new List<int>();

    #endregion

    #region 属性

    // 属性配置表信息
    public static CsvFile PropCsv { get { return mPropCsv; } }

    // 属性配置表信息
    public static CsvFile UpdateScriptCsv { get { return mUpdateScriptCsv; } }

    // 属性价值配置信息
    public static CsvFile PropValueCsv { get { return mPropValueCsv; } }

    // 主属性配置信息
    public static CsvFile MainPropCsv {get { return mMainPropCsv; } }

    //  次要属性配置信息
    public static CsvFile MinorPropCsv {get { return mMinorPropCsv; } }

    // 主属性列表
    public static List<int> MainPropList { get{ return mMainPropList; } }

    // 次要属性列表
    public static List<int> MinorPropList { get{ return mMinorPropList; } }

    #endregion

    #region 内部接口

    static PropMgr()
    {
        // 注册刷新角色装备的回调函数
        PropMgr.RegisterRefreshAffectHook("equip", EquipMgr.RefreshEquipAffect);

        // 注册刷新角色状态的回调函数
        PropMgr.RegisterRefreshAffectHook("status", StatusMgr.RefreshStatusAffect);

        // 注册刷新角色状态的回调函数
        PropMgr.RegisterRefreshAffectHook("skill", SkillMgr.RefreshSkillAffect);
    }

    // 合并缓存的附加属性计算结果
    private static void MergeProps(Property who)
    {
        // 获取temp_improvement
        LPCMapping v = who.QueryTemp<LPCMapping>("temp_improvement");

        // 数据异常
        if (v == null)
            return;

        LPCMapping m = null;
        LPCMapping propMap = null;

        // 遍历数据
        foreach (string type in v.Keys)
        {
            // 取该类型附加属性
            m = v[type].AsMapping;

            // 遍历所有权重类附加属性计算结果，进行合并
            if (m.ContainsKey(PROP_TYPE_WEIGHT_STR))
            {
                propMap = m[PROP_TYPE_WEIGHT_STR].AsMapping;
                foreach (string k in propMap.Keys)
                {
                    MergeWeightProps(who, GetPropPath(k), propMap[k]);
                }
            }

            // 遍历所有权独立类附加属性计算结果
            if (m.ContainsKey(PROP_TYPE_NO_MERGE_STR))
            {
                propMap = m[PROP_TYPE_NO_MERGE_STR].AsMapping;
                foreach (string k in propMap.Keys)
                {
                    MergeIndependentProps(who, GetPropPath(k), propMap[k]);
                }
            }

            // 遍历所有布尔类附加属性计算结果
            if (m.ContainsKey(PROP_TYPE_BOOL_STR))
            {
                propMap = m[PROP_TYPE_BOOL_STR].AsMapping;
                // 遍历所有布尔类附加属性计算结果
                foreach (string k in propMap.Keys)
                {
                    LPCArray value = propMap[k].AsArray;
                    if (value.Count == 1)
                        who.SetTemp(GetPropPath(k), value[0]);
                    else
                    {
                        // 由外挂脚本决定最终结果
                        object valueRet = FETCH_BOOL_PROP_MERGE_RESULT.Call(who, k, value);
                        if (valueRet == null)
                            continue;

                        // 设置属性值
                        who.SetTemp(GetPropPath(k), valueRet as LPCValue);
                    }
                }
            }
        }
    }

    /// <summary>
    /// 合并独立类附加属性
    /// </summary>
    private static void MergeIndependentProps(Property who, string path, LPCValue value)
    {
        // 如果是mapping格式数据
        if (value.IsMapping)
        {
            // 转换数据格式
            LPCMapping valueMap = value.AsMapping;

            // 多层属性，遍历所有下一级路径，递归来进行合并
            foreach (string key in valueMap.Keys)
                MergeIndependentProps(who, string.Format("{0}/{1}", path, key), valueMap[key]);

            return;
        }

        // 合并之(array)
        if (who.QueryTemp(path) == null)
            who.SetTemp(path, value);
        else
            who.QueryTemp(path).AsArray.Add(value);
    }

    // 缓存计算结果
    private static void CacheCalcResult(int propType, string path, Property target, string field, LPCValue propValue)
    {
        // 根据不同类型合并属性
        switch (propType)
        {
            case PROP_TYPE_WEIGHT:
                // 权重型，直接累加
                MergeWeightProps(target, path + "/2/" + field, propValue);

                break;

            case PROP_TYPE_BOOL:
                // BOOL型，全部记录下来
                path += "/1/" + field;
                LPCValue values = target.QueryTemp(path);
                if (values == null)
                    values = LPCValue.CreateArray();

                values.AsArray.Add(propValue);
                target.SetTemp(path, values);

                break;

            case PROP_TYPE_NO_MERGE:
                // 不可叠加型
                path += "/0/" + field;

                LPCValue data = target.QueryTemp(path);
                if (data == null)
                    data = LPCValue.CreateArray();

                // 没有重复加入进去
                if (data.AsArray.IndexOf(propValue) < 0)
                    data.AsArray.Add(propValue);

                // 重置数据
                target.SetTemp(path, data);

                break;
        }
    }

    // 合并权重类附加属性
    private static void MergeWeightProps(Property who, string path, LPCValue v)
    {
        // 如果v是mapping类型，需要字段Merge
        if (v.IsMapping)
        {
            LPCMapping vMap = v.AsMapping;

            // 多层属性，遍历所有下一级路径，递归来进行合并
            foreach (string k in vMap.Keys)
            {
                LPCValue m = vMap[k];
                MergeWeightProps(who, path + "/" + k, m);
            }
            return;
        }

        // 获取原有数据
        LPCValue oldValue = who.QueryTemp(path);

        // 如果没有该属性，直接设置该属性
        if (oldValue == null)
        {
            who.SetTemp(path, v);
            return;
        }

        // 根据类型设置属性
        if (oldValue.IsInt && v.IsInt)
            who.SetTemp(path, LPCValue.Create(oldValue.AsInt + v.AsInt));
        else if (oldValue.IsFloat && v.IsFloat)
            who.SetTemp(path, LPCValue.Create(oldValue.AsFloat + v.AsFloat));
        else if (oldValue.IsArray && v.IsArray)
        {
            oldValue.AsArray.Append(v.AsArray);
            who.SetTemp(path, oldValue);
        }
        else
            Debug.Assert(false);
    }

    #endregion

    #region  公共接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        mNameToPropId.Clear();
        mNameToPath.Clear();

        // 载入配置表信息
        mPropCsv = CsvFileMgr.Load("prop");

        // 遍历各个数据统计mAliasToPropId
        string propName = string.Empty;
        foreach (CsvRow data in mPropCsv.rows)
        {
            propName = data.Query<string>("prop_name");

            // 添加属性名=>属性id映射关系
            mNameToPropId.Add(propName, data.Query<int>("prop_id"));

            // 添加属性名=>属性存储位置映射关系
            mNameToPath.Add(propName, string.Format("{0}/{1}", data.Query<string>("prop_path"), propName));
        }

        // 载入属性刷新脚本配置信息
        mUpdateScriptCsv = CsvFileMgr.Load("update_script");

        // 载入属性价值配置
        mPropValueCsv = CsvFileMgr.Load("prop_value");

        // 载入主属性配置表
        mMainPropCsv = CsvFileMgr.Load("main_prop");

        // 遍历主属性配置表
        if (mMainPropCsv != null)
        {
            foreach (CsvRow row in mMainPropCsv.rows)
            {
                // 属性id
                int propId = row.Query<int>("prop_id");

                // 缓存主属性id
                if (!mMainPropList.Contains(propId))
                    mMainPropList.Add(propId);
            }
        }

        // 载入次要属性配置表
        mMinorPropCsv = CsvFileMgr.Load("minor_prop");

        // 遍历次属性配置表
        if (mMinorPropCsv != null)
        {
            foreach (CsvRow row in mMinorPropCsv.rows)
            {
                // 属性id
                int propId = row.Query<int>("prop_id");

                // 缓存次属性id
                if (!mMinorPropList.Contains(propId))
                    mMinorPropList.Add(propId);
            }
        }
    }

    /// <summary>
    /// 根据装备类型获取主属性列表
    /// </summary>
    public static List<int> GetPropByEquipType(int equipType, int propType)
    {
        if (equipType < 0)
            return new List<int>();

        List<int> propList = new List<int>();

        int lastPropId = 0;

        CsvFile cf = null;

        if (propType.Equals(EquipConst.MAIN_PROP))
            cf = mMainPropCsv;
        else if (propType.Equals(EquipConst.MINOR_PROP) || propType.Equals(EquipConst.PREFIX_PROP))
            cf = mMinorPropCsv;

        for (int i = 0; i < cf.rows.Length; i++)
        {
            if (cf.rows[i] == null)
                continue;

            int weight = cf.rows[i].Query<int>(equipType.ToString());

            if (weight <= 0)
                continue;

            int Id = cf.rows[i].Query<int>("prop_id");

            if (Id == lastPropId)
                continue;

            propList.Add(cf.rows[i].Query<int>("prop_id"));

            lastPropId = Id;
        }

        return propList;
    }

    /// <summary>
    /// 获取属性价值的修正参数
    /// </summary>
    public static int GetPropFixValue(int propId, int rarity)
    {
        // 获取配置信息
        CsvRow calcInfo = mPropValueCsv.FindByKey(propId);

        // 没有配置价值计算公式，计作0
        if (calcInfo == null)
            return 0;

        // 返回配置信息
        return calcInfo.Query<int>(rarity.ToString());
    }

    /// <summary>
    /// 获取属性价值
    /// </summary>
    public static int GetPropValue(LPCArray prop, int star, int equipType, int propType)
    {
        // 获取配置信息
        CsvRow calcInfo = mPropValueCsv.FindByKey(prop[0].AsInt);

        // 没有配置价值计算公式，计作0
        if (calcInfo == null)
            return 0;

        // 没有value_script直接返回
        LPCValue scriptNo = calcInfo.Query<LPCValue>("value_script");
        if (!scriptNo.IsInt || scriptNo.AsInt == 0)
            return 0;

        // 调用脚本计算价值
        return (int)ScriptMgr.Call(scriptNo.AsInt, prop, star, equipType, propType, calcInfo.Query<LPCValue>("value_arg"));
    }

    /// <summary>
    /// 获取属性战力
    /// </summary>
    public static int GetPropFighting(LPCArray prop)
    {
        // 获取配置信息
        CsvRow calcInfo = mPropValueCsv.FindByKey(prop[0].AsInt);

        // 没有配置战力计算公式，计作0
        if (calcInfo == null)
            return 0;

        // 没有fighting_script直接返回
        LPCValue scriptNo = calcInfo.Query<LPCValue>("fighting_script");
        if (!scriptNo.IsInt || scriptNo.AsInt == 0)
            return 0;

        // 调用脚本计算战力
        return (int)ScriptMgr.Call(scriptNo.AsInt, prop, calcInfo.Query<LPCValue>("fighting_arg"));
    }

    ///<summary>
    /// 刷新角色的属性作用效果
    /// 1. 应用所有装备的属性效果
    /// 2. 应用所有技能的属性效果
    /// 3. 应用所有临时的属性效果
    /// 4. 通过脚本计算最终属性效果，生成improvement
    /// </summary>
    public static void RefreshAffect(Property ob)
    {
        RefreshAffect(ob, string.Empty);
    }

    /// <summary>
    /// 刷新角色的属性作用效果
    /// 1. 应用所有装备的属性效果
    /// 2. 应用所有技能的属性效果
    /// 3. 应用所有临时的属性效果
    /// 4. 通过脚本计算最终属性效果，生成improvement
    /// </summary>
    public static void RefreshAffect(Property ob, string type)
    {
        // 参数必须为 string
        Debug.Assert(type is string);

        if (ob == null)
            return;

        // 取角色类型
        int objectType;
        if (ob == ME.user)
            objectType = ObjectType.OBJECT_TYPE_USER;
        else
            objectType = ob.objectType;

        // 根据角色的类型取计算装备效果的脚本
        int scriptNo = GetUpdateScript(objectType);
        if (scriptNo == 0)
            return;

        // 初始化improvement属性
        LPCValue targetImprovement = LPCValue.CreateMapping();
        ob.SetTemp("improvement", targetImprovement);

        // 重置光环属性和触发属性
        ob.SetTemp("halo", LPCValue.CreateMapping());
        ob.SetTemp("trigger", LPCValue.CreateMapping());

        // 要刷新的类型
        List<string> arr = new List<string>();
        if (!string.IsNullOrEmpty(type))
            arr.Add(type);
        else
        {
            // 全部刷新
            foreach (string k in refreshHooks.Keys)
                arr.Add(k);

            // 添加assigned_props属性
            arr.Add("assigned_props");
        }

        // 巡视所有需要计算的附加属性
        string pType;
        for (int i = 0; i < arr.Count; i++)
        {
            // 刷新类型
            pType = arr[i];

            // 初始化缓存的临时附加属性计算结果，先将旧的数据清除之
            ob.DeleteTemp("temp_improvement/" + pType);

            if (pType == "assigned_props")
            {
                // 收集所有临时附加属性
                LPCArray tempProps = ob.Query<LPCArray>("assigned_props", true);

                // 计算全部属性
                if (tempProps != null)
                    CalcAllProps(ob, tempProps, pType);

                continue;
            }

            // 调用回调函数，计算该类型附加属性
            if (refreshHooks.ContainsKey(pType))
                refreshHooks[pType](ob);
        }

        // 合并缓存的计算结果
        // 普通属性合并到improvement
        // 光环属性合并到halo
        // 触发属性合并到trigger
        MergeProps(ob);

        // 重新计算装备者的效果
        ScriptMgr.Call(scriptNo, ob, targetImprovement);

        // 标记提升属性已经通过最终计算
        targetImprovement.AsMapping["_final"] = LPCValue.Create(1);
        ob.SetTemp("improvement", targetImprovement);
    }

    /// <summary>
    /// Calculates all properties.
    /// </summary>
    public static void RefreshAttrib(Property ob, string attrib)
    {
        // 如果正在刷新属性中不处理
        LPCMapping improvement = ob.QueryTemp<LPCMapping>("improvement");
        if (improvement == null ||
            ! improvement.ContainsKey("_final"))
            return;

        // 添加到improvement中
        improvement.Add(attrib, CALC_PET_IMP_ATTRIB.Call(ob, improvement, attrib));

        // 遍历各个需要修正的属性，上限的修正
        foreach (string attribName in PropertyInitConst.AttribList)
        {
            // 获取该属性最大属性
            int maxValue = ob.QueryAttrib("max_" + attribName);

            // 如果当前血量小于maxValue，不处理
            if (ob.Query<int>(attribName) <= maxValue)
                continue;

            // 设置属性
            ob.Set(attribName, LPCValue.Create(maxValue));
        }

        // 重新设置数据
        ob.SetTemp("improvement", LPCValue.Create(improvement));
    }

    /// <summary>
    /// Calculates all properties.
    /// </summary>
    public static void CalcAllProps(Property target, LPCArray allProps, string type)
    {
        // 提升效果必须还没有经过最终计算
        Debug.Assert(target.QueryTemp("improvement/_final") == null);

        // 附加属性的存放路径
        string path = string.Format("temp_improvement/{0}", type);

        LPCArray prop;
        CsvRow row;
        LPCValue script;

        // 逐条属性处理
        for (int i = 0; i < allProps.Count; i++)
        {
            // 逐条属性处理
            prop = allProps[i].AsArray;
            row = GetPropInfo(prop[0].AsInt);

            // 取属性作用脚本与参数
            script = row.Query<LPCValue>("apply_script");

            // 如果特殊内置属性脚本直接处理，以提升计算速度
            if (script.IsString)
            {
                // 在target上缓存计算结果
                CacheCalcResult(row.Query<int>("prop_type"), path, target, row.Query<string>("apply_arg"), prop[1]);
            }
            else
            {
                // 由脚本调用计算
                object r = ScriptMgr.Call(script.AsInt, target, row.Query<LPCValue>("apply_arg"), prop);

                //如果返回
                if (r == null)
                    continue;

                // 转换数据结构
                Dictionary<string, object> retPara = r as Dictionary<string, object>;

                // 缓存计算结果
                CacheCalcResult(row.Query<int>("prop_type"),
                    path,
                    target,
                    (string)retPara["field"],
                    retPara["value"] as LPCValue);
            }
        }
    }

    /// <summary>
    /// 取得属性信息
    /// </summary>
    public static CsvRow GetPropInfo(int propID)
    {
        return PropCsv.FindByKey(propID);
    }

    /// <summary>
    /// 获取属性的描述信息
    /// </summary>
    public static string GetPropDesc(LPCArray prop, int propType)
    {
        int propId = prop[0].AsInt;
        CsvRow propInfo = GetPropInfo(propId);
        if (propInfo == null)
            return string.Empty;

        // 属性值的颜色标签
        string valueColor = string.Empty;

        // 描述文字的颜色标签
        string descColor = string.Empty;

        switch (propType)
        {
            case EquipConst.MAIN_PROP:
                valueColor = "[FDE3C8]";
                descColor = "[FDE3C8]";
                break;

            case EquipConst.PREFIX_PROP:
                valueColor = "[FDE3C8]";
                descColor = "[FDE3C8]";
                break;

            case EquipConst.MINOR_PROP:
                valueColor = "[FDE3C8]";
                descColor = "[ffdd8a]";
                break;

            case EquipConst.SUIT_PROP:
                valueColor = "[ADFFA7]";
                descColor = "[ADFFA7]";
                break;

            default:
                break;
        }

        // 计算描述
        int script = propInfo.Query<int>("desc_script");
        string descArg = LocalizationMgr.Get(propInfo.Query<string>("desc_arg"));
        object ret = ScriptMgr.Call(script, descArg, prop, valueColor);

        // 异常情况没有脚本
        if (ret == null)
            return string.Empty;

        // 返回属性描述
        return string.Format("{0}{1}", descColor, ret as string);
    }

    /// <summary>
    /// 获取属性描述信息
    /// </summary>
    /// <returns>The property desc.</returns>
    /// <param name="prop">Property.</param>
    public static string GetPropDesc(LPCArray prop)
    {
        int propId = prop[0].AsInt;
        CsvRow propInfo = GetPropInfo(propId);
        if (propInfo == null)
            return string.Empty;

        // 计算描述
        int script = propInfo.Query<int>("desc_script");
        string descArg = LocalizationMgr.Get(propInfo.Query<string>("desc_arg"));

        object ret = ScriptMgr.Call(script, descArg, prop, string.Empty);

        return ret as string;
    }

    /// <summary>
    ///  获取属性描述，不带文字文字描述
    /// </summary>
    public static string GetPropValueDesc(LPCArray prop, int rank = 0)
    {
        int propId = prop[0].AsInt;

        // 获取属性信息
        CsvRow propInfo = GetPropInfo(propId);

        if (propInfo == null)
            return string.Empty;

        string color = string.Empty;

        if (rank.Equals(GameSettingMgr.GetSettingInt("equip_intensify_limit_level") - 1))
            color = "[ff5151]";
        else
            color = "[ffffff]";

        // 计算描述
        int script = propInfo.Query<int>("desc_script");
        string valueArg = propInfo.Query<string>("value_arg");
        object ret = ScriptMgr.Call(script, valueArg, prop, color);

        // 异常情况没有脚本
        if (ret == null)
            return string.Empty;

        return ret as string;
    }

    /// <summary>
    /// 取得属性的dbase数据
    /// </summary>
    public static LPCValue GetPropData(int propID)
    {
        return GetPropInfo(propID).Query<LPCValue>("dbase");
    }

    /// <summary>
    /// 根据属性名获取propId
    /// </summary>
    public static int GetPropId(string propName)
    {
        // 不存在的属性
        if (!mNameToPropId.ContainsKey(propName))
            return -1;

        // 返回属性id
        return mNameToPropId[propName];
    }

    /// <summary>
    /// 根据属性名获取存储路径
    /// </summary>
    public static string GetPropPath(string propName)
    {
        // 不存在的属性,默认路径是improvement
        if (! mNameToPath.ContainsKey(propName))
            return string.Format("improvement/{0}", propName);

        // 返回属性存储路径
        return mNameToPath[propName];
    }

    /// <summary>
    /// 取得最终属性计算脚本
    /// </summary>
    public static int GetUpdateScript(int type)
    {
        CsvRow row = UpdateScriptCsv.FindByKey(type);
        if (row == null)
            return 0;

        return row.Query<int>("script");
    }

    /// <summary>
    /// 注册刷新属性回调函数
    /// </summary>
    public static void RegisterRefreshAffectHook(string type, RefreshAffectHook f)
    {
        refreshHooks[type] = f;
    }

    // 对象刷新属性后回调
    public static void RefreshAttribAfter(Property who, List<string> attribList = null)
    {
        // 角色对象不存在，不处理
        if (who == null)
            return;

        // 删除final_attrib，再下一次获取的时候重置缓存
        who.DeleteTemp("final_attrib");

        // 设置角色的速度final_speed
        int finalSpeed = who.Query<int>("final_speed");
        int speed = who.QueryAttrib("speed");
        if (finalSpeed != speed)
            who.Set("final_speed", LPCValue.Create(speed));

        // 设置角色的优先出手标识first_hand
        int final_first_hand = who.Query<int>("final_first_hand");
        int first_hand = who.QueryAttrib("first_hand");
        if (final_first_hand != first_hand)
            who.Set("final_first_hand", LPCValue.Create(first_hand));

        // 没有需要修正的属性，不处理
        if (attribList == null)
            return;

        // 遍历各个需要修正的属性，上限的修正
        foreach (string attribName in attribList)
        {
            // 获取该属性最大属性
            int maxValue = who.QueryAttrib("max_" + attribName);

            // 如果当前血量小于maxValue，不处理
            if (who.Query<int>(attribName) <= maxValue)
                continue;

            // 设置属性
            who.Set(attribName, LPCValue.Create(maxValue));
        }
    }

    /// <summary>
    /// 获取装备描述前缀
    /// </summary>
    public static string GetPropPrefix(int propId)
    {
        CsvRow propRow = GetPropInfo(propId);

        if (propRow == null)
            return string.Empty;

        return LocalizationMgr.Get(propRow.Query<string>("prefix"));
    }

    #endregion
}
