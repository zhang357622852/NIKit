/// <summary>
/// MainWindow.cs
/// Created by xuhd Nov/27/2014
/// 主窗口，包含4个子窗口
/// TitleWnd、CombatWnd、MainMenuWnd、CombatAreaWnd
/// </summary>
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using LPC;
using QCCommonSDK.Addition;
using QCCommonSDK;

public class MainWnd : WindowBase<MainWnd>
{
    #region 成员变量

    // 玩家等级
    public UILabel mPlayerLevel;

    // 玩家姓名
    public UILabel mPlayerName;

    // 玩家经验百分比
    public UILabel mPlayerExp;

    // 玩家经验条
    public UISprite mExpSprite;

    // 玩家头像
    public UITexture mPlayerIcon;

    // 使魔
    public GameObject mPet;

    public UILabel mPetLabel;

    // 任务
    public GameObject mTask;

    public UILabel mTaskLabel;

    // 好友
    public GameObject mFriend;

    public UILabel mFriendLabel;

    // 冒险
    public GameObject mRisk;

    public UILabel mRiskLabel;

    // 商城按钮
    public GameObject mMarket;
    public UILabel mMarketLb;

    public GameObject mDungeonsBtn;
    public UILabel mDungeonsBtnLb;

    public GameObject mRetrunCityBtn;
    public UILabel mRetrunCityBtnLb;

    public GameObject mTowerBtn;
    public UILabel mTowerBtnLb;

    public UIScrollView mLeftScrollView;
    public UIGrid mLeftGrid;

    public UIScrollView mRightScrollView;
    public UIGrid mRightGrid;

    public GameObject mItem;

    public GameObject mLeftArrow;

    public TweenRotation mLeftArrowRotation;

    public GameObject mRightArrow;

    public TweenRotation mRightArrowRotation;

    public UIScrollBar mLeftBar;

    public UIScrollBar mRightBar;

    public GameObject mLeftDescViewWnd;

    // 右边 功能入口的描述Tip
    public GameObject mRightDescViewWnd;

    public GameObject mGotoViewWnd;

    public GameObject mLeftbg;
    public GameObject mRightbg;

    // 获得新宠物提示
    public UISpriteAnimation mNewPetTips;

    // 新的任务奖励提示
    public UISpriteAnimation mNewTaskRewardTips;

    // 新的好友请求提示
    public UISpriteAnimation mNewFriendRequestTips;

    // 新地图开启提示
    public UISpriteAnimation mNewMapTips;

    // Mardet new图标
    public UISpriteAnimation mNewMarketTips;

    public GameObject mTowerDiffSelectWnd;

    public GameObject mRedPoint;

    public GameObject mGameCourseTips;

    // 右侧ScrollView实体的初始位置
    Vector3 mRightPos;

    // 左侧ScrollView实体的初始位置
    Vector3 mLeftPos;

    // 活动图每页标限制数量
    int LimitAmount = 5;

    bool mIsUp = false;

    string mScreenLeftItem = "screen_left";
    string mScreenRightItem = "screen_right";

    // 主城界面小功能按钮缓存列表
    Dictionary<string, List<GameObject>> mButtons = new Dictionary<string, List<GameObject>>();

    // 系统功能列表
    Dictionary<int, List<LPCMapping>> mSystemFuntion = new Dictionary<int, List<LPCMapping>>();

    //// 临时代码，记录mainWnd打开途径，实际上就是窗口打开时的调用栈信息
    /// 定位bug用PQTC-868
    [HideInInspector]
    public string OnEnableStackTrace = string.Empty;

    #endregion

    #region 内部接口

    /// <summary>
    /// 检测是否有新的任务奖励
    /// </summary>
    void DoCheckNewTaskReward()
    {
        if (ME.user == null)
            return;

        // 获取系统功能列表
        mSystemFuntion[SystemFunctionConst.SCREEN_LEFT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_LEFT, SceneMgr.MainScene);

        // 刷新活动数据
        RefreshSystemFuncData(SystemFunctionConst.SCREEN_LEFT);

        // 是否有新的奖励
        if (TaskMgr.CheckHasBonus(ME.user))
        {
            mNewTaskRewardTips.gameObject.SetActive(true);

            mNewTaskRewardTips.ResetToBeginning();
        }
        else
        {
            mNewTaskRewardTips.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 检测是否有新装备或宠物
    /// </summary>
    void DoCheckNewPetOrEquip()
    {
        mNewPetTips.gameObject.SetActive(false);

        if (ME.user == null)
            return;

        List<Property> pets = BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_PET_GROUP);

        if (BaggageMgr.HasNewItem(pets))
        {
            mNewPetTips.gameObject.gameObject.SetActive(true);

            mNewPetTips.ResetToBeginning();

            return;
        }

        List<Property> equips = BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_ITEM_GROUP);

        if (BaggageMgr.HasNewItem(equips))
        {
            mNewPetTips.gameObject.gameObject.SetActive(true);

            mNewPetTips.ResetToBeginning();
        }
    }

    /// <summary>
    /// 检测是否有新的好友请求/好友邀请任务可领取奖励
    /// </summary>
    void DoCheckFriendRequest()
    {
        mNewFriendRequestTips.gameObject.SetActive(false);

        if (FriendMgr.HasFriendRequest(ME.user) || TaskMgr.GetInviteFriendTaskBounsCounts() > 0)
        {
            mNewFriendRequestTips.gameObject.SetActive(true);

            mNewFriendRequestTips.ResetToBeginning();
        }
    }

    /// <summary>
    /// 检测是否开启新地图
    /// </summary>
    void DoCheckNewMap()
    {
        mNewMapTips.gameObject.SetActive(false);

        if (MapMgr.HasNewMap(ME.user))
        {
            mNewMapTips.gameObject.SetActive(true);

            mNewMapTips.ResetToBeginning();
        }
    }

    void OnEnable()
    {
        //// 临时代码，记录mainWnd打开途径，实际上就是窗口打开时的调用栈信息
        /// 定位bug用PQTC-868
        OnEnableStackTrace = LogMgr.GetStackTrace();

        //// 如果这个时候MainWnd存在处于显示中
        //// 定位bug用PQTC-868
        GameObject gameOb = WindowMgr.GetWindow(CombatWnd.WndType);

        // 如果当前窗口处于显示状态
        if (gameOb != null && gameOb.activeSelf)
        {
            // 向服务器上传MainWnd打开是调用栈信息
            LogMgr.Error("Mainwnd in Combat Scene (Mainwnd)\n{0}", OnEnableStackTrace);
        }

        // 显示结算完成窗口
        ArenaMgr.ShowSettlementFinishWnd();

        // 设置玩家信息
        SetPlayerInfo();

        // 执行指引
        GuideMgr.DoGuide(GuideMgr.TOWER_GROUP);
    }

    /// <summary>
    /// Start this instance.
    /// </summary>
    void Start()
    {
        // 注册事件;
        RegisterEvent();

        // 初始化窗口;
        InitWnd();

        // 检测是否有新的任务奖励
        DoCheckNewTaskReward();

        // 检测是否有新装备或宠物
        DoCheckNewPetOrEquip();

        // 检测是否有新的好友请求信息
        DoCheckFriendRequest();

        // 检测是否开启新地图
        DoCheckNewMap();

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }
    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        // 在窗口组中移除;
        WindowMgr.RemoveWindowMapByName(MainWnd.WndType);

