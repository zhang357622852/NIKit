/// <summary>
/// Container.cs
/// Copy from zhangyg 2014-10-22
/// 容器基类
/// </summary>

using LPC;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 容器 
/// </summary>
public class Container : Char
{
    /// <summary>
    /// 包裹容器 
    /// </summary>
    public BaggageContainer baggage;

    public Container(LPCMapping data)
        : base(data)
    {
        baggage = new BaggageContainer(this);

        // 如果指明了dbase数据，需要吸收进来
        if (data != null && data["dbase"] != null && data["dbase"].IsMapping)
        {
            // 吸入dbase数据
            this.dbase.Absorb(data["dbase"].AsMapping);

            // 设置名称
            if (data["dbase"].AsMapping["name"] != null)
                SetName(data["dbase"].AsMapping["name"].AsString);
        }
    }

    override public void Destroy()
    {
        base.Destroy();
        baggage.Destroy();
    }

    /// <summary>
    /// 移除一个物件 
    /// </summary>
    public bool UnloadProperty(Property o)
    {
        return baggage.UnloadProperty(o);
    }

    public bool UnloadProperty(string pos)
    {
        return this.baggage.UnloadProperty(pos);
    }

    /// <summary>
    /// 载入一个物件 
    /// </summary>
    public void LoadProperty(Property o, string pos)
    {
        baggage.LoadProperty(o, pos);
    }

    /// <summary>
    /// 获取包裹容器全部道具
    /// </summary>
    public List<Property> GetAllProperty()
    {
        return baggage.GetAllProperty();
    }
}
