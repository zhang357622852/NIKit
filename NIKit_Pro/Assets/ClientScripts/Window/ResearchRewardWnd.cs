/// <summary>
/// ResearchRewardWnd.cs
/// Created by lic 2017/01/23
/// 探索奖励界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ResearchRewardWnd  : WindowBase<ResearchRewardWnd>
{
    public UILabel mTitle;

    public GameObject mCompleteMark;
    public UILabel mMarkLb;
    public UILabel mCompleteTitle;
    public UILabel mCompleteLb;

    public UILabel mReceiveTypeTitle;
    public UILabel mReceiveTypeLb;

    public UILabel mBonusTitle;

    public GameObject[] mRewardGroup;

    public UILabel mTips;

    public Transform mPanel;

    public GameObject mRewardItem;

    #region 私有字段

    List<GameObject> mChildList = new List<GameObject>();

    #endregion

    #region 内部函数

    void Start()
    {
        // 初始化显示
        InitWnd();

        // 隐藏item模板
        mRewardItem.SetActive (false);
    }

    void OnEnable()
    {
        Redraw ();
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitWnd()
    {
        mMarkLb.text = LocalizationMgr.Get("ResearchRewardWnd_5");
        mCompleteTitle.text = LocalizationMgr.Get("ResearchRewardWnd_1");
        mReceiveTypeTitle.text = LocalizationMgr.Get("ResearchRewardWnd_2");
        mBonusTitle.text = LocalizationMgr.Get("ResearchRewardWnd_3");
        mTips.text = LocalizationMgr.Get("ResearchRewardWnd_4");
    }

    /// <summary>
    /// 刷新界面
    /// </summary>
    void Redraw()
    {
        // 先将所有item隐藏
        for (int i = 0; i < mChildList.Count; ++i)
        {
            mChildList [i].SetActive(false);
            mChildList [i].GetComponent<ResearchRewardItem> ().SetSelect (false);
        }
            

        int index = 0;
        GameObject item;

        // 获取配置的任务信息
        List<int> taskData = TaskMgr.GetTasksData (ME.user, TaskConst.RESEARCH_TASK, false);

        for (int i = 0; i < taskData.Count; i++)
        {
            // 任务已经完成
            if (TaskMgr.IsCompleted (ME.user, taskData [i]))
                continue;

            item = CreateItem (index);

            item.SetActive(true);

            item.GetComponent<ResearchRewardItem> ().BindData (taskData[i], ResearchRewardState.Uncomplete);

            // 每次刷新，默认选中第一个 
            if (index == 0)
                OnItemClick (item);
                
            index++;
        }

        // 获取正在发放奖励的数据
        LPCArray bonusData = ME.user.Query<LPCArray>("research_task/bonus");

        // 正在领取奖励的列表
        List<int> receivingList = new List<int>();

        if (bonusData != null)
        {
            for (int i = 0; i < bonusData.Count; i++)
            {
                item = CreateItem (index);
                item.SetActive(true);

                int taskId = bonusData[i].AsMapping.GetValue<int>("task_id");

                item.GetComponent<ResearchRewardItem> ().BindData (taskId,
                    ResearchRewardState.Receiving, bonusData[i].AsMapping.GetValue<int>("receive_times"));

                receivingList.Add(taskId);

                index++;
            }
        }

        // 获取已完成任务
        LPCArray completeData = ME.user.Query<LPCArray> ("research_task/complete");

        if (completeData != null)
        {
            for (int i = 0; i < completeData.Count; i++)
            {
                // 还有奖励未领取
                if (receivingList.Contains(completeData[i].AsInt))
                    continue;

                item = CreateItem (index);
                item.SetActive(true);

                item.GetComponent<ResearchRewardItem> ().BindData (completeData[i].AsInt, ResearchRewardState.Received);

                index++;
            }
        }
    }

    /// <summary>
    /// 创建格子
    /// </summary>
    /// <param name="i">The index.</param>
    GameObject CreateItem(int index)
    {
        if (index < mChildList.Count)
            return mChildList [index];

        // 重新创建item
        GameObject item = Instantiate(mRewardItem) as GameObject;
        item.transform.parent = mPanel;
        item.transform.localScale = Vector3.one;

        item.name = string.Format ("item_{0}", index);
        item.transform.localPosition = new Vector3(0, - index * 115f, 0);

        UIEventListener.Get(item).onClick = OnItemClick; 

        mChildList.Add (item);

        return item;
    }

    /// <summary>
    /// item被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnItemClick(GameObject ob)
    {
        ResearchRewardItem item = ob.GetComponent<ResearchRewardItem> ();

        // 已选中，不刷新
        if (item.IsSelect)
            return;

        foreach (Transform tf in mPanel)
        {
            if (tf.GetComponent<ResearchRewardItem> ().IsSelect)
                tf.GetComponent<ResearchRewardItem> ().SetSelect (false);
        }

        item.SetSelect (true);

        // 刷新奖励显示
        RedrawRewardInfo(item.TaskId, item.State, item.ReceiveTimes);
    }

    /// <summary>
    /// 刷新界面显示
    /// </summary>
    /// <param name="taskId">Task identifier.</param>
    /// <param name="ReceiveTimes">Receive times.</param>
    void RedrawRewardInfo(int taskId, ResearchRewardState state, int ReceiveTimes)
    {
        CsvRow item = TaskMgr.GetTaskInfo(taskId);
        if(item == null)
            return;

        mTitle.text = string.Format ("◆ {0} ◆", LocalizationMgr.Get(item.Query<string>("title")));

        mCompleteLb.text = LocalizationMgr.Get(item.Query<string> ("desc_args"));

        LPCArray bonus_args = item.Query<LPCArray> ("bonus_args");

        if (item.Query<int> ("is_one_time_receive") == 1)
            mReceiveTypeLb.text = LocalizationMgr.Get("ResearchRewardWnd_8");
        else
            mReceiveTypeLb.text = string.Format(LocalizationMgr.Get("ResearchRewardWnd_7"), bonus_args.Count);

        bool isShowReceive = (state == ResearchRewardState.Uncomplete) ? false: true;

        bool isShowDays = (item.Query<int> ("is_one_time_receive") == 1) ? false: true;

        mCompleteMark.SetActive (isShowReceive);

        for (int i = 0; i < mRewardGroup.Length; i++)
        {
            if (i < bonus_args.Count)
            {
                mRewardGroup [i].SetActive (true);

                if (state == ResearchRewardState.Receiving)
                {
                    if (i >= ReceiveTimes)
                        isShowReceive = false;
                }

                LPCMapping bonus = bonus_args[i].AsMapping.GetValue<LPCMapping>("bonus");

                mRewardGroup [i].GetComponent<SignItemWnd> ().Bind (bonus,
                    LocalizationMgr.Get("ResearchRewardWnd_9"), false, isShowReceive, isShowDays ? i + 1 : -1);
                
                continue;
            }

            mRewardGroup [i].SetActive (false);
        }
    }

    #endregion

}
