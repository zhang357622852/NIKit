/// <summary>
/// OptionMgr.cs
/// Created by zhaozy 2015/08/31
/// 提供存储配置开关相关功能
/// </summary>

using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using LPC;

/// <summary>
/// 配置信息管理
/// </summary>
public static class OptionMgr
{
    #region 变量

    // 设置信息表
    private static CsvFile mOptionCsv;

    // 缓存的配置信息
    private static Dictionary<string, LPCValue> mCacheOptionMap = new Dictionary<string, LPCValue>();

    #endregion

    #region 属性

    // 获取配置表信息
    public static CsvFile OptionCsv { get { return mOptionCsv; } }

    #endregion

    #region 私有接口

    // 检查开关设置是否符合要求
    private static bool CheckOptionValid(Property user, string optionName, LPCValue value)
    {
        // 查找配置信息
        CsvRow data = OptionCsv.FindByKey(optionName);

        // 没有该配置选项
        if (data == null)
            return false;

        // 允许设置
        return true;
    }

    /// <summary>
    /// 添加缓存配置信息
    /// </summary>
    private static void AddCacheOption(string optionName, LPCValue value)
    {
        if (mCacheOptionMap.ContainsKey(optionName))
            mCacheOptionMap[optionName] = value;
        else
            mCacheOptionMap.Add(optionName, value);
    }

    /// <summary>
    /// 添加缓存配置信息
    /// </summary>
    private static LPCValue GetCacheOption(string optionName)
    {
        // 没有该缓存数据
        if (!mCacheOptionMap.ContainsKey(optionName))
            return null;

        // 返还缓存数据
        return mCacheOptionMap[optionName];
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        // 载入配置表
        mOptionCsv = CsvFileMgr.Load("option");
    }

    /// <summary>
    /// 设置账号信息
    /// </summary>
    public static bool SetAccountOption(string account, string optionName, LPCValue value)
    {
        // 条件检查没有通过
        if (! CheckOptionValid(null, optionName, value))
            return false;

        // 查找配置信息
        CsvRow data = OptionCsv.FindByKey(optionName);

        // 如果不是public数据不允许设置
        if (data.Query<int>("public") != 1)
        {
            LogMgr.Trace("{0}不是public类型,设置失败。", optionName);
            return false;
        }

        // 添加缓存数据
        string saveOptionName = string.Format("{0}_{1}", account, optionName);
        AddCacheOption(saveOptionName, value);

        // 本地存档全部存成string
        PlayerPrefs.SetString(saveOptionName, LPCSaveString.SaveToString(value));
        PlayerPrefs.Save();

        // 抛出设置系统设置成功事件
        LPCMapping eventPara = new LPCMapping();
        eventPara.Add("option", optionName);
        eventPara.Add("value", value);
        EventMgr.FireEvent(EventMgrEventType.EVENT_SET_OPTION, MixedValue.NewMixedValue<LPCMapping>(eventPara));

        // 存储数据成功
        return true;
    }

    /// <summary>
    /// 获取账号设置信息
    /// </summary>
    public static LPCValue GetAccountOption(string account, string optionName)
    {
        // 查找配置信息
        CsvRow data = OptionCsv.FindByKey(optionName);

        // 没有配置的信息
        if (data == null)
            return null;

        // 如果不是public数据不允许获取
        if (data.Query<int>("public") != 1)
        {
            LogMgr.Trace("{0}不是public类型,获取失败。", optionName);
            return null;
        }

        // 首先获取缓存数据
        string saveOptionName = string.Format("{0}_{1}", account, optionName);
        LPCValue optionValue = GetCacheOption(saveOptionName);

        // 如果有缓存数据
        if (optionValue != null)
            return optionValue;

        // 有本地存储信息
        if (PlayerPrefs.HasKey(saveOptionName))
        {
            // 转换存数数据格式
            optionValue = LPCRestoreString.RestoreFromString(PlayerPrefs.GetString(saveOptionName));
        }
        else
        {
            // 获取配置信息默认值
            optionValue = data.Query<LPCValue>("value");
        }

        // 添加临时缓存数据
        AddCacheOption(saveOptionName, optionValue);

        // 返回数据
        return optionValue;
    }

    /// <summary>
    /// 删除通用设置信息
    /// </summary>
    public static bool DeletePublicOption(string optionName)
    {
        PlayerPrefs.DeleteKey(optionName);
        PlayerPrefs.Save();

        // 移除缓存数据
        if (mCacheOptionMap.ContainsKey(optionName))
            mCacheOptionMap.Remove(optionName);

        return true;
    }

