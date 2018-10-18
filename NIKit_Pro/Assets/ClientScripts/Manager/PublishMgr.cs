/// <summary>
/// PublishMgr.cs
/// Create by zhaozy 2015/09/01
/// 发布道具管理模块
/// </summary>

using System;
using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using LPC;

public static class PublishMgr
{
    #region 公共接口

    /// <summary>
    /// 获取发布信息的tag
    /// 正确的发布tag格式
    /// </summary>
    public static LPCMapping GetPublishData(Property who, Property itemOb, string prefixDesc = "")
    {
        // 发布道具对象不存在
        if (itemOb == null)
            return null;

        // 当前只支持发布宠物和装备
        if(!MonsterMgr.IsMonster(itemOb) &&
            !EquipMgr.IsEquipment(itemOb))
            return null;

        LPCMapping pubData = new LPCMapping();

        pubData.Add("publish_str", GetPublicTag(who, itemOb));
        pubData.Add("rid", itemOb.GetRid());
        pubData.Add("class_id", itemOb.GetClassID());

        pubData.Add("star", itemOb.GetStar());

        // 前置描述
        pubData.Add("prefix_desc", prefixDesc);

        if(MonsterMgr.IsMonster(itemOb))
        {
            pubData.Add("rank", itemOb.GetRank());
            pubData.Add("element", itemOb.Query<int>("element"));
        }
        else
        {
            pubData.Add("rank", itemOb.GetRank());
            pubData.Add("rarity", itemOb.GetRarity());
            pubData.Add("prop", itemOb.Query<LPCMapping>("prop"));
        }

        // 当前只支持发布宠物和装备
        return pubData;
    }

    /// <summary>
    /// 获取发布信息在输入框中的描述
    /// </summary>
    /// <returns>The public item desc.</returns>
    /// <param name="itemOb">Item ob.</param>
    public static string GetPublicTag(Property who, Property itemOb)
    {
        if(itemOb == null)
            return string.Empty;

        // 宠物和道具再输入框中的显示格式
        return string.Format("「{0}」", itemOb.Short());
    }

    /// <summary>
    /// 检测发布信息是否有效
    /// </summary>
    public static bool CheckPublishValid(Property who, string pulishStr, out string pulishTag)
    {
        string tag = pulishStr.Substring(1, pulishStr.Length - 2);
        string[] arr = tag.Split(new char[] { ':' });

        // 发布标签真正tag标签
        pulishTag = string.Empty;

        // 数据不正确
        if (arr.Length != 2)
            return false;

        // 获取道具的rid
        Property itemOb = Rid.FindObjectByRid(arr[1]);

        // 发布道具不存在
        if (itemOb == null || itemOb.IsDestroyed)
            return false;

        // 如果是装备
        string itemName = itemOb.Short();

        // 道具名称异常
        if (!itemName.Equals(arr[0]))
            return false;

        // 重新构建pulish_tag
        if (EquipMgr.IsEquipment(itemOb))
            pulishTag = string.Format("[{0}:{1}:{2}]", itemName, itemOb.GetRid(), itemOb.Query<int>("rank"));
        else
            pulishTag = string.Format("[{0}x{1}:{2}:{3}]", itemOb.Short(), itemOb.GetAmount(), itemOb.GetRid(), itemOb.Query<int>("rank"));

        // 可以发布道具
        return true;
    }

    /// <summary>
    /// 获取发布信息
    /// </summary>
    public static void FetchPublishEntity(string pulishId)
    {
        Operation.CmdFetchPublishEntity.Go(pulishId);
    }

    #endregion
}
