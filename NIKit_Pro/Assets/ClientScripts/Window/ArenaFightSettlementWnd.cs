/// <summary>
/// ArenaFightSettlementWnd.cs
/// Created by fengsc 2016/10/20
/// 竞技场战斗结算界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using LPC;

public class ArenaFightSettlementWnd : WindowBase<ArenaFightSettlementWnd>
{
    #region 成员变量

    // 黑色阴影背景
    public GameObject mBG;

    // 白色阴影遮罩
    public GameObject mWhiteMask;

    // 战斗胜利实体对象
    public GameObject mFightVictory;

    // 战斗失败实体对象
    public GameObject mFightFailed;

    // 胜点显示实体对象
    public GameObject mArenaScore;

    // 胜方的玩家头像
    public UITexture mWinsIcon;

    // 胜方的玩家等级
    public UILabel mWinsLevel;

    // 胜方的玩家名称
    public UILabel mWinsName;

    // 胜方的玩家胜点
    public UILabel mWinsPiont;

    public UILabel mWinsPiontLb;

    // 胜利方功勋值
    public UILabel mWinsExploit;

    public UILabel mWinsExploitLb;

    // 失败方的玩家头像
    public UITexture mFailIcon;

    // 失败方的玩家等级
    public UILabel mFailLevel;

    // 失败方的玩家名称
    public UILabel mFailName;

    // 失败方的玩家胜点
    public UILabel mFailPiont;

    public UILabel mFailPiontLb;

    // 界面顶端旋转光效
    public GameObject mTopLight;

    public GameObject mLeaveWnd;

    // 重新挑战按钮
    public GameObject mAgainBtn;

    // 返回按钮
    public GameObject mReturnBtn;

    public UILabel mAgainLabel;

    public UILabel mReturnLb;

    // 再来一次的消耗属性的图标
    public UISprite mAgainCostIcon;

    // 再来一次的属性消耗
    public UILabel mAgainCost;

    // 奖励的金币数量
    public UILabel mRewardGoldAmount;

    // 钻石数量
    public UILabel mRewardDiamondAmount;

    // 竞技场入场券的数量
    public UILabel mRewardApAmount;

    public UILabel mRewardLb;

    public GameObject mClearanceTime;

    // 最佳时间
    public UILabel mBestTime;
    public UILabel mBestTimeTitle;

    // 本次时间
    public UILabel mThisTime;
    public UILabel mThisTimeTitle;

    // 新纪录提示
    public UILabel mNewRecoedTips;

    public UILabel mRoundTips;

    // 分享录像按钮
    public GameObject mShareVideoBtn;
    public UILabel mShareVideoBtnLb;
    public GameObject mShareVideoBtnMark;

    // 通关数据
    LPCMapping mClearanceInstanceData = new LPCMapping();

    // 副本id
    string mInstanceId = string.Empty;

    string mOpponentRid = string.Empty;

    LPCMapping opponentTop = new LPCMapping();

    LPCMapping defenseData = new LPCMapping();

    // 地图类型
    int mMapType = 0;

    // 是否玩家操作
    private bool mIsOperated = false; 

    bool mResult = false;

    #endregion

    void Awake()
    {
        // 注册事件
        RegisterEvent();

        // 初始化TweenPosition
        InitTweenPosition();
    }