    /// <summary>
    /// 设置通用设置信息
    /// </summary>
    public static bool SetPublicOption(string optionName, LPCValue value)
    {
        // 条件检查没有通过
        if (! CheckOptionValid(null, optionName, value))
            return false;

        // 查找配置信息
        CsvRow data = OptionCsv.FindByKey(optionName);

        // 如果不是public数据不允许设置
        if (data.Query<int>("public") != 1)
        {
            LogMgr.Trace("{0}不是public类型,设置失败。", optionName);
            return false;
        }

        // 添加缓存数据
        AddCacheOption(optionName, value);

        // 本地存档全部存成string
        PlayerPrefs.SetString(optionName, LPCSaveString.SaveToString(value));
        PlayerPrefs.Save();

        // 抛出设置系统设置成功事件
        LPCMapping eventPara = new LPCMapping();
        eventPara.Add("option", optionName);
        eventPara.Add("value", value);
        EventMgr.FireEvent(EventMgrEventType.EVENT_SET_OPTION, MixedValue.NewMixedValue<LPCMapping>(eventPara));

        // 存储数据成功
        return true;
    }

    /// <summary>
    /// 获取通用设置信息
    /// </summary>
    public static LPCValue GetPublicOption(string optionName)
    {
        // 查找配置信息
        CsvRow data = OptionCsv.FindByKey(optionName);

        // 没有配置的信息
        if (data == null)
            return null;

        // 如果不是public数据不允许获取
        if (data.Query<int>("public") != 1)
        {
            LogMgr.Trace("{0}不是public类型,获取失败。", optionName);
            return null;
        }

        // 首先获取缓存数据
        LPCValue optionValue = GetCacheOption(optionName);

        // 如果有缓存数据
        if (optionValue != null)
            return optionValue;

        // 有本地存储信息
        if (PlayerPrefs.HasKey(optionName))
        {
            // 转换存数数据格式
            optionValue = LPCRestoreString.RestoreFromString(PlayerPrefs.GetString(optionName));
        }
        else
        {
            // 获取配置信息默认值
            optionValue = data.Query<LPCValue>("value");
        }

        // 添加临时缓存数据
        AddCacheOption(optionName, optionValue);

        // 返回数据
        return optionValue;
    }

    /// <summary>
    /// 删除角色级的本地缓存
    /// </summary>
    public static bool DeleteOption(Property who, string optionName)
    {
        // 玩家对象不存在
        if (who == null)
            return false;

        // 条件检查没有通过
        if (! CheckOptionValid(who, optionName, null))
        {
            LogMgr.Trace("{0}没有配置,设置失败。", optionName);
            return false;
        }

        PlayerPrefs.DeleteKey(string.Format("{0}/{1}", who.GetRid(), optionName));
        PlayerPrefs.Save();

        // 移除缓存数据
        if (mCacheOptionMap.ContainsKey(optionName))
            mCacheOptionMap.Remove(optionName);

        return true;
    }

    /// <summary>
    /// 设置角色级信息
    /// </summary>
    public static bool SetOption(Property who, string optionName, LPCValue value)
    {
        // 玩家对象不存在
        if (who == null)
            return false;

        // 条件检查没有通过
        if (! CheckOptionValid(who, optionName, value))
        {
            LogMgr.Trace("{0}没有配置,设置失败。", optionName);
            return false;
        }

        // 查找配置信息
        CsvRow data = OptionCsv.FindByKey(optionName);

        // 如果不是public数据不允许获取
        if (data.Query<int>("public") == 1)
        {
            LogMgr.Trace("{0}是public类型,设置失败。", optionName);
            return false;
        }

        // 本地存档
        if (!string.Equals(data.Query<string>("class"), "server"))
        {
            // 本地存档全部存成string
            string optionKey = string.Format("{0}/{1}", who.GetRid(), optionName);
            PlayerPrefs.SetString(optionKey, LPCSaveString.SaveToString(value));
            PlayerPrefs.Save();

            // 添加临时缓存数据
            AddCacheOption(optionKey, value);
        }
        else
        {
            // 服务器存档
            // 本地记一份，不存档，下次登录时服务器会下发
            who.Set(string.Format("option/{0}", optionName), value);

            // 通知服务器更新数据
            Operation.CmdSetOption.Go(optionName, value);
        }

        // 抛出设置系统设置成功事件
        LPCMapping eventPara = new LPCMapping();
        eventPara.Add("option", optionName);
        eventPara.Add("value", value);
        EventMgr.FireEvent(EventMgrEventType.EVENT_SET_OPTION, MixedValue.NewMixedValue<LPCMapping>(eventPara));

        // 存储数据成功
        return true;
    }

