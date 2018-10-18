using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class LesenaDairyWnd : WindowBase<LesenaDairyWnd>
{

    #region 成员变量

    // 界面关闭按钮
    public GameObject closeBtn;

    // 拖拽界面的panel
    public UIPanel panel;

    // 子任务按钮
    public BoxCollider[] missionBtns;

    // 子任务按钮文本
    public UILabel[] missionBtnLab;

    // 子任务按钮容器
    public UIWidget[] missionBtnWidget;

    // 子任务描述
    public UILabel[] missionContents;

    // 子任务物品奖励
    public SignItemWnd[] missionRewardItem;

    // 子任务积分奖励
    public UILabel[] missionRewardScore;

    // 神秘大礼进度条
    public UISlider mysteryGiftProgress;

    // 神秘大礼积分显示
    public UILabel mysteryGiftScoreLab;

    // 积分按钮
    public BoxCollider[] mysteryGiftBtns;

    // 积分按钮文本
    public UILabel[] mysteryGiftBtnLab;

    // 积分按钮容器
    public UIWidget[] mysteryGiftBtnWidget;

    // 积分第一组奖励
    public SignItemWnd[] mysteryGiftRewardOne;

    // 积分第二组奖励
    public SignItemWnd[] mysteryGiftRewardTwo;

    // 活动标题
    public UILabel title;

    // 活动时间
    public UILabel activityTime;

    // 活动内容
    public UILabel activityContent;

    // 具体活动内容
    public UILabel activityContentValue;

    // 子任务标题
    public UILabel subActivityTitle;

    // 神秘大礼标题
    public UILabel mysteryGiftTitle;

    // 神秘大礼描述1
    public UILabel mysteryGiftDesc1;

    // 神秘大礼描述2
    public UILabel mysteryGiftDesc2;

    #endregion

    #region 私有变量

    // cookie
    string mActivityCookie;

    // 活动数据
    private LPCMapping mActivityData = LPCMapping.Empty;

    // 神秘大礼积分
    int[] mysteryGiftScore;

    /// <summary>
    /// The m property ob.
    /// </summary>
    Property mPropOb = null;

    #endregion

    #region 内部函数

    void Start()
    {
        // 初始化窗口
        InitWnd();

        // 初始化多语言
        initLocalization();

        TweenScale mTweenScale = GetComponent<TweenScale>();

        if (mTweenScale == null)
            return;

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    void OnDestroy()
    {
        // 取消消息回调
        MsgMgr.RemoveDoneHook("MSG_RECEIVE_SCORE_BONUS", "LesenaDairyWnd");

        // 取消监听
        if (ME.user != null)
            ME.user.dbase.RemoveTriggerField("LesenaDairyWnd");

        // 析构临时对象
        if (mPropOb != null)
            mPropOb.Destroy();
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    void InitWnd()
    {
        // 注册事件
        registerEvent();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void registerEvent()
    {
        UIEventListener.Get(closeBtn).onClick = OnCloseClick;

        // 监听数据发生变化
        ME.user.dbase.RemoveTriggerField("LesenaDairyWnd");
        ME.user.dbase.RegisterTriggerField("LesenaDairyWnd", new string[] { "activity_data" }, new CallBack(OnActivityChanged));

        // 监听领取奖励成功消息
        MsgMgr.RegisterDoneHook("MSG_RECEIVE_SCORE_BONUS", "LesenaDairyWnd", OnReceiveScoreBonusMsg);

        // 注册每日任务按钮事件
        registerMissionBtnEvent();

        // 注册积分按钮事件
        registerScoreBtnEvent();
    }

    void OnReceiveScoreBonusMsg(string cmd, LPCValue para)
    {
        DialogMgr.ShowSingleBtnDailog(
            null,
            LocalizationMgr.Get("LesenaDairyWnd_13"),
            string.Empty,
            string.Empty,
            true,
            this.transform
        );
    }

    /// <summary>
    /// 注册每日任务按钮事件
    /// </summary>
    void registerMissionBtnEvent()
    {
        for (int i = 0; i < missionBtns.Length; i++)
        {
            UIEventListener.Get(missionBtns[i].gameObject).onClick = OnMissionBtnClick;
        }
    }

    /// <summary>
    /// 注册积分按钮事件
    /// </summary>
    void registerScoreBtnEvent()
    {
        for (int i = 0; i < mysteryGiftBtns.Length; i++)
        {
            UIEventListener.Get(mysteryGiftBtns[i].gameObject).onClick = OnScoreBtnClick;
        }
    }

    /// <summary>
    /// 初始化多语言 
    /// </summary>
    void initLocalization()
    {
        title.text = LocalizationMgr.Get("LesenaDairyWnd_1");
        activityContent.text = LocalizationMgr.Get("LesenaDairyWnd_3");
        activityContentValue.text = LocalizationMgr.Get("LesenaDairyWnd_4");
        subActivityTitle.text = LocalizationMgr.Get("LesenaDairyWnd_5");
        mysteryGiftTitle.text = LocalizationMgr.Get("LesenaDairyWnd_10");
        mysteryGiftDesc1.text = LocalizationMgr.Get("LesenaDairyWnd_11");
    }

    /// <summary>
    /// 根据数据展示活动信息
    /// </summary>
    /// <param name="activityInfo">Activity info.</param>
    void initByData(LPCMapping activityInfo)
    {
        // 赋值
        mActivityData = activityInfo;

        // 获取 cookie
        mActivityCookie = mActivityData.GetValue<LPCValue>("cookie").AsString;

        // 有效时间段列表
        LPCArray validPeriod = mActivityData.GetValue<LPCArray>("valid_period");

        // 刷新活动的剩余时间
        activityTime.text = ActivityMgr.GetActivityTimeDesc(mActivityData.GetValue<string>("activity_id"), validPeriod);

        // 奖励领取提示
        mysteryGiftDesc2.text = ActivityMgr.GetActivityBonusTipDesc(mActivityData.GetValue<string>("activity_id"), mActivityData);

        // 根据数据绘制界面
        redraw();
    }

    /// <summary>
    /// 根据数据绘制界面
    /// </summary>
    void redraw()
    {
        // 没有活动数据
        if (mActivityData == null ||
            mActivityData.Count == 0)
            return;

        // 获取ActivityID
        string activityID = mActivityData.GetValue<LPCValue>("activity_id").AsString;

        // 根据数据初始化子活动 
        initSubActivityByDaily(ActivityMgr.GetActivityTaskList(activityID));

        // 根据数据初始化神秘大礼积分
        initMysteryGiftByData(mActivityData);
    }

    /// <summary>
    /// 根据数据初始化子活动 
    /// </summary>
    void initSubActivityByDaily(List<CsvRow> activityInfo)
    {
        // 配置表中任务数量与窗口控件数量不一致
        if (activityInfo.Count < missionBtns.Length)
            return;

        // 遍历所有的子任务
        for (int i = 0; i < missionBtns.Length; i++)
        {
            // 获取子任务ID
            int taskId = activityInfo[i].Query<int>("task_id");

            // 根据数据展示子活动是否可领取
            showSubActivityCanReceive(i, taskId);

            // 获取任务描述
            missionContents[i].text = ActivityMgr.GetActivityTaskDesc(ME.user, mActivityCookie, taskId);

            // 显示任务奖励
            showSubActivityReward(i, ActivityMgr.GetActivityTaskBonus(taskId));
        }
    }

    /// <summary>
    /// 根据数据展示子活动是否可领取
    /// </summary>
    void showSubActivityCanReceive(int i, int taskId)
    {
        // 任务未完成
        if (!ActivityMgr.IsCompletedActivityTask(ME.user, mActivityCookie, taskId))
        {
            missionBtnLab[i].text = LocalizationMgr.Get("LesenaDairyWnd_8"); 
            missionBtns[i].enabled = false;
            missionBtnWidget[i].alpha = 0.3f;
            return;
        }

        // 任务是否可领取
        if (ActivityMgr.HasActivityTaskBonus(ME.user, mActivityCookie, taskId))
        {
            missionBtnLab[i].text = LocalizationMgr.Get("LesenaDairyWnd_7");

            missionBtnWidget[i].alpha = 1.0f;

            missionBtns[i].enabled = true;

            return;
        }

        // 任务已领取
        missionBtnLab[i].text = LocalizationMgr.Get("LesenaDairyWnd_9");
        missionBtns[i].enabled = false;
        missionBtnWidget[i].alpha = 0.3f;
    }

    /// <summary>
    /// 显示任务奖励
    /// </summary>
    /// <param name="bonusA">Bonus a.</param>
    void showSubActivityReward(int i, LPCArray bonusA)
    {
        // 遍历奖励
        for (int k = 0; k < bonusA.Count; k++)
        {
            if (!bonusA[k].IsMapping)
                continue;

            // 转成mapping
            LPCMapping bonusM = bonusA[k].AsMapping;

            // 获取字段名
            string field = FieldsMgr.GetFieldInMapping(bonusM);

            // 获取奖励数量
            int count = bonusM.GetValue<int>(field);

            // 临时处理，  需要策划在field文件中配置score字段
            if (field.Equals("score"))
            {
                missionRewardScore[i].transform.parent.gameObject.SetActive(true);

                // 展示积分数量
                missionRewardScore[i].text = string.Format("+{0}", count);
                continue;
            }

            missionRewardItem[i].gameObject.SetActive(true);

            // 注册点击事件
            UIEventListener.Get(missionRewardItem[i].gameObject).onClick = OnItemBtn;

            // 通过Mapping 展示物品
            missionRewardItem[i].Bind(bonusM, "", false, false, -1, "small_icon_bg");
        }
    }

    /// <summary>
    /// 根据数据初始化神秘大礼积分
    /// </summary>
    /// <param name="activityInfo">Activity info.</param>
    void initMysteryGiftByData(LPCMapping activityInfo)
    {
        // 当前积分
        int score = ME.user.Query<int>(string.Format("activity_data/{0}/score", mActivityCookie));

        // 获取活动的配置表
        CsvRow config = ActivityMgr.ActivityCsv.FindByKey(mActivityData.GetValue<string>("activity_id"));

        LPCMapping dbase = config.Query<LPCMapping>("dbase");

        // 获取最大积分
        int maxScore = dbase.GetValue<int>("max_score");

        score = Mathf.Clamp(score, 0, maxScore);

        // 填充当前积分
        mysteryGiftScoreLab.text = score.ToString();

        // 进度条显示
        mysteryGiftProgress.value = score / (float)maxScore;

        // 获取活动奖励列表
        LPCValue bonusV = ActivityMgr.GetActivityBonus(mActivityData);

        // 奖励是否为空，或不是Mapping
        if (bonusV == null || !bonusV.IsMapping)
            return;

        // 显示神秘大礼奖励
        showMysteryGiftReward(bonusV.AsMapping);

        // 检测奖励是否可领取
        checkMysteryGiftCanReceive(bonusV.AsMapping);
    }

    /// <summary>
    /// 检测积分奖励是否可领取
    /// </summary>
    /// <param name="bonusM">Bonus m.</param>
    void checkMysteryGiftCanReceive(LPCMapping bonusM)
    {
        // 当前积分
        int score = ME.user.Query<int>(string.Format("activity_data/{0}/score", mActivityCookie));

        int bonusIndex = 0;
        mysteryGiftScore = new int[bonusM.Count];
        foreach (int key in bonusM.Keys)
        {
            // 当前积分大于目标积分，则这个小目标完成
            bool isComplete = score >= key;

            // 当前积分是否已领取
            bool isReceived = ActivityMgr.ActivityBonusIsReceived(ME.user, mActivityCookie, key);

            mysteryGiftScore[bonusIndex] = key;

            // 设置每个积分的状态
            setMysteryGiftState(isComplete, isReceived, bonusIndex, key);

            bonusIndex++;
        }
    }

    /// <summary>
    /// 设置积分按钮的状态
    /// </summary>
    /// <param name="isComplete">If set to <c>true</c> is complete.</param>
    /// <param name="isReceived">If set to <c>true</c> is received.</param>
    /// <param name="index">Index.</param>
    /// <param name="score">Score.</param>
    void setMysteryGiftState(bool isComplete, bool isReceived, int index, int score)
    {
        // 本地化文本
        string localValue = "";

        // 积分任务未完成
        if (!isComplete)
        {
            localValue = LocalizationMgr.Get("LesenaDairyWnd_12");
            mysteryGiftBtnLab[index].text = string.Format("{0}{1}", score, localValue);   
            mysteryGiftBtnWidget[index].alpha = 0.3f;
            mysteryGiftBtns[index].enabled = false;

            return;
        }

        // 积分任务已领取
        if (isReceived)
        {
            localValue = LocalizationMgr.Get("LesenaDairyWnd_9");
            mysteryGiftBtnLab[index].text = localValue;
            mysteryGiftBtnWidget[index].alpha = 0.3f;
            mysteryGiftBtns[index].enabled = false;

            return;
        }

        mysteryGiftBtnWidget[index].alpha = 1.0f;
        mysteryGiftBtns[index].enabled = true;

        // 积分任务可领取
        localValue = LocalizationMgr.Get("LesenaDairyWnd_7");
        mysteryGiftBtnLab[index].text = localValue;
    }

    /// <summary>
    ///  显示神秘大礼奖励
    /// </summary>
    /// <param name="bonusM">Bonus m.</param>
    void showMysteryGiftReward(LPCMapping bonusM)
    {
        // 所有奖励的索引
        int bonusIndex = 0;

        // 遍历所有的奖励
        foreach (LPCValue value in bonusM.Values)
        {
            if (value == null || !value.IsArray)
                continue;

            // 转成 LPCMapping
            LPCArray valueArr = value.AsArray;

            // 单个奖励的索引
            int rewardIndex = 0;

            // 遍历单个奖励里的所有奖励
            for (int k = 0; k < valueArr.Count; k++)
            {
                if (valueArr[k] == null || !valueArr[k].IsMapping)
                    continue;

                SignItemWnd rewardItem = rewardIndex == 0 ? mysteryGiftRewardOne[bonusIndex] : mysteryGiftRewardTwo[bonusIndex];

                rewardItem.gameObject.SetActive(true);

                // 注册点击事件
                UIEventListener.Get(rewardItem.gameObject).onClick = OnItemBtn;

                // 计算奖励物品的坐标
                Vector3 rewardPos = switchGiftPos(valueArr.Count, rewardIndex);

                // 在左边的奖励物品要乘 -1
                if (bonusIndex % 2 == 0)
                    rewardPos.x *= -1; 

                // 赋值奖励物品的坐标
                rewardItem.transform.localPosition = rewardPos;

                // 获取Item组件 并赋值
                rewardItem.Bind(valueArr[k].AsMapping, "", false, false, -1, "small_icon_bg");

                rewardIndex++;
            }

            bonusIndex++;
        }
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
    /// 计算神秘大礼物品的坐标
    /// </summary>
    /// <returns>The gift position.</returns>
    /// <param name="count">Count.</param>
    /// <param name="index">Index.</param>
    Vector3 switchGiftPos(int count, int index)
    {
        // 返回的结果值
        Vector3 result = Vector3.zero;
        switch (count)
        {
            case 1:
                result = new Vector3(-336, 4, 0);
                break;
            case 2:
                if (index == 0)
                    result = new Vector3(-290, 5, 0);
                else if (index == 1)
                    result = new Vector3(-390, 5, 0);
                break;
        }

        return result;
    }

    /// <summary>
    /// 当活动数据发生改变回调
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="_params">Parameters.</param>
    void OnActivityChanged(object para, params object[] _params)
    {
        redraw();
    }

    /// <summary>
    /// 每日任务按钮点击
    /// </summary>
    private void OnMissionBtnClick(GameObject _obj)
    {
        // 根据按钮名字获取点击按钮的索引
        string btnIndex = _obj.name.Substring(_obj.name.LastIndexOf("_") + 1);
        int index = -1;
        int.TryParse(btnIndex, out index);

        index += 1;

        ActivityMgr.ReceiveActivityTaskBonus(ME.user, mActivityCookie, index);
    }

    /// <summary>
    /// 积分按钮点击
    /// </summary>
    private void OnScoreBtnClick(GameObject _obj)
    {
        // 根据按钮名字获取点击按钮的索引
        string btnIndex = _obj.name.Substring(_obj.name.LastIndexOf("_") + 1);
        int index = -1;
        int.TryParse(btnIndex, out index);

        // 客户端配置表的积分数量与服务器不一致
        if (index >= mysteryGiftScore.Length)
            return;

        // 领取奖励
        ActivityMgr.ReceiveActivityBonus(mActivityCookie, LPCValue.Create(mysteryGiftScore[index]));
    }

    /// <summary>
    /// 关闭按钮点击
    /// </summary>
    /// <param name="_obj">Object.</param>
    private void OnCloseClick(GameObject _obj)
    {
        WindowMgr.DestroyWindow(LesenaDairyWnd.WndType);
    }

    #endregion

    #region 外部接口

    public void Bind(LPCMapping activityInfo)
    {
        initByData(activityInfo);
    }

    #endregion
}