    // Use this for initialization
    void Start()
    {
        // 初始化本地化文本
        InitLocalText();
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    void OnEnable()
    {
        mIsOperated = false;
    }

    /// <summary>
    /// 初始化tween动画终点位置（限定在屏幕范围内）
    /// </summary>
    void InitTweenPosition()
    {
        // UI根节点
        Transform uiRoot = WindowMgr.UIRoot;
        if (uiRoot == null)
            return;

        UIPanel panel = uiRoot.GetComponent<UIPanel>();
        if (panel == null)
            return;

        // UI根节点panel四角的坐标
        Vector3[] pos = panel.localCorners;

        TweenPosition failTweenPos = mFightFailed.GetComponent<TweenPosition>();

        UILabel fail = mFightFailed.transform.Find("Failed").GetComponent<UILabel>();

        float offset = 10f;

        float failY = pos[1].y - fail.height - offset;

        // 设置失败tween动画的终点坐标
        failTweenPos.to = new Vector3(failTweenPos.to.x, failY, failTweenPos.to.z);

        UILabel normal_2 = mFightVictory.transform.Find("normal_2").GetComponent<UILabel>();

        TweenPosition normal2TweenPos = normal_2.GetComponent<TweenPosition>();

        // 计算pos的Y坐标
        float normalY = pos[1].y - normal_2.height - offset;

        // 设置胜利tween动画的终点坐标
        normal2TweenPos.to = new Vector3(normal2TweenPos.to.x, normalY, normal2TweenPos.to.z);
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mReturnBtn).onClick = OnClickRetrunBtn;
        UIEventListener.Get(mAgainBtn).onClick = OnClickAgainBtn;
        UIEventListener.Get(mShareVideoBtn).onClick = OnClickShareVideoBtn;

        // 注册消息回调
        // 关注MSG_GET_ARENA_OPPONENT_DEFENSE_DATA消息
        MsgMgr.RegisterDoneHook("MSG_GET_ARENA_OPPONENT_DEFENSE_DATA", "ArenaFightSettlementWnd", OnMsgGetArenaOpponentDefenseData);

        EventMgr.RegisterEvent("ArenaFightSettlementWnd", EventMgrEventType.EVENT_PUBLISH_VIDEO, OnPublishVideoEvent);
    }

    /// <summary>
    /// 视频发布事件回调
    /// </summary>
    void OnPublishVideoEvent(int eventId, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();
        if (data == null || data.Count == 0)
            return;

        int result = data.GetValue<int>("result");

        // 发布失败
        if (result != 1)
            return;

        mShareVideoBtnMark.SetActive(true);

        string videoId = data.GetValue<string>("id");

        DialogMgr.ShowSingleBtnDailog(new CallBack(OnDialogCallBack, videoId), LocalizationMgr.Get("ArenaFightSettlementWnd_9"));
    }

    void OnDialogCallBack(object para, params object[] param)
    {
        // 回放录像id
        string videoId = para as string;

        string message = string.Format(LocalizationMgr.Get("ArenaFightSettlementWnd_10"), mWinsName.text, mFailName.text);

        LPCArray publish = LPCArray.Empty;

        LPCMapping data = LPCMapping.Empty;
        data.Add("video_id", videoId);

        publish.Add(data);

        // 向聊天频道发送消息
        ChatRoomMgr.SendChatMessage(ME.user, ChatConfig.WORLD_CHANNEL, string.Empty, publish, message);
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("ArenaFightSettlementWnd");

        Coroutine.DispatchService(WaitInvoke());
    }

    IEnumerator WaitInvoke()
    {
        yield return null;

        // 移除消息监听
        MsgMgr.RemoveDoneHook("MSG_GET_ARENA_OPPONENT_DEFENSE_DATA", "ArenaFightSettlementWnd");
    }

