/// <summary>
/// BaggageContainer.cs
/// Copy from zhangyg 2014-10-22
/// 普通包裹属性
/// </summary>

using System;
using System.Collections;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 普通包裹属性
/// </summary>
public class BaggageContainer
{
    private Property owner;

    /// <summary>
    /// 包裹变化的回调
    /// </summary>
    public delegate void CarryChangeHook(string[] pos);

    public event CarryChangeHook eventCarryChange;

    // 携带的下属物件
    private Dictionary<string, Property> carry = new Dictionary<string, Property>();

    // 变更的道具坐标集合(key是pos，value是true)
    private Dictionary<string, bool> carryChangedPos = new Dictionary<string, bool>();

    /// <summary>
    /// 合并道具到指定位置
    /// 目前只有两种情况可合并和不能合并两种情况
    /// 如果以后有堆叠数来现在，再调整该逻辑
    /// </summary>
    private Property combineToPos(Property property, string pos)
    {
        // 获取目标位置道具
        Property ob = GetCarryByPos(pos);

        // 该位置没有道具不处理
        if (ob == null)
            return LoadProperty(property, pos) ? property : null;

        // 获取需要合并道具的数量
        int amount = property.GetAmount();

        // 合并new标识
        if (property.QueryTemp<int>("is_new") == 1)
            ob.SetTemp("is_new", LPCValue.Create(1));

        // 增加道具数量
        (ob as Item).AddAmount(amount);

        // 将原来的道具消耗掉
        (property as Item).CostAmount(amount);

        // 通知：该位置的对象发生了变化
        NotifyCarryChanged(pos);

        // 返回合并成功
        return ob;
    }

    public BaggageContainer(Property property)
    {
        this.owner = property;
    }

    public void Destroy()
    {
        // 清空包裹信息
        Clean();
    }

    /// <summary>
    /// 包裹大小
    /// </summary>
    public LPCMapping ContainerSize
    {
        get { return owner.Query("container_size").AsMapping; }
        set
        {
            LPCValue v = LPCValue.CreateMapping();
            v.AsMapping = value;
            owner.dbase.Set("container_size", v);
        }
    }

    /// <summary>
    /// 包裹类型
    /// </summary>
    public int ContainerType
    {
        get { return owner.Query<int>("container_type"); }
        set { owner.dbase.Set("container_type", value); }
    }

    /// <summary>
    /// 激活指定的一批格子
    /// </summary>
    public bool ActivateSlot(string begin, int count)
    {
        // TO DO
        return false;
    }

    /// <summary>
    /// 清空包裹
    /// </summary>
    public void Clean()
    {
        foreach (Property o in new List<Property>(carry.Values))
            o.Destroy();

        carry.Clear();
    }

    /// <summary>
    /// 获取包裹中全部道具
    /// </summary>
    public List<Property> GetAllProperty()
    {
        return new List<Property>(carry.Values);
    }

    /// <summary>
    /// 根据位置获得物品
    /// </summary>
    public Property GetCarryByPos(string pos)
    {
        // 如果没有指定位置信息
        if (string.IsNullOrEmpty(pos))
            return null;

        // 获取指定位置物品
        Property pro = null;
        carry.TryGetValue(pos, out pro);

        // 返回该位置物品
        return pro;
    }

    /// <summary>
    /// 取某个物件的空闲位置
    /// </summary>
    public string GetFreePos(Property o)
    {
        return ContainerMgr.GetFreePos(owner as Container, o);
    }

    /// <summary>
    /// 获取某页的空闲位置
    /// </summary>
    public string GetFreePos(int page, bool ignoreSize = false)
    {
        return ContainerMgr.GetFreePos(owner as Container, page, ignoreSize);
    }

    /// <summary>
    /// 获取指定容器指定页的空闲位置数
    /// </summary>
    public int GetFreePosCount(int page = ContainerConfig.POS_ITEM_GROUP)
    {
        return ContainerMgr.GetFreePosCount(owner as Container, page);
    }

