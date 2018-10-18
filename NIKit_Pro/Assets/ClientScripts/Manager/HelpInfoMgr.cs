/// <summary>
/// HelpInfoMgr.cs
/// Created by fengsc 2016/07/11
///帮助信息管理类
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class HelpInfoMgr 
{
    #region 成员变量;

    // 帮助信息配置表;
    private static CsvFile mHelpCsv;

    #endregion

    #region 属性

    // 帮助信息配置表
    public static CsvFile HelpCsv { get { return mHelpCsv; } }

    #endregion

    #region 公共接口

    /// <summary>
    ///初始化数据
    /// </summary>
    public static void Init()
    {
        //加载帮助信息配置表;
        mHelpCsv = CsvFileMgr.Load("help");
    }

    /// <summary>
    /// 获取帮助描述信息列表,根据类型获取对应的描述信息
    /// </summary>
    public static CsvRow GetDescList(int helpId)
    {
        return HelpCsv.FindByKey(helpId);
    }

    #endregion
}
