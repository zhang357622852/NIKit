/// <summary>
/// Attrib.cs
/// Copy from zhangyg 2014-10-22
/// 属性特性
/// </summary>

using LPC;

/// <summary>
/// 属性特性 
/// </summary>
public class Attrib
{
    private Property owner;

    public Attrib(Property property)
    {
        this.owner = property;
    }

    /// <summary>
    /// 判断是否可用支付消耗
    /// </summary>
    public bool CanCostAttrib(LPCMapping data)
    {
        // 没有消耗详细信息
        if (data == null)
            return false;

        // 获得消耗者的当前数据
        LPCMapping dbase = this.owner.dbase.QueryEntireDbase();
        int curValue = 0;
        int dataValue = 0;

        // 判断消耗是否足够消耗
        foreach (string attribName in data.Keys)
        {
            // 没有该属性
            if (!dbase.ContainsKey(attribName))
                return false;

            // 获取需要消耗数值
            dataValue = data[attribName].AsInt;

            // 数值不能小于0
            if (dataValue < 0)
                return false;

            // 获取当前属性值
            curValue = dbase[attribName].AsInt;

            // 该属性不能支付消耗
            if (curValue < dataValue)
                return false;
        }

        // 属性足够支付
        return true;
    }

    /// <summary>
    /// 消耗对象的各项属性
    /// </summary>
    public bool CostAttrib(LPCMapping data)
    {
        // 检查是否是否可以支付消耗
        if (!CanCostAttrib(data))
            return false;

        // 获得消耗者的当前数据
        LPCMapping dbase = this.owner.QueryEntireDbase();

        // 支付消耗
        foreach (string attribName in data.Keys)
        {
            // int型属性
            this.owner.Set(attribName,
                LPCValue.Create(dbase[attribName].AsInt - data[attribName].AsInt));
        }

        // 返回成功
        return true;
    }

    /// <summary>
    /// 增加对象的各项属性
    /// </summary>
    public bool AddAttrib(LPCMapping data)
    {
        // 没有详细属性数据
        if (data == null)
            return false;

        // 获得消耗者的当前数据
        LPCMapping dbase = this.owner.QueryEntireDbase();
        int dataValue = 0;

        // 支付消耗
        foreach (string attribName in data.Keys)
        {
            // 由于dbase和配置中的值可以为int也可以为float，以下逻辑将值全部转为float再进行判断
            // 计算新数值
            dataValue = data[attribName].AsInt;

            // 数值不能小于0
            if (dataValue < 0)
                return false;

            // 属性过高无法增加其属性
            if ((MaxAttrib.GetMaxAttrib(attribName) - dbase[attribName].AsInt) < dataValue)
                return false;
        }

        // 增加属性
        foreach (string attribName in data.Keys)
        {
            // int型属性
            this.owner.Set(attribName,
                LPCValue.Create(dbase[attribName].AsInt + data[attribName].AsInt));
        }

        // 返回成功
        return true;
    }

    /// <summary>
    /// 检索属性数据
    /// </summary>
    public int Query(string attrib)
    {
        return Query(attrib, true);
    }

    /// <summary>
    /// 检索属性数据
    /// </summary>
    public int Query(string attrib, bool all)
    {
        // 属性名无效
        if (attrib == null || string.IsNullOrEmpty(attrib))
            return 0;

        // 获取基础数值
        // 这个地方为什么不直接走query接口是由于配置表配置的属性没有做属性偏移
        // 基础数据和配置表数据处理方式不一样
        LPCValue v = this.owner.dbase.Query(attrib);
        int vR = 0;
        if (v != null)
        {
            vR = v.AsInt;
        }
        else
        {
            // 从本初对象上去查找
            vR = this.owner.BasicQueryNoDuplicate<int>(attrib);
        }

        // 获取improvement下的临时数据
        if (all)
        {
            // 获取数据
            v = this.owner.QueryTemp("improvement/" + attrib);

            // improvement下有该属性
            if (v != null)
                vR = vR + v.AsInt;
        }

        // 返回数据
        return vR;
    }
}