    /// <summary>
    /// 获得物品在某页中的位置.
    /// </summary>
    public string GetCurPos(int page, string rid)
    {
        Dictionary<string, Property> pageList = GetPageCarry(page);
        foreach (KeyValuePair<string, Property> item in pageList)
        {
            if (item.Value.GetRid() == rid)
                return item.Key;
        }

        return string.Empty;
    }

    /// <summary>
    /// 获得物品在某页中的位置.
    /// 注意：使用前请确认是否可以用classId判断
    /// </summary>
    public string GetCurPos(int page, int classId)
    {
        Dictionary<string, Property> pageList = GetPageCarry(page);
        foreach (KeyValuePair<string, Property> item in pageList)
        {
            if (item.Value.GetClassID() == classId)
                return item.Key;
        }

        return string.Empty;
    }

    /// <summary>
    /// 取某一页下所有的物件
    /// </summary>
    public Dictionary<string, Property> GetPageCarry(int page)
    {
        Dictionary<string, Property> result = new Dictionary<string, Property>();
        foreach (string pos in carry.Keys)
        {
            int x, y, z;
            POS.Read(pos, out x, out y, out z);
            if (x != page)
                continue;

            result[pos] = carry[pos];
        }

        return result;
    }

    /// <summary>
    /// 判断某位置对指定物件是否有效，可使用
    /// 如果allow_occuppied为TRUE，则表示只要位置坐标合法，即使位置被占用
    /// 也返回有效；如果其为FALSE，则要求位置必须为空白位置
    /// </summary>
    public bool ValidPosFor(Property property, string dst, bool allow_occuppied = false)
    {
        int x, y, z;

        // 位置不符合格式要求
        if (!POS.Read(dst, out x, out y, out z))
            return false;

        // 判断包裹空间大小
        int size = GetPageSize(x);
        if (size <= z)
            return false;

        if (!allow_occuppied && GetCarryByPos(dst) != null)
        {
            // 位置已经被占用了，并且调用者要求这个位置必须为空
            LogMgr.Trace("容器的位置{0}已经被占据了，并且不允许调整位置，不能放置新道具。\n", dst);
            return false;
        }

        // 返回该位置有效
        return true;
    }

    #region                                                   数 据 处 理

    /// <summary>
    /// 通过ClassId获取某个装备的第一个
    /// </summary>
    public Property GetEquipment(int classId)
    {
        foreach (Property item in GetPageCarry(ContainerConfig.POS_ITEM_GROUP).Values)
        {
            if (item.GetClassID() == classId)
            {
                return item;
            }
        }
        return null;
    }

    #endregion

    /// <summary>
    /// 取得包裹信息
    /// </summary>
    public Dictionary<string, Property> GetCarry()
    {
        return carry;
    }

    /// <summary>
    /// 取得页面大小
    /// </summary>
    public int GetPageSize(int page)
    {
        LPCMapping m = ContainerSize;
        if (m == null || !m.ContainsKey(page))
            return 0;

        return m[page].AsInt;
    }

    /// <summary>
    /// 包裹是否满了
    /// </summary>
    public bool IsFull(int page)
    {
        Dictionary<string, Property> items = GetPageCarry(page);
        int size = GetPageSize(page);

        return items.Count >= size;
    }

    /// <summary>
    /// 判断是否是所有的格子都激活了
    /// 不具备延迟载入（即lazy_load标志）的容器其格子是全部激活的
    /// lazy_load这个标志是服务器传送下来的
    /// </summary>
    public bool IsAllSlotsActivated()
    {
        return !(owner.Query("lazy_load", true) != null &&
        owner.Query<int>("lazy_load", true).Equals(1));
    }

    /// <summary>
    /// 判断位置是不是被占用了
    /// </summary>
    /// <returns>
    public bool IsPosOccuppied(string pos)
    {
        return carry.ContainsKey(pos);
    }

    /// <summary>
    /// 判断某个位置是否激活
    /// </summary>
    public bool IsSlotActivated(string pos)
    {
        return ContainerMgr.IsSlotActivated(owner as Container, pos);
    }

