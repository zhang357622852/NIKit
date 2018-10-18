using LPC;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 玩家登陆成功后一次性通知的所有物品
/// </summary>
public class MsgAllPropertyLoad : MsgHandler
{
    public string GetName()
    {
        return "msg_all_property_load";
    }

    public void Go(LPCValue para)
    {
        LPCMapping m = para.AsMapping;

        // 查找容器
        Container ob = Rid.FindObjectByRid(m["container"].AsString) as Container;

        // 没有找到指定的容器
        if (ob == null)
        {
            LogMgr.Trace("没有找到承载道具的容器({0})", m["container"].AsString);
            return;
        }

        // 载入该容器下全部道具
        List<Property> validPropertyList = new List<Property>();
        LPCArray propertyList = m["property_info"].AsArray;
        foreach (LPCValue v in propertyList.Values)
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

            // 物件载入失败
            if (property == null)
                continue;

            // 添加到列表中
            validPropertyList.Add(property);

            // 遍历所有下属物件信息
            if (arr != null && arr.IsArray)
            {
                foreach (LPCValue v2 in arr.AsArray.Values)
                    DoPropertyLoaded(property as Container, v2.AsMapping);
            }

            // 重新计算附加属性
            // 刷新附加属性流程不再是客户端自行 refresh affect，而是直接吸收服务器的结果
            if (ob == ME.user && property is Monster)
                PropMgr.RefreshAffect(property);
        }

        // 清除无效道具
        foreach (Property propertyOb in ob.GetAllProperty())
        {
            // 无效道具或者道具是该容器内有效道具
            if (propertyOb == null ||
                validPropertyList.IndexOf(propertyOb) != -1)
                continue;

            // 卸载该无效道具
            ob.UnloadProperty(propertyOb);
        }
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

        // 物品对象已经存在
        Property ob = Rid.FindObjectByRid(rid);
        if (ob != null)
        {
            // 判断物品位置和容器是否发生变化， 如果位置和容器发生了变化需要从原来的位置或者容器中unload
            Container oldContainer = ob.move.father;
            if (oldContainer != null &&
                (! string.Equals(oldContainer.GetRid(), ownerRid) || ! string.Equals(ob.move.GetPos(), pos)))
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

            // 载入物品到容器中
            container.LoadProperty(ob, pos);
        }

        // 返回角色对象
        return ob;
    }
}