    void OnMsgGetArenaOpponentDefenseData(string cmd, LPCValue para)
    {
        LPCMapping args = para.AsMapping;

        if (args == null)
            return;

        defenseData = args.GetValue<LPCMapping>("opponent_data");

        if (defenseData == null)
            return;

        // 打开世界地图场景
        if (string.Equals(SceneMgr.MainScene, SceneConst.SCENE_WORLD_MAP))
            SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OpenSelectFight), true);
        else
            SceneMgr.LoadScene("Main", SceneConst.SCENE_MAIN_CITY, new CallBack(OpenSelectFight), true);
    }

    /// <summary>
    /// 打开战斗场景回调
    /// </summary>
    private void OpenSelectFight(object para, object[] param)
    {
        // 离开副本;
        InstanceMgr.LeaveInstance(ME.user);

        // 销毁本窗口
        WindowMgr.DestroyWindow(ArenaFightSettlementWnd.WndType);

        // 打开选择副本界面
        if (GuideMgr.IsGuiding())
            EventMgr.FireEvent(EventMgrEventType.EVENT_GUIDE_RETUEN_OPERATE, MixedValue.NewMixedValue<int>(GuideConst.AGAIN_ARENA), true);

        // 异步打开选择副本界面
        Coroutine.DispatchService(ShowSelectFightWnd());
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 副本配置数据
        LPCMapping instanceInfo = InstanceMgr.GetInstanceInfo(mInstanceId);
        if (instanceInfo == null)
            instanceInfo = LPCMapping.Empty;

        // 地图配置数据
        CsvRow row = MapMgr.GetMapConfig(instanceInfo.GetValue<int>("map_id"));

        int mapType = -1;

        if (row != null)
            mapType = row.Query<int>("map_type");

        if (mapType == MapConst.ARENA_MAP || mapType == MapConst.ARENA_REVENGE_MAP)
            mShareVideoBtn.SetActive(true);
        else
            mShareVideoBtn.SetActive(false);

        // 奖励数据
        LPCMapping bonusMap = mClearanceInstanceData.GetValue<LPCMapping>("bonus_map");

        if (bonusMap == null)
            return;

        // 属性奖励
        LPCMapping attrib = bonusMap.GetValue<LPCMapping>("attrib");

        LPCValue v = bonusMap.GetValue<LPCValue>("box");

        LPCMapping box = LPCMapping.Empty;

        if (v != null && v.IsMapping)
            box = v.AsMapping;

        // 金币
        mRewardGoldAmount.text = string.Format("{0} {1}", "+", attrib.GetValue<int>("money") + box.GetValue<int>("money"));

        // 钻石
        mRewardDiamondAmount.text = string.Format("{0} {1}", "+", attrib.GetValue<int>("gold_coin") + box.GetValue<int>("gold_coin"));

        // 竞技场券
        mRewardApAmount.text = string.Format("{0} {1}", "+", attrib.GetValue<int>("ap") + box.GetValue<int>("ap"));

        // 竞技场积分
        LPCMapping arenaScore = bonusMap.GetValue<LPCMapping>("arena_score");

        if (arenaScore == null)
            arenaScore = LPCMapping.Empty;

        if (opponentTop == null || opponentTop.Count < 1)
            return;

        // 本次战斗被挑战者所获得的胜点
        int opponentPoint = arenaScore.GetValue<int>(mOpponentRid);

        int userPoint = arenaScore.GetValue<int>(ME.user.Query<string>("rid"));

        string icon = string.Empty;

        LPCValue iconValue = ME.user.Query<LPCValue>("icon");
        if (iconValue != null && iconValue.IsString)
            icon = iconValue.AsString;

        LPCMapping userData = new LPCMapping();
        userData.Add("icon", icon);
        userData.Add("level", ME.user.Query<int>("level"));
        userData.Add("name", ME.user.Query<string>("name"));

        // 由于流程调整客户端先更新arena_top属性
        userData.Add("score", ME.user.Query<int>("arena_top/score") - userPoint);

        // 攻击方
        // 加载玩家头像;
        string iconName = userData.GetValue<string>("icon");
        string resPath = string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconName);

        Texture2D tex = ResourceMgr.LoadTexture(resPath);

        mWinsIcon.mainTexture = tex;

        // 玩家等级
        mWinsLevel.text = string.Format(LocalizationMgr.Get("ArenaFightSettlementWnd_1"), userData.GetValue<int>("level"));

        // 玩家名称
        mWinsName.text = userData.GetValue<string>("name");

        mResult = mClearanceInstanceData.GetValue<int>("result") == 1 ? true : false;

        // 玩家胜点
        if (userPoint > 0)
            mWinsPiont.text = string.Format("{0} (+{1})", Mathf.Max(userData.GetValue<int>("score") + userPoint, 0), userPoint);
        else if (userPoint < 0)
            mWinsPiont.text = string.Format("{0} (-{1})", Mathf.Max(userData.GetValue<int>("score") + userPoint, 0), Mathf.Abs(userPoint));
        else
        {
            if (mResult)
                mWinsPiont.text = string.Format("{0} (+{1})", Mathf.Max(userData.GetValue<int>("score") + userPoint, 0), userPoint);
            else
                mWinsPiont.text = string.Format("{0} (-{1})", Mathf.Max(userData.GetValue<int>("score") + userPoint, 0), userPoint);
        }

        // 胜方获得的竞技场功勋
        int addExploit = attrib.GetValue<int>("exploit") + box.GetValue<int>("exploit");

        // 竞技场功勋
        mWinsExploit.text = string.Format("+ {0}", addExploit);

        // 防守方
        if (opponentTop == null)
            return;

        // 失败方相关数据显示
        // 玩家头像名称
        // 加载玩家头像;
        LPCValue iconV = opponentTop.GetValue<LPCValue>("icon");
        if (iconV != null && iconV.IsString)
            mFailIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconV.AsString));
        else
            mFailIcon.mainTexture = null;

        // 玩家等级
        mFailLevel.text = string.Format(LocalizationMgr.Get("ArenaFightSettlementWnd_1"), opponentTop.GetValue<int>("level"));

        // 玩家名称
        mFailName.text = opponentTop.GetValue<string>("name");

        // 胜点
        if (opponentPoint > 0)
            mFailPiont.text = string.Format("{0} (+{1})", Mathf.Max(opponentTop.GetValue<int>("score") + opponentPoint, 0), opponentPoint);
        else if (opponentPoint < 0)
            mFailPiont.text = string.Format("{0} (-{1})", Mathf.Max(opponentTop.GetValue<int>("score") + opponentPoint, 0), Mathf.Abs(opponentPoint));
        else
        {
            if (mResult)
                mFailPiont.text = string.Format("{0} (-{1})", Mathf.Max(opponentTop.GetValue<int>("score") + opponentPoint, 0), opponentPoint);
            else
                mFailPiont.text = string.Format("{0} (+{1})", Mathf.Max(opponentTop.GetValue<int>("score") + opponentPoint, 0), opponentPoint);
        }
        
    }

    /// <summary>
    /// 显示竞技场积分
    /// </summary>
    void ShowArenaScoreWnd()
    {
        // 显示通关时间
        ShowClearanceTime(mClearanceInstanceData);

        TweenAlpha alpha = mArenaScore.GetComponent<TweenAlpha>();
        alpha.enabled = true;

        if (! mResult)
            EventMgr.FireEvent(EventMgrEventType.EVENT_FAILED_ANIMATION_FINISH, null, true);
        else
            EventMgr.FireEvent(EventMgrEventType.EVENT_SETTLEMENT_BONUS_SHOW_FINISH, null, true);

        Coroutine.DispatchService(DelayProcess(), "DelayProcess");
    }

    /// <summary>
    /// 延迟处理
    /// </summary>
    IEnumerator DelayProcess()
    {
        // 等待一帧
        yield return null;

        mLeaveWnd.SetActive(true);

        Coroutine.StopCoroutine("DelayProcess");
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLocalText()
    {
        mWinsPiontLb.text = LocalizationMgr.Get("ArenaFightSettlementWnd_2");
        mWinsExploitLb.text = LocalizationMgr.Get("ArenaFightSettlementWnd_3");
        mReturnLb.text = LocalizationMgr.Get("ArenaFightSettlementWnd_5");
        mAgainLabel.text = LocalizationMgr.Get("ArenaFightSettlementWnd_6");
        mRewardLb.text = LocalizationMgr.Get("ArenaFightSettlementWnd_7");
        mShareVideoBtnLb.text = LocalizationMgr.Get("ArenaFightSettlementWnd_8");

        mBestTimeTitle.text = LocalizationMgr.Get("FightSettlementWnd_18");
        mThisTimeTitle.text = LocalizationMgr.Get("FightSettlementWnd_19");
        mNewRecoedTips.text = LocalizationMgr.Get("FightSettlementWnd_20");
    }

    /// <summary>
    /// 分享录像按钮点击事件
    /// </summary>
    void OnClickShareVideoBtn(GameObject go)
    {
        if (mIsOperated)
            return;

        mIsOperated = true;

        // 发布视频
        VideoMgr.PublishVideo(ME.user);

        mIsOperated = false;
    }

    /// <summary>
    /// 返回竞技场按钮点击事件
    /// </summary>
    void OnClickRetrunBtn(GameObject go)
    {
        // 玩家不在副本中不做处理;
        // 获取玩家已经操作过其他按钮
        if (ME.user == null ||
            mIsOperated)
            return;

        // 标识mIsOperated
        mIsOperated = true;

        Coroutine.DispatchService(DoLoadScene());
    }

    IEnumerator DoLoadScene()
    {
        // 同步抛出事件
        if (GuideMgr.IsGuiding())
            EventMgr.FireEvent(EventMgrEventType.EVENT_GUIDE_RETUEN_OPERATE, MixedValue.NewMixedValue<int>(GuideConst.RETURN_ARENA), true);

        yield return null;

        // 打开主场景竞技场窗口
        if (string.Equals(SceneMgr.MainScene, SceneConst.SCENE_WORLD_MAP))
            SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OpenArenaWnd));
        else
            SceneMgr.LoadScene("Main", SceneConst.SCENE_MAIN_CITY, new CallBack(OpenArenaWnd));
    }

    /// <summary>
    /// 打开战斗场景回调
    /// </summary>
    private void OpenArenaWnd(object para, object[] param)
    {
        // 离开副本;
        InstanceMgr.LeaveInstance(ME.user);

        // 销毁本窗口
        WindowMgr.DestroyWindow(ArenaFightSettlementWnd.WndType);

        ARENA_PAGE page;

        // 显示竞技场窗口
        switch (mMapType)
        {
            case MapConst.ARENA_MAP:
                page = ARENA_PAGE.RANKINGBATTLE_PAGE;
                break;
            case MapConst.ARENA_REVENGE_MAP:
                page = ARENA_PAGE.DEFENCERECORD_PAGE;
                break;
            default:
                page = ARENA_PAGE.PRACTICEBATTLE_PAGE;
                break;
        }

        // 打开竞技场窗口
        GameObject arenaWnd = WindowMgr.OpenWnd("ArenaWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (arenaWnd == null)
            return;

        // 打开竞技场窗口
        arenaWnd.GetComponent<ArenaWnd>().BindPage((int) page);
    }

    /// <summary>
    /// 再来一次按钮点击事件
    /// </summary>
    void OnClickAgainBtn(GameObject go)
    {
        if (ME.user == null)
            return;

        if (mMapType == MapConst.ARENA_MAP)
            // 向服务器请求被挑战者的防御列表
            Operation.CmdGetArenaOpponentDefenseData.Go(mOpponentRid);
        else
        {
            //获取副本配置信息
            List<CsvRow> list = InstanceMgr.GetInstanceFormation(mInstanceId);

            LPCArray defenceList = new LPCArray();

            foreach (CsvRow item in list)
            {
                LPCMapping para = new LPCMapping();

                // 调用脚本参数计算怪物class_id;
                int classIdScript = item.Query<int>("class_id_script");
                int classId = (int) ScriptMgr.Call(classIdScript, ME.user.GetLevel(),
                    item.Query<LPCValue>("class_id_args"));

                para.Add("rid", Rid.New());
                para.Add("class_id", classId);

                // 获取始化参数;
                int initScript = item.Query<int>("init_script");
                LPCMapping initArgs = ScriptMgr.Call(initScript, ME.user.GetLevel(),
                    item.Query<LPCValue>("init_script_args"), para) as LPCMapping;

                // 获取始化参数
                para.Append(initArgs);

                // 添加列表
                defenceList.Add(para);
            }

            defenseData.Add("defense_list", defenceList);

            // 打开主场景竞技场窗口
            if (string.Equals(SceneMgr.MainScene, SceneConst.SCENE_WORLD_MAP))
                SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OpenSelectFight), true);
            else
                SceneMgr.LoadScene("Main", SceneConst.SCENE_MAIN_CITY, new CallBack(OpenSelectFight), true);
        }
    }

    IEnumerator ShowSelectFightWnd()
    {
        yield return null;

        // 显示选择战斗窗口
        GameObject wnd = WindowMgr.OpenWnd("SelectFighterWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 创建窗口失败
        if (wnd == null)
        {
            LogMgr.Trace("打开SelectFighterWnd窗口失败。");
            yield break;
        }

        // 没有防御信息
        if (defenseData == null)
            yield break;

        // 设置本次通关的副本ID
        wnd.GetComponent<SelectFighterWnd>().Bind("ArenaWnd", mInstanceId, defenseData);
    }

    /// <summary>
    /// 显示通关时间
    /// </summary>
    private void ShowClearanceTime(LPCMapping clearanceData)
    {
        // 通关时间
        LPCValue clearanceTime = clearanceData.GetValue<LPCValue>("clearance_time");
        if (clearanceTime == null)
        {
            mClearanceTime.SetActive(false);
            return;
        }

        mClearanceTime.SetActive(true);

        if (clearanceTime.IsInt)
        {
            // 显示本次通关时间
            mThisTime.text = TimeMgr.ConvertTime(clearanceTime.AsInt, false);

            // 隐藏新纪录和最佳时间显示控件
            mNewRecoedTips.gameObject.SetActive(false);
            mBestTime.gameObject.SetActive(false);
        }
        else if(clearanceTime.IsMapping)
        {
            // 上次通关时间
            int oldTime = clearanceTime.AsMapping.GetValue<int>("old_clearance_time");

            // 本次通关时间
            int curTime = clearanceTime.AsMapping.GetValue<int>("cur_clearance_time");

            // 显示本次通关时间
            mThisTime.text = TimeMgr.ConvertTime(curTime, false);

            if (clearanceData.GetValue<int>("result") != 1)
            {
                // 通关失败

                // 隐藏新纪录和最佳时间显示控件
                mNewRecoedTips.gameObject.SetActive(false);
                mBestTime.gameObject.SetActive(false);
            }
            else
            {
                // 显示最佳时间
                if (oldTime < 0)
                {
                    mBestTime.text = LocalizationMgr.Get("FightSettlementWnd_21");

                    mNewRecoedTips.gameObject.SetActive(true);
                }
                else
                {
                    mBestTime.text = TimeMgr.ConvertTime(oldTime, false);

                    // 显示新纪录标识
                    mNewRecoedTips.gameObject.SetActive(curTime < oldTime);
                }

                mBestTime.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    /// 战斗结束显示回合提示
    /// </summary>
    private void ShowRoundTips()
    {
        // 战斗最大回合数限制
        int maxRound = GameSettingMgr.GetSettingInt("max_combat_rounds");

        // 因为回合不足导致战斗失败需要在失败界面有文字提示
        if (InstanceMgr.GetRoundCount(ME.user) >= maxRound)
        {
            // 未在限制回合内结束战斗
            mRoundTips.text = string.Format(LocalizationMgr.Get("FightSettlementWnd_23"), maxRound);

            mRoundTips.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 战斗失败显示相关窗口
    /// </summary>
    private void FailShowWnd()
    {
        // 战斗结束显示回合提示
        ShowRoundTips();

        // 显示积分窗口
        ShowArenaScoreWnd();
    }

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data)
    {
        if (data == null || data.Count < 1)
            return;

        // 副本通关数据
        mClearanceInstanceData = data;

        mInstanceId = data.GetValue<string>("instance_id");

        // 被挑战者的账号信息
        LPCValue opponentData = data.GetValue<LPCMapping>("bonus_map").GetValue<LPCValue>("opponent_top");

        if (opponentData != null && opponentData.IsMapping)
        {
            mOpponentRid = opponentData.AsMapping.GetValue<string>("rid");
            opponentTop = opponentData.AsMapping;
        }

        Redraw();
    }

    /// <summary>
    /// 战斗胜利的动画以及显示处理
    /// </summary>
    public void FightVectory(int mapType)
    {
        mArenaScore.SetActive(true);

        mMapType = mapType;

        //战斗胜利的白色遮罩;
        mWhiteMask.SetActive(true);

        mFightVictory.SetActive(true);

        mBG.SetActive(true);

        mBG.GetComponent<TweenAlpha>().enabled = true;

        mTopLight.GetComponent<TweenRotation>().enabled = true;

        Transform normal_1 = mFightVictory.transform.Find("normal_1");
        if (normal_1 != null)
            normal_1.GetComponent<UILabel>().text = LocalizationMgr.Get("FightSettlementWnd_24");

        Transform normal_2 = mFightVictory.transform.Find("normal_2");
        if (normal_2 != null)
            normal_2.GetComponent<UILabel>().text = LocalizationMgr.Get("FightSettlementWnd_24");

        Transform light = mFightVictory.transform.Find("light");
        if (light != null)
            light.GetComponent<UILabel>().text = LocalizationMgr.Get("FightSettlementWnd_24");

        Transform shadow = mFightVictory.transform.Find("shadow");
        if (shadow != null)
            shadow.GetComponent<UILabel>().text = LocalizationMgr.Get("FightSettlementWnd_24");

        EventDelegate.Add(normal_2.GetComponent<TweenPosition>().onFinished, ShowArenaScoreWnd);

        // 调整返回竞技场按钮的相对位置
        mReturnBtn.transform.localPosition = new Vector3(-324, -262, 0);

        // 隐藏再次挑战按钮
        mAgainBtn.SetActive(false);
    }

    /// <summary>
    /// 战斗失败的处理
    /// </summary>
    public void FightFail(int mapType)
    {
        mArenaScore.SetActive(true);

        mMapType = mapType;

        mWhiteMask.SetActive(false);

        mTopLight.SetActive(false);

        mFightFailed.SetActive(true);

        Transform Failed = mFightFailed.transform.Find("Failed");
        if (Failed != null)
            Failed.GetComponent<UILabel>().text = LocalizationMgr.Get("FightSettlementWnd_25");

        Transform shadow = mFightFailed.transform.Find("shadow");
        if (shadow != null)
            shadow.GetComponent<UILabel>().text = LocalizationMgr.Get("FightSettlementWnd_25");

        EventDelegate.Add(mFightFailed.GetComponent<TweenPosition>().onFinished, FailShowWnd);

        mBG.SetActive(true);

        mBG.GetComponent<TweenAlpha>().enabled = true;

        if (string.IsNullOrEmpty(mInstanceId))
            return;

        //获取进入副本开销;
        LPCMapping data = InstanceMgr.GetInstanceCostMap(ME.user, mInstanceId);

        if (data == null || data.Count <= 0)
            return;

        string field = FieldsMgr.GetFieldInMapping(data);

        if(mapType == MapConst.ARENA_REVENGE_MAP)
        {
            mAgainBtn.SetActive(false);
            mReturnBtn.transform.localPosition = new Vector3(-324, -262, 0);
        }
        else
        {
            mAgainBtn.SetActive(true);
            mAgainCostIcon.spriteName = FieldsMgr.GetFieldIcon(field);
            mAgainCost.text = data.GetValue<int>(field).ToString();
            mAgainLabel.text = LocalizationMgr.Get("ArenaFightSettlementWnd_6");
            mReturnBtn.transform.localPosition = new Vector3(-464, -262, 0);
        }
    }

    #endregion
}
