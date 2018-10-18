/// <summary>
/// GameSettingMgr.cs
/// Created by zhaozy 2015-06-25
/// 游戏配置信息表
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using LPC;

// 技能管理
public static class GameSettingMgr
{
    #region 变量

    // 配置表信息
    private static CsvFile mGameSettingCsv;

    #endregion

    #region 属性

    // 配置表信息
    public static CsvFile GameSettingCsv { get { return mGameSettingCsv; } }

    #endregion
    
    #region  公共接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        // 载入配置表信息
        mGameSettingCsv = CsvFileMgr.Load("game_settings_common");
    }

    /// <summary>
    /// 获取数值型配置信息
    /// 没有配置的属性，默认值0x7FFFFFFF
    /// </summary>
    public static int GetSettingInt(string key)
    {
        CsvRow data = mGameSettingCsv.FindByKey(key);

        // 没有配置返回空0x7FFFFFFF
        if (data == null)
            return ConstantValue.MAX_VALUE;

        // 返回配置信息
        return data.Query<int>("value");
    }

    /// <summary>
    /// 获取配置信息
    /// </summary>
    public static LPCValue GetSetting(string key)
    {
        // 返回配置信息
        return GetSetting<LPCValue>(key);
    }

    /// <summary>
    /// 获取配置信息
    /// </summary>
    public static T GetSetting<T>(string key, T def = default(T))
    {
        if(mGameSettingCsv == null)
            return def;

        CsvRow data = mGameSettingCsv.FindByKey(key);

        // 没有配置的字段
        if (data == null)
        {
            LogMgr.Trace("game_settings_common.csv未配置字段{0}", key);
            return def;
        }

        // 返回配置信息
        return data.Query<T>("value", def);
    }

    #endregion
}
