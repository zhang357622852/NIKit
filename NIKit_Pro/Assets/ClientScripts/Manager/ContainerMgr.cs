using LPC;
using System;
using System.Collections.Generic;

/// <summary>
/// 容器管理
/// </summary>
public class ContainerMgr
{
    // 每次激活的单位页面大小
    // 注：客户端页面大小和服务器端无需保持一致
    public const int PAGE_SIZE = 30;

    // 目前创建的所有容器，以rid存储容器
    private static Dictionary<int, string> containerTable = new Dictionary<int, string>();

    static ContainerMgr()
    {
        // TODO
    }

    /// <summary>
    /// 通过容器类型获取已存在的容器
    /// </summary>
    public static Container GetContainerByType(int type)
    {
        string rid;
        if (containerTable.TryGetValue(type, out rid))
            return Rid.FindObjectByRid(rid) as Container;
        else
            return null;
    }

    /// <summary>
    /// 容器关闭了
    /// </summary>
    public static void ContainerClosed(string rid)
    {
        // 容器不存在，不处理
        Container container = Rid.FindObjectByRid(rid) as Container;
        if (container == null)
            return;

        // 清除该容器的记录
        containerTable.Remove(container.baggage.ContainerType);

        // 销毁容器
        container.Destroy();
    }

    /// <summary>
    /// 容器载入了
    /// </summary>
    public static void ContainerLoaded(int type, LPCValue dbase)
    {
        string rid = Dbase.ExpressQuery("rid", dbase.AsMapping, string.Empty);
        if (string.IsNullOrEmpty(rid))
            throw new Exception("bad container dbase, missing rid.");

        Container container = Rid.FindObjectByRid(rid) as Container;
        if (container != null)
        {
            // 已经存在这个容器
            // 更新容器属性
            container.dbase.Absorb(dbase.AsMapping);

            // 标记容器未激活
            container.Delete("activate_flags");
        }
        else
        {
            // 创建容器对象
            LPCMapping data = LPCMapping.Empty;
            data.Add("dbase", dbase);
            container = new Container(data);
        }

        // 设置容器类型
        container.baggage.ContainerType = type;

        // 记录新加载的容器
        containerTable[type] = rid;
    }

    /// <summary>
    /// 标记格子未激活
    /// </summary>
    public static void MarkUnactivateSlot(Container container, string start, int count)
    {
        // 格子全部是激活的，不处理
        if (container.baggage.IsAllSlotsActivated())
            return;

        // 取坐标分项
        int x = 0, z = 0;
        if (!Game.ReadPos(start, ref x, ref z))
            return;

        // 计算需要激活的界面编号集合
        string xy_pos = string.Format("{0}-{1}-", x, 0);
        int begin_page_no = z / PAGE_SIZE;
        int end_page_no = (z + count - 1) / PAGE_SIZE;

        // 激活所有页面
        for (int page_no = begin_page_no; page_no <= end_page_no; page_no++)
        {
            int index = page_no * PAGE_SIZE;
            string flag_path = string.Format("activate_flags/{0}{1}", xy_pos, index);
            container.Delete(flag_path);
        }
    }

    /// <summary>
    /// 判断某个位置是否激活
    /// </summary>
    /// <returns>
    public static bool IsSlotActivated(Container container, string pos)
    {
        int x = 0, z = 0;
        if (!Game.ReadPos(pos, ref x, ref z))
            return false;

        string flag_path = string.Format("activate_flags/{0}", Game.MakePos(x, z / PAGE_SIZE * PAGE_SIZE));
        return container.Query<bool>(flag_path, true);
    }

    /// <summary>
    /// 获取指定容器指定页的空闲位置
    /// </summary>
    /// <param name='container_ob'>
    /// 指定容器
    /// </param>
    /// <param name='page'>
    /// 指定页
    /// </param>
    public static string GetFreePos(Container container, int page, bool ignoreSize = false)
    {
        // 容量不能忽略
        if(!ignoreSize)
        {
            if(GetFreePosCount(container, page) <= 0)
                return string.Empty;
        }

        int i = 0;
        string pos = string.Empty;
        while(true)
        {
            pos = Game.MakePos(page, i);

            i++;

            if (container.baggage.GetCarryByPos(pos) == null)
                break;
        }

        return pos;
    }

    /// <summary>
    /// 获取指定容器指定页的多个空闲位置
    /// </summary>
    /// <param name='container_ob'>
    /// 指定容器
    /// </param>
    /// <param name='page'>
    /// 指定页
    /// </param>
    /// <param name='number'>
    /// 空闲位置数量
    /// </param>
    public static List<string> GetFreePosList(Container container, int page, int number, bool ignoreSize = false)
    {
        if(!ignoreSize)
        {
            if(GetFreePosCount(container, page) < number)
                return new List<string>();
        }

        List<string> posList = new List<string>();

        int i = 0;

        while(true)
        {
            string pos = Game.MakePos(page, i);

            i++;

            if (container.baggage.GetCarryByPos(pos) == null)
            {
                posList.Add(pos);

                if(posList.Count >= number)
                    break;
            }
        }

        return posList;
    }

