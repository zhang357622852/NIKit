/// <summary>
/// ResearchRewardItem.cs
/// Created by lic 2017/01/23
/// 探索奖励item界面
/// </summary>

using UnityEngine;
using System.Collections;

public enum ResearchRewardState
{
    Uncomplete, // 任务未完成
    Receiving,  // 正在领奖
    Received,   // 奖励已领取完成
}

public class ResearchRewardItem  : WindowBase<ResearchRewardWnd>
{
    public UISprite mBg;
    public GameObject mSelect;
    public UILabel mTitle;
    public UILabel mStateLb;

    public int TaskId { get; private set; }

    public bool IsSelect { get; private set; }

    public ResearchRewardState State { get; private set; }

    public int ReceiveTimes { get; private set; }

    #region 内部函数

    /// <summary>
    /// 刷新界面
    /// </summary>
    void Redraw()
    {
        CsvRow item = TaskMgr.GetTaskInfo(TaskId);
        if(item == null)
            return;

        mTitle.text = LocalizationMgr.Get(item.Query<string> ("title"));

        switch (State)
        {
        case ResearchRewardState.Uncomplete:
            mStateLb.gameObject.SetActive (false);
            mStateLb.text = LocalizationMgr.Get("ResearchRewardItemWnd_1");
            mTitle.transform.localPosition = new Vector3 (0f, 0f, 0f);
            mBg.alpha = 1.0f;
            break;

        case ResearchRewardState.Receiving:
            mStateLb.gameObject.SetActive (true);
            mStateLb.text = LocalizationMgr.Get("ResearchRewardItemWnd_2");
            mTitle.transform.localPosition = new Vector3(0f, 17f, 0f);
            mBg.alpha = 1.0f;
            break;

        case ResearchRewardState.Received:
            mStateLb.gameObject.SetActive (true);
            mStateLb.text = LocalizationMgr.Get("ResearchRewardItemWnd_3");
            mTitle.transform.localPosition = new Vector3(0f, 17f, 0f);
            mBg.alpha = 0.5f;
            break;
        }
    }

    #endregion

    #region 内部函数

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="state">State.</param>
    public void BindData(int taskId, ResearchRewardState state, int progress = 0)
    {
        TaskId = taskId;

        State = state;

        ReceiveTimes = progress;

        Redraw ();
    }

    /// <summary>
    /// 设定选中
    /// </summary>
    /// <param name="isSelect">If set to <c>true</c> is select.</param>
    public void SetSelect(bool isSelect)
    {
        IsSelect = isSelect;

        mSelect.SetActive (isSelect);

        if (!isSelect && State == ResearchRewardState.Received)
            mBg.alpha = 0.5f;
        else
            mBg.alpha = 1.0f;    
    }

    #endregion
}
