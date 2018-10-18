/// <summary>
/// GrowthManualItemWnd.cs
/// Created by 2018/01/16
/// 成长任务基础格子
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GrowthManualItemWnd : WindowBase<GrowthManualItemWnd>
{
    #region 成员变量

    // 任务图标
    public UITexture mIcon;

    // 任务标题
    public UILabel mTitle;

    // 完成标识
    public GameObject mFinish;

    // 进行中标识
    public GameObject mQuestionMark;

    // 新任务标识
    public GameObject mNewTips;

    // 奖励领取红点提示
    public GameObject mRedPoint;

    public GameObject mIconMask;

    public UISprite[] mLine;

    // 任务id
    [HideInInspector]
    public int mTaskId = 0;

    #endregion

    // Use this for initialization
    void Start ()
    {
        mNewTips.GetComponent<UISpriteAnimation>().namePrefix = ConfigMgr.IsCN ? "cnew" : "new";
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 获取任务配置数据
        CsvRow row = TaskMgr.GetTaskInfo(mTaskId);
        if (row == null)
            return;

        // 载入任务图标
        mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/task/{0}.png", row.Query<string>("icon")));

        // 显示任务标题
        mTitle.text = LocalizationMgr.Get(row.Query<string>("title"));

        // 刷新tips
        RefreshTips();
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(int taskId)
    {
        mTaskId = taskId;

        // 绘制窗口
        Redraw();
    }

    /// <summary>
    /// 刷新提示
    /// </summary>
    public void RefreshTips()
    {
        // 初始化角标
        mFinish.SetActive(false);
        mQuestionMark.SetActive(false);
        mNewTips.SetActive(false);
        mRedPoint.SetActive(false);
        mIconMask.SetActive(false);

        // 是否解锁
        if (TaskMgr.isUnlocked(ME.user, mTaskId))
        {
            // 任务是否完成
            if (TaskMgr.IsCompleted(ME.user, mTaskId))
            {
                // 奖励是否领取
                if (TaskMgr.isBounsReceived(ME.user, mTaskId))
                    mFinish.SetActive(true);
                else
                    mRedPoint.SetActive(true);
            }
            else
            {
                // 任务查看数据
                LPCValue task_view_data = OptionMgr.GetOption(ME.user, "task_view_data");
                if (task_view_data == null || !task_view_data.IsArray)
                {
                    mNewTips.SetActive(true);
                }
                else
                {
                    if (task_view_data.AsArray.IndexOf(mTaskId) != -1)
                        mQuestionMark.SetActive(true);
                    else
                        mNewTips.SetActive(true);
                }
            }

            for (int i = 0; i < mLine.Length; i++)
                mLine[i].color = new Color(1, 1, 1, 1);
        }
        else
        {
            mIconMask.SetActive(true);

            float rgb = 125f / 255f;

            for (int i = 0; i < mLine.Length; i++)
                mLine[i].color = new Color(rgb, rgb, rgb, rgb);
        }
    }
}
