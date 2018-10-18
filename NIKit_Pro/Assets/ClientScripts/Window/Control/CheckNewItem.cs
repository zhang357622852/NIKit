/// <summary>
/// CheckNewItem.cs
/// Created by fucj 2014-12-16
/// 检查是否得到新物品
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class CheckNewItem : MonoBehaviour
{

    public GameObject sign;
    public string page = string.Empty;

    // Use this for initialization
    void Start()
    {
        // 注册包裹变化回掉
        ME.user.baggage.eventCarryChange += ChangeNewThings;

        // 注册新物品信息被清除
        EventMgr.RegisterEvent(gameObject.name, EventMgrEventType.EVENT_CLEAR_NEW, ClearNewInfo);
    }

    /// <summary>
    /// 包裹变化回调
    /// </summary>
    void ChangeNewThings(string[] pos)
    {
        // 延迟刷新
        MergeExecuteMgr.DispatchExecute(CheckNewThings);
    }

    /// <summary>
    /// 清除新物品标示
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    void ClearNewInfo(int eventId, MixedValue para)
    {
        // 延迟刷新
        MergeExecuteMgr.DispatchExecute(CheckNewThings);
    }

    /// <summary>
    /// 检查新物品
    /// </summary>
    public void CheckNewThings()
    {
        if (ME.user == null)
            return;

        if (IsHaveNewItem())
            sign.SetActive(true);
        else
            sign.SetActive(false);
    }

    /// <summary>
    /// 是否有新物品
    /// </summary>
    bool IsHaveNewItem()
    {
        List<Property> items = BaggageMgr.GetItemsByPage(ME.user, AliasMgr.GetInt(page));

        if (items.Count < 1)
            return false;

        foreach (Property item in items)
        {
            if (BaggageMgr.IsNew(item))
                return true;
        }

        return false;
    }
}
