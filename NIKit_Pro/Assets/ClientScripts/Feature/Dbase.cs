/// <summary>
/// Dbase.cs
/// Copy from zhangyg 2014-10-22
/// Dbase特性
/// </summary>

using System;
using System.Collections.Generic;
using LPC;

/// <summary>
/// Dbase特性 
/// </summary>
public class Dbase
{
    static  Dictionary<string, string[]> pathSplits = new Dictionary<string, string[]>();
    private LPCMapping dbase = new LPCMapping();

    public Dbase()
    {
    }

    public Dbase(LPCMapping dbase)
    {
        this.dbase = dbase;
    }

    public Dbase(LPCValue dbase)
    {
        if (dbase.IsMapping)
            this.dbase = dbase.AsMapping;
    }

    virtual public void CopyTo(Dbase dbase)
    {
        LPCValue d = LPCValue.CreateMapping();
        d.AsMapping = this.dbase;
        dbase.dbase = LPCValue.Duplicate(d).AsMapping;
    }

    /// <summary>
    /// 取得整个dbase数据 
    /// </summary>
    /// <returns>dbase数据</returns>
    public LPCMapping QueryEntireDbase()
    {
        return this.dbase;
    }

    /// <summary>
    /// 替换整个dbase数据 
    /// </summary>
    /// <param name="dbase">新的数据</param>
    public void ReplaceEntireDbase(LPCMapping dbase)
    {
        this.dbase = dbase;
    }

    /// <summary>
    /// 吸收数据 
    /// </summary>
    virtual public void Absorb(LPCMapping dbase)
    {
        foreach (object k in dbase.Keys)
        {
            if (k is string)
                Set(k as string, dbase [k as string]);
        }
    }

    /// <summary>
    /// 设置数据
    /// </summary>
    /// <param name="path">存放的路径</param>
    /// <param name="v">数据</param>
    virtual public void Set(string path, int v)
    {
        ExpressSet(path, this.dbase, v);
    }

    virtual public void Set(string path, float v)
    {
        ExpressSet(path, this.dbase, v);
    }

    virtual public void Set(string path, string v)
    {
        ExpressSet(path, this.dbase, v);
    }

    virtual public void Set(string path, LPCValue v)
    {
        ExpressSet(path, this.dbase, v);
    }

    /// <summary>
    /// 删除数据 
    /// </summary>
    virtual public void Delete(string path)
    {
        ExpressDelete(path, this.dbase);
    }

    /// <summary>
    /// 检索数据 
    /// </summary>
    public int Query(string path, int defaultValue)
    {
        LPCValue v = ExpressQuery(path, this.dbase);
        if (v != null)
        {
            if (! v.IsInt)
                throw new Exception("value isn't int type.");
            return v.AsInt;
        }

        return defaultValue;
    }

    public float Query(string path, float defaultValue)
    {
        LPCValue v = ExpressQuery(path, this.dbase);
        if (v != null)
        {
            if (! v.IsFloat)
                throw new Exception("value isn't float type.");
            return v.AsFloat;
        }

        return defaultValue;
    }

    public string Query(string path, string defaultValue)
    {
        LPCValue v = ExpressQuery(path, this.dbase);
        if (v != null)
        {
            if (! v.IsString)
                throw new Exception("value isn't string type:" + v.ToString());
            return v.AsString;
        }

        return defaultValue;
    }

    public LPCValue Query(string path)
    {
        return ExpressQuery(path, this.dbase);
    }

    /// <summary>
    /// 清空数据 
    /// </summary>
    public void Clear()
    {
        this.dbase = new LPCMapping();
    }

    /// <summary>
    /// 重载ToString函数 
    /// </summary>
    public override string ToString()
    {
        return this.dbase._GetDescription(0);
    }

    #region Express的操作

    /// <summary>
    /// 等同于LPC的express_set 
    /// </summary>
    public static void ExpressSet(string path, LPCMapping dbase, string v)
    {
        ExpressSet(path, dbase, LPCValue.Create(v));
    }

    public static void ExpressSet(string path, LPCMapping dbase, int v)
    {
        ExpressSet(path, dbase, LPCValue.Create(v));
    }

    public static void ExpressSet(string path, LPCMapping dbase, float v)
    {
        ExpressSet(path, dbase, LPCValue.Create(v));
    }

