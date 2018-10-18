/// <summary>
/// TriggerDbase.cs
/// Copy from zhangyg 2014-10-22
/// 带触发器的Dbase
/// </summary>

using System;
using System.Collections.Generic;
using UnityEngine;
using LPC;

/// <summary>
/// 带触发器的Dbase
/// </summary>
public class TriggerDbase : Dbase
{
    // 触发器的回调列表
    private Dictionary<string, Dictionary<string, CallBack>> cbs = new Dictionary<string, Dictionary<string, CallBack>>();

    public TriggerDbase() : base()
    {
    }
    public TriggerDbase(LPCMapping data) : base(data)
    {
    }

    public void CopyTo(TriggerDbase dbase)
    {
        base.CopyTo(dbase);
        dbase.cbs = new Dictionary<string, Dictionary<string, CallBack>>(this.cbs);
    }

    /// <summary>
    /// 重载Absorb接口
    /// </summary>
    public override void Absorb(LPCMapping dbase)
    {
        foreach (string k in dbase.Keys)
            Set(k, dbase [k]);
    }

    /// <summary>
    /// 重载Set接口
    /// </summary>
    public override void Set(string path, int v)
    {
        base.Set(path, v);
        TriggerField(path);
    }
    public override void Set(string path, float v)
    {
        base.Set(path, v);
        TriggerField(path);
    }
    public override void Set(string path, string v)
    {
        base.Set(path, v);
        TriggerField(path);
    }
    public override void Set(string path, LPCValue v)
    {
        base.Set(path, v);
        TriggerField(path);
    }

    /// <summary>
    /// 重载Delete接口
    /// </summary>
    public override void Delete(string path)
    {
        base.Delete(path);
        TriggerField(path);
    }

    /// <summary>
    /// 注册触发器
    /// </summary>
    /// <param name="id">触发器的名字，应该具备唯一性</param>
    /// <param name="fields">关注的字段名</param>
    /// <param name="cb">回调处理</param>
    public void RegisterTriggerField(string id, string[] fields, CallBack cb)
    {
        foreach (string field in fields)
        {
            if (! this.cbs.ContainsKey(field))
                this.cbs.Add(field, new Dictionary<string, CallBack>());
            Dictionary<string, CallBack> data = this.cbs [field];

            if (data.ContainsKey(id))
            {
                LogMgr.Trace("[TriggerDbase.cs] 字段{0}的触发器{1}已经存在，忽略。", field, id);
                continue;
            }
            data.Add(id, cb);
        }
    }

    /// <summary>
    /// 反注册触发器
    /// </summary>
    public void RemoveTriggerField(string id)
    {
        foreach (Dictionary<string, CallBack> data in this.cbs.Values)
        {
            data.Remove(id);
        }
    }

    // 触发器
    private void TriggerField(string path)
    {
        Dictionary<string, CallBack> tasks = null;
        cbs.TryGetValue(path, out tasks);
        if (tasks != null)
        {
            foreach (CallBack task in new List<CallBack>(tasks.Values))
            {
                try
                {
                    task.Go();
                } catch (Exception e)
                {
                    LogMgr.Exception(e);
                }
            }
        }
    }
}