        // 解注册事件
        EventMgr.UnregisterEvent("MainWnd");

        if (ME.user != null)
        {
            // 移除角色属性字段变化回调
            ME.user.dbase.RemoveTriggerField("MainWnd");

            ME.user.tempdbase.RemoveTriggerField("MainWnd");

            // 解注册包裹变化回调
            ME.user.baggage.eventCarryChange -= BaggageChange;
        }

        // 取消消息关注
        MsgMgr.RemoveDoneHook("MSG_EXPRESS_OPERATE_DONE", "MainWnd_ExpressOperateDone");
        MsgMgr.RemoveDoneHook("MSG_NOTIFY_NEW_MAIL", "MainWnd_NotifyNewMail");
        MsgMgr.RemoveDoneHook("MSG_DO_ONE_GUIDE", "MainWnd_Guide");

        // 取消调用
        CancelInvoke("RefreshList");

#if !UNITY_EDITOR

        NativeSupport.OnNewMessageResult -= NewMessageHook;

#endif
    }

    /// <summary>
    /// Registers the event.
    /// </summary>
    void RegisterEvent()
    {
        // 监听销毁窗口事件
        EventMgr.RegisterEvent("MainWnd", EventMgrEventType.EVENT_DESTROY_WINDOW, OnDestroyWindow);

        // 监听好友请求事件
        EventMgr.RegisterEvent("MainWnd", EventMgrEventType.EVENT_CLICK_SECNE_WND, OnSceneWndClick);

        // 监听好友请求事件
        EventMgr.RegisterEvent("MainWnd", EventMgrEventType.EVENT_CLICK_MAP_WND, OnMapWndClick);

        // 监听活动列表变化事件
        EventMgr.RegisterEvent("MainWnd", EventMgrEventType.EVENT_NOTIFY_ACTIVITY_LIST, OnNotifyActivityList);

        // 监听好友操作结果事件
        EventMgr.RegisterEvent("MainWnd", EventMgrEventType.EVENT_FRIEND_OPERATE_DONE, OnFriendOperateDone);

        // 监听好友请求事件
        EventMgr.RegisterEvent("MainWnd", EventMgrEventType.EVENT_FRIEND_REQUEST, OnFriendRequest);

        // 注册新物品信息被清除
        EventMgr.RegisterEvent("MainWnd", EventMgrEventType.EVENT_CLEAR_NEW, ClearNewInfo);

        EventMgr.RegisterEvent("MainWnd", EventMgrEventType.EVENT_CLEARANCE_DATA_UPDATE, OnClearanceChange);

        EventMgr.RegisterEvent("MainWnd", EventMgrEventType.EVENT_NOTIFY_SETTLEMENT_FINISH, OnNotifySettlementFinsh);

        EventMgr.RegisterEvent("MainWnd", EventMgrEventType.EVENT_REFRESH_GROWTH_TASK_TIPS, OnRefeshGrowthTaskTips);

        EventMgr.RegisterEvent("MainWnd", EventMgrEventType.EVENT_REFRESH_EXPRESS_LIST, OnRefeshExpressList);

        // 关注MSG_EXPRESS_OPERATE_DONE
        MsgMgr.RegisterDoneHook("MSG_EXPRESS_OPERATE_DONE", "MainWnd_ExpressOperateDone", OnMsgExpressOperateDone);
        MsgMgr.RegisterDoneHook("MSG_NOTIFY_NEW_MAIL", "MainWnd_NotifyNewMail", OnMsgNotifyNewMail);

        // 关注MSG_DO_ONE_GUIDE消息
        MsgMgr.RegisterDoneHook("MSG_DO_ONE_GUIDE", "MainWnd_Guide", OnMsgDoOneGuide);

        //注册主城界面的点击事件;
        UIEventListener.Get(mPet).onClick = OnClickPetBtn;
        UIEventListener.Get(mTask).onClick = OnClickTaskBtn;
        UIEventListener.Get(mFriend).onClick = OnClickFriendBtn;
        UIEventListener.Get(mRisk).onClick = OnClickRiskBtn;
        UIEventListener.Get(mMarket).onClick = OnClickMarketBtn;
        UIEventListener.Get(mPlayerIcon.gameObject).onClick = OnClickIconBtn;
        UIEventListener.Get(mDungeonsBtn).onClick = OnClickDungeons;
        UIEventListener.Get(mRetrunCityBtn).onClick = OnClickReturnCity;
        UIEventListener.Get(mTowerBtn).onClick = OnClickTowerBtn;
//        UIEventListener.Get(mRightArrow).onClick = OnClickRightArrow;
//        UIEventListener.Get(mLeftArrow).onClick = OnClickLeftArrow;

        // 注册滑动条变化事件
        mRightBar.onChange.Add(new EventDelegate(OnRightScrollBarChage));
        mLeftBar.onChange.Add(new EventDelegate(OnLeftScrollBarChage));

#if !UNITY_EDITOR

        NativeSupport.OnNewMessageResult += NewMessageHook;

        NativeSupport.GetHSNewMessage();
#endif

        if (ME.user == null)
            return;

        // 监听玩家等级字段变化
        ME.user.dbase.RegisterTriggerField("MainWnd", new string[] { "level" }, new CallBack(OnChangeLevel));

        ME.user.dbase.RegisterTriggerField("MainWnd", new string[] { "exp" }, new CallBack(OnChangeLevel));

        // 监听玩家名称字段变化
        ME.user.dbase.RegisterTriggerField("MainWnd", new string[] { "name" }, new CallBack(OnChangeName));

        // 关注double_bonus_data字段变化
        ME.user.dbase.RegisterTriggerField("MainWnd", new string[]{ "double_bonus_data" }, new CallBack(OnChangeDounleItemList));
        ME.user.dbase.RegisterTriggerField("MainWnd", new string[]{ "free_unequip_data" }, new CallBack(OnChangeDounleItemList));

        ME.user.dbase.RegisterTriggerField("MainWnd", new string[] {"task"}, new CallBack (OnTaskChange));
        ME.user.dbase.RegisterTriggerField("MainWnd", new string[] {"daily_task"}, new CallBack (OnDailyTaskChange));

        ME.user.dbase.RegisterTriggerField("MainWnd", new string[] {"assign_task"}, new CallBack (OnAssignTaskChange));

        ME.user.dbase.RegisterTriggerField("MainWnd", new string[] {"clearance"}, new CallBack (OnClearanceFieldsChange));

        ME.user.dbase.RegisterTriggerField("MainWnd", new string[] { "icon" }, new CallBack(OnUserIconFiledsChange));

        ME.user.dbase.RegisterTriggerField("MainWnd", new string[] { "level_bonus" }, new CallBack(OnLevelBonusChange));

        ME.user.dbase.RegisterTriggerField("MainWnd", new string[] { "new_function" }, new CallBack(OnNewFunctionChange));

        ME.user.dbase.RegisterTriggerField("MainWnd", new string[]{"dynamic_gift", "intensify_gift", "limit_buy_data", "level_gift"}, new CallBack(OnFieldsChange));

        ME.user.tempdbase.RegisterTriggerField("MainWnd", new string[]{ "unlock_new_course" }, new CallBack(OnGameCourseFieldsChange));

        // 注册包裹变化回调
        ME.user.baggage.eventCarryChange += BaggageChange;

    }

    void OnGameCourseFieldsChange(object para, params object[] param)
    {
        // 显示游戏历程提示
        mGameCourseTips.SetActive(GameCourseMgr.GetNewUnlockCourses(ME.user).Count != 0);
    }

    void OnFieldsChange(object para, params object[] param)
    {
        // 获取系统功能列表
        mSystemFuntion[SystemFunctionConst.SCREEN_RIGHT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_RIGHT, SceneMgr.MainScene);

        // 刷新活动数据
        RefreshSystemFuncData(SystemFunctionConst.SCREEN_RIGHT);
    }

    void OnRefeshExpressList(int eventId, MixedValue para)
    {
        // 获取系统功能列表
        mSystemFuntion[SystemFunctionConst.SCREEN_LEFT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_LEFT, SceneMgr.MainScene);

        // 刷新活动数据
        RefreshSystemFuncData(SystemFunctionConst.SCREEN_LEFT);
    }

    void OnRefeshGrowthTaskTips(int eventId, MixedValue para)
    {
        // 获取系统功能列表
        mSystemFuntion[SystemFunctionConst.SCREEN_LEFT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_LEFT, SceneMgr.MainScene);

        // 刷新活动数据
        RefreshSystemFuncData(SystemFunctionConst.SCREEN_LEFT);
    }

    void OnNotifySettlementFinsh(int eventId, MixedValue para)
    {
        if (! gameObject.activeInHierarchy)
            return;

        ArenaMgr.ShowSettlementFinishWnd();
    }

    /// <summary>
    /// 清楚新物品信息回调
    /// </summary>
    void ClearNewInfo(int eventId, MixedValue para)
    {
        // 检测新物品提示
        DoCheckNewPetOrEquip();
    }

    /// <summary>
    /// 副本通关数据变化回调
    /// </summary>
    void OnClearanceChange(int eventId, MixedValue para)
    {
        // 检测是否开启新地图
        DoCheckNewMap();
    }

    /// <summary>
    /// 玩家头像字段变化
    /// </summary>
    void OnUserIconFiledsChange(object para, params object[] _params)
    {
        // 刷新玩家头像
        SetUserIcon();
    }

    void OnClearanceFieldsChange(object para, params object[] _params)
    {
        // 检测是否开启新地图
        DoCheckNewMap();
    }

    /// <summary>
    /// 好友操作结果事件回调
    /// </summary>
    void OnFriendOperateDone(int eventId, MixedValue para)
    {
        // 检测是否有新的好友请求信息
        DoCheckFriendRequest();
    }

    /// <summary>
    /// 好友请求事件回调
    /// </summary>
    void OnFriendRequest(int eventId, MixedValue para)
    {
        // 检测是否有新的好友请求信息
        DoCheckFriendRequest();
    }

    /// <summary>
    /// 包裹变化回调
    /// </summary>
    void BaggageChange(string[] pos)
    {
        // 检测是否有新装备或宠物
        DoCheckNewPetOrEquip();
    }

    /// <summary>
    /// 任务变化回调
    /// </summary>
    void OnTaskChange(object para, params object[] _params)
    {
        // 检测是否有新的任务奖励
        DoCheckNewTaskReward();
        DoCheckFriendRequest();
    }

    void OnDailyTaskChange(object para, params object[] _params)
    {
        // 检测是否有新的任务奖励
        DoCheckNewTaskReward();
    }

    void OnAssignTaskChange(object para, params object[] _params)
    {
        LPCMapping temp = LPCMapping.Empty;
        LPCValue v = ME.user.QueryTemp<LPCValue>("assign_task_sys_func");
        if (v != null && v.IsMapping)
            temp = v.AsMapping;

        List<int> idList = new List<int>();

        foreach (int id in temp.Keys)
            idList.Add(id);

        for (int i = 0; i < idList.Count; i++)
            temp[idList[i]] = LPCValue.Create(1);

        ME.user.SetTemp("assign_task_sys_func", LPCValue.Create(temp));

        // 获取系统功能列表
        mSystemFuntion[SystemFunctionConst.SCREEN_LEFT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_LEFT, SceneMgr.MainScene);

        // 刷新活动数据
        RefreshSystemFuncData(SystemFunctionConst.SCREEN_LEFT);
    }

    // 指引消息回调
    private void OnMsgDoOneGuide(string cmd, LPCValue para)
    {
        // 获取系统功能列表
        mSystemFuntion[SystemFunctionConst.SCREEN_LEFT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_LEFT, SceneMgr.MainScene);

        // 刷新活动数据
        RefreshSystemFuncData(SystemFunctionConst.SCREEN_LEFT);

        // 获取系统功能列表
        mSystemFuntion[SystemFunctionConst.SCREEN_RIGHT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_RIGHT, SceneMgr.MainScene);

        // 刷新活动数据
        RefreshSystemFuncData(SystemFunctionConst.SCREEN_RIGHT);

        // 刷新使魔按钮位置
        RefreshPetBtnPosition();
    }

    /// <summary>
    /// 双倍道具列表变化回调
    /// </summary>
    void OnChangeDounleItemList(object para, params object[] param)
    {
        // 获取系统功能列表
        mSystemFuntion[SystemFunctionConst.SCREEN_RIGHT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_RIGHT, SceneMgr.MainScene);

        // 刷新活动数据
        RefreshSystemFuncData(SystemFunctionConst.SCREEN_RIGHT);
    }

    /// <summary>
    /// 等级奖励发生变化
    /// </summary>
    void OnLevelBonusChange(object para, params object[] param)
    {
        // 获取系统功能列表
        mSystemFuntion[SystemFunctionConst.SCREEN_RIGHT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_RIGHT, SceneMgr.MainScene);

        // 刷新活动数据
        RefreshSystemFuncData(SystemFunctionConst.SCREEN_RIGHT);
    }

    /// <summary>
    /// 新功能变化回调
    /// </summary>
    void OnNewFunctionChange(object para, params object[] param)
    {
        if (ME.user.Query<int>("new_function/tower") == 1)
            mTowerBtn.SetActive(mDungeonsBtn.activeSelf);
        else
            mTowerBtn.SetActive(false);
    }

    /// <summary>
    /// 活动列表通知事件回调
    /// </summary>
    void OnNotifyActivityList(int eventID, MixedValue value)
    {
        // 获取系统功能列表
        mSystemFuntion[SystemFunctionConst.SCREEN_RIGHT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_RIGHT, SceneMgr.MainScene);

        // 刷新活动数据
        RefreshSystemFuncData(SystemFunctionConst.SCREEN_RIGHT);
    }

    /// <summary>
    /// 左侧滑动条变化回调
    /// </summary>
    void OnLeftScrollBarChage()
    {
        mLeftDescViewWnd.SetActive(false);

        mGotoViewWnd.SetActive(false);

        // TweenRotation动画的参数
        SetArrowRotation(mLeftArrowRotation, mLeftBar, mLeftArrowRotation.transform.localRotation.eulerAngles);
    }

    /// <summary>
    /// 右侧滑动条变化回调
    /// </summary>
    void OnRightScrollBarChage()
    {
        mRightDescViewWnd.GetComponent<DescViewWnd>().HideView();

        // TweenRotation动画的参数
        SetArrowRotation(mRightArrowRotation, mRightBar, mRightArrow.transform.localRotation.eulerAngles);
    }

    /// <summary>
    /// 设置箭头按钮的Tween动画参数
    /// </summary>
    void SetArrowRotation(TweenRotation tween, UIScrollBar scrollBar, Vector3 eulerAngles)
    {
        if (scrollBar.value >= 1.0f)
        {
            if (eulerAngles.z == 180f)
                return;

            tween.from.z = 0f;
            tween.to.z = 180f;

            mIsUp = true;
        }
        if (scrollBar.value <= 0f)
        {
            if (eulerAngles.z == 0f)
                return;

            tween.from.z = 180f;
            tween.to.z = 0f;

            mIsUp = false;
        }

        if (scrollBar.value > 0f && scrollBar.value < 1.0f)
            return;

        tween.PlayForward();
    }

    /// <summary>
    ///初始化窗口
    /// </summary>
    void InitWnd()
    {
        // 设置New本地化
        SetNewIcon();

        //初始化label
        SetLabelInitContent();

        // 刷新使魔按钮位置
        RefreshPetBtnPosition();

        // 显示游戏历程提示
        mGameCourseTips.SetActive(GameCourseMgr.GetNewUnlockCourses(ME.user).Count != 0);

        mItem.SetActive(false);

        mRedPoint.SetActive(false);

        mLeftDescViewWnd.SetActive(false);
        mRightDescViewWnd.GetComponent<DescViewWnd>().HideView();
        mGotoViewWnd.SetActive(false);

        mRightPos = mRightScrollView.transform.localPosition;

        mLeftPos = mLeftScrollView.transform.localPosition;

        mLeftBar.value = 0f;
        mRightBar.value = 0f;

        // 初始化缓存数据
        mButtons.Add(mScreenLeftItem, new List<GameObject>());
        mButtons.Add(mScreenRightItem, new List<GameObject>());

        mSystemFuntion.Add(SystemFunctionConst.SCREEN_LEFT, SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_LEFT, SceneMgr.MainScene));
        mSystemFuntion.Add(SystemFunctionConst.SCREEN_RIGHT, SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_RIGHT, SceneMgr.MainScene));

        // 创建按系统功能格子
        CreateSysFuncList(SystemFunctionConst.SCREEN_LEFT);
        CreateSysFuncList(SystemFunctionConst.SCREEN_RIGHT);

        // 刷新系统功能数据
        RefreshSystemFuncData(SystemFunctionConst.SCREEN_LEFT);
        RefreshSystemFuncData(SystemFunctionConst.SCREEN_RIGHT);

        // 每天零点刷新列表
        InvokeRepeating("RefreshList", (float) Game.GetZeroClock(1), (float) Game.GetZeroClock(1));
    }

    void SetNewIcon()
    {
        string namePrefix = ConfigMgr.IsCN ? "cnew" : "new";

        mNewFriendRequestTips.namePrefix = namePrefix;
        mNewMapTips.namePrefix = namePrefix;
        mNewPetTips.namePrefix = namePrefix;
        mNewTaskRewardTips.namePrefix = namePrefix;
        mNewMarketTips.namePrefix = namePrefix;
    }

    void RefreshList()
    {
        // 获取系统功能列表
        mSystemFuntion[SystemFunctionConst.SCREEN_RIGHT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_RIGHT, SceneMgr.MainScene);

        // 刷新活动数据
        RefreshSystemFuncData(SystemFunctionConst.SCREEN_RIGHT);

        ME.user.SetTemp("system_function", LPCValue.Create(LPCArray.Empty));
    }

    void SetMainListBox(UIScrollBar bar, GameObject bg, int count)
    {
        if (count > 4)
        {
            bg.SetActive(true);
            bar.gameObject.SetActive(true);
        }
        else
        {
            bg.SetActive(false);
            bar.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 创建系统功能列表
    /// </summary>
    void CreateSysFuncList(int showPos)
    {
        if (!mSystemFuntion.ContainsKey(showPos))
            return;

        List<LPCMapping> list = mSystemFuntion[showPos];
        if (list == null || list.Count == 0)
            return;

        Transform parent = null;

        bool isLeft = false;

        if (showPos.Equals(SystemFunctionConst.SCREEN_LEFT))
        {
            // 显示在屏幕左边

            parent = mLeftGrid.transform;

            isLeft = true;
        }
        else if (showPos.Equals(SystemFunctionConst.SCREEN_RIGHT))
        {
            // 显示在屏幕右边
            parent = mRightGrid.transform;

            isLeft = false;
        }
        else
        {

        }

        if (parent == null)
            return;

        for (int i = 0; i < list.Count; i++)
            AddButton(parent, isLeft, false);
    }

    /// <summary>
    /// 刷新系统功能数据
    /// </summary>
    void RefreshSystemFuncData(int showPos)
    {
        if (!mSystemFuntion.ContainsKey(showPos))
            return;

        List<LPCMapping> list = mSystemFuntion[showPos];
        if (list == null)
            return;

        UIGrid grid = null;

        UIScrollBar scrollBar = null;

        string itemName = string.Empty;

        GameObject bg = null;

        bool isLeft = false;

        if (showPos.Equals(SystemFunctionConst.SCREEN_LEFT))
        {
            // 显示在屏幕左边
            grid = mLeftGrid;

            scrollBar = mLeftBar;

            itemName = mScreenLeftItem;

            bg = mLeftbg;

            isLeft = true;
        }
        else if (showPos.Equals(SystemFunctionConst.SCREEN_RIGHT))
        {
            // 显示在屏幕右边
            grid = mRightGrid;

            scrollBar = mRightBar;

            itemName = mScreenRightItem;

            bg = mRightbg;

            isLeft = false;
        }
        else
        {
            return;
        }

        if (list.Count == 0)
        {
            for (int i = 0; i < mButtons[itemName].Count; i++)
            {
                GameObject btn = mButtons[itemName][i];
                if (btn == null)
                    continue;

                btn.SetActive(false);
            }

            return;
        }

        GameObject go = null;
        for (int i = 0; i < list.Count; i++)
        {
            if (i + 1 > mButtons[itemName].Count)
            {
                // 活动列表数量大于缓存的item数量,则添加的新的item
                go = AddButton(grid.transform, isLeft, false);
            }
            else
                go = mButtons[itemName][i];

            SystemFunctionItemWnd script = go.GetComponent<SystemFunctionItemWnd>();
            if (script == null)
                continue;

            if (go != null)
            {
                // 注册活动图标点击事件
                UIEventListener.Get(go).onClick = OnClickSysFuncBtn;
            }

            // 绑定数据
            script.Bind(list[i], i);

            go.SetActive(true);
        }

        // 隐藏多余的item
        for (int i = list.Count; i < mButtons[itemName].Count; i++)
            mButtons[itemName][i].SetActive(false);

        SetMainListBox(scrollBar, bg, list.Count);

        mRightDescViewWnd.GetComponent<DescViewWnd>().HideView();
        mLeftDescViewWnd.SetActive(false);
        mGotoViewWnd.SetActive(false);

        // 排序
        grid.repositionNow = true;
    }

    /// <summary>
    /// 活动图标点击回调
    /// </summary>
    //void OnClickSysFuncBtn(GameObject go, bool isPress)
    void OnClickSysFuncBtn(GameObject go)
    {
        LPCMapping data = LPCMapping.Empty;
        SystemFunctionItemWnd func = go.GetComponent<SystemFunctionItemWnd>();
        if (func == null)
            return;

        data = func.mData;

        if (data.ContainsKey("task_id"))
        {
            CsvRow taskData = TaskMgr.GetTaskInfo(data.GetValue<int>("task_id"));

            LPCValue taskScript = taskData.Query<LPCValue>("leave_for_script");
            if (!taskScript.IsInt || taskScript.AsInt == 0)
                return;

            // 立即前往 最后一个参数isPress = true
            ScriptMgr.Call(taskScript.AsInt, ME.user, taskData.Query<LPCValue>("leave_for_arg"), data, go, true);

            return;
        }

        // 点击事件回调执行脚本
        LPCValue executeScript = data.GetValue<LPCValue>("execute_script");
        if (!executeScript.IsInt || executeScript.AsInt == 0)
            return;

        // 调用脚本
        ScriptMgr.Call(executeScript.AsInt, data, go);
    }

    /// <summary>
    /// 销毁窗口事件
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="para"></param>
    private void OnDestroyWindow(int eventId, MixedValue para)
    {
        string wndName = para.GetValue<string>();

        // 处理登入礼包提示 播放图标移动动画
        if (ME.user != null)
        {
            LPCMapping giftTipsInfo = ME.user.QueryTemp<LPCMapping>("gift_tips_info");

            if (giftTipsInfo != null && giftTipsInfo.ContainsKey("gift_class_id") && giftTipsInfo.ContainsKey("gift_wnd_name") &&
                giftTipsInfo.GetValue<string>("gift_wnd_name").Equals(wndName))
            {
                GameObject wnd = WindowMgr.OpenWnd(GiftTipsAnimWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

                if (wnd == null)
                    return;

                wnd.GetComponent<GiftTipsAnimWnd>().BindData(giftTipsInfo.GetValue<int>("gift_class_id"));

                // 删除礼包信息
                ME.user.DeleteTemp("gift_tips_info");
            }
        }
    }

    /// <summary>
    /// 主窗口小窗口点击事件
    /// </summary>
    void OnSceneWndClick(int eventId, MixedValue para)
    {
        string wndName = para.GetValue<string>();

        switch (wndName)
        {
        case  "ArenaWnd":
            OnClickArenaWnd ();
            break;
        case  "DefenseTowerWnd":
            OnClickDefenseTowerWnd ();
            break;
        case  "LotteryWnd":
            OnClickLotteryWnd ();
            break;
        case  "MarketWnd":
            OnClickMarketWnd ();
            break;
        case  "SummonWnd":
            OnClickSummonWnd ();
            break;
        case  "StoreWnd":
            OnClickStoreWnd ();
            break;
        case  "PetSynthesisWnd":
            OnClickPetSynthesisWnd ();
            break;
        case  "SmeltWnd":
            OnClickSmeltWnd ();
            break;
        case  "PetInstituteWnd":
            OnClickPetInstituteWnd ();
            break;
        case  "GangWnd":
            OnClickGangWnd ();
            break;
        case  "DiscussWnd":
            OnClickDiscussWnd ();
            break;
        default:
            break;
        }

    }

    /// <summary>
    /// 地图小窗口点击事件
    /// </summary>
    void OnMapWndClick(int eventId, MixedValue para)
    {
        List<object> args = para.GetValue<List<object>>();
        if (args == null)
            return;

        int mapId = (int)args [0];

        Vector3 pos = ((Transform) args[1]).position;

        // x偏移量
        float offset_x = 1.0f;

        pos.x += offset_x;

        CsvRow mapConfig = MapMgr.GetMapConfig(mapId);

        // 地图类型
        int mapType = mapConfig.Query<int>("map_type");

        switch (mapType)
        {
            case MapConst.DUNGEONS_MAP_1 :

                // 通关兰达平原普通所有副本
                if (! GuideMgr.IsGuided(4))
                {
                    DialogMgr.Notify(LocalizationMgr.Get("GuideWnd_1"));

                    return;
                }

                // 创建地下城窗口
                GameObject dungeonsWnd = WindowMgr.OpenWnd(DungeonsWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (dungeonsWnd == null)
                    return;

                dungeonsWnd.GetComponent<DungeonsWnd>().Bind(string.Empty, 0, pos);

                WindowMgr.HideWindow(this.gameObject);

                mTowerDiffSelectWnd.SetActive(false);

                break;

            case MapConst.TOWER_MAP:

                if (mTowerDiffSelectWnd == null)
                    return;

                Vector3 position = new Vector3(4.76f, 7.78f, -20.4f);

                SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
                if (control != null)
                    control.MoveCamera(SceneMgr.SceneCamera.transform.position, position, 0.3f, 0f, new CallBack(OnMoveCallBack));

                // 缓存场景相机的位置
                SceneMgr.SceneCameraToPos = position;
                SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;

                break;

            case MapConst.ARENA_MAP:

                WindowMgr.HideWindow(this.gameObject);
                WindowMgr.OpenWnd(ArenaWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

                break;

            default:

                //获取副本选择界面;
                GameObject wnd = WindowMgr.OpenWnd(SelectInstanceWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

                // 窗口创建失败
                if (wnd == null)
                    return;

                SelectInstanceWnd selectInstanceScript = wnd.GetComponent<SelectInstanceWnd>();

                if (selectInstanceScript == null)
                    return;

                // 绑定数据
                selectInstanceScript.Bind(mapId, pos);

                WindowMgr.HideWindow(this.gameObject);

                mTowerDiffSelectWnd.SetActive(false);

                break;
        }
    }

    /// <summary>
    /// 相机移动完成回调
    /// </summary>
    void OnMoveCallBack(object para, params object[] param)
    {
        mTowerDiffSelectWnd.SetActive(true);
    }

    /// <summary>
    /// 消息回调
    /// </summary>
    void OnMsgExpressOperateDone(string cmd, LPCValue para)
    {
        // 获取系统功能列表
        mSystemFuntion[SystemFunctionConst.SCREEN_LEFT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_LEFT, SceneMgr.MainScene);

        // 刷新活动数据
        RefreshSystemFuncData(SystemFunctionConst.SCREEN_LEFT);
    }

    /// <summary>
    /// 新邮件消息回调
    /// </summary>
    void OnMsgNotifyNewMail(string cmd, LPCValue para)
    {
        // 获取系统功能列表
        mSystemFuntion[SystemFunctionConst.SCREEN_LEFT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_LEFT, SceneMgr.MainScene);

        // 刷新活动数据
        RefreshSystemFuncData(SystemFunctionConst.SCREEN_LEFT);
    }

    /// <summary>
    /// 玩家等级变化的回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="param">Parameter.</param>
    void OnChangeLevel(object para, params object[] param)
    {
        // 获取系统功能列表
        mSystemFuntion[SystemFunctionConst.SCREEN_LEFT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_LEFT, SceneMgr.MainScene);

        // 刷新玩家等级
        RedrawUserLevel();
    }

    /// <summary>
    /// 玩家名称变化回调
    /// </summary>
    void OnChangeName(object para, params object[] param)
    {
        mPlayerName.text = ME.user.Query<string>("name");
    }

    /// <summary>
    ///设置窗口label的初始内容
    /// </summary>
    void SetLabelInitContent()
    {
        mRiskLabel.text = LocalizationMgr.Get("MainWnd_4");
        mFriendLabel.text = LocalizationMgr.Get("MainWnd_5");
        mTaskLabel.text = LocalizationMgr.Get("MainWnd_6");
        mPetLabel.text = LocalizationMgr.Get("MainWnd_7");
        mDungeonsBtnLb.text = LocalizationMgr.Get("MainWnd_17");
        mRetrunCityBtnLb.text = LocalizationMgr.Get("MainWnd_18");
        mMarketLb.text = LocalizationMgr.Get("MainWnd_19");
        mTowerBtnLb.text = LocalizationMgr.Get("MainWnd_20");
    }

    /// <summary>
    /// 设置玩家信息
    /// </summary>
    void SetPlayerInfo()
    {
        //没有玩家信息;
        if (ME.user == null)
            return;

        //显示玩家名字;
        mPlayerName.text = ME.user.Query<string>("name");

        // 设置玩家头像
        SetUserIcon();

        RedrawUserLevel();
    }

    /// <summary>
    /// 设置玩家头像
    /// </summary>
    void SetUserIcon()
    {
        // 加载玩家头像
        LPCValue iconValue = ME.user.Query<LPCValue>("icon");
        if (iconValue != null && iconValue.IsString)
            mPlayerIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconValue.AsString));
        else
            mPlayerIcon.mainTexture = null;
    }

    /// <summary>
    /// 绘制玩家等级
    /// </summary>
    void RedrawUserLevel()
    {
        //玩家等级
        int level = ME.user.GetLevel();

        //获取玩家当前的经验;
        int exp = ME.user.Query<int>("exp");

        //获得玩家升级需要的标准经验;
        int currentLevelExp = StdMgr.GetUserStdExp(level + 1);

        float expPercent = 0;

        if (currentLevelExp == 0)
            expPercent = 0;
        else
            //计算经验百分比;
            expPercent = exp / (float)currentLevelExp;

        //设置玩家等级;
        mPlayerLevel.text = string.Format(LocalizationMgr.Get("MainWnd_13"), level);

        //玩家达到最大等级;
        if (level == MaxAttrib.GetMaxAttrib("max_user_level"))
        {
            mExpSprite.fillAmount = 1;
            mPlayerExp.text = LocalizationMgr.Get("MainWnd_14");
            return;
        }

        //设置进度条的值;
        mExpSprite.fillAmount = expPercent;

        //显示经验百分比;
        mPlayerExp.text = string.Format("{0}{1}", Math.Truncate(expPercent * 1000) / 10, "%");
    }

    /// <summary>
    /// 主城界面加入小功能按钮
    /// </summary>
    GameObject AddButton(Transform parent, bool isLeft, bool isActive = true)
    {
        if (mItem == null)
            return null;

        GameObject button = Instantiate(mItem);

        button.SetActive(isActive);

        List<GameObject> buttons = new List<GameObject>();
        string dir = string.Empty;
        if (isLeft)
        {
            if (mButtons.ContainsKey(mScreenLeftItem))
                buttons = mButtons[mScreenLeftItem];
            dir = mScreenLeftItem;
        }
        else
        {
            if (mButtons.ContainsKey(mScreenRightItem))
                buttons = mButtons[mScreenRightItem];
            dir = mScreenRightItem;
        }

        // 设置父级
        button.transform.SetParent(parent);
        button.transform.localPosition = Vector3.zero;
        button.transform.localScale = Vector3.one;

        button.name = string.Format("{0}_{1}", buttons.Count, dir);
        buttons.Add(button);

        if (buttons.Count > LimitAmount)
        {
            // 暂时屏蔽
//            if (!isLeft)
//                mRightArrow.SetActive(true);
//            else
//                mLeftArrow.SetActive(true);
        }

        // 将创建的按钮缓存到列表中
        if (mButtons.ContainsKey(dir))
            mButtons.Remove(dir);
        mButtons.Add(dir, buttons);

        return button;
    }

    /// <summary>
    /// 右侧箭头点击事件
    /// </summary>
    void OnClickRightArrow(GameObject go)
    {
        // 设置ScrollView的位置
        SetScrollView(mScreenRightItem, mRightGrid.cellHeight, mRightPos, mRightScrollView, mRightBar, mIsUp);
    }

    /// <summary>
    /// 右侧箭头点击事件
    /// </summary>
    void OnClickLeftArrow(GameObject go)
    {
        // 设置ScrollView的位置
        SetScrollView(mScreenLeftItem, mLeftGrid.cellHeight, mLeftPos, mLeftScrollView, mLeftBar, mIsUp);
    }

    /// <summary>
    /// 设置ScrollView的位置
    /// </summary>
    /// <param name="key">键值</param>
    /// <param name="cellHeight">Grid的高度</param>
    /// <param name="initPos">ScrollView实体的初始位置</param>
    /// <param name="scrollView">ScrollView 组件</param>
    /// <param name="arrow">箭头实体</param>
    void SetScrollView(string key, float cellHeight, Vector3 initPos, UIScrollView scrollView, UIScrollBar scrollBar, bool isUp)
    {
        if (!mButtons.ContainsKey(key))
            return;

        // 目标位置
        Vector3 targetPos;

        // 当前页数
        int currentPage = 0;

        // 总页数
        int totalPage = 0;

        // 下一页数
        int next_page = 0;

        if (mButtons[mScreenRightItem].Count >= LimitAmount * 2)
        {
            currentPage = CalcCurrentPage(scrollView.transform.localPosition, initPos, cellHeight);

            totalPage = CalcCurrentPage(
                new Vector3(initPos.x, initPos.y + cellHeight * mButtons[mScreenRightItem].Count, initPos.z),
                initPos,
                cellHeight);

            if (currentPage + 1 >= totalPage)
                currentPage = totalPage - 1;

            if (isUp)
            {
                // 向上翻页
                next_page = currentPage - 1;
            }
            else
            {
                // 向下翻页
                next_page = currentPage + 1;
            }

            if (next_page >= totalPage)
                next_page = totalPage - 1;

            if (next_page < 0)
                next_page = 0;

            targetPos = new Vector3(initPos.x,
                initPos.y + next_page * LimitAmount * cellHeight,
                initPos.z);
        }
        else
        {
            targetPos = new Vector3(initPos.x,
                initPos.y + mButtons[key].Count * cellHeight,
                initPos.z);
        }

        // 开始滑动
        SpringPanel.Begin(scrollView.gameObject, targetPos, 10f);
    }

    /// <summary>
    /// 计算当前页数
    /// </summary>
    int CalcCurrentPage(Vector3 currentPos, Vector3 initPos, float cellHeight)
    {
        // 获取y轴的偏移量
        float offset_y = Mathf.Abs(currentPos.y - initPos.y);

        float pageHeight = LimitAmount * cellHeight;

        float remainder = offset_y % pageHeight;

        int result = (int) (offset_y / pageHeight);

        return remainder >= pageHeight * 0.5f ? result + 1 : result;
    }

    /// <summary>
    /// 主城玩家论坛点击回调
    /// </summary>
    void OnClickDiscussWnd()
    {
        string url = ConfigMgr.Get<string>("bbs_url", string.Empty);

        // 没有论坛地址不显示
        if (string.IsNullOrEmpty(url))
            return;

        GameObject wnd = WindowMgr.OpenWnd("WebViewWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        wnd.GetComponent<WebViewWnd>().BindData(url);
    }

    /// <summary>
    /// 公会点击事件回调
    /// </summary>
    void OnClickGangWnd()
    {
        // 打开工会窗口
        WindowMgr.OpenWnd(GangWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 竞技场窗口点击事件
    /// </summary>
    void OnClickArenaWnd()
    {
        WindowMgr.HideWindow(this.gameObject);
        WindowMgr.OpenWnd(ArenaWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 防御之塔点击事件
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnClickDefenseTowerWnd()
    {
        LogMgr.Trace("OnClickDefenseTowerWnd");
    }

    /// <summary>
    /// 抽奖窗口点击事件
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnClickLotteryWnd()
    {
        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<MaskWnd>().Play();
        wnd.GetComponent<MaskWnd>().Bind(new CallBack(OnLotteryMaskCallback));
    }

    /// <summary>
    /// 进入抽奖场景的云回调
    /// </summary>
    /// <param name="para"></param>
    /// <param name="param"></param>
    private void OnLotteryMaskCallback(object para, object[] param)
    {
        WindowMgr.HideWindow(gameObject);

        // 进入抽奖场景
        SceneMgr.LoadScene("Main", SceneConst.SCENE_LOTTERY_BONUS, new CallBack(OnEnterLotteryScene));
    }

    /// <summary>
    /// 进入抽奖场景回调
    /// </summary>
    private void OnEnterLotteryScene(object para, object[] param)
    {
        WindowMgr.HideAllWnd();

        WindowMgr.OpenWnd(LotteryBonusWnd.WndType, null, WindowOpenGroup.MULTIPLE_OPEN_WND);
        WindowMgr.OpenWnd(LotteryBonusSVWnd.WndType, null, WindowOpenGroup.MULTIPLE_OPEN_WND);

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd != null)
            wnd.GetComponent<MaskWnd>().PlayerRevers();


    }

    /// <summary>
    /// 市场窗口点击事件
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnClickMarketWnd()
    {
        GameObject wnd = WindowMgr.OpenWnd(ShopWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
        {
            LogMgr.Trace("ShopWnd窗口创建失败");
            return;
        }

        // 隐藏窗口
        WindowMgr.HideWindow(this.gameObject);

        SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();

        SceneMgr.SceneCameraFromPos = control.transform.localPosition;

        SceneMgr.SceneCameraToPos = new Vector3(3.29f, 0.64f, -16.28f);

        if (control != null)
            control.MoveCamera(control.transform.localPosition, SceneMgr.SceneCameraToPos);
    }

    /// <summary>
    /// 召唤窗口点击事件
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnClickSummonWnd()
    {
        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<MaskWnd>().Play();

        wnd.GetComponent<MaskWnd>().Bind(new CallBack(OnSummonMaskCallBack));
    }

    void OnSummonMaskCallBack(object para, object[] param)
    {
        // 关闭主界面
        WindowMgr.HideWindow(gameObject);

        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_SUMMON, new CallBack(OnEnterSummonScene));
    }

    /// <summary>
    /// 打开召唤界面
    /// </summary>
    private void OnEnterSummonScene(object para, object[] param)
    {
        WindowMgr.HideAllWnd();

        // 打开主场景
        WindowMgr.OpenWnd("SummonWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd != null)
            wnd.GetComponent<MaskWnd>().PlayerRevers();
    }

    /// <summary>
    /// 创库窗口点击事件
    /// </summary>
    void OnClickStoreWnd()
    {
        // 关闭主界面
        WindowMgr.HideWindow(gameObject);

        WindowMgr.OpenWnd("StoreWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 宠物合成窗口点击事件
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnClickPetSynthesisWnd()
    {
        WindowMgr.HideWindow(gameObject);

        // 显示宠物合成窗口
        WindowMgr.OpenWnd("PetSynthesisWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 精髓合成窗口点击事件
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnClickSmeltWnd()
    {
        // 关闭主界面
        WindowMgr.HideWindow(gameObject);

        WindowMgr.OpenWnd("SynthesisWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 宠物研究所窗口点击事件
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnClickPetInstituteWnd()
    {
        WindowMgr.HideWindow(gameObject);

        WindowMgr.OpenWnd("PetStrengthenWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    ///冒险按钮点击事件
    /// </summary>
    void OnClickRiskBtn(GameObject go)
    {
        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<MaskWnd>().Play();

        wnd.GetComponent<MaskWnd>().Bind(new CallBack(OnWorldMaskCallBack));
    }

    void OnWorldMaskCallBack(object para, params object[] param)
    {
        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OnEnterWorldMapScene));
    }

    /// <summary>
    /// 打开世界地图回调
    /// </summary>
    private void OnEnterWorldMapScene(object para, object[] param)
    {
        WindowMgr.HideAllWnd();

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd != null)
            wnd.GetComponent<MaskWnd>().PlayerRevers();

        // 隐藏主场景窗口
        ShowMainUIBtn(false);

        WindowMgr.OpenMainWnd();

        // 刷新系统功能图标
        mSystemFuntion[SystemFunctionConst.SCREEN_LEFT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_LEFT, SceneMgr.MainScene);
        mSystemFuntion[SystemFunctionConst.SCREEN_RIGHT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_RIGHT, SceneMgr.MainScene);

        RefreshSystemFuncData(SystemFunctionConst.SCREEN_LEFT);
        RefreshSystemFuncData(SystemFunctionConst.SCREEN_RIGHT);
    }

    /// <summary>
    /// 商城按钮点击事件
    /// </summary>
    void OnClickMarketBtn(GameObject go)
    {
        WindowMgr.HideWindow(this.gameObject);

        // 打开商城界面
        WindowMgr.OpenWnd(MarketWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 北境圣域按钮点击事件
    /// </summary>
    void OnClickDungeons(GameObject go)
    {
        // 指引没有完成
        if (! GuideMgr.IsGuided(4))
        {
            DialogMgr.Notify(LocalizationMgr.Get("GuideWnd_1"));

            return;
        }

        // 相机移动的目标位置
        Vector3 targetPos = new Vector3(-4.25f, 10.86f, -15);

        // 创建地下城窗口
        GameObject wnd = WindowMgr.OpenWnd(DungeonsWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<DungeonsWnd>().Bind(string.Empty, 0, targetPos);

        // 隐藏当前窗口
        WindowMgr.HideWindow(this.gameObject);

        mTowerDiffSelectWnd.SetActive(false);
    }

    /// <summary>
    ///返回城镇按钮点击事件
    /// </summary>
    void OnClickReturnCity(GameObject go)
    {
        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<MaskWnd>().Play();

        wnd.GetComponent<MaskWnd>().Bind(new CallBack(OnMainMaskCallBack));
    }

    void OnMainMaskCallBack(object para, params object[] param)
    {
        // 抛出切换地图事件
        SceneMgr.LoadScene("Main", SceneConst.SCENE_MAIN_CITY, new CallBack(OnEnterMainCityScene));
    }

    /// <summary>
    /// 打开主城回调
    /// </summary>
    private void OnEnterMainCityScene(object para, object[] param)
    {
        // 返回城镇
        ShowMainUIBtn(true);

        mTowerDiffSelectWnd.SetActive(false);

        WindowMgr.HideAllWnd();

        GameObject wnd = WindowMgr.OpenWnd(MaskWnd.WndType);
        if (wnd != null)
            wnd.GetComponent<MaskWnd>().PlayerRevers();

        WindowMgr.OpenMainWnd();

        // 刷新系统功能图标
        mSystemFuntion[SystemFunctionConst.SCREEN_LEFT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_LEFT, SceneMgr.MainScene);
        mSystemFuntion[SystemFunctionConst.SCREEN_RIGHT] = SystemFunctionMgr.GetFuncList(ME.user, SystemFunctionConst.SCREEN_RIGHT, SceneMgr.MainScene);

        RefreshSystemFuncData(SystemFunctionConst.SCREEN_LEFT);
        RefreshSystemFuncData(SystemFunctionConst.SCREEN_RIGHT);
    }

    /// <summary>
    /// 通天塔按钮点击事件
    /// </summary>
    void OnClickTowerBtn(GameObject go)
    {
        if (mTowerDiffSelectWnd.activeInHierarchy)
            return;

        if (mTowerDiffSelectWnd == null)
            return;

        Vector3 position = new Vector3(4.76f, 7.78f, -20.4f);

        SceneCamera control = SceneMgr.SceneCamera.gameObject.GetComponent<SceneCamera>();
        if (control != null)
            control.MoveCamera(SceneMgr.SceneCamera.transform.position, position, 0.3f, 0f, new CallBack(OnMoveCallBack));

        // 缓存场景相机的位置
        SceneMgr.SceneCameraToPos = position;
        SceneMgr.SceneCameraFromPos = SceneMgr.SceneCamera.transform.position;
    }

    /// <summary>
    ///邮件按钮点击事件
    /// </summary>
    void OnClickMailBtn(GameObject go)
    {
        WindowMgr.OpenWnd("MailWnd");
    }

    /// <summary>
    ///玩家头像点击事件
    /// </summary>
    void OnClickIconBtn(GameObject go)
    {
        WindowMgr.OpenWnd("SystemWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    ///使魔按钮点击事件
    /// </summary>
    void OnClickPetBtn(GameObject go)
    {
        GameObject wnd = WindowMgr.OpenWnd("BaggageWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        WindowMgr.HideWindow(this.gameObject);
    }

    /// <summary>
    /// 获取sku消息的回调
    /// </summary>
    private void NewMessageHook(QCEventResult result)
    {
        if(!result.result.ContainsKey("number"))
            return;

        string number = result.result["number"];

        mRedPoint.SetActive(int.Parse(number) > 0 ? true : false);
    }

    /// <summary>
    ///任务按钮点击事件
    /// </summary>
    void OnClickTaskBtn(GameObject go)
    {
        // 关闭主界面
        WindowMgr.HideWindow(gameObject);

        WindowMgr.OpenWnd("TaskWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 好友按钮点击事件
    /// </summary>
    void OnClickFriendBtn(GameObject go)
    {
        // 关闭主界面
        WindowMgr.HideWindow(gameObject);

        // 打开好友界面
        WindowMgr.OpenWnd(FriendWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 刷新使魔按钮位置
    /// </summary>
    void RefreshPetBtnPosition()
    {
        if (SceneMgr.MainScene.Equals(SceneConst.SCENE_MAIN_CITY))
        {
            if (GuideMgr.IsGuided(GuideConst.ARENA_FINISH))
            {
                mPet.transform.localPosition = new Vector3(-528.8f, 93, 0);
            }
            else
            {
                mPet.transform.localPosition = new Vector3(-202, 93, 0);
            }
        }
        else if (SceneMgr.MainScene.Equals(SceneConst.SCENE_WORLD_MAP))
        {
            // 竞技场知应完成，并且通天塔功能开启
            if (GuideMgr.IsGuided(GuideConst.ARENA_FINISH) && ME.user.Query<int>("new_function/tower") == 1)
            {
                mPet.transform.localPosition = new Vector3(-457f, 93, 0);
            }
            else
            {
                mPet.transform.localPosition = new Vector3(-335.8f, 93, 0);
            }
        }
    }

    #endregion

    #region 外部接口

    public void ShowMainUIBtn(bool isShow)
    {
        // 刷新使魔按钮位置
        RefreshPetBtnPosition();

        // 隐藏主城相关窗口
        mRisk.SetActive(isShow);
        mFriend.SetActive(isShow);
        mTask.SetActive(isShow);
        mMarket.SetActive(isShow);
        mLeftScrollView.gameObject.SetActive(isShow);

        if (! isShow)
            mGotoViewWnd.GetComponent<GotoWnd>().HideWnd();

        // 显示大地图相关窗口
        mDungeonsBtn.SetActive(!isShow);
        mRetrunCityBtn.SetActive(!isShow);

        if (ME.user == null)
            return;

        if (ME.user.Query<int>("new_function/tower") == 1)
            mTowerBtn.SetActive(!isShow);
        else
            mTowerBtn.SetActive(false);
    }

    /// <summary>
    /// 获取右边功能菜单
    /// </summary>
    /// <param name="functionId"></param>
    /// <returns></returns>
    public Transform GetRightFunctionMenu(int functionId)
    {
        if (mRightGrid == null)
            return null;

        List<Transform> children = mRightGrid.GetChildList();

        if (children.Count <= 0)
            return null;

        SystemFunctionItemWnd ctrl;

        for (int i = 0; i < children.Count; i++)
        {
            ctrl = children[i].GetComponent<SystemFunctionItemWnd>();

            if (ctrl != null && ctrl.mData != null && ctrl.mData.GetValue<int>("id") == functionId)
                return children[i];
        }

        return null;
    }

    /// <summary>
    /// 获取商店菜单
    /// </summary>
    /// <returns></returns>
    public Transform GetShopMenu()
    {
        if (mMarket == null)
            return null;

        return mMarket.transform;
    }

    /// <summary>
    /// 指引点击通天塔
    /// </summary>
    public void GuideClickTower()
    {
        OnClickTowerBtn(mTowerBtn);
    }

    #endregion
}
