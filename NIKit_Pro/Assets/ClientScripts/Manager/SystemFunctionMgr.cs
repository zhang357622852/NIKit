/// <summary>
/// SystemFunctionMgr.cs
/// Created by fengsc 2017/06/06
/// 系统功能管理模块
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class SystemFunctionMgr
{
    #region 成员变量

    // 配置表数据
    private static Dictionary<int, List<CsvRow>> mConfigData = new Dictionary<int, List<CsvRow>>();

    #endregion

    #region 属性
    #endregion

    #region 内部接口

    /// <summary>
    /// 载入csv表格
    /// </summary>
    private static void LoadCsv(string file)
    {
        // 载表
        CsvFile csv = CsvFileMgr.Load(file);
        if (csv == null)
            return;

        // 清除原有数据
        mConfigData.Clear();

        // 根据需求分类
        foreach (CsvRow row in csv.rows)
        {
            if (row == null)
                continue;

            // 系统功能显示位置
            int show_pos = row.Query<int>("show_pos");

            List<CsvRow> list = new List<CsvRow>();

            mConfigData.TryGetValue(show_pos, out list);

            if (list == null)
                list = new List<CsvRow>();

            list.Add(row);

            mConfigData[show_pos] = list;
        }
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 载入csv表格
        LoadCsv("system_function");
    }

    /// <summary>
    /// 获取等级礼包功能入口配置信息
    /// </summary>
    /// <param name="level"></param>
    /// <returns></returns>
    public static LPCMapping GetLevelGiftConfigByLevel(int level)
    {
        if (ME.user == null)
            return null;

        List<LPCMapping> list = GetFuncList(ME.user, SystemFunctionConst.SCREEN_RIGHT, SceneMgr.MainScene);

        LPCMapping showArgs;

        for (int i = 0; i < list.Count; i++)
        {
            showArgs = list[i].GetValue<LPCMapping>("show_args");

            if (showArgs.ContainsKey("level"))
            {
                if (showArgs.GetValue<int>("level") == level)
                    return list[i];
            }
        }

        return null;
    }

    /// <summary>
    /// 系统功能列表
    /// </summary>
    public static List<LPCMapping> GetFuncList(Property who, int showPos, string curScene)
    {
        // 可显示的功能列表
        List<LPCMapping> funcList = new List<LPCMapping>();

        if (!mConfigData.ContainsKey(showPos))
            return funcList;

        // 获取屏幕左边系统功能配置信息
        List<CsvRow> leftList = mConfigData[showPos];

        if (leftList == null)
            return funcList;

        for (int i = 0; i < leftList.Count; i++)
        {
            CsvRow row = leftList[i];
            if (row == null)
                continue;

            // 显示场景
            LPCArray show_scene = row.Query<LPCArray>("show_scene");
            if (show_scene.IndexOf(curScene) == -1)
                continue;

            LPCMapping dbase = row.Query<LPCMapping>("dbase");

            // 显示判断脚本
            LPCValue show_script = row.Query<LPCValue>("show_script");
            if (show_script.IsInt && show_script.AsInt != 0)
            {
                LPCMapping showArgs = row.Query<LPCMapping>("show_args");

                object ret = ScriptMgr.Call(show_script.AsInt, who, showArgs, dbase, curScene);

                // 不能显示
                if (!(bool)ret)
                    continue;
            }

            LPCMapping Data = row.ConvertLpcMap();

            // 获取数据脚本
            LPCValue get_data_script = row.Query<LPCValue>("get_data_script");
            if (get_data_script.IsInt && get_data_script.AsInt != 0)
            {
                object ret = ScriptMgr.Call(get_data_script.AsInt, who, dbase);
                if (ret != null)
                {
                    if (ret is LPCArray)
                    {
                        LPCArray dataList = ret as LPCArray;

                        for (int j = 0; j < dataList.Count; j++)
                            Data.Append(dataList[j].AsMapping);
                    }
                    else
                    {
                        Data.Append(ret as LPCMapping);
                    }
                }
            }

            funcList.Add(Data);
        }

        return funcList;
    }

    #endregion
}
