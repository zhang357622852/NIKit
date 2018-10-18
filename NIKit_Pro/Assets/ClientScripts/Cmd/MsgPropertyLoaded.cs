using LPC;

/// <summary>
/// 包裹中的物品(道具，宠物等)更新了
/// </summary>
public class MsgPropertyLoaded : MsgHandler
{
    public string GetName()
    {
        return "msg_property_loaded";
    }

    public void Go(LPCValue para)
    {
        LPCMapping m = para.AsMapping;

        Container ob = Rid.FindObjectByRid(m["container"].AsString) as Container;
        if (ob == null)
        {
            LogMgr.Trace("没有找到承载道具的容器({0})", m["container"].AsString);
            return;
        }

        LPCArray properties = m["property_info"].AsArray;
        foreach (LPCValue v in properties.Values)
        {
            // 包含了下属物件信息
            LPCValue arr = null;
            if (v.AsMapping.ContainsKey("properties"))
            {
                arr = v.AsMapping["properties"];
                v.AsMapping.Remove("properties");
            }

            // 载入物件
            Property property = DoPropertyLoaded(ob, v.AsMapping);
            if (property == null)
                // 物件载入失败
                continue;

            // 遍历所有下属物件信息
            if (arr != null && arr.IsArray)
            {
                foreach (LPCValue v2 in arr.AsArray.Values)
                    DoPropertyLoaded(property as Container, v2.AsMapping);
            }
        }

        // 如果是宠物需要刷新属性
        if (!ob.IsMonster())
            return;

        // 刷新宠物属性
        PropMgr.RefreshAffect(ob);
    }

    // 执行物件载入
    public static Property DoPropertyLoaded(Container container, LPCMapping info)
    {
        string rid = string.Empty;
        if (info.ContainsKey("rid"))
            rid = info["rid"].AsString;
        else
            rid = Game.GetRidByDomain(info["domain_address"].AsString);

        // 获取物品新容器rid
        string ownerRid = container.GetRid();
        string pos = info["pos"].AsString;

        // 先销毁掉之前的物品
        Property ob = Rid.FindObjectByRid(rid);
        if (ob != null)
        {
            // 判断物品位置和容器是否发生变化， 如果位置和容器发生了变化需要从原来的位置或者容器中unload
            Container oldContainer = ob.move.father;
            if (oldContainer != null &&
                (!string.Equals(oldContainer.GetRid(), ownerRid) || !string.Equals(ob.move.GetPos(), pos)))
            {
                // 将物品从旧容器中unload
                oldContainer.UnloadProperty(ob);

                // 吸收入数据
                ob.dbase.Absorb(info);

                // 载入物品到新容器中
                container.LoadProperty(ob, pos);
            }
            else if (oldContainer == null)
            {
                // 吸收入数据
                ob.dbase.Absorb(info);

                // 载入物品到新容器中
                container.LoadProperty(ob, pos);
            }
            else
            {
                // 吸收入数据
                ob.dbase.Absorb(info);
            }
        }
        else
        {
            // 自己填充一下owner
            info.Add("owner", ownerRid);

            // 创建角色对象
            ob = PropertyMgr.CreateProperty(info, true);

            // 包裹中没有相同的物品
            if (ContainerConfig.IS_PET_POS(pos) ||
                ContainerConfig.IS_ITEM_POS(pos))
            {
                // 标识新物品
                ob.SetTemp("is_new", LPCValue.Create(1));
            }

            // 载入物品到容器中
            container.LoadProperty(ob, pos);
        }

        // 返回角色对象
        return ob;
    }
}
