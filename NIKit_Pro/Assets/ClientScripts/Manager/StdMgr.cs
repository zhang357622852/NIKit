/// <summary>
/// StdMgr.cs
/// Create by zhaozy 2016-07-13
/// 标准数值管理模块
/// </summary>

using System;
using System.Diagnostics;
using System.Collections.Generic;
using LPC;

/// 道具管理器
public static class StdMgr
{
    #region 变量

    // 标准属性
    private static CsvFile mStdAttribCsv;

    // 标准经验
    private static CsvFile mStdExpCsv;

    // 标准属性缩放
    private static Dictionary<string, CsvRow> mStdAttribScale = new Dictionary<string, CsvRow>();

    // 好友地下城标准属性
    private static CsvFile mFriendDungeonStdAttribCsv;

    /// <summary>
    /// 通天塔标准属性配置
    /// </summary>
    private static Dictionary<int, Dictionary<int, CsvRow>> mTowerStdAttribFile = new Dictionary<int, Dictionary<int, CsvRow>>();

    #endregion

    #region 私有接口

    /// <summary>
    /// 载入属性缩放配置表
    /// </summary>
    /// <param name="fileName">File name.</param>
    private static void LoadStdAttribScale(string fileName)
    {
        // 载入配置表
        CsvFile file = CsvFileMgr.Load(fileName);

        // 载入配置表信息失败
        if (file == null)
            return;

        // 清除数据
        mStdAttribScale.Clear();

        // 遍历数据重新组织数据
        foreach (CsvRow data in file.rows)
            mStdAttribScale.Add(string.Format("{0}_{1}", data.Query<int>("type"), data.Query<int>("map_type")), data);
    }


    /// <summary>
    /// 载入通天塔标准属性配置表
    /// </summary>
    /// <param name="fileName">File name.</param>
    private static void LoadTowerStdAttrib(string fileName)
    {
        // 载入副本配置表
        CsvFile stdAttribFile = CsvFileMgr.Load(fileName);

        // 载入配置表信息失败
        if (stdAttribFile == null)
            return;

        // 清空数据
        mTowerStdAttribFile.Clear();

        int difficulty = 0;
        int layer = 0;

        // 数据分类
        foreach (CsvRow data in stdAttribFile.rows)
        {
            // 获取难度
            difficulty = data.Query<int>("difficulty");
            layer = data.Query<int>("layer");

            // 初始化数据
            if (!mTowerStdAttribFile.ContainsKey(difficulty))
                mTowerStdAttribFile.Add(difficulty, new Dictionary<int, CsvRow>());

            // 记录数据
            mTowerStdAttribFile[difficulty].Add(layer, data);
        }
    }

    #endregion

    #region 功能接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        // 标准属性
        mStdAttribCsv = CsvFileMgr.Load("std_attrib");

        // 标准经验
        mStdExpCsv = CsvFileMgr.Load("std_exp");

        // 载入好友地下城标准属性配置表
        mFriendDungeonStdAttribCsv = CsvFileMgr.Load("secret_dungeon_std_attrib");

        // 载入标准属性缩放配置表
        LoadStdAttribScale("std_attrib_scale");

        // 载入通天塔标准属性配置表
        LoadTowerStdAttrib("tower_std_attrib");
    }

    /// <summary>
    /// 获取通天塔
    /// </summary>
    public static int GetTowerStdAttrib(int difficulty, int layer, string attrib)
    {
        // 没有配置的难度
        Dictionary<int, CsvRow> difficultyMap;
        if (!mTowerStdAttribFile.TryGetValue(difficulty, out difficultyMap))
            return 0;

        // 没有配置的难度
        CsvRow data;
        if (!difficultyMap.TryGetValue(layer, out data))
            return 0;

        // 获取相应属性数字
        return data.Query<int>(attrib);
    }

    /// <summary>
    /// 获取好友地下城标准属性
    /// </summary>
    public static int GetFriendDungeonStdAttrib(int rebornTimes, string attrib)
    {
        // 获取配置表信息
        CsvRow data = mFriendDungeonStdAttribCsv.FindByKey(rebornTimes);

        // 没有配置数据
        if (data == null)
            return 0;

        // 获取相应属性数字
        return data.Query<int>(attrib);
    }

    /// <summary>
    /// Gets the std scale.
    /// </summary>
    public static int GetStdAttribScale(int type, int mapType, string attrib, LPCMapping para)
    {
        // 获取数据存储位置
        string path = string.Format("{0}_{1}", type, mapType);
        CsvRow data;

        // 没有配置属性
        if (! mStdAttribScale.TryGetValue(path, out data))
            return 0;

        // 获取配置信息
        LPCValue value = data.Query<LPCValue>(attrib);

        // 直接配置的是数字
        if (value.IsInt)
            return value.AsInt;

        // 如果是string，公式名
        if (value.IsString)
            return (int) FormulaMgr.InvokeFormulaByName(value.AsString, type, mapType, para);

        // 不支持的类型直接返回0
        return 0;
    }

    /// <summary>
    /// 获取宠物标准经验
    /// </summary>
    public static int GetPetStdExp(int level, int star)
    {
        // 获取配置信息
        CsvRow expRow = mStdExpCsv.FindByKey(level);

        // 没有配置信息
        if (expRow == null)
            return 0;

        // 获取cd信息
        return expRow.Query<int>(star.ToString());
    }

    /// <summary>
    /// 获取宠物当前等级到满级需要的经验
    /// </summary>
    /// <param name="curLevel">当前等级</param>
    /// <param name="curStar">当前星级</param>
    /// <returns></returns>
    public static int GetPetExpsToMaxLV(int curLevel, int curStar)
    {
        int maxLevel = GetStdAttrib("max_level", curStar);
        int totalExps = 0;

        for (int i = curLevel; i <= maxLevel; i++)
        {
            totalExps += GetPetStdExp(i, curStar);
        }

        return totalExps;
    }

    /// <summary>
    /// 获取宠物标准经验
    /// </summary>
    public static int GetUserStdExp(int level)
    {
        // 获取配置信息
        CsvRow expRow = mStdExpCsv.FindByKey(level);

        // 没有配置信息
        if (expRow == null)
            return 0;

        // 获取cd信息
        return expRow.Query<int>("exp");
    }

    /// <summary>
    /// 获取宠物标准经验
    /// </summary>
    public static int GetStdAttrib(string attrib, int star)
    {
        // 获取配置信息
        CsvRow attribRow = mStdAttribCsv.FindByKey(attrib);

        // 没有配置信息
        if (attribRow == null)
            return 0;

        // 获取cd信息
        return attribRow.Query<int>(star.ToString());
    }

    #endregion
}
