/// <summary>
/// CombatActionMgr.cs
/// Created by wangxw 2014-11-05
/// 战斗行为管理器
/// </summary>

using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;
using LPC;

public static class CombatActionMgr
{
    #region 成员变量

    // 行为集合资源列表
    private static Dictionary<string, ActionSetData> mActionSetDataMap = new Dictionary<string, ActionSetData>();

    // 行为集合实例索引表
    private static HashSet<CombatActionSet> mActionSetListSet = new HashSet<CombatActionSet>();

    // 行为节点实例索引表
    private static HashSet<ActionBase> mActionListSet = new HashSet<ActionBase>();

    // 独立驱动的序列
    private static List<CombatActionSet> mIndependentActionSetDriveList = new List<CombatActionSet>();

    #endregion

    #region 内部函数

    // 在移动平台上用Resources.Load加载
    private static void LoadAllSkillActionData()
    {
        // 清理数据
        mActionSetDataMap.Clear();

        // 解析skill_action Xml文件
        foreach (string actionFile in ResourceMgr.LoadAllSkillActionFile())
            ParseSkillActionFile(actionFile);
    }

    // 移动平台下的技能节点解析接口，传入参数略有不同
    private static void ParseSkillActionFile(string str)
    {
        XmlDocument doc = new XmlDocument();
        doc.LoadXml(str);
        // 遍历 <action_set> 节点
        XmlNodeList asnList = doc.SelectNodes("skill_action/action_set");
        if (asnList.Count <= 0)
            return;

        IEnumerator asnIt = asnList.GetEnumerator();
        while (asnIt.MoveNext())
        {
            ActionSetData newASD = new ActionSetData();
            XmlNode asn = (XmlNode)asnIt.Current;

            // 读取各个属性
            newASD.Name = asn.Attributes ["name"].Value;                                                        // 名称
            //Game.EnumTryParse<ActionSetType>(asn.Attributes ["type"].Value, out newASD.ASType);                 // 类型
            //Game.EnumTryParse<SpeedControlType>(asn.Attributes ["speed_factor"].Value, out newASD.SCType);      // 速度因子
            // 类型
            newASD.ASType = ActionSetType.GetTypeByAlias(asn.Attributes ["type"].Value);

            // 速度因子
            newASD.SCType = SpeedControlType.GetTypeByAlias(asn.Attributes ["speed_factor"].Value);

            // 遍历 <action> 节点 
            XmlNodeList anList = asn.SelectNodes("action");
            if (asnList.Count <= 0)
                continue;

            IEnumerator anIt = anList.GetEnumerator();
            while (anIt.MoveNext())
            {
                ActionData newAD = new ActionData();
                XmlNode an = (XmlNode)anIt.Current;

                // 读取各个属性
                XmlAttribute tempAnAtt = null;
                // 节点对象类型
                if ((tempAnAtt = an.Attributes ["type"]) != null)
                    newAD.NodeType = Type.GetType(tempAnAtt.Value);

                // 取消时中止节点
                if ((tempAnAtt = an.Attributes ["stop_when_cancel"]) != null)
                    bool.TryParse(tempAnAtt.Value, out newAD.StopWhenCancel);

                // 取消后中止创建
                if ((tempAnAtt = an.Attributes ["create_when_canceled"]) != null)
                    bool.TryParse(tempAnAtt.Value, out newAD.CreateWhenCanceled);

                if (newAD.NodeType == null)
                {
                    LogMgr.Trace("无法解析action节点类型：{0}", an.Attributes ["type"].Value);
                    continue;
                }

                // 遍历 <property> 节点
                XmlNodeList propList = an.SelectNodes("property");
                IEnumerator propIt = propList.GetEnumerator();
                while (propIt.MoveNext())
                {
                    XmlNode pn = (XmlNode)propIt.Current;

                    // 解析并添加各个属性
                    XmlAttribute tempPnAtt = null;
                    string propName = ((tempPnAtt = pn.Attributes ["name"]) != null) ? tempPnAtt.Value : string.Empty; // 属性名
                    string propValue = ((tempPnAtt = pn.Attributes ["value"]) != null) ? tempPnAtt.Value : string.Empty; // 属性值
                    string propCtype = ((tempPnAtt = pn.Attributes ["ctype"]) != null) ? tempPnAtt.Value : string.Empty; // 值类型
                    ActionDataPropertyParser.Parse(ref newAD, ref newASD, propName, propValue, propCtype);
                }

                // 节点触发时间
                if ((tempAnAtt = an.Attributes ["start_time"]) != null)
                {
                    float startTime = -1;
                    if (float.TryParse(tempAnAtt.Value, out startTime))
                    {
                        // 添加进AD列表，时间触发
                        newAD.StartTime = startTime; // 记录时间
                        newASD.ActionDataList.Add(newAD);
                    } else
                    {
                        // 添加进AD列表，事件触发
                        newAD.StartEvent = tempAnAtt.Value;
                        if (! newASD.EventActionDataList.ContainsKey(newAD.StartEvent))
                            newASD.EventActionDataList.Add(newAD.StartEvent, new List<ActionData>());

                        newASD.EventActionDataList [newAD.StartEvent].Add(newAD);
                    }
                }
            }

            // 时间排序
            newASD.ActionDataList = ActionDataListSort(newASD.ActionDataList);

            // 添加进ASD列表
            mActionSetDataMap.Add(newASD.Name, newASD);
        }
    }

