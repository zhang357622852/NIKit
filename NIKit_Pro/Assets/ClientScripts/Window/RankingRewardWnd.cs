/// <summary>
/// RankingRewardWnd.cs
/// Created by fengsc 2016/09/23
/// 竞技场排名奖励窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class RankingRewardWnd : WindowBase<RankingRewardWnd>
{
    #region 成员变量

    public GameObject mItem;                    // 列表格子对象;
    public GameObject mParent;              // 格子复用组件;

    // 结算提示
    public UILabel mSettlementTips;

    // 奖励提示
    public UILabel mRewardTips;

    /// <summary>
    /// The m scroll view.
    /// </summary>
    public UIScrollView mScrollView;

    private List<GameObject> mList = new List<GameObject>();

    #endregion

    // Use this for initialization
    void Awake()
    {
        // 创建一批基础格子
        CreateBatchObject();

        // 注册事件
        RegisterEvent();

        mSettlementTips.text = LocalizationMgr.Get("RankingBattleWnd_19");
        mRewardTips.text = LocalizationMgr.Get("RankingBattleWnd_20");
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    void OnEnable()
    {
        // 重置面板位置
        mScrollView.ResetPosition();

        // 绘制窗口
        Redraw();
    }

    void OnDestroy()
    {
        // 移除消息关注
        MsgMgr.RemoveDoneHook("MSG_ARENA_TOP_DATA", "RankingRewardWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 关注MSG_ARENA_TOP_DATA消息
        MsgMgr.RegisterDoneHook("MSG_ARENA_TOP_DATA", "RankingRewardWnd", OnMsgCallBack);
    }

    /// <summary>
    /// MSG_ARENA_TOP_DATA 消息回调
    /// </summary>
    void OnMsgCallBack(string cmd, LPCValue para)
    {
        // 重新绘制窗口
        Redraw();
    }

    /// <summary>
    /// 创建一批格子
    /// </summary>
    void CreateBatchObject()
    {
        LPCMapping data = ArenaMgr.GetAllBonus();

        List<int> stepList = new List<int> ();

        foreach (int item in data.Keys)
            stepList.Add(item);

        if(mItem == null)
            return;

        mItem.SetActive(false);

        for (int i = 0; i < stepList.Count; i++)
            CreateSingleObejct(i);
    }

    void CreateSingleObejct(int index)
    {
        GameObject clone = Instantiate(mItem);
        clone.transform.SetParent(mParent.transform);

        clone.transform.localScale = Vector3.one;
        clone.transform.localPosition = Vector3.zero;
        clone.transform.localPosition = new Vector3 (clone.transform.localPosition.x,
            clone.transform.localPosition.y - index * 110, 1);

        clone.SetActive(false);

        mList.Add(clone);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        LPCMapping data = ArenaMgr.GetAllBonus();

        List<int> stepList = new List<int> ();

        foreach (int item in data.Keys)
            stepList.Add(item);

        stepList.Sort();

        if(mItem == null)
            return;

        mItem.SetActive(false);

        for (int i = 0; i < stepList.Count; i++)
        {
            if (i + 1 > mList.Count)
                CreateSingleObejct(mList.Count);

            GameObject clone = mList[i];

            // 绑定数据
            clone.GetComponent<RankingRewardItemWnd>().Bind(stepList[i]);

            clone.SetActive(true);
        }
    }
}