    /// <summary>
    /// 设置角色级信息
    /// </summary>
    public static bool SetLocalOption(Property who, string optionName, LPCValue value)
    {
        // 玩家对象不存在
        if (who == null)
            return false;

        // 本地存档全部存成string
        string optionKey = string.Format("{0}/{1}", who.GetRid(), optionName);
        PlayerPrefs.SetString(optionKey, LPCSaveString.SaveToString(value));
        PlayerPrefs.Save();

        // 添加临时缓存数据
        AddCacheOption(optionKey, value);

        // 抛出设置系统设置成功事件
        LPCMapping eventPara = new LPCMapping();
        eventPara.Add("option", optionName);
        eventPara.Add("value", value);
        EventMgr.FireEvent(EventMgrEventType.EVENT_SET_OPTION, MixedValue.NewMixedValue<LPCMapping>(eventPara));

        // 存储数据成功
        return true;
    }

    /// <summary>
    /// 获取角色级设置信息
    /// </summary>
    public static LPCValue GetOption(Property who, string optionName)
    {
        // 玩家对象不存在
        if (who == null)
        {
            LogMgr.Trace("玩家对象不存在,获取信息失败。");
            return null;
        }

        // 查找配置信息
        CsvRow data = OptionCsv.FindByKey(optionName);

        // 没有配置的信息
        if (data == null)
        {
            LogMgr.Trace("{0}没有配置,获取信息失败。", optionName);
            return null;
        }

        // 如果不是public数据不允许获取
        if (data.Query<int>("public") == 1)
        {
            LogMgr.Trace("{0}是public类型,获取失败。", optionName);
            return null;
        }

        LPCValue optionValue;

        // 本地存档
        if (!string.Equals(data.Query<string>("class"), "server"))
        {
            // 首先获取缓存数据
            string optionKey = string.Format("{0}/{1}", who.GetRid(), optionName);
            optionValue = GetCacheOption(optionKey);

            // 如果有缓存数据
            if (optionValue != null)
                return optionValue;

            // 有本地存储信息
            if (PlayerPrefs.HasKey(optionKey))
                optionValue = LPCRestoreString.RestoreFromString(PlayerPrefs.GetString(optionKey));
            else
                optionValue = data.Query<LPCValue>("value");

            // 添加临时缓存数据
            AddCacheOption(optionKey, optionValue);

            // 否则返回默认配置
            return optionValue;
        }

        // 服务器存档数据
        optionValue = who.Query(string.Format("option/{0}", optionName), true);
        return (optionValue == null ? data.Query<LPCValue>("value") : optionValue);
    }

    /// <summary>
    /// 获取角色级设置信息
    /// </summary>
    public static LPCValue GetLocalOption(Property who, string optionName)
    {
        // 玩家对象不存在
        if (who == null)
        {
            LogMgr.Trace("玩家对象不存在,获取信息失败。");
            return null;
        }

        // 首先获取缓存数据
        string optionKey = string.Format("{0}/{1}", who.GetRid(), optionName);
        LPCValue optionValue = GetCacheOption(optionKey);

        // 如果有缓存数据
        if (optionValue != null)
            return optionValue;

        // 有本地存储信息
        if(PlayerPrefs.HasKey(optionKey))
        {
            optionValue = LPCRestoreString.RestoreFromString(PlayerPrefs.GetString(optionKey));

            // 添加临时缓存数据
            AddCacheOption(optionKey, optionValue);
        }
        else
            optionValue = null;

        // 否则返回默认配置
        return optionValue;
    }

    /// <summary>
    /// 清空本地缓存数据
    /// </summary>
    public static void DeleteAllOption(Property user)
    {
        foreach (CsvRow row in OptionCsv.rows)
        {
            if (row.Query<int>("clear_data") != 1)
                continue;

            if (row.Query<int>("public") == 1)
                DeletePublicOption(row.Query<string>("option_name"));
            else
                DeleteOption(user, row.Query<string>("option_name"));
        }
    }

    #endregion
}
