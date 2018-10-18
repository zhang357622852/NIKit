using LPC;
using System.Diagnostics;

/// <summary>
/// 通知某实体外观发生改变
/// </summary>
public class MsgObjectAppearanceUpdated : MsgHandler
{
    public string GetName()
    {
        return "msg_object_appearance_updated";
    }

    public void Go(LPCValue para)
    {
        LPCMapping m = para.AsMapping;

        string rid = m["rid"].AsString;
        Property ob = Rid.FindObjectByRid(rid);
        if (ob == null)
            // 没有找到该实体
            return;

        // 取得外观信息
        LPCMapping appearance = m["appearance"].AsMapping;
        // LPCValue EquipObjectTemplates = appearance["equip_object_templates"];
        appearance.Remove("equip_object_templates");

        LPCValue properties = appearance["properties"];
        appearance.Remove("properties");

        // 更新该实体的信息
        ob.dbase.Absorb(appearance);

        // 遍历目标身上的物件
        if (properties == null)
            return;
        foreach (LPCValue v in properties.AsArray.Values)
        {
            LPCMapping info = v.AsMapping;
            if (info.ContainsKey("rid"))
                rid = info["rid"].AsString;
            else
                rid = Game.GetRidByDomain(info["domain_address"].AsString);

            LPCValue improvement = info["improvement"];
            info.Remove("improvement");

            Property property = Rid.FindObjectByRid(rid);
            if (property != null)
            {
                // 吸收字段
                property.dbase.Absorb(info);

                // 对improvement字段需要单独吸收
                if (improvement != null && improvement.IsMapping)
                {
                    foreach (LPCValue k in improvement.AsMapping.Keys)
                    {
                        property.dbase.Set("improvement/" + k.AsString,
                            improvement.AsMapping[k.AsString]);
                    }
                }
            }
            else
            {
                // 构造物件
                property = PropertyMgr.CreateProperty(info, true);
                if (improvement != null && improvement.IsMapping)
                    property.dbase.Set("improvement", improvement);

                // 载入到目标身上
                (ob as Container).LoadProperty(property, info["pos"].AsString);
            }

            // TODO: 发布物件规则、装备OT逻辑待补充
        }
    }
}