    /// <summary>
    /// 获取指定容器指定页的空闲位置数
    /// </summary>
    /// <param name='container_ob'>
    /// 指定容器
    /// </param>
    /// <param name='page'>
    /// 指定页
    /// </param>
    public static int GetFreePosCount(Container container, int page)
    {
        // 获取当前包裹空间
        int size = container.baggage.GetPageSize(page);

        // 取得当前
        int amount = container.baggage.GetPageCarry(page).Count;

        // 此处是有可能出现包裹中数量比页面大小大的情况
        return (size - amount) >= 0 ? (size - amount) : 0;

    }

    /// <summary>
    /// 获取指定容器指定物件的空闲位置
    /// </summary>
    public static string GetFreePos(Container container, Property property)
    {
        int ob_type = property.Query<int>("type");
        CsvRow row = null;//GameConfig_container_arrange.instance.FindByKey("type", ob_type);
        if (row == null)
            return string.Empty;

        // 找到物件可以存在的页面
        LPCValue lpc_pages = row.Query<LPCValue>("page");
        if (lpc_pages == null)
            return string.Empty;

        // 如果只能存储在单独页
        if (lpc_pages.IsInt && (lpc_pages.AsInt > 0))
        {
            int script_no = lpc_pages.AsInt;
            if (!ScriptMgr.Contains(script_no))
                return string.Empty;

            lpc_pages = (LPCValue)ScriptMgr.Call(script_no, ob_type, property);
            if (lpc_pages == null)
                return string.Empty;
        }

        // 如果能够存储在多个页
        if (lpc_pages.IsArray)
        {
            foreach (LPCValue lpc_page in lpc_pages.AsArray.Values)
            {
                string free_pos = GetFreePos(container, lpc_page.AsInt);
                if (string.IsNullOrEmpty(free_pos))
                    continue;
                return free_pos;
            }
        }

        return string.Empty;
    }

    /// <summary>
    /// 通过指定道具获取道具所在页
    /// </summary>
    public static int GetPageByItem(Item ob)
    {
        // 暂时只有一个包裹
        return ContainerConfig.POS_ITEM_GROUP;
    }

    /// <summary>
    /// 获得拥有道具的数量
    /// </summary>
    public static int GetAmount(User who, int class_id, LPCMapping condition = null)
    {
        int amount = 0;
        bool issame = false; 

        // 获取包裹所有物品
        Dictionary<string, Property> items = who.baggage.GetPageCarry(ContainerConfig.POS_ITEM_GROUP);
        if (items.Count <= 0)
            return 0;

        foreach (Property item in items.Values)
        {
            if (string.IsNullOrEmpty(item.GetRid()) ||
                item.GetClassID() != class_id)
                continue;

            if (condition != null && condition.Count > 0)
            {
                issame = false;
                foreach (string key in condition.Keys)
                {
                    if (condition[key] == null)
                        continue;

                    if (item.Query(key) == null)
                    {
                        issame = false;
                        break;
                    }

                    if (item.Query(key) != condition[key])
                    {
                        issame = false;
                        break;
                    }

                    issame = true;
                }

                if (issame)
                    amount += item.GetAmount();
            }
            else
                amount += item.GetAmount();
        }

        return amount;
    }

    /// <summary>
    /// 获得拥有所有道具
    /// </summary>
    public static List<Property> GetSameItems(User who, int class_id, LPCMapping condition = null)
    {
        List<Property> same_items = new List<Property>();
        bool issame = false; 
        
        // 获取包裹所有物品
        Dictionary<string, Property> items = who.baggage.GetPageCarry(ContainerConfig.POS_ITEM_GROUP);
        if (items.Count <= 0)
            return new List<Property>();
        
        foreach (Property item in items.Values)
        {
            if (string.IsNullOrEmpty(item.GetRid()) ||
                item.GetClassID() != class_id)
                continue;
            
            if (condition != null && condition.Count > 0)
            {
                issame = false;
                foreach (string key in condition.Keys)
                {
                    if (condition[key] == null)
                        continue;
                    
                    if (item.Query(key) == null)
                    {
                        issame = false;
                        break;
                    }
                    
                    if (item.Query(key) != condition[key])
                    {
                        issame = false;
                        break;
                    }
                    
                    issame = true;
                }
                
                if (issame)
                    same_items.Add(item);
            }
            else
                same_items.Add(item);
        }
        
        return same_items;
    }
}
