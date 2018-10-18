/// <summary>
/// GrowthTaskBonusWnd.cs
/// Created by fengsc 2018/01/17
/// 成长任务奖励领取界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GrowthTaskBonusWnd : WindowBase<GrowthTaskBonusWnd>
{
    // 标题
    public UILabel mTitle;

    public UISprite mBg;

    // 关闭按钮
    public GameObject mCloseBtn;

    // 描述
    public UILabel mDesc;

    // 提示
    public UILabel mTips;

    // 解锁任务提示
    public UILabel mUnLockTips;

    public UITexture mTaskItems;

    public UIGrid mTaskGrid;

    public UILabel mNoTask;

    public GameObject mItemWnd;
    public UIGrid mItemWndGrid;

    // 领取按钮
    public GameObject mReceiveBtn;
    public UILabel mReceiveBtnLb;

    public UISpriteAnimation mAnimation;

    public GameObject mBtnMask;

    public GameObject mBtnLock;

    public GameObject mFinish;

    public GameObject mDescLock;

    // 奖励显示框
    public GameObject mBonusScrollView;

    public UILabel mLockTips;

    // 奖励显示框可以显示数量
    private int mShowBonusItemNum = 3;

    // 任务id
    int mTaskId = 0;

    CsvRow mRow = null;

    LPCArray mBonus = LPCArray.Empty;

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();

        // 初始化本地化文本
        InitText();
    }

    void OnDestroy()
    {
        if (ME.user == null)
            return;

        // 移除字段关注
        ME.user.dbase.RemoveTriggerField("GrowthTaskBonusWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mReceiveBtn).onClick = OnClickReceiveBtn;

        // 关注字段变化
        ME.user.dbase.RemoveTriggerField("GrowthTaskBonusWnd");
        ME.user.dbase.RegisterTriggerField("GrowthTaskBonusWnd", new string[] {"task"}, new CallBack (OnTaskChange));
    }

    /// <summary>
    /// task字段变化回调
    /// </summary>
    void OnTaskChange(object para, params object[] _params)
    {
        Refresh();
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitText()
    {
        mNoTask.text = LocalizationMgr.Get("GrowthTaskBonusWnd_4");
        mUnLockTips.text = LocalizationMgr.Get("GrowthTaskBonusWnd_3");
    }

    /// <summary>
    /// 关闭按钮点击事件回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 领取按钮点击事件回调
    /// </summary>
    void OnClickReceiveBtn(GameObject go)
    {
        // 任务未解锁
        if (!TaskMgr.isUnlocked(ME.user, mTaskId))
            return;

        // 任务没有完成
        if (!TaskMgr.IsCompleted(ME.user, mTaskId))
        {
            int script = mRow.Query<int>("leave_for_script");

            if(script <= 0)
                return;

            if ((bool) ScriptMgr.Call(script, ME.user, mRow.Query<LPCValue>("leave_for_arg")))
            {
                WindowMgr.DestroyWindow(TaskWnd.WndType);
                WindowMgr.DestroyWindow(GrowthManualWnd.WndType);
                WindowMgr.DestroyWindow(GrowthTaskBonusWnd.WndType);
            }

            return;
        }

        // 奖励已经领取
        if (TaskMgr.isBounsReceived(ME.user, mTaskId))
            return;

        // 检测是否可以领取奖励
        if(!TaskMgr.CheckCanReceiveBonus(ME.user, mTaskId))
            return;

        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);

        // 服务器领取奖励
        Operation.CmdReceiveBonus.Go(mTaskId);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        Refresh();

        List<int> afterId = TaskMgr.GetPostpositionTasksById(mTaskId);

        if (afterId == null)
            afterId = new List<int>();

        mTaskItems.gameObject.SetActive(false);
        for (int i = 0; i < afterId.Count; i++)
        {
            GameObject clone = Instantiate(mTaskItems.gameObject);

            clone.transform.SetParent(mTaskGrid.transform);

            clone.transform.localPosition = Vector3.zero;

            clone.transform.localScale = Vector3.one;

            clone.name = "task_item_" + i;

            clone.GetComponent<UITexture>().mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/task/{0}.png", TaskMgr.GetIcon(afterId[i])));

            clone.SetActive(true);
        }

        // 排序控件
        mTaskGrid.Reposition();

        if (afterId == null || afterId.Count == 0)
            mNoTask.gameObject.SetActive(true);

        // 奖励
        mBonus = TaskMgr.GetBonus(ME.user, mTaskId);
        if (mBonus == null)
            mBonus = LPCArray.Empty;

        mItemWnd.SetActive(false);

        for (int i = 0; i < mBonus.Count; i++)
        {
            GameObject item = Instantiate(mItemWnd);

            item.transform.SetParent(mItemWndGrid.transform);

            item.transform.localPosition = Vector3.zero;

            item.transform.localScale = Vector3.one;

            item.name = "item_" + i;

            item.SetActive(true);

            SignItemWnd script = item.GetComponent<SignItemWnd>();
            if (script == null)
                continue;

            // 绑定数据
            script.NormalItemBind(mBonus[i].AsMapping, false);
            script.ShowAmount(false);
        }

        mItemWndGrid.Reposition();

        if (mBonus.Count > mShowBonusItemNum)
            mBonusScrollView.GetComponent<UIScrollView>().ResetPosition();
    }

    void Refresh()
    {
        // 任务配置数据
        mRow = TaskMgr.GetTaskInfo(mTaskId);
        if (mRow == null)
            return;

        LPCArray rgb = mRow.Query<LPCArray>("icon_color");

        mBg.color = new Color(rgb[0].AsFloat / 255f, rgb[1].AsFloat / 255f, rgb[2].AsFloat / 255f);

        mBtnMask.SetActive(false);
        mBtnLock.SetActive(false);
        mReceiveBtnLb.gameObject.SetActive(false);
        mTips.gameObject.SetActive(false);
        mDesc.gameObject.SetActive(false);
        mLockTips.gameObject.SetActive(false);
        mDescLock.SetActive(false);
        mNoTask.gameObject.SetActive(false);
        mFinish.SetActive(false);

        string title = LocalizationMgr.Get(mRow.Query<string>("title"));

        // 是否解锁
        if (TaskMgr.isUnlocked(ME.user, mTaskId))
        {
            // 任务是否完成
            if (TaskMgr.IsCompleted(ME.user, mTaskId))
            {
                if (TaskMgr.isBounsReceived(ME.user, mTaskId))
                {
                    mFinish.SetActive(true);
                }
                else
                {
                    mReceiveBtnLb.text = LocalizationMgr.Get("GrowthTaskBonusWnd_1");
                    mReceiveBtnLb.gameObject.SetActive(true);
                }
            }
            else
            {
                mReceiveBtnLb.text = LocalizationMgr.Get("GrowthTaskBonusWnd_7");

                mReceiveBtnLb.gameObject.SetActive(true);
            }

            mTips.gameObject.SetActive(true);
            mDesc.gameObject.SetActive(true);

            // 任务标题
            mTitle.text = string.Format("◆ {0} ◆", title);

            // 任务描述
            mDesc.text = TaskMgr.GetTaskDesc(ME.user, mTaskId);

            // 任务提示
            mTips.text = LocalizationMgr.Get(mRow.Query<string>("tips"));
        }
        else
        {
            mBtnMask.SetActive(true);
            mBtnLock.SetActive(true);
            mLockTips.gameObject.SetActive(true);
            mDescLock.SetActive(true);

            // 任务标题
            mTitle.text = string.Format("◆ {0}{1} ◆", title, LocalizationMgr.Get("GrowthTaskBonusWnd_5"));

            string optionName = string.Empty;

            switch (mRow.Query<int>("type"))
            {
                case TaskConst.GROWTH_TASK_LEVEL_STAR: 
                    optionName = LocalizationMgr.Get("GrowthManualWnd_3");
                    break;

                case TaskConst.GROWTH_TASK_SUIT_INTENSIFY: 
                    optionName = LocalizationMgr.Get("GrowthManualWnd_4");
                    break;

                case TaskConst.GROWTH_TASK_AWAKE: 
                    optionName = LocalizationMgr.Get("GrowthManualWnd_5");
                    break;

                case TaskConst.GROWTH_TASK_ARENA: 
                    optionName = LocalizationMgr.Get("GrowthManualWnd_6");
                    break;
                default:
                    break;
            }

            CsvRow preRow = TaskMgr.GetTaskInfo(mRow.Query<int>("pre_id"));

            mLockTips.text = string.Format(
                LocalizationMgr.Get("GrowthTaskBonusWnd_2"),
                optionName,
                LocalizationMgr.Get(preRow.Query<string>("title"))
            );
        }
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
}