    /// <summary>
    /// 物品载入容器
    /// </summary>
    public bool LoadProperty(Property property, string pos)
    {
        // 如果载入道具的位置已经被占用了，则直接析构目标位置道具
        if (IsPosOccuppied(pos))
        {
            LogMgr.Trace("位置({0})已经有物件({1})，先析构。", pos, (carry[pos]).GetName());
            carry[pos].Destroy();
        }

        // 道具数量小于0
        if (property.GetAmount() <= 0)
        {
            // 析构道具
            property.Destroy();

            // 通知包裹变化
            NotifyCarryChanged(pos);

            // load成功
            return true;
        }

        // 记录物件的位置
        property.move.SetPos(pos);

        // 登记成员，同时根据位置建立索引
        carry[pos] = property;
        property.move.father = owner as Container;

        // 通知：该位置的对象发生了变化
        NotifyCarryChanged(pos);

        // 加载成功
        return true;
    }

    /// <summary>
    /// 置换位置
    /// </summary>
    public void SwitchProperty(string from, string to)
    {
        Property fromOb, toOb;
        carry.TryGetValue(from, out fromOb);
        carry.TryGetValue(to, out toOb);

        if (fromOb != null)
            fromOb.move.SetPos(to);
        if (toOb != null)
            toOb.move.SetPos(from);

        if (toOb != null)
            carry[from] = toOb;
        else if (carry.ContainsKey(from))
            carry.Remove(from);

        if (fromOb != null)
            carry[to] = fromOb;
        else if (carry.ContainsKey(to))
            carry.Remove(to);

        // 通知：该位置的对象变化了
        // 由于目前使用了预表现的效果，在console执行switch_property时就已经更改了物品的
        // 位置信息，之后在收到msg_property_loaded消息时将不再析构物品并重新创建，以避免
        // 包裹格闪烁，但这样一来将不会产生carry_changed事件，客户端界面将不会执行重绘，
        // 因此需要在这里就产生一次carry_changed事件。
        NotifyCarryChanged(from);
        NotifyCarryChanged(to);
    }

    /// <summary>
    /// 物品卸除
    /// </summary>
    public bool UnloadProperty(Property property)
    {
        // 根本就不在包裹中，不需要移除
        string pos = property.move.GetPos();

        // 没有位置信息
        if (string.IsNullOrEmpty(pos))
            return true;

        // 查看道具是否在包裹中
        if (!carry.ContainsKey(pos))
            return true;

        // 移除该位置道具
        carry.Remove(pos);

        // 如果是装备位置
        if (ContainerConfig.IS_EQUIP_POS(pos) && !owner.IsDestroyed)
            PropMgr.RefreshAffect(owner);

        // 通知：该位置的对象消失了
        NotifyCarryChanged(pos);
        return true;
    }

    /// <summary>
    /// 卸除指定位置物品
    /// </summary>
    public bool UnloadProperty(string pos)
    {
        if (!carry.ContainsKey(pos))
            return false;

        // 移除该位置道具
        carry.Remove(pos);

        // 如果是装备位置
        if (ContainerConfig.IS_EQUIP_POS(pos) && !owner.IsDestroyed)
            PropMgr.RefreshAffect(owner);

        // 通知：该位置的对象消失了
        NotifyCarryChanged(pos);
        return true;
    }

    /// <summary>
    /// 通知包裹改变了
    /// </summary>
    public void NotifyCarryChanged(string pos)
    {
        // 如果已经不在游戏中了则不通知客户端角色carry变化
        if (!ME.isInGame)
            return;

        // 记录这个格子发生变化了
        carryChangedPos[pos] = true;

        // 执行包裹改变回调
        InvokeCarryChangedCB();
    }

    // 包裹变化的回调
    private void InvokeCarryChangedCB()
    {
        string[] posArr = new List<string>(carryChangedPos.Keys).ToArray();
        if (posArr.Length < 1)
            return;

        carryChangedPos.Clear();
        if (eventCarryChange != null)
            eventCarryChange(posArr);
    }
}
