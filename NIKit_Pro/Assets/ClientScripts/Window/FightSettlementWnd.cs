/// <summary>
/// FightSettlementWnd.cs
/// Created by fengsc 2016/07/14
///战斗结算界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using LPC;

public class FightSettlementWnd : WindowBase<FightSettlementWnd>
{
    #region 成员变量

    /// <summary>
    ///黑色阴影背景
    /// </summary>
    public GameObject mBG;

    /// <summary>
    ///白色阴影遮罩
    /// </summary>
    public GameObject mWhiteMask;

    public GameObject mFightVictory;

    public GameObject mFightFailed;

    /// <summary>
    ///宝箱
    /// </summary>
    public GameObject mBox;

    public GameObject mRewardWnd;

    public GameObject mTopLight;

    public GameObject mLeaveWnd;

    public GameObject mAgainBtn;

    public GameObject mNextBtn;

    public UILabel mAgainLabel;

    public UILabel mNextLabel;

    public GameObject mLockBtn;
    public UILabel mLockBtnLb;
    public UILabel mLockTips;

    public GameObject mUnLockBtn;

    public GameObject mBoxTips;

    public UISprite mAgainPower;

    public UILabel mAgainPowerLabel;

    public UISprite mNextPower;

    public UILabel mNextPowerLabel;

    public UILabel mRewardGoldAmount;

    public UILabel mRewardDiamondAmount;

    public UILabel mRewardLife;

    public GameObject mRiskBtn;

    public GameObject mEquipBtn;

    /// <summary>
    /// 通关奖励道具实体对象
    /// </summary>
    public GameObject mRewardProp;

    /// <summary>
    /// 奖励物品图标
    /// </summary>
    public UITexture mIcon;

    /// <summary>
    /// 奖励道具名称
    /// </summary>
    public UILabel mRewardPropName;

    /// <summary>
    /// 装备星级
    /// </summary>
    public GameObject[] mStars;

    /// <summary>
    /// 套装图标
    /// </summary>
    public UITexture mSuitIcon;

    public UILabel mLevel;

    public GameObject LeaveWnd;

    //本地化文本;
    public UILabel mRewardTitle;

    public UILabel mEquipLable;

    public UILabel mRiskBtnLb;

    // 出售提示
    public UILabel mSellTips;

    // 出售货币图标
    public UISprite mSellPriceIcon;

    // 出售价格
    public UILabel mSellPrice;

    // 循环战斗倒计时提示
    public UILabel mLoopFightSecond;

    public GameObject mClearanceTime;

    // 最佳时间
    public UILabel mBestTime;
    public UILabel mBestTimeTitle;

    // 本次时间
    public UILabel mThisTime;
    public UILabel mThisTimeTitle;

    // 新纪录提示
    public UILabel mNewRecoedTips;

    // 回合提示
    public UILabel mRoundTips;

    /// <summary>
    ///副本id
    /// </summary>
    string InstanceId = string.Empty;

    /// <summary>
    ///动画控制器
    /// </summary>
    Animator mAnim;

    /// <summary>
    /// 奖励数据
    /// </summary>
    LPCMapping bonusMap = new LPCMapping();

    LPCMapping mClearanceData = LPCMapping.Empty;

    // 是否是循环战斗
    bool mIsLoopFight = false;

    int mRemainTime = 0;

    bool mIsCountDown = false;

    float mLastTime = 0;

    // 是否玩家操作
    private bool mIsOperated = false;

    LPCMapping mInstanceData = LPCMapping.Empty;

    string mNexInstanceId = string.Empty;

    int mMapType = 0;

    int mResult = 0;

    #endregion

    #region 内部函数

    void Awake()
    {
        // 初始化TweenPosition
        InitTweenPosition();
    }

    void Start()
    {
        InitWnd();

        mSellTips.gameObject.SetActive(false);
        mSellPriceIcon.gameObject.SetActive(false);
        mSellPrice.gameObject.SetActive(false);
    }

    void Update()
    {
        if (mIsCountDown)
        {
            if ((Time.realtimeSinceStartup > mLastTime + 1.0f))
            {
                mLastTime = Time.realtimeSinceStartup;
                CountDown();
            }
        }
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    void OnEnable()
    {
        mIsOperated = false;
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("FightSettlementWnd");

        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("FightSettlementWnd");
    }

    /// <summary>
    ///注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mRiskBtn).onClick = OnClickRiskBtn;
        UIEventListener.Get(mEquipBtn).onClick = OnClickEquipBtn;

        // 注册宝箱开启完成事件
        EventMgr.RegisterEvent("FightSettlementWnd", EventMgrEventType.EVENT_BOX_OPEN_FINISH, OnBoxOpenFinish);

        EventMgr.RegisterEvent("FightSettlementWnd", EventMgrEventType.EVENT_BOX_FALL_FINISH, OnBoxFallFinish);

        // 关注字段变化
        if (ME.user == null)
            return;

        ME.user.dbase.RegisterTriggerField("FightSettlementWnd", new string[]{ "history_max_level" }, new CallBack(OnMaxLevelFieldsChange));
    }