    // 数据比较函数，按触发时间来区分大小
    public static List<ActionData> ActionDataListSort(List<ActionData> actionDataList)
    {
        // 遍历各个序列
        for (int i = 1; i < actionDataList.Count; ++i)
        {
            ActionData tempData = actionDataList[i];
            int j = i;

            // 掺入排序
            while ((j > 0) && (actionDataList[j - 1].StartTime > tempData.StartTime))
            {
                actionDataList[j] = actionDataList[j - 1];
                --j;
            }

            // 交换数据
            actionDataList[j] = tempData;
        }

        // 返回数据
        return actionDataList;
    }

    /// <summary>
    /// 调用技能脚本，获取实例化参数
    /// </summary>
    /// <returns>The script parameter.</returns>
    /// <param name="data">静态数据</param>
    /// <param name="args">脚本参数</param>
    private static PropertiesParameter ConvertScriptParameter(ActionSetData data, LPCMapping args)
    {
        PropertiesParameter para = new PropertiesParameter();

        // 遍历所有节点数据，取出所有的脚本
        foreach (KeyValuePair<string, KeyValuePair<int, MixedValue>> mks in data.DynamicIDSet)
        {
            object ret = ScriptMgr.Call(mks.Value.Key, args, mks.Value.Value);

            // 异常情况没有脚本
            if (ret == null)
                para.Parameter.Add(mks.Key, new MixedValue());
            else
                para.Parameter.Add(mks.Key, ret as MixedValue);
        }

        return para;
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Init()
    {
        // 加载资源数据
        LoadAllSkillActionData();
    }

    /// <summary>
    /// 驱动战斗系统更新
    /// </summary>
    /// <param name="deltaTime">相对上一帧流失的时间</param>
    public static void Update(float deltaTime)
    {
        // 时间参数
        // 独立驱动列表中的节点，默认使用1.0时间缩放
        TimeDeltaInfo info = new TimeDeltaInfo(deltaTime, 1.0f);

        // 驱动行为列表执行
        for (int index = mIndependentActionSetDriveList.Count - 1; index >= 0; index--)
        {
            CombatActionSet cas = mIndependentActionSetDriveList[index];

            // 移除非法数据
            if (cas == null)
            {
                mIndependentActionSetDriveList.RemoveAt(index);
                continue;
            }

            // 优先尝试结束操作
            if (cas.ShouldEnded())
            {
                cas.End();
                CombatActionMgr.DestroyActionSet(cas);
                mIndependentActionSetDriveList.RemoveAt(index);
            }
            else
            {
                // 驱动一次
                cas.Update(info);
            }
        }
    }

    /// <summary>
    /// 转换指定的ActionSet到管理器内部独立驱动
    /// </summary>
    /// <param name="actionSet">行为序列对象</param>
    public static void TransferActionSet(CombatActionSet actionSet)
    {
        // 存入列表即可
        mIndependentActionSetDriveList.Add(actionSet);
    }

    /// <summary>
    /// 是否拥有指定名称的ActionSet
    /// </summary>
    public static bool HasActionSetData(string actionSetName)
    {
        return mActionSetDataMap.ContainsKey(actionSetName);
    }

    /// <summary>
    /// 获取指定名称的ActionSetData数据
    /// </summary>
    /// <returns>The action set data.</returns>
    /// <param name="dataName">数据名</param>
    public static ActionSetData GetActionSetData(string dataName)
    {
        ActionSetData data = null;
        if (! mActionSetDataMap.TryGetValue(dataName, out data))
            return null;

        // 返回数据
        return data;
    }

    /// <summary>
    /// 获取指定名称的ActionSetType数据
    /// </summary>
    public static int GetActionSetType(string dataName)
    {
        ActionSetData data = null;
        if (!mActionSetDataMap.TryGetValue(dataName, out data))
            return ActionSetType.AST_ACT;

        // 返回技能序列类型
        return data.ASType;
    }

    /// <summary>
    /// 创建行为序列
    /// </summary>
    /// <returns>The action set.</returns>
    /// <param name="actionSetName">序列名</param>
    /// <param name="cookieValue">序列编号</param>
    /// <param name="actor">序列绑定的目标角色，为null则表示独立序列</param>
    /// <param name="para">战斗参数</param>
    public static CombatActionSet CreateActionSet(string actionSetName, string cookieValue, CombatActor actor, LPCMapping args)
    {
        // 获取配置数据
        ActionSetData data = GetActionSetData(actionSetName);
        if (data == null)
            return null;

        // 逻辑脚本数据转换
        PropertiesParameter para = ConvertScriptParameter(data, args);

        // 创建新序列
        CombatActionSet newAS = new CombatActionSet(cookieValue, actor, data, para);
        newAS.ExtraArgs = args;
        mActionSetListSet.Add(newAS);
        return newAS;
    }

    /// <summary>
    /// 销毁行为序列
    /// </summary>
    /// <param name="actionSet">Action set.</param>
    public static void DestroyActionSet(CombatActionSet actionSet)
    {
        // 找出来，删掉
        mActionSetListSet.Remove(actionSet);
    }

    /// <summary>
    /// 销毁所有行为序列
    /// </summary>
    public static void DestroyAllActionSet()
    {
        mIndependentActionSetDriveList.Clear();
        mActionSetListSet.Clear();
    }

    /// <summary>
    /// 战斗行为节点
    /// </summary>
    /// <returns>The action.</returns>
    /// <param name="actor">绑定角色</param>
    /// <param name="actionSet">所属序列</param>
    /// <param name="para">动态参数</param>
    public static ActionBase CreateAction(CombatActor actor, CombatActionSet actionSet, ActionData data, PropertiesParameter para)
    {
        System.Diagnostics.Debug.Assert(data != null);
        System.Diagnostics.Debug.Assert(para != null);

        // 准备参数
        para.RefActionData = data;
        object[] args = { actor, actionSet, para };

        // 按类型构造
        Type nodeType = data.NodeType;
        ActionBase action = Activator.CreateInstance(nodeType, args) as ActionBase;

        mActionListSet.Add(action);
        return action;
    }

    /// <summary>
    /// 销毁行为节点
    /// </summary>
    /// <param name="action">Action.</param>
    public static void DestroyAction(ActionBase action)
    {
        // 找出来，删掉
        mActionListSet.Remove(action);
    }

    /// <summary>
    /// 销毁所有行为节点
    /// </summary>
    public static void DestroyAllAction()
    {
        mActionListSet.Clear();
    }

#if UNITY_EDITOR

    /// <summary>
    /// 重新加载技能数据
    /// </summary>
    public static void ReloadSkillActionData()
    {
        // 清理数据
        mActionSetDataMap.Clear();

        // 遍历所有配置文件，解析之
        DirectoryInfo dirInfo = new DirectoryInfo("Assets/ActionSet");
        if (!dirInfo.Exists)
            return;

        FileInfo[] files = dirInfo.GetFiles(CombatConfig.SKILL_ACTION_FILENAME);
        foreach (FileInfo file in files)
            DebugParseSkillActionFile(file);
    }

    /// <summary>
    /// debug解析指定的技能序列文件
    /// </summary>
    /// <param name="file">File.</param>
    private static void DebugParseSkillActionFile(FileInfo file)
    {
        // 读取技能配置xml表格
        XmlDocument doc = new XmlDocument();
        doc.Load(file.OpenRead());

        // 遍历 <action_set> 节点
        XmlNodeList asnList = doc.SelectNodes("skill_action/action_set");
        if (asnList.Count <= 0)
            return;

        IEnumerator asnIt = asnList.GetEnumerator();
        while (asnIt.MoveNext())
        {
            ActionSetData newASD = new ActionSetData();
            XmlNode asn = (XmlNode)asnIt.Current;

            // 读取各个属性
            newASD.Name = asn.Attributes["name"].Value;                                                        // 名称

            // 类型
            newASD.ASType = ActionSetType.GetTypeByAlias(asn.Attributes["type"].Value);

            // 速度因子
            newASD.SCType = SpeedControlType.GetTypeByAlias(asn.Attributes["speed_factor"].Value);

            // Game.EnumTryParse<int>(asn.Attributes ["type"].Value, out newASD.ASType);                 // 类型
            // Game.EnumTryParse<int>(asn.Attributes ["speed_factor"].Value, out newASD.SCType);      // 速度因子

            // 遍历 <action> 节点
            XmlNodeList anList = asn.SelectNodes("action");
            if (asnList.Count <= 0)
                continue;

            IEnumerator anIt = anList.GetEnumerator();
            while (anIt.MoveNext())
            {
                ActionData newAD = new ActionData();
                XmlNode an = (XmlNode)anIt.Current;

                // 读取各个属性
                XmlAttribute tempAnAtt = null;

                // 节点对象类型
                if ((tempAnAtt = an.Attributes["type"]) != null)
                    newAD.NodeType = Type.GetType(tempAnAtt.Value);

                // 取消时中止节点
                if ((tempAnAtt = an.Attributes["stop_when_cancel"]) != null)
                    bool.TryParse(tempAnAtt.Value, out newAD.StopWhenCancel);

                // 取消后中止创建
                if ((tempAnAtt = an.Attributes["create_when_canceled"]) != null)
                    bool.TryParse(tempAnAtt.Value, out newAD.CreateWhenCanceled);

                if (newAD.NodeType == null)
                {
                    LogMgr.Trace("无法解析action节点类型：{0}", an.Attributes["type"].Value);
                    continue;
                }

                // 遍历 <property> 节点
                XmlNodeList propList = an.SelectNodes("property");
                IEnumerator propIt = propList.GetEnumerator();

                while (propIt.MoveNext())
                {
                    XmlNode pn = (XmlNode)propIt.Current;

                    // 解析并添加各个属性
                    XmlAttribute tempPnAtt = null;
                    string propName = ((tempPnAtt = pn.Attributes["name"]) != null) ? tempPnAtt.Value : string.Empty; // 属性名
                    string propValue = ((tempPnAtt = pn.Attributes["value"]) != null) ? tempPnAtt.Value : string.Empty; // 属性值
                    string propCtype = ((tempPnAtt = pn.Attributes["ctype"]) != null) ? tempPnAtt.Value : string.Empty; // 值类型
                    ActionDataPropertyParser.Parse(ref newAD, ref newASD, propName, propValue, propCtype);
                }

                // 节点触发时机
                // float，表示在指定时间触发Action
                // string，表示通过事件触发Action
                if ((tempAnAtt = an.Attributes["start_time"]) == null)
                    continue;

                float startTime = -1;
                if (float.TryParse(tempAnAtt.Value, out startTime))
                {
                    // 添加进AD列表，时间触发
                    newAD.StartTime = startTime; // 记录时间
                    newASD.ActionDataList.Add(newAD);
                }
                else
                {
                    // 添加进AD列表，事件触发
                    newAD.StartEvent = tempAnAtt.Value;
                    if (!newASD.EventActionDataList.ContainsKey(newAD.StartEvent))
                        newASD.EventActionDataList.Add(newAD.StartEvent, new List<ActionData>());

                    newASD.EventActionDataList[newAD.StartEvent].Add(newAD);
                }
            }

            // 时间排序
            newASD.ActionDataList = ActionDataListSort(newASD.ActionDataList);

            // 添加进ASD列表
            if (mActionSetDataMap.ContainsKey(newASD.Name))
                LogMgr.Trace("{0}中，名为\"{1}\"的节点被重复配置", file.Name, newASD.Name);
            else
                mActionSetDataMap.Add(newASD.Name, newASD);
        }
    }

    #endif

    #endregion
}