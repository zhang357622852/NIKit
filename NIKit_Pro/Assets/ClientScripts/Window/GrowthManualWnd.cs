/// <summary>
/// GrowthManualWnd.cs
/// Created by fengsc 2018/01/16
/// 成长手册界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class GrowthManualWnd : WindowBase<GrowthManualWnd>
{
    #region 成员变量

    // 标题
    public UILabel mTitle;

    // 任务数量
    public UILabel mAmount;

    // 完成提示
    public UILabel mFinishTips;
    public UILabel mItemName;
    public UILabel mItemDesc;

    // 奖励物品格子
    public GameObject mRewardItem;

    // 奖励物品图标
    public UITexture mIcon;

    public UILabel mItemAmount;

    public GameObject mRewardRedPoint;

    public GameObject mRewardFinish;

    public GameObject mRewardEffect;

    // 关闭按钮
    public GameObject mCloseBtn;

    // 星级等级按钮
    public GameObject mLevelBtn;
    public UILabel mLevelBtnLb;

    // 套装强化按钮
    public GameObject mSuitStrengthBtn;
    public UILabel mSuitStrengthBtnLb;

    // 进阶觉醒
    public GameObject mAwakeBtn;
    public UILabel mAwakeBtnLb;

    // 竞技王者
    public GameObject mSportsBtn;
    public UILabel mSportsBtnLb;

    // 详细帮助
    public GameObject mManualHelpBtn;
    public UILabel mManualHelpBtnLb;

    public TweenScale mTweenScale;

    public GameObject[] mCheckMarks;
    public GameObject[] mBgs;
    public GameObject[] mRedPoints;
    public GameObject[] mLabels;

    public GameObject[] mManualItem1;
    public GameObject[] mManualItem2;
    public GameObject[] mManualItem3;
    public GameObject[] mManualItem4;

    public GameObject[] mWnds;

    // 提示窗口相关控件
    public GameObject mTipWnd;
    public UILabel mTipWndTitle;
    public UILabel mTipWndName;
    public UITexture mTipWndIcon;
    public UILabel mTipWndBonusTip;
    public UILabel mTipWndConfirm;
    public UILabel mTipWndToggleTip;
    public GameObject mTipWndToggleBtn;
    public GameObject mItemWnd;
    public UIGrid mItemWndGrid;
    public GameObject mBonusScrollView;

    // 奖励显示框可以显示数量
    private int mShowBonusItemNum = 3;

    // 分页常量
    private const int PAGE1 = 0;
    private const int PAGE2 = 1;
    private const int PAGE3 = 2;
    private const int PAGE4 = 3;

    // 当前分页
    private int mCurrentPage = PAGE1;

    private int mTaskType = TaskConst.GROWTH_TASK_LEVEL_STAR;

    List<int> mTypeList = new List<int>();

    bool mIsClick = false;

    Property mItemOb;

    /// <summary>
    /// The m property ob.
    /// </summary>
    Property mPropOb = null;

    LPCMapping mBonus = LPCMapping.Empty;

    int mFinishTaskId = 0;

    /// <summary>
    /// The m cache sign item window.
    /// </summary>
    private List<SignItemWnd> mCacheSignItemWnd = new List<SignItemWnd>();

    #endregion

    void OnDestroy()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        // 取消关注
        MsgMgr.RemoveDoneHook("MSG_RECEIVE_BONUS", "GrowthManualWnd");

        // 析构临时窗口
        if (mItemOb != null)
            mItemOb.Destroy();

        // 析构临时对象
        if (mPropOb != null)
            mPropOb.Destroy();

        // 移除字段关注
        if (ME.user != null)
            ME.user.dbase.RemoveTriggerField("GrowthManualWnd");
    }

    // Use this for initialization
    void Start ()
    {
        // 初始化本地化文本
        InitText();

        // 注册事件
        RegisterEvent();

        // 绘制窗口
        Redraw();

        // 显示最终奖励
        ShowFinishReward();

        // 绘制提示窗口
        RedrawTipWnd();

        // 刷新
        DoRefresh();
    }

    /// <summary>
    /// Redraws the tip window.
    /// </summary>
    void RedrawTipWnd()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 获取玩家是否需要显示提示窗口
        int hideGrowthTipWnd = ME.user.QueryTemp<int>("hide_growth_tip_wnd");

        // 不需要显示
        if (hideGrowthTipWnd == 1 ||
            TaskMgr.IsCompleted(ME.user, mFinishTaskId) ||
            mItemOb == null)
        {
            mTipWnd.SetActive(false);
            return;
        }

        // 显示文本信息
        mTipWndTitle.text = LocalizationMgr.Get("GrowthManualWnd_12");
        mTipWndBonusTip.text = LocalizationMgr.Get("GrowthManualWnd_13");
        mTipWndConfirm.text = LocalizationMgr.Get("GrowthManualWnd_14");
        mTipWndToggleTip.text = LocalizationMgr.Get("GrowthManualWnd_15");

        // 显示窗口
        mTipWnd.SetActive(true);

        // 绘制头像信息
        mTipWndName.text = LocalizationMgr.Get(mItemOb.GetName());
        mTipWndIcon.mainTexture = ItemMgr.GetTexture(mItemOb.Query<string>("clear_icon"));

        // 填充奖励数据
        LPCArray summonList = mItemOb.Query<LPCArray>("summon_list");
        if (summonList == null)
            return;

        // 填充数据
        for (int i = 0; i < summonList.Count; i++)
        {
            int classId = summonList[i].AsInt;

            // 获取一个控件
            SignItemWnd wndOb = GetSignItemWnd(i);
            if (wndOb == null)
                continue;

            LPCMapping dbase = LPCMapping.Empty;
            dbase.Add("class_id", summonList[i]);

            // 绑定数据
            wndOb.NormalItemBind(dbase, false);
            wndOb.ShowAmount(false);

            // 注册点击事件
            UIEventListener.Get(wndOb.gameObject).onClick = OnItemBtn;
        }

        // 重置位置
        mItemWndGrid.Reposition();

        // 重置UIScrollView位置
        if (mBonus.Count > mShowBonusItemNum)
            mBonusScrollView.GetComponent<UIScrollView>().ResetPosition();
    }

    /// <summary>
    /// Gets the sign item window.
    /// </summary>
    /// <returns>The sign item window.</returns>
    /// <param name="index">Index.</param>
    private SignItemWnd GetSignItemWnd(int index)
    {
        // 直接获取缓存数据
        if (mCacheSignItemWnd.Count > index &&
            mCacheSignItemWnd[index] != null)
            return mCacheSignItemWnd[index];

        // 创建新对象
        GameObject item = Instantiate(mItemWnd);
        item.transform.SetParent(mItemWndGrid.transform);
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one;
        item.name = "item_" + index;
        item.SetActive(true);

        // 添加到缓存列表中
        SignItemWnd signItemWnd = item.GetComponent<SignItemWnd>();
        mCacheSignItemWnd.Add(signItemWnd);
        return signItemWnd;
    }

    /// <summary>
    /// 初始本地化文本
    /// </summary>
    void InitText()
    {
        mTitle.text = LocalizationMgr.Get("GrowthManualWnd_1");
        mFinishTips.text = LocalizationMgr.Get("GrowthManualWnd_2");
        mLevelBtnLb.text = LocalizationMgr.Get("GrowthManualWnd_3");
        mSuitStrengthBtnLb.text = LocalizationMgr.Get("GrowthManualWnd_4");
        mAwakeBtnLb.text = LocalizationMgr.Get("GrowthManualWnd_5");
        mSportsBtnLb.text = LocalizationMgr.Get("GrowthManualWnd_6");
        mManualHelpBtnLb.text = LocalizationMgr.Get("GrowthManualWnd_7");
        mItemDesc.text = LocalizationMgr.Get("GrowthManualWnd_16");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mRewardItem).onClick = OnClickRewardItem;
        UIEventListener.Get(mLevelBtn).onClick = OnClickLevelBtn;
        UIEventListener.Get(mSuitStrengthBtn).onClick = OnClickSuitBtn;
        UIEventListener.Get(mAwakeBtn).onClick = OnClickAwakeBtn;
        UIEventListener.Get(mSportsBtn).onClick = OnClickSportsBtn;
        UIEventListener.Get(mManualHelpBtn).onClick = OnClickManualHelpBtn;
        UIEventListener.Get(mTipWndConfirm.gameObject).onClick = OnClickConfirmBtn;
        UIEventListener.Get(mTipWndToggleBtn).onClick = OnClickTipWndToggleBtn;

        for (int i = 0; i < mManualItem1.Length; i++)
            UIEventListener.Get(mManualItem1[i]).onClick = OnClickManualItem;

        for (int i = 0; i < mManualItem2.Length; i++)
            UIEventListener.Get(mManualItem2[i]).onClick = OnClickManualItem;

        for (int i = 0; i < mManualItem3.Length; i++)
            UIEventListener.Get(mManualItem3[i]).onClick = OnClickManualItem;

        for (int i = 0; i < mManualItem4.Length; i++)
            UIEventListener.Get(mManualItem4[i]).onClick = OnClickManualItem;

        // 关注字段变化
        ME.user.dbase.RemoveTriggerField("GrowthManualWnd");
        ME.user.dbase.RegisterTriggerField("GrowthManualWnd", new string[] {"task"}, new CallBack (OnTaskChange));

        // 关注msg_receive_bonus消息变化
        MsgMgr.RegisterDoneHook("MSG_RECEIVE_BONUS", "GrowthManualWnd", OnMsgReceiveBonus);
    }

    /// <summary>
    /// 奖励领取消息回调
    /// </summary>
    void OnMsgReceiveBonus(string cmd, LPCValue para)
    {
        if (!para.IsMapping || ! para.AsMapping.ContainsKey("task_id"))
            return;

        int taskId = para.AsMapping.GetValue<int>("task_id");

        // 任务配置数据
        CsvRow row = TaskMgr.GetTaskInfo(taskId);
        if (row == null)
            return;

        LPCValue receiveTips = row.Query<LPCValue>("receive_tips");
        if (receiveTips == null || !receiveTips.IsInt)
            return;

        // 调用脚本显示领取提示
        ScriptMgr.Call(receiveTips.AsInt, row.Query<string>("receive_tips_arg"), taskId, ME.user);
    }

    /// <summary>
    /// task字段变化回调
    /// </summary>
    void OnTaskChange(object para, params object[] _params)
    {
        // 刷新界面
        DoRefresh();
    }

    /// <summary>
    /// 执行刷新
    /// </summary>
    void DoRefresh()
    {
        // 刷新按钮状态
        RefreshToggleBtnStatus();

        // 刷新任务数据
        RefreshTaskData(mTaskType);
    }

    /// <summary>
    /// 关闭按钮点击回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 关闭当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 点击奖励品回调
    /// </summary>
    void OnClickRewardItem(GameObject go)
    {
        if (mItemOb == null)
            return;

        if (TaskMgr.IsCompleted(ME.user, mFinishTaskId) && !TaskMgr.isBounsReceived(ME.user, mFinishTaskId))
        {
            int amount = 1;

            if (mBonus.ContainsKey("class_id"))
            {
                if (mBonus.ContainsKey("amount"))
                    amount = mBonus.GetValue<int>("amount");
            }
            else
            {
                string fields = FieldsMgr.GetFieldInMapping(mBonus);

                amount = mBonus.GetValue<int>(fields);
            }

            DialogMgr.ShowSingleBtnDailog(
                new CallBack(ConfirmCallBack),
                string.Format(LocalizationMgr.Get("GrowthManualWnd_9"), LocalizationMgr.Get(mItemOb.GetName()), amount),
                LocalizationMgr.Get("GrowthManualWnd_8")
            );

            return;
        }

        if (MonsterMgr.IsMonster(mItemOb))
        {
            if (mItemOb == null)
                return;

            // 创建窗口
            GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

            // 创建窗口失败
            if (wnd == null)
                return;

            PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();

            if (script == null)
                return;

            script.Bind(mItemOb);
            script.ShowBtn(true);
        }
        else if (EquipMgr.IsEquipment(mItemOb) || ItemMgr.IsItem(mItemOb))
        {
            if (mItemOb == null)
                return;

            // 创建窗口
            GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (wnd == null)
                return;

            wnd.GetComponent<RewardItemInfoWnd>().SetPropData(mItemOb, true, false, LocalizationMgr.Get("GrowthManualWnd_10"));
            wnd.GetComponent<RewardItemInfoWnd>().SetMask(true);
        }
    }

    void ConfirmCallBack(object para, params object[] param)
    {
        // 服务器领取奖励
        Operation.CmdReceiveBonus.Go(mFinishTaskId);
    }

    /// <summary>
    /// 星级等级按钮点击回调
    /// </summary>
    void OnClickLevelBtn(GameObject go)
    {
        if (mCurrentPage == PAGE1)
            return;

        mCurrentPage = PAGE1;

        mTaskType = TaskConst.GROWTH_TASK_LEVEL_STAR;

        DoRefresh();
    }

    /// <summary>
    /// 套装强化按钮点击回调
    /// </summary>
    void OnClickSuitBtn(GameObject go)
    {
        if (mCurrentPage == PAGE2)
            return;

        mCurrentPage = PAGE2;

        mTaskType = TaskConst.GROWTH_TASK_SUIT_INTENSIFY;

        DoRefresh();
    }

    /// <summary>
    /// 进阶觉醒按钮点击回调
    /// </summary>
    void OnClickAwakeBtn(GameObject go)
    {
        if (mCurrentPage == PAGE3)
            return;

        mCurrentPage = PAGE3;

        mTaskType = TaskConst.GROWTH_TASK_AWAKE;

        DoRefresh();
    }

    /// <summary>
    /// 竞技王者按钮点击回调
    /// </summary>
    void OnClickSportsBtn(GameObject go)
    {
        if (mCurrentPage == PAGE4)
            return;

        mCurrentPage = PAGE4;

        mTaskType = TaskConst.GROWTH_TASK_ARENA;

        DoRefresh();
    }

    /// <summary>
    /// 详细帮助按钮点击回调
    /// </summary>
    void OnClickManualHelpBtn(GameObject go)
    {
        // 获取帮助信息界面;
        GameObject wnd = WindowMgr.OpenWnd(HelpWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (wnd == null)
            return;

        switch (mCurrentPage)
        {
            case PAGE1:
                wnd.GetComponent<HelpWnd>().Bind(HelpConst.PET_STAR_ID);
                break;

            case PAGE2:
                wnd.GetComponent<HelpWnd>().Bind(HelpConst.EQUIP_ID);
                break;

            case PAGE3:
                wnd.GetComponent<HelpWnd>().Bind(HelpConst.PET_RANK_ID);
                break;

            case PAGE4:
                wnd.GetComponent<HelpWnd>().Bind(HelpConst.MAIN_CITY_ID);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Raises the click confirm button event.
    /// </summary>
    /// <param name="go">Go.</param>
    void OnClickConfirmBtn(GameObject go)
    {
        // 隐藏窗口
        mTipWnd.SetActive(false);
    }

    /// <summary>
    /// Raises the click tip window toggle button event.
    /// </summary>
    /// <param name="go">Go.</param>
    void OnClickTipWndToggleBtn(GameObject go)
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 设置玩家是否需要显示提示窗口
        ME.user.SetTemp("hide_growth_tip_wnd", LPCValue.Create(go.GetComponent<UIToggle>().value ? 1 : 0));
    }

    /// <summary>
    /// 物体被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnItemBtn(GameObject ob)
    {
        // 获取奖励数据
        LPCMapping itemData = ob.GetComponent<SignItemWnd>().mData;
        if (itemData == null)
            return;

        if (itemData.ContainsKey("class_id"))
        {
            int classId = itemData.GetValue<int>("class_id");

            // 构造参数
            LPCMapping dbase = LPCMapping.Empty;

            dbase.Append(itemData);
            dbase.Add("rid", Rid.New());

            // 克隆物件对象
            if (mPropOb != null)
                mPropOb.Destroy();

            mPropOb = PropertyMgr.CreateProperty(dbase);

            if (MonsterMgr.IsMonster(classId))
            {
                // 显示宠物悬浮窗口
                GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();

                script.Bind(mPropOb);
                script.ShowBtn(true, false, false);
            }
            else if (EquipMgr.IsEquipment(classId))
            {
                GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

                script.SetEquipData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));

                script.SetMask(true);
            }
            else
            {
                GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

                script.SetPropData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));

                script.SetMask(true);
            }
        }
        else
        {
            string fields = FieldsMgr.GetFieldInMapping(itemData);

            int classId = FieldsMgr.GetClassIdByAttrib(fields);

            // 构造参数
            LPCMapping dbase = LPCMapping.Empty;
            dbase.Add("class_id", classId);
            dbase.Add("amount", itemData.GetValue<int>(fields));
            dbase.Add("rid", Rid.New());

            if (mPropOb != null)
                mPropOb.Destroy();

            mPropOb = PropertyMgr.CreateProperty(dbase);

            GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (wnd == null)
                return;

            RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

            script.SetPropData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));

            script.SetMask(true);
        }
    }

    /// <summary>
    /// 任务格子点击事件
    /// </summary>
    void OnClickManualItem(GameObject go)
    {
        if (mIsClick)
            return;

        mIsClick = true;

        GrowthManualItemWnd script = go.GetComponent<GrowthManualItemWnd>();
        if (script == null)
            return;

        LPCArray list = LPCArray.Empty;

        LPCValue v = OptionMgr.GetOption(ME.user, "task_view_data");
        if (v != null && v.IsArray)
            list = v.AsArray;

        int taskId = script.mTaskId;

        if (list.IndexOf(taskId) == -1)
        {
            list.Add(taskId);

            // 缓存数据
            OptionMgr.SetOption(ME.user, "task_view_data", LPCValue.Create(list));

            EventMgr.FireEvent(EventMgrEventType.EVENT_REFRESH_GROWTH_TASK_TIPS, null);
        }

        // 刷新提示信息
        script.RefreshTips();

        // 打开奖励窗口
        GameObject wnd = WindowMgr.OpenWnd(GrowthTaskBonusWnd.WndType);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<GrowthTaskBonusWnd>().Bind(taskId);

        // 刷新红点提示
        RefreshRedPoint();

        mIsClick = false;
    }

    void OnFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        if (mTweenScale != null)
        {
            float scale = Game.CalcWndScale();
            mTweenScale.to = new Vector3(scale, scale, scale);

            EventDelegate.Add(mTweenScale.onFinished, OnFinish);
        }

        mTypeList.Add(TaskConst.GROWTH_TASK_LEVEL_STAR);
        mTypeList.Add(TaskConst.GROWTH_TASK_SUIT_INTENSIFY);
        mTypeList.Add(TaskConst.GROWTH_TASK_AWAKE);
        mTypeList.Add(TaskConst.GROWTH_TASK_ARENA);

        List<int> taskId = new List<int>();

        int completeAmount = 0;

        for (int i = 0; i < mTypeList.Count; i++)
            taskId.AddRange(TaskMgr.GetTasksByType(mTypeList[i]));

        for (int i = 0; i < taskId.Count; i++)
        {
            if (TaskMgr.IsCompleted(ME.user, taskId[i]))
                completeAmount++;
        }

        // 显示完成的任务数量和任务总数
        mAmount.text = string.Format("({0}/{1})", completeAmount, taskId.Count);
    }

    /// <summary>
    /// 刷新任务数据
    /// </summary>
    void RefreshTaskData(int type)
    {
        for (int i = 0; i < mWnds.Length; i++)
        {
            if (i == mCurrentPage)
                mWnds[i].SetActive(true);
            else
                mWnds[i].SetActive(false);
        }

        // 根据类型获取任务列表
        List<int> taskList = TaskMgr.GetTasksByType(type);
        if (taskList == null)
            return;

        GameObject[] items = null;
        switch (type)
        {
            case TaskConst.GROWTH_TASK_LEVEL_STAR: 
                items = mManualItem1;
                break;

            case TaskConst.GROWTH_TASK_SUIT_INTENSIFY: 
                items = mManualItem2;
                break;

            case TaskConst.GROWTH_TASK_AWAKE: 
                items = mManualItem3;
                break;

            case TaskConst.GROWTH_TASK_ARENA: 
                items = mManualItem4;
                break;

            default:
                break;
        }

        for (int i = 0; i < taskList.Count; i++)
        {
            if (i + 1 > items.Length)
                break;

            GrowthManualItemWnd script = items[i].GetComponent<GrowthManualItemWnd>();
            if (script == null)
                continue;

            // 绑定数据
            script.Bind(taskList[i]);
        }
    }

    /// <summary>
    /// 刷新按钮状态
    /// </summary>
    void RefreshToggleBtnStatus()
    {
        for (int i = 0; i < mBgs.Length; i++)
        {
            if (i == mCurrentPage)
                mBgs[i].SetActive(false);
            else
                mBgs[i].SetActive(true);
        }

        for (int i = 0; i < mCheckMarks.Length; i++)
        {
            if (i == mCurrentPage)
                mCheckMarks[i].SetActive(true);
            else
                mCheckMarks[i].SetActive(false);
        }

        for (int i = 0; i < mLabels.Length; i++)
        {
            GameObject item = mLabels[i];
            if (i == mCurrentPage)
            {
                item.transform.localPosition = new Vector3(55.2f, item.transform.localPosition.y, item.transform.localPosition.z);
            }
            else
            {
                item.transform.localPosition = new Vector3(64f, item.transform.localPosition.y, item.transform.localPosition.z);
            }
        }

        // 刷新红点提示
        RefreshRedPoint();

        bool isRecevie = TaskMgr.isBounsReceived(ME.user, mFinishTaskId);

        // 最终奖励红点提示
        mRewardRedPoint.SetActive(TaskMgr.IsCompleted(ME.user, mFinishTaskId) && ! isRecevie);

        // 最终奖励是否领取
        mRewardFinish.SetActive(isRecevie);

        mRewardEffect.SetActive(! isRecevie);
    }

    /// <summary>
    /// 刷新红点提示
    /// </summary>
    void RefreshRedPoint()
    {
        for (int i = 0; i < mRedPoints.Length; i++)
        {
            GameObject item = mRedPoints[i];
            if (i == mCurrentPage)
                item.transform.localPosition = new Vector3(-30.1f, item.transform.localPosition.y, item.transform.localPosition.z);
            else
                item.transform.localPosition = new Vector3(-12.1f, item.transform.localPosition.y, item.transform.localPosition.z);

            // 红点提示
            if (TaskMgr.CheckHasBonus(ME.user, mTypeList[i]))
                item.SetActive(true);
            else
                item.SetActive(false);
        }
    }

    /// <summary>
    /// 显示最终奖励物品
    /// </summary>
    void ShowFinishReward()
    {
        List<int> finishTask = TaskMgr.GetTasksByType(TaskConst.GROWTH_TASK);

        mFinishTaskId = finishTask[0];

        // 任务配置数据
        CsvRow task = TaskMgr.GetTaskInfo(mFinishTaskId);
        if (task == null)
            return;

        mFinishTips.text = LocalizationMgr.Get(task.Query<string>("tips"));

        // 最终奖励
        LPCArray bonusList = TaskMgr.GetBonus(ME.user, finishTask[0]);

        mBonus = bonusList[0].AsMapping;

        Texture2D tex = null;

        string descAmount = string.Empty;

        LPCMapping para = LPCMapping.Empty;
        para.Add("rid", Rid.New());

        if (mBonus.ContainsKey("class_id"))
        {
            mAmount.alignment = NGUIText.Alignment.Right;

            if (mBonus.ContainsKey("amount"))
                descAmount = "×" + mBonus.GetValue<int>("amount");
            else
                descAmount = "×" + 1;

            int classId = mBonus.GetValue<int>("class_id");

            para.Add("class_id", classId);

            if (MonsterMgr.IsMonster(classId))
            {
                tex = MonsterMgr.GetTexture(classId, mBonus.GetValue<int>("rank"));

                para.Add("rank", mBonus.GetValue<int>("rank"));
                para.Add("star", mBonus.GetValue<int>("star"));
                para.Add("level", mBonus.GetValue<int>("level"));
            }
            else if (EquipMgr.IsEquipment(classId))
            {
            }
            else if (ItemMgr.IsItem(classId))
            {
                tex = ItemMgr.GetTexture(classId, false);
            }
        }
        else
        {
            mAmount.alignment = NGUIText.Alignment.Center;

            string fields = FieldsMgr.GetFieldInMapping(mBonus);

            para.Add("class_id", FieldsMgr.GetClassIdByAttrib(fields));

            tex = ResourceMgr.LoadTexture(FieldsMgr.GetFieldTexture(fields));

            descAmount = mBonus.GetValue<string>(fields);
        }

        // 奖励物品图标
        mIcon.mainTexture = tex;

        mItemAmount.text = descAmount;

        mItemOb = PropertyMgr.CreateProperty(para);

        // 绘制道具的名称
        mItemName.text = mItemOb.Short();
    }
}