    /// <summary>
    /// 字段变化回调
    /// </summary>
    void OnMaxLevelFieldsChange(object para, params object[] param)
    {
        if(mMapType.Equals(MapConst.INSTANCE_MAP_1))
            RefreshLockTips();
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
    ///初始化窗口
    /// </summary>
    void InitWnd()
    {
        // 初始化本地化文本;
        mRewardTitle.text = LocalizationMgr.Get("FightSettlementWnd_1");
        mEquipLable.text = LocalizationMgr.Get("FightSettlementWnd_3");
        mRiskBtnLb.text = LocalizationMgr.Get("FightSettlementWnd_4");

        mBestTimeTitle.text = LocalizationMgr.Get("FightSettlementWnd_18");
        mThisTimeTitle.text = LocalizationMgr.Get("FightSettlementWnd_19");
        mNewRecoedTips.text = LocalizationMgr.Get("FightSettlementWnd_20");
    }

    /// <summary>
    /// 宝箱开启完成事件回调
    /// </summary>
    void OnBoxOpenFinish(int eventId, MixedValue para)
    {
        // 显示宝箱奖励物品
        ShowClearanceBonusProp();

        ShowLeaveWnd();
    }

    /// <summary>
    /// 宝箱下落完成事件回调
    /// </summary>
    void OnBoxFallFinish(int eventId, MixedValue para)
    {
        // 注册宝箱点击事件
        UIEventListener.Get(mBox).onClick = OnClickBox;
    }

    /// <summary>
    /// 倒计时
    /// </summary>
    void CountDown()
    {
        if (mRemainTime < 1)
        {
            // 取消倒计时
            mIsCountDown = false;

            // 玩家对象不存在
            if (ME.user == null)
                return;

            string instance = InstanceId;

            if (mMapType.Equals(MapConst.TOWER_MAP))
            {
                LPCMapping instanceInfo = InstanceMgr.GetInstanceInfo(InstanceId);

                string next = instanceInfo.GetValue<string>("next_instance_id");

                if (mResult == 1 && !string.IsNullOrEmpty(next))
                    instance = next;
            }

            // 打开世界地图场景
            SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(DoSelectAgainFight, instance));

            return;
        }

        if (mMapType.Equals(MapConst.TOWER_MAP))
        {
            mLoopFightSecond.text = string.Format(LocalizationMgr.Get("FightSettlementWnd_28"), mRemainTime);
        }
        else
        {
            mLoopFightSecond.text = string.Format(LocalizationMgr.Get("FightSettlementWnd_17"), mRemainTime);
        }

        mRemainTime--;
    }

    /// <summary>
    /// 显示副本通关奖励道具信息
    /// </summary>
    void ShowClearanceBonusProp()
    {
        for (int i = 0; i < mStars.Length; i++)
            mStars[i].SetActive(false);

        mSuitIcon.gameObject.SetActive(false);

        mLevel.gameObject.SetActive(false);

        if (bonusMap == null || bonusMap.Count == 0)
            return;

        // 获取奖励道具rid;
        LPCValue box = bonusMap.GetValue<LPCValue>("box");

        if (box == null)
            return;

        Property pro = null;

        // 如果是道具奖励
        if (box.IsString)
            pro = Rid.FindObjectByRid(box.AsString);
        else if (box.IsMapping)
        {
            // 转换数据格式
            LPCMapping boxMap = box.AsMapping;

            // 如果是道具详细信息
            if (boxMap.ContainsKey("class_id"))
            {
                // 道具奖励
                pro = PropertyMgr.CreateProperty(boxMap);
            }
            else
            {
                // 属性奖励
                foreach (string key in boxMap.Keys)
                {
                    LPCMapping dbase = LPCMapping.Empty;
                    dbase.Add("class_id", FieldsMgr.GetClassIdByAttrib(key));
                    dbase.Add("amount", boxMap.GetValue<int>(key));

                    // 属性道具奖励
                    pro = PropertyMgr.CreateProperty(dbase);
                }
            }
        }
        else if (box.IsInt)
        {

            // 开启秘密地下城
            LPCMapping dbase = LPCMapping.Empty;
            dbase.Add("class_id", box.AsInt);

            // 属性道具奖励
            pro = PropertyMgr.CreateProperty(dbase);

            mIcon.mainTexture = MonsterMgr.GetTexture(pro.GetClassID(), pro.Query<int>("rank"));

            mIcon.gameObject.SetActive(true);

            mRewardProp.SetActive(true);

            mRewardPropName.text = string.Format("[{0}]{1}{2}[-]", "ffffff", LocalizationMgr.Get("DynamicInstanceInfoWnd_4"), pro.Short());

            if (! mIsLoopFight)
            {
                ShowFriendDungeonsWnd(pro);
            }

            // 不走以下流程
            return;
        }
        else
        {
            // 奖励格式暂不支持
            return;
        }

        // 物件对象克隆失败
        if (pro == null)
        {
            // TODO: 临时查找bug，上传奖励数据
            Debug.LogError(string.Format("物件对象克隆失败___bonusMap == {0}, instance_id == {1}", bonusMap, InstanceId));

            return;
        }

        if (EquipMgr.IsEquipment(pro))
        {
            // 显示装备信息
            ShowEquipInfoWnd(pro);
        }
        else if (ItemMgr.IsItem(pro))
        {
            // 显示道具信息
            ShowItemInfoWnd(pro);
        }
        else if (MonsterMgr.IsMonster(pro))
        {
            // 显示宠物信息
            ShowPetInfoWnd(pro);
        }

        // 销毁临时道具
        if (box.IsMapping)
            pro.Destroy();
    }

    /// <summary>
    /// 好友地下城掉落显示弹框
    /// </summary>
    void ShowFriendDungeonsWnd(Property ob)
    {
        if (ob == null)
            return;

        GameObject wnd = WindowMgr.OpenWnd(DynamicInstanceInfoWnd.WndType);
        if (wnd == null)
            return;

        // 绑定数据
        bool isShare = ShareMgr.IsOpenShare() && GuideMgr.IsGuided(GuideMgr.SHARE_SHOW_GUIDE_GROUP);
        wnd.GetComponent<DynamicInstanceInfoWnd>().Bind(ob, InstanceId, new CallBack(OnFriendDungeonsCallBack, ob), isShare);
    }