    public static void ExpressSet(string path, LPCMapping dbase, LPCValue v)
    {
        // 截断路径
        string[] steps;
        if (! pathSplits.TryGetValue(path, out steps))
        {
            // 截断路径
            steps = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (steps.Length < 1)
                throw new Exception("bad path format.");

            // 添加到缓存中
            pathSplits.Add(path, steps);

#if UNITY_EDITOR
            // 缓存数据过大
            if (pathSplits.Count > 1024)
                LogMgr.Trace("保存的分割string数量过多，告知zhaozy");
#endif
        }

        LPCMapping curr = dbase;
        for (int i = 0; i < steps.Length - 1; i++)
        {
            string step = steps [i];
            if (! curr.ContainsKey(step))
            {
                // 此路径不存在，需要创建之
                LPCValue m = LPCValue.CreateMapping();
                curr.Add(step, m);
                curr = m.AsMapping;
                continue;
            }

            // 取得当前路径的数据
            LPCValue data = curr[step];
            if (data.IsMapping)
            {
                // 这是个mapping数据，继续下一级
                curr = data.AsMapping;
                continue;
            }

            // 覆盖掉
            curr.Remove(step);
            LPCValue newData = LPCValue.CreateMapping();
            curr.Add(step, newData);
            curr = newData.AsMapping;
        }

        // 设置数据
        string last = steps [steps.Length - 1];
        if (curr.ContainsKey(last))
            curr.Remove(last);
        curr.Add(last, v);
    }

    /// <summary>
    /// 对应于LPC的express_delete 
    /// </summary>
    public static void ExpressDelete(string path, LPCMapping dbase)
    {
        // 截断路径
        string[] steps;
        if (! pathSplits.TryGetValue(path, out steps))
        {
            // 截断路径
            steps = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (steps.Length < 1)
                throw new Exception("bad path format.");

            // 添加到缓存中
            pathSplits.Add(path, steps);

#if UNITY_EDITOR
            // 缓存数据过大
            if (pathSplits.Count > 1024)
                LogMgr.Trace("保存的分割string数量过多，告知zhaozy");
#endif
        }

        LPCMapping curr = dbase;
        int i = 0;
        for (i = 0; i < steps.Length - 1; i++)
        {
            // 获取数据
            LPCValue v = curr[steps[i]];
            if (v == null || ! v.IsMapping)
                return;

            // 转换数据格式
            curr = v.AsMapping;
        }

        // 数据发生异常
        if (i >= steps.Length)
            throw new Exception("Unexcepted error.");

        // 删除数据
        curr.Remove(steps[i]);
    }

    /// <summary>
    /// 对应于LPC的express_query 
    /// </summary>
    public static string ExpressQuery(string path, LPC.LPCMapping dbase, string defaultVal)
    {
        LPC.LPCValue v = ExpressQuery(path, dbase);
        if (v != null)
        {
            if (!v.IsString)
                throw new Exception("value is not string type.");

            return v.AsString;
        }

        return defaultVal;
    }

    public static int ExpressQuery(string path, LPCMapping dbase, int defaultValue)
    {
        LPCValue v = ExpressQuery(path, dbase);
        if (v != null)
        {
            if (! v.IsInt)
                throw new Exception("value isn't int type.");
            return v.AsInt;
        }

        return defaultValue;
    }

    public static float ExpressQuery(string path, LPCMapping dbase, float defaultValue)
    {
        LPCValue v = ExpressQuery(path, dbase);
        if (v != null)
        {
            if (! v.IsFloat)
                throw new Exception("value isn't int float.");
            return v.AsFloat;
        }

        return defaultValue;
    }

    public static LPCValue ExpressQuery(string path, LPCMapping dbase)
    {
        // 截断路径
        string[] steps;
        if (! pathSplits.TryGetValue(path, out steps))
        {
            // 截断路径
            steps = path.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            if (steps.Length < 1)
                throw new Exception("bad path format.");

            // 添加到缓存中
            pathSplits.Add(path, steps);

#if UNITY_EDITOR
            // 缓存数据过大
            if (pathSplits.Count > 1024)
                LogMgr.Trace("保存的分割string数量过多，告知zhaozy");
#endif

        }

        LPCMapping curr = dbase;
        int i = 0;
        for (i = 0; i < steps.Length - 1; i++)
        {
            // 获取数据
            LPCValue v = curr[steps[i]];
            if (v == null || ! v.IsMapping)
                return null;

            // 转换数据格式
            curr = v.AsMapping;
        }

        if (i < steps.Length)
            return curr[steps[i]];

        // 转换数据格式
        return LPCValue.Create(curr);
    }

    #endregion
}
