/// <summary>
/// HistoryRankingWnd.cs
/// Created by fengsc 2016/09/08
/// 竞技场历史排名窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class HistoryRankingWnd : WindowBase<HistoryRankingWnd>
{
    // 窗口关闭按钮
    public GameObject mCloseBtn;

    public GameObject mItem;

    // 父级
    public UIGrid mGrid;

    public UILabel mTitle;

    public GameObject mMask;

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();

        // 绘制窗口
        Redraw();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mMask).onClick = OnClickCloseBtn;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        mTitle.text = LocalizationMgr.Get("HistoryRankingWnd_4");
        if (mItem.activeSelf)
            mItem.SetActive(false);

        // 获取排行榜数据
        LPCValue v = ME.user.Query<LPCValue>("arena_top");
        if (v == null || ! v.IsMapping)
            return;

        LPCMapping arenaTop = v.AsMapping;

        LPCMapping data = LPCMapping.Empty;
        for (int i = 0; i  < 3; i++)
        {
            GameObject item = Instantiate(mItem);
            item.transform.SetParent(mGrid.transform);
            item.transform.localPosition = Vector3.zero;
            item.transform.localScale = Vector3.one;
            item.SetActive(true);

            HistoryRankingItemWnd script = item.GetComponent<HistoryRankingItemWnd>();
            if (script == null)
                continue;

            if (i + 1 == 1)
            {
                // 当前排名
                script.Bind(arenaTop, LocalizationMgr.Get("HistoryRankingWnd_1"));
            }
            else if (i + 1 == 2)
            {
                LPCValue topData = arenaTop.GetValue<LPCValue>("last_top_data");

                if (topData == null || !topData.IsMapping)
                    data = LPCMapping.Empty;
                else
                    data = topData.AsMapping;

                // 上周排名
                script.Bind(data, LocalizationMgr.Get("HistoryRankingWnd_2"));
            }
            else
            {
                LPCValue record = arenaTop.GetValue<LPCValue>("high_record");

                if (record == null || !record.IsMapping)
                    data = LPCMapping.Empty;
                else
                    data = record.AsMapping;
                
                // 最高记录
                script.Bind(data, LocalizationMgr.Get("HistoryRankingWnd_3"));
            }

            // 激活排序组件
            mGrid.repositionNow = true;
        }
    }

    /// <summary>
    /// 关闭窗口按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }
}