    /// <summary>
    /// 隐藏圣域弹框点击回调
    /// </summary>
    void OnFriendDungeonsCallBack(object para, params object[] param)
    {
        if ((bool)param[0])
        {
            string type = param[1] as string;
            if (type.Equals(MesssgeBoxConst.SHARE))
            {
                if (GuideMgr.IsGuided(GuideMgr.SHARE_SHOW_GUIDE_GROUP) && ShareMgr.IsOpenShare())
                {
                    //分享开启隐藏圣域
                    GameObject shareWnd = WindowMgr.OpenWnd(ShareOperateWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                    if (shareWnd == null)
                        return;

                    shareWnd.GetComponentInChildren<ShareOperateWnd>().BindData(ShareOperateWnd.ShareOperateType.Sacred, para as Property);
                }
                else
                {
                    // 立即前往
                    if (ME.user == null ||
                        mIsOperated)
                        return;

                    // 标识mIsOperated
                    mIsOperated = true;

                    // 打开世界地图场景
                    SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OpenFriendDungeonsWnd));
                }
            }
            else
            {
                // 立即前往
                if (ME.user == null ||
                    mIsOperated)
                    return;

                // 标识mIsOperated
                mIsOperated = true;

                // 打开世界地图场景
                SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OpenFriendDungeonsWnd));
            }
        }
        else
        {
            // 关闭窗口
            //窗口对象不存在;
            if(LeaveWnd == null)
                return;

            LeaveWnd.SetActive(true);

            LeaveWnd.GetComponent<TweenAlpha>().enabled = true;
        }
    }

    /// <summary>
    /// 打开好友地下城
    /// </summary>
    void OpenFriendDungeonsWnd(object para, object[] param)
    {
        // 离开副本
        DoLeaveInstance();

        // 打开主界面
        GameObject mainWnd = WindowMgr.OpenWnd ("MainWnd");
        if (mainWnd == null)
            return;

        mainWnd.GetComponent<MainWnd>().ShowMainUIBtn(false);

        mainWnd.SetActive(false);

        // 创建窗口
        GameObject wnd = WindowMgr.OpenWnd(DungeonsWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        // 窗口创建失败
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<DungeonsWnd>().Bind(InstanceId, 19, new Vector3(-4.25f, 10.86f, -15f));
    }

    /// <summary>
    /// 显示装备信息窗口
    /// </summary>
    void ShowEquipInfoWnd(Property pro)
    {
        EventMgr.FireEvent(EventMgrEventType.EVENT_SHOW_EQUIP, null, true);

        mIcon.mainTexture = EquipMgr.GetTexture(pro.GetClassID(), pro.GetRarity());

        mIcon.gameObject.SetActive(true);

        int suitId = pro.Query<int>("suit_id");

        if (suitId < 1)
        {
            mRewardPropName.text = string.Empty;
            return;
        }

        // 获取装备的强化等级
        int rank = pro.GetRank();

        // 未强化(level = 0)不显示等级
        if (rank > 0)
        {
            mLevel.text = string.Format("+{0}", rank);
            mLevel.gameObject.SetActive(true);
        }

            //设置套装图标
        mSuitIcon.mainTexture = EquipMgr.GetSuitTexture(suitId);

        mSuitIcon.gameObject.SetActive(true);

        mRewardPropName.text = string.Format("[{0}]{1}[-]", ColorConfig.GetColor(pro.GetRarity()), pro.Short());

        mRewardPropName.gameObject.SetActive(true);

        //获取装备的星级;
        int star = pro.GetStar();

        int count = star < mStars.Length ? star : mStars.Length;

        for (int i = 0; i < count; i++)
            mStars[i].gameObject.SetActive(true);

        mRewardProp.SetActive(true);

        // 当前处于循环战斗
        if (mIsLoopFight)
        {
            // 地图类型
            int mapType = InstanceMgr.GetInstanceMapType(InstanceId);

            if (mapType.Equals(MapConst.DUNGEONS_MAP_2) || mapType.Equals(MapConst.TOWER_MAP))
            {
                mRewardPropName.gameObject.SetActive(true);

                mSellTips.text = LocalizationMgr.Get("FightSettlementWnd_27");
                mSellTips.gameObject.SetActive(true);
            }
            else
            {
                mRewardPropName.gameObject.SetActive(false);

                // 获取装备的出售价格
                LPCMapping price = PropertyMgr.GetSellPrice(pro);
                if (price != null && price.Count > 0)
                {
                    string fields = FieldsMgr.GetFieldInMapping(price);

                    // 货币图标
                    mSellPriceIcon.spriteName = FieldsMgr.GetFieldIcon(fields);

                    // 出售价格
                    mSellPrice.text = string.Format("+ {0}", price.GetValue<int>(fields));
                }

                mSellTips.text = LocalizationMgr.Get("FightSettlementWnd_15");

                mSellTips.gameObject.SetActive(true);
                mSellPriceIcon.gameObject.SetActive(true);
                mSellPrice.gameObject.SetActive(true);

                // 出售装备
                SellItem(pro);
            }

            return;
        }

        Coroutine.DispatchService(ShowBonusDialog(pro), "ShowBonusDialog");
    }

    /// <summary>
    /// 等待一帧显示奖励弹框
    /// </summary>
    IEnumerator ShowBonusDialog(Property pro)
    {
        yield return null;

        GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType);
        RewardItemInfoWnd rewardItemInfoWnd = wnd.GetComponent<RewardItemInfoWnd>();

        rewardItemInfoWnd.SetEquipData(pro);
        rewardItemInfoWnd.SetCallBack(new CallBack(OnDialogCallBack, pro));

        // 显示窗口
        WindowMgr.ShowWindow(wnd);

        Coroutine.StopCoroutine("ShowBonusDialog");
    }

    /// <summary>
    /// 显示道具信息窗口
    /// </summary>
    void ShowItemInfoWnd(Property pro)
    {
        Texture2D tx = null;

        int petId = pro.Query<int>("pet_id");
        if (petId != 0)
            tx = MonsterMgr.GetTexture(petId, MonsterMgr.GetDefaultRank(petId));
        else
        {
            tx = ItemMgr.GetTexture(pro.GetClassID(), true);
            if (tx == null)
                tx = ItemMgr.GetTexture(pro.GetClassID());
        }

        mIcon.mainTexture = tx;

        mIcon.gameObject.SetActive(true);

        mRewardPropName.text = string.Format("[{0}]{1}{2}[-]", "ffffff", pro.Short(), "x" + pro.GetAmount());

        mRewardPropName.gameObject.SetActive(true);

        mRewardProp.SetActive(true);

        // 循环战斗不需要悬浮框
        if (mIsLoopFight)
        {
            mSellTips.text = LocalizationMgr.Get("FightSettlementWnd_27");

            mSellTips.gameObject.SetActive(true);

            return;
        }

        GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType);
        RewardItemInfoWnd rewardItemInfoWnd = wnd.GetComponent<RewardItemInfoWnd>();
        rewardItemInfoWnd.SetCallBack(new CallBack(OnDialogCallBack));
        rewardItemInfoWnd.SetPropData(pro, true, false, LocalizationMgr.Get("RewardEquipInfoWnd_3"));

        // 显示窗口
        WindowMgr.ShowWindow(wnd);
    }

    /// <summary>
    /// 显示宠物信息窗口
    /// </summary>
    void ShowPetInfoWnd(Property pro)
    {
        mIcon.mainTexture = MonsterMgr.GetTexture(pro.GetClassID(), pro.Query<int>("rank"));

        mIcon.gameObject.SetActive(true);

        mRewardProp.SetActive(true);

        mRewardPropName.text = string.Format("[{0}]{1}[-]", "ffffff", pro.Short());

        // 循环战斗不需要悬浮框
        if (mIsLoopFight)
        {
            mSellTips.text = LocalizationMgr.Get("FightSettlementWnd_27");

            mSellTips.gameObject.SetActive(true);
            return;
        }

        GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType);

        PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();
        script.Bind(pro, false);
        script.ShowBtn(true, false);
        script.SetCallBack(new CallBack(OnDialogCallBack));
    }

    /// <summary>
    /// 出售道具
    /// </summary>
    void SellItem(Property ob)
    {
        //获取装备的数量;
        int amount = ob.GetAmount();

        if(amount < 1)
            return;

        //构建参数;
        LPCMapping data= new LPCMapping ();

        LPCMapping itemData = new LPCMapping();
        itemData.Add(string.IsNullOrEmpty(ob.Query<string>("original_rid")) ? ob.GetRid() : ob.Query<string>("original_rid"), amount);

        data.Add("rid", itemData);

        //通知服务器出售道具
        Operation.CmdSellItem.Go(data);
    }

    void OnDialogCallBack(object para, params object[] _params)
    {
        //窗口对象不存在;
        if(LeaveWnd == null)
            return;

        LPCMapping data = LPCMapping.Empty;

        data.Add("result", mResult);
        data.Add("instance_id", InstanceId);

        // 同步抛事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_SETTLEMENT_BONUS_SHOW_FINISH, MixedValue.NewMixedValue<LPCMapping>(data), true);

        Coroutine.DispatchService(DelayProcess(para, _params[0]), "DelayProcess");
    }

    /// <summary>
    /// 延迟处理
    /// </summary>
    IEnumerator DelayProcess(object para, object result)
    {
        // 等待一帧
        yield return null;

        LeaveWnd.SetActive(true);

        LeaveWnd.GetComponent<TweenAlpha>().enabled = true;

        if (!(bool)result)
        {
            Coroutine.StopCoroutine("DelayProcess");

            yield break;
        }

        SellItem(para as Property);

        Coroutine.StopCoroutine("DelayProcess");
    }

    /// <summary>
    ///宝箱点击事件
    /// </summary>
    void OnClickBox(GameObject go)
    {
        if (mAnim == null || bonusMap == null)
            return;

        mAnim.Play(CombatConfig.ANIMATION_BASE_LAYER + "open",
            CombatConfig.ANIMATION_BASE_LAYER_INEDX,
            0f);

        mWhiteMask.GetComponent<TweenAlpha>().duration = 1.5f;

        // 播放宝箱开启音效
        GameSoundMgr.PlayGroupSound("openBox");

        //点击开启宝箱关闭开启提示;
        mBoxTips.SetActive(false);

        Invoke("ShowClearanceBonusProp", 0.8f);

        UIEventListener.Get(mBox).onClick -= OnClickBox;
    }

    /// <summary>
    ///Tween动画执行完回调函数
    /// </summary>
    void AnimationControl()
    {
        if (!bonusMap.ContainsKey("box"))
        {
            ShowLeaveWnd();

            return;
        }

        mBox.SetActive(true);

        if (mAnim == null)
            return;

        mAnim.Play(CombatConfig.ANIMATION_BASE_LAYER + "luoxia",
            CombatConfig.ANIMATION_BASE_LAYER_INEDX,
            0f);
    }

    /// <summary>
    /// 点击装备按钮回调
    /// </summary>
    void OnClickEquipBtn(GameObject go)
    {
        //玩家不在副本中不做处理;
        if (ME.user == null)
            return;

        GameObject wnd = WindowMgr.OpenWnd("BaggageWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<BaggageWnd>().BindPage((int)BAGGAGE_PAGE.EQUIP_PAGE, false);
    }

    /// <summary>
    ///点击冒险按钮回调
    /// </summary>
    void OnClickRiskBtn(GameObject go)
    {
        // 玩家不在副本中不做处理;
        // 获取玩家已经操作过其他按钮
        if (ME.user == null ||
            mIsOperated)
            return;

        // 标识mIsOperated
        mIsOperated = true;

        // 打开主场景
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(DoRisk));
    }

    /// <summary>
    /// 返回主城之后回调
    /// </summary>
    private void DoRisk(object para, object[] param)
    {
        // 离开副本
        DoLeaveInstance();

        // 继续指引
        if (GuideMgr.IsGuiding())
        {
            EventMgr.FireEvent(EventMgrEventType.EVENT_GUIDE_RETUEN_OPERATE, MixedValue.NewMixedValue<int>(GuideConst.RETURN_RISK), true);
        }
        else
        {
            LPCMapping instanceInfo = InstanceMgr.GetInstanceInfo(InstanceId);
            if (instanceInfo == null)
                return;

            // 地图配置信息
            CsvRow mapConfig = MapMgr.GetMapConfig(instanceInfo.GetValue<int>("map_id"));
            if (mapConfig == null)
                return;

            if (mapConfig.Query<int>("map_type").Equals(MapConst.TOWER_MAP))
            {
                // 抛出通天之塔关闭事件
                EventMgr.FireEvent(EventMgrEventType.EVENT_CLOSE_TOWER_SCENE, null);
            }

            // 显示主界面;
            GameObject wnd = WindowMgr.OpenMainWnd();
            if (wnd == null)
                return;

            // 设置为世界地图显示方式
            wnd.GetComponent<MainWnd>().ShowMainUIBtn(false);
        }
    }

    /// <summary>
    ///点击再来一次按钮回调
    /// </summary>
    void OnClickAgainBtn(GameObject go)
    {
        // 玩家不在副本中不做处理;
        // 获取玩家已经操作过其他按钮
        if (ME.user == null || mIsOperated)
            return;

        // 标识mIsOperated
        mIsOperated = true;

        // 打开世界地图场景
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(DoSelectAgainFight, InstanceId), true);
    }

    /// <summary>
    /// 执行离开副本
    /// </summary>
    private void DoLeaveInstance()
    {
        // 离开副本;
        InstanceMgr.LeaveInstance(ME.user);

        // 销毁自己
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 打开副本出战界面
    /// </summary>
    private void DoSelectAgainFight(object para, object[] param)
    {
        // 离开副本
        DoLeaveInstance();

        //获得选择战斗窗口
        GameObject wnd = WindowMgr.OpenWnd(SelectFighterWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        string nextInstance = para as string;

        // 地图类型
        int mapType = InstanceMgr.GetMapTypeByInstanceId(nextInstance);

        string wndName = string.Empty;
        if (mapType == MapConst.DUNGEONS_MAP_2 ||
            mapType == MapConst.SECRET_DUNGEONS_MAP ||
            mapType == MapConst.PET_DUNGEONS_MAP)
        {
            wndName = DungeonsWnd.WndType;
        }
        else if(mapType.Equals(MapConst.TOWER_MAP))
        {
            wndName = TowerWnd.WndType;
        }
        else
        {
            wndName = SelectInstanceWnd.WndType;
        }

        if (mapType.Equals(MapConst.TOWER_MAP))
        {
            // 根据副本id获取通天之塔的配置数据
            CsvRow row = TowerMgr.GetTowerInfoByInstance(nextInstance);
            if (row == null)
                return;

            // 绑定数据
            wnd.GetComponent<SelectFighterWnd>().TowerBind(wndName, nextInstance, row.Query<int>("layer"), row.Query<int>("difficulty"), mIsLoopFight);
        }
        else if (mapType.Equals(MapConst.SECRET_DUNGEONS_MAP))
        {
            if (mInstanceData == null)
                return;

            LPCMapping data = mInstanceData.GetValue<LPCMapping>("dynamic_map");
            if (data == null)
                return;

            // 设置本次通关的副本ID
            wnd.GetComponent<SelectFighterWnd>().Bind(wndName, nextInstance, data, mIsLoopFight);
        }
        else if (mapType.Equals(MapConst.PET_DUNGEONS_MAP))
        {
            if (mInstanceData == null)
                return;

            LPCMapping data = mInstanceData.GetValue<LPCMapping>("dynamic_map");
            if (data == null)
                return;

            // 设置本次通关的副本ID
            wnd.GetComponent<SelectFighterWnd>().Bind(wndName, nextInstance, data, mIsLoopFight);
        } else
        {
            // 设置本次通关的副本ID
            wnd.GetComponent<SelectFighterWnd>().Bind(wndName, nextInstance, null, mIsLoopFight);
        }
    }

    /// <summary>
    ///点击下一关卡按钮回调
    /// </summary>
    void OnClickNextBtn(GameObject go)
    {
        // 玩家不在副本中不做处理;
        // 获取玩家已经操作过其他按钮
        if (ME.user == null ||
            mIsOperated)
            return;

        // 下一个关卡没有解锁
        if (! InstanceMgr.IsUnlocked(ME.user, mNexInstanceId)
            && ! InstanceMgr.IsUnLockLevel(ME.user, mNexInstanceId))
            return;

        // 标识mIsOperated
        mIsOperated = true;

        // 获取副本的配置信息;
        LPCMapping data = InstanceMgr.GetInstanceInfo(InstanceId);
        if (data == null)
            return;

        //没有获取到下一关副本ID;
        string nextInstanceId = data.GetValue<string>("next_instance_id");
        if (string.IsNullOrEmpty(nextInstanceId))
        {
            LogMgr.Trace("没有获取到下一关副本ID,");
            return;
        }

        // 打开世界地图场景
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(DoSelectAgainFight, nextInstanceId));
    }

    /// <summary>
    ///返回地图界面
    /// </summary>
    void OnClickReturnMapBtn(GameObject go)
    {
        // 玩家不在副本中不做处理;
        // 获取玩家已经操作过其他按钮
        if (ME.user == null ||
            mIsOperated)
            return;

        // 标识mIsOperated
        mIsOperated = true;

        // 打开世界地图场景
        SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(OpenWorldMapWnd));
    }

    /// <summary>
    /// 打开世界地图
    /// </summary>
    private void OpenWorldMapWnd(object para, object[] param)
    {
        // 离开副本
        DoLeaveInstance();

        // 继续指引
        if (GuideMgr.IsGuiding())
            EventMgr.FireEvent(EventMgrEventType.EVENT_GUIDE_RETUEN_OPERATE, MixedValue.NewMixedValue<int>(GuideConst.RETURN_SELECT_INSTANCE), true);

        Coroutine.DispatchService(ShowSeletInstance());
    }

    IEnumerator ShowSeletInstance()
    {
        yield return null;

        // 地图类型
        int mapType = InstanceMgr.GetMapTypeByInstanceId(InstanceId);

        // 副本配置信息
        LPCMapping instanceConfig = InstanceMgr.GetInstanceInfo(InstanceId);

        if (instanceConfig == null)
            yield break;

        // 地图id
        int mapId = instanceConfig.GetValue<int>("map_id");

        if (mapType == MapConst.PET_DUNGEONS_MAP)
        {
            // 创建窗口
            GameObject wnd = WindowMgr.OpenWnd(DungeonsWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

            // 窗口创建失败
            if (wnd == null)
                yield break;

            // 绑定数据
            wnd.GetComponent<DungeonsWnd>().Bind(
                InstanceId,
                mapId,
                SceneMgr.SceneCameraFromPos,
                mInstanceData.GetValue<LPCMapping>("dynamic_map")
            );
        }
        else if (mapType == MapConst.DUNGEONS_MAP_2 || mapType == MapConst.SECRET_DUNGEONS_MAP)
        {
            // 创建窗口
            GameObject wnd = WindowMgr.OpenWnd(DungeonsWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

            // 窗口创建失败
            if (wnd == null)
                yield break;

            // 绑定数据
            wnd.GetComponent<DungeonsWnd>().Bind(
                InstanceId,
                mapId,
                SceneMgr.SceneCameraFromPos
            );
        }
        else if (mapType == MapConst.TOWER_MAP)
        {
            // 抛出通天塔关闭
            EventMgr.FireEvent(EventMgrEventType.EVENT_CLOSE_TOWER_SCENE, null);

            // 打开主界面
            GameObject mainWnd = WindowMgr.OpenWnd ("MainWnd");
            if (mainWnd == null)
                yield break;

            mainWnd.GetComponent<MainWnd>().ShowMainUIBtn(false);
        }
        else
        {
            // 创建窗口
            GameObject wnd = WindowMgr.OpenWnd(SelectInstanceWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

            // 窗口创建失败
            if (wnd == null)
                yield break;

            wnd.GetComponent<SelectInstanceWnd>().Bind(mapId, SceneMgr.SceneCameraFromPos);
        }
    }

    /// <summary>
    /// Shows the reward window.
    /// </summary>
    private void ShowRewardWnd()
    {
        mRewardWnd.GetComponent<TweenAlpha>().enabled = true;
    }

    /// <summary>
    /// Shows the window.
    /// </summary>
    private void ShowWnd()
    {
        // 显示通关时间
        ShowClearanceTime(mClearanceData);

        // 显示回合提示
        ShowRoundTips();

        mRewardWnd.GetComponent<TweenAlpha>().enabled = true;

        ShowLeaveWnd();

        EventMgr.FireEvent(EventMgrEventType.EVENT_FAILED_ANIMATION_FINISH, null);
    }

    private void ShowLeaveWnd()
    {
        LeaveWnd.SetActive(true);

        TweenAlpha alpha = LeaveWnd.GetComponent<TweenAlpha>();

        alpha.enabled = true;

        if (!mIsLoopFight)
            return;

        mRemainTime = 3;
        mLoopFightSecond.text = mRemainTime.ToString();
        mLoopFightSecond.gameObject.SetActive(true);

        // 开启倒计时
        mIsCountDown = true;
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

    private void SetSettlementBtn(bool isReturn)
    {
        if (isReturn)
        {
            mAgainLabel.text = LocalizationMgr.Get("FightSettlementWnd_12");

            mAgainLabel.transform.localPosition = Vector3.zero;

            mAgainPower.gameObject.SetActive(false);

            mNextLabel.text = LocalizationMgr.Get("FightSettlementWnd_5");

            //复活按钮注册事件;
            UIEventListener.Get(mAgainBtn).onClick = OnClickReturnMapBtn;

            //再来一次按钮注册事件;
            UIEventListener.Get(mNextBtn).onClick = OnClickAgainBtn;
        }
        else
        {
            //下一关按钮注册事件;
            UIEventListener.Get(mNextBtn).onClick = OnClickNextBtn;

            //再来一次按钮注册的事件;
            UIEventListener.Get(mAgainBtn).onClick = OnClickAgainBtn;
        }
    }

    void ClearanceTime()
    {
        ShowClearanceTime(mClearanceData);
    }

    /// <summary>
    /// 刷新未解锁提示
    /// </summary>
    void RefreshLockTips()
    {
        if (! InstanceMgr.IsUnLockLevel(ME.user, mNexInstanceId))
        {
            mLockBtn.SetActive(true);
            mUnLockBtn.SetActive(false);

            mLockBtnLb.text = LocalizationMgr.Get("FightSettlementWnd_6");

            // 下一关副本id
            LPCMapping nexInstance = InstanceMgr.GetInstanceInfo(mNexInstanceId);

            // 副本解锁提示
            mLockTips.text = string.Format(LocalizationMgr.Get("FightSettlementWnd_26"), nexInstance.GetValue<int>("unlock_level"));
        }
        else
        {
            mLockBtn.SetActive(false);
            mUnLockBtn.SetActive(true);
        }
    }

    /// <summary>
    ///战斗胜利结算界面动画以及相关内容的显示
    /// </summary>
    private void ClearanceVictory()
    {
        //战斗胜利的白色遮罩;
        mWhiteMask.SetActive(true);

        mFightVictory.SetActive(true);

        mBG.SetActive(true);

        mBG.GetComponent<TweenAlpha>().enabled = true;

        mTopLight.GetComponent<TweenRotation>().enabled = true;

        Transform normal = mFightVictory.transform.Find("normal_1");
        if (normal != null)
            normal.GetComponent <UILabel>().text = LocalizationMgr.Get("FightSettlementWnd_24");

        EventDelegate.Add(normal.GetComponent<TweenScale>().onFinished, AnimationControl);

        EventDelegate.Add(normal.GetComponent<TweenAlpha>().onFinished, ShowRewardWnd);

        Transform normal2 = mFightVictory.transform.Find("normal_2");
        if (normal2 != null)
            normal2.GetComponent <UILabel>().text = LocalizationMgr.Get("FightSettlementWnd_24");

        Transform light = mFightVictory.transform.Find("light");
        if (light != null)
            light.GetComponent <UILabel>().text = LocalizationMgr.Get("FightSettlementWnd_24");

        Transform shadow = mFightVictory.transform.Find("shadow");
        if (shadow != null)
            shadow.GetComponent <UILabel>().text = LocalizationMgr.Get("FightSettlementWnd_24");

        EventDelegate.Add(normal2.GetComponent<TweenPosition>().onFinished, ClearanceTime);

        mAnim = mBox.GetComponent<Animator>();

        mAgainBtn.GetComponent<UISprite>().spriteName = "SuitNormalBtn";

        mAgainLabel.text = LocalizationMgr.Get("FightSettlementWnd_5");

        mNextLabel.text = LocalizationMgr.Get("FightSettlementWnd_6");

        // 循环战斗不做以下操作
        if (mIsLoopFight)
            return;

        mNextBtn.SetActive(true);

        if (string.IsNullOrEmpty(InstanceId))
            return;

        // 获取副本消耗数据
        LPCMapping data = InstanceMgr.GetInstanceCostMap(ME.user, InstanceId, mInstanceData);

        if (data == null)
            return;

        string field = FieldsMgr.GetFieldInMapping(data);

        mAgainPower.spriteName = FieldsMgr.GetFieldIcon(field);

        // 再来一次副本需要的开销
        mAgainPowerLabel.text = data.GetValue<int>(field).ToString();

        bool isReturn = false;

        if (mMapType == MapConst.PET_DUNGEONS_MAP)
        {
            mNextBtn.SetActive(false);
            isReturn = true;
        }
        else if (mNexInstanceId.Equals(InstanceId))
        {
            isReturn = true;
        }
        else
        {
            if (mMapType == MapConst.DUNGEONS_MAP_2)
                isReturn = true;
            else if(mMapType.Equals(MapConst.INSTANCE_MAP_1))
            {
                RefreshLockTips();
                isReturn = false;
            }
            else
            {
                isReturn = false;
            }
        }

        SetSettlementBtn(isReturn);

        // 获取下一个副本的配置信息;
        LPCMapping nextMap = InstanceMgr.GetInstanceCostMap(ME.user, mNexInstanceId, mInstanceData);

        if (nextMap == null)
            return;

        string nextField = FieldsMgr.GetFieldInMapping(nextMap);

        // 显示进入下一关的副本开销;
        mNextPowerLabel.text = nextMap.GetValue<int>(nextField).ToString();

        mNextPower.spriteName = FieldsMgr.GetFieldIcon(nextField);
    }

    /// <summary>
    ///战斗失败结算界面相关内容的显示
    /// </summary>
    private void ClearanceFiled()
    {
        mWhiteMask.SetActive(false);

        EventDelegate.Add(mFightFailed.GetComponent<TweenPosition>().onFinished, ShowWnd);

        mBG.SetActive(true);

        mBG.GetComponent<TweenAlpha>().enabled = true;

        if (bonusMap.ContainsKey("box"))
        {
            mTopLight.SetActive(true);

            mFightVictory.SetActive(true);

            Transform normal_1 = mFightVictory.transform.Find("normal_1");
            if (normal_1 != null)
                normal_1.GetComponent<UILabel>().text = LocalizationMgr.Get("FightSettlementWnd_25");

            Transform normal_2 = mFightVictory.transform.Find("normal_2");
            if (normal_2 != null)
                normal_2.GetComponent<UILabel>().text = LocalizationMgr.Get("FightSettlementWnd_25");

            Transform light = mFightVictory.transform.Find("light");
            if (light != null)
                light.GetComponent<UILabel>().text = LocalizationMgr.Get("FightSettlementWnd_25");

            Transform shadow = mFightVictory.transform.Find("shadow");
            if (shadow != null)
                shadow.GetComponent <UILabel>().text = LocalizationMgr.Get("FightSettlementWnd_25");

            EventDelegate.Add(normal_1.GetComponent<TweenScale>().onFinished, AnimationControl);

            EventDelegate.Add(normal_1.GetComponent<TweenAlpha>().onFinished, ShowRewardWnd);

            mAnim = mBox.GetComponent<Animator>();
        }
        else
        {
            Transform Failed = mFightFailed.transform.Find("Failed");
            if (Failed != null)
                Failed.GetComponent<UILabel>().text = LocalizationMgr.Get("FightSettlementWnd_25");

            Transform shadow = mFightFailed.transform.Find("shadow");
            if (shadow != null)
                shadow.GetComponent <UILabel>().text = LocalizationMgr.Get("FightSettlementWnd_25");

            mTopLight.SetActive(false);

            mFightFailed.SetActive(true);
        }

        // 循环战斗不做以下操作
        if (mIsLoopFight)
            return;

        SetSettlementBtn(true);

        if (string.IsNullOrEmpty(InstanceId))
            return;

        //获取进入副本开销;
        LPCMapping data = InstanceMgr.GetInstanceCostMap(ME.user, InstanceId, mInstanceData);

        if (data == null || data.Count <= 0)
            return;

        string field = FieldsMgr.GetFieldInMapping(data);

        // 显示进入下一关的副本开销;
        mNextPowerLabel.text = data.GetValue<int>(field).ToString();

        mNextPower.spriteName = FieldsMgr.GetFieldIcon(field);
    }


    #endregion

    #region 外部接口

    /// <summary>
    ///副本通关
    /// </summary>
    public void InstanceClearance(bool isLoopFight = false)
    {
        mIsLoopFight = isLoopFight;

        mResult = 1;

        //显示玩家通关的动画;
        ClearanceVictory();

        // 循环战斗不需要显示按钮
        if (mIsLoopFight)
        {
            mNextBtn.SetActive(false);
            mAgainBtn.SetActive(false);
            mEquipBtn.SetActive(false);
            mRiskBtn.SetActive(false);
        }
        else
        {
        }

        if (bonusMap == null)
            return;

        //获取属性奖励(包括金币,钻石)
        LPCMapping attribMap = bonusMap.GetValue<LPCMapping>("attrib");
        if (attribMap == null)
            return;

        // 获取副本通关奖励的金币,钻石;
        mRewardGoldAmount.text = attribMap.GetValue<int>("money").ToString();

        mRewardDiamondAmount.text = attribMap.GetValue<int>("diamond").ToString();

        mRewardLife.text = attribMap.GetValue<int>("life").ToString();
    }

    /// <summary>
    ///副本通关失败
    /// </summary>
    public void InstanceClearanceFailed(bool isLoopFight = false)
    {
        mIsLoopFight = isLoopFight;

        mResult = 0;

        // 循环战斗不需要显示按钮
        if (mIsLoopFight)
        {
            mNextBtn.SetActive(false);
            mAgainBtn.SetActive(false);
            mEquipBtn.SetActive(false);
            mRiskBtn.SetActive(false);
        }
        else
        {
            //复活按钮注册事件;
            UIEventListener.Get(mAgainBtn).onClick = OnClickReturnMapBtn;

            //再来一次按钮注册事件;
            UIEventListener.Get(mNextBtn).onClick = OnClickAgainBtn;
        }

        //获取属性奖励(包括金币,钻石)
        LPCMapping attribMap = bonusMap.GetValue<LPCMapping>("attrib");
        if (attribMap == null)
            return;

        // 获取副本通关奖励的金币,钻石;
        mRewardLife.text = attribMap.GetValue<int>("life").ToString();

        mRewardGoldAmount.text = attribMap.GetValue<int>("money").ToString();

        mRewardDiamondAmount.text = attribMap.GetValue<int>("diamond").ToString();

        //显示通关失败的动画;
        ClearanceFiled();
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data)
    {
        // 如果玩家对象不存在
        if (ME.user == null)
            return;

        // 获取绑定副本id
        InstanceId = data.GetValue<string>("instance_id");

        // 获取奖励信息
        bonusMap = data.GetValue<LPCMapping>("bonus_map");

        mClearanceData = data;

        LPCMapping curData = InstanceMgr.GetInstanceInfo(InstanceId);
        if (curData == null || curData.Count == 0)
            return;

        // 地图配置信息
        CsvRow mapConfig = MapMgr.GetMapConfig(curData.GetValue<int>("map_id"));
        if (mapConfig == null)
            return;

        // 获取下一个的副本id;
        mNexInstanceId = curData.GetValue<string>("next_instance_id");

        mMapType = mapConfig.Query<int>("map_type");

        // 如果是当期地图的最后一个副本或者是地下城副，显示当前副本的消耗
        if (string.IsNullOrEmpty(mNexInstanceId)
            || mMapType.Equals(MapConst.DUNGEONS_MAP_2))
            mNexInstanceId = InstanceId;

        // 注册事件
        RegisterEvent();

        // 获取玩家当前副本通关信息
        mInstanceData = ME.user.Query<LPCMapping>("instance");
    }

    /// <summary>
    /// 指引点击下一关按钮
    /// </summary>
    public void GuideOnClickNextBtn()
    {
        OnClickNextBtn(mNextBtn);
    }

    #endregion
}
