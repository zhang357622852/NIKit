/// <summary>
/// HaloMgr.cs
/// Created by zhaozy 2014-12-25
/// 光环管理
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 玩家管理
/// </summary>
public static class HaloMgr
{
    #region 变量

    // 光环配置信息
    private static CsvFile mHaloCsv;

    #endregion

    #region 属性

    // 获取配置表信息
    public static CsvFile HaloCsv { get { return mHaloCsv; } }

    #endregion

    #region 内部接口

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化接口
    /// </summary>
    public static void Init()
    {
        // 载入配置表信息
        mHaloCsv = CsvFileMgr.Load("halo");
    }

    /// <summary>
    /// 刷新角色的光环效果
    /// </summary>
    /// <param name="ob">Ob.</param>
    public static void RefreshHaloAffect(Property ob, Property targetOb)
    {
        // 角色对象不存在
        if (ob == null || ob.IsDestroyed)
            return;

        // 获取角色的improvement属性列表
        LPCMapping haloMap = ob.QueryTemp<LPCMapping>("halo");
        if (haloMap == null || haloMap.Count == 0)
            return;

        int selectScriptNo = 0;
        int applyScriptNo = 0;
        int clearScriptNo = 0;

        // 遍历各个光环属性
        foreach (string key in haloMap.Keys)
        {
            // 获取配置信息
            CsvRow data = mHaloCsv.FindByKey(key);

            // 配置数据异常
            if (data == null)
                continue;

            // 获取目标选择脚本
            selectScriptNo = data.Query<int>("select_script");
            if (selectScriptNo == 0)
                continue;

            // 获取apply脚本
            applyScriptNo = data.Query<int>("apply_script");
            if (applyScriptNo == 0)
                continue;

            // 调用脚本收集目标
            List<Property> selectList = ScriptMgr.Call(selectScriptNo, ob, data,
                haloMap[key], data.Query<LPCValue>("select_args")) as List<Property>;

            // 没有收集到任何目标
            if (selectList == null ||
                selectList.Count == 0 ||
                selectList.IndexOf(targetOb) == -1)
                continue;

            // 遍历各个目标逐个clear
            clearScriptNo = data.Query<int>("clear_script");
            if (clearScriptNo != 0)
            {
                // 调用脚本clear清除该角色带来的光环效果
                // 同一个角色光环不能重复叠加
                ScriptMgr.Call(clearScriptNo, targetOb, ob, data,
                    haloMap[key], data.Query<LPCValue>("clear_args"));
            }

            // 获取check脚本
            int checkScriptNo = data.Query<int>("check_script");
            if (checkScriptNo != 0)
            {
                // 调用检测脚本
                bool checkRet = (bool)ScriptMgr.Call(checkScriptNo, ob, data,
                    haloMap[key], data.Query<LPCValue>("check_args"));

                // 检测脚本没有通过
                if (!checkRet)
                    continue;
            }

            // 调用脚本apply
            ScriptMgr.Call(applyScriptNo, targetOb, ob, data,
                haloMap[key], data.Query<LPCValue>("apply_args"));
        }
    }

    /// <summary>
    /// 刷新角色的光环效果
    /// </summary>
    /// <param name="ob">Ob.</param>
    public static void RefreshHaloAffect(Property ob)
    {
        // 角色对象不存在
        if (ob == null || ob.IsDestroyed)
            return;

        // 获取角色的improvement属性列表
        LPCMapping haloMap = ob.QueryTemp<LPCMapping>("halo");
        if (haloMap == null || haloMap.Count == 0)
            return;

        int selectScriptNo = 0;
        int applyScriptNo = 0;
        int clearScriptNo = 0;

        // 遍历各个光环属性
        foreach (string key in haloMap.Keys)
        {
            // 获取配置信息
            CsvRow data = mHaloCsv.FindByKey(key);

            // 配置数据异常
            if (data == null)
                continue;

            // 获取目标选择脚本
            selectScriptNo = data.Query<int>("select_script");
            if (selectScriptNo == 0)
                continue;

            // 获取apply脚本
            applyScriptNo = data.Query<int>("apply_script");
            if (applyScriptNo == 0)
                continue;

            // 调用脚本收集目标
            List<Property> selectList = ScriptMgr.Call(selectScriptNo, ob, data,
                                            haloMap[key], data.Query<LPCValue>("select_args")) as List<Property>;

            // 没有收集到任何目标
            if (selectList == null || selectList.Count == 0)
                continue;

            // 遍历各个目标逐个clear
            clearScriptNo = data.Query<int>("clear_script");
            if (clearScriptNo != 0)
            {
                for (int i = 0; i < selectList.Count; i++)
                {
                    // 角色对象不存在
                    if (selectList[i] == null)
                        continue;

                    // 调用脚本clear清除该角色带来的光环效果
                    // 同一个角色光环不能重复叠加
                    ScriptMgr.Call(clearScriptNo, selectList[i], ob, data,
                        haloMap[key], data.Query<LPCValue>("clear_args"));
                }
            }

            // 获取check脚本
            int checkScriptNo = data.Query<int>("check_script");
            if (checkScriptNo != 0)
            {
                // 调用脚本收集目标
                bool checkRet = (bool)ScriptMgr.Call(checkScriptNo, ob, data,
                                    haloMap[key], data.Query<LPCValue>("check_args"));

                // 检测脚本没有通过
                if (!checkRet)
                    continue;
            }

            // 遍历各个目标逐个apply
            for (int i = 0; i < selectList.Count; i++)
            {
                // 角色对象不存在
                if (selectList[i] == null)
                    continue;

                // 调用脚本apply
                ScriptMgr.Call(applyScriptNo, selectList[i], ob, data,
                    haloMap[key], data.Query<LPCValue>("apply_args"));
            }
        }
    }

    #endregion
}
