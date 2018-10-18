/// <summary>
/// SelectFighterWnd.cs
/// Created by tanzy 2016/05/09
/// 选择出战界面
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class SelectFighterWnd : WindowBase<SelectFighterWnd>
{
    #region 成员变量

    // 进入副本.
    public GameObject enterBtn;

    // 放弃战斗按钮
    public GameObject cancleBtn;

    // 关闭按钮
    public GameObject closeBtn;

    // 玩家宠物的位置.
    public GameObject[] SelectNormalParent;

    // 敌方宠物的位置;
    public Transform[] SelectEnemyPos;

    // 宠物格子
    public GameObject SelectFighterTemplte;

    // 玩家出战阵容的父级
    public GameObject mPlayerFormationParent;

    // 敌方出战阵容的父级
    public GameObject mEnemyFormationParent;

    public GameObject mBossBg;

    // 玩家选中英雄列表
    [HideInInspector]
    public List<PetItemWnd> mPlayerSelectList = new List<PetItemWnd>();

    // 副本id
    [HideInInspector]
    public string mInstanceId;

    // 绑定竞技场防守方相关数据
    private LPCMapping mDefenseData = LPCMapping.Empty;

    // 离开战斗按钮label
    public UILabel mLeaveLabel;

    // 任务查看按钮
    public GameObject mTaskBtn;

    // 挑战任务按钮label
    public UILabel mTaskLabel;

    // 副本名字
    public UILabel mInstanceName;

    // 开始战斗按钮label
    public UILabel mStartFighter;

    // 队长技能效果描述
    public UILabel mLeaderSkillDesc;

    // 一次战斗需要的体力值
    public UILabel mPower;

    public UISprite mCostIcon;

    public GameObject mDungeonsPetParent;

    public GameObject mBossItem;
    public GameObject mDungeonsBossBg;

    public PlayerPetWnd mPlayerPetWnd;

    public FriendPetWnd mFriendPetWnd;

    // 设定队长按钮
    public GameObject mSetLeaderBtn;
    public UILabel mSetLeaderBtnLb;

    // 分前后排的站位
    public GameObject mSpecial;

    public GameObject mFrontParent;
    public UILabel mFrontLb;
    public UISprite mFrontbg;

    public GameObject mBackParent;
    public UILabel mBackLb;

    // 设置队长提示
    public UILabel mSetLeaderDesc;

    // 设置队长遮罩
    public GameObject mSetLeaderMask;

    public GameObject mAutoPanel;

    // 自动战斗按钮
    public UIToggle mAutoFight;

    public UILabel mAutoTips1;

    public UILabel mCountDown;

    public UILabel mAutoTips2;

    // 自动战斗剩余时间
    public UILabel mAutoRemainTime;

    public GameObject mLifeWnd;

    public GameObject mApWnd;

    public GameObject mAppendSkillItem;

    public UILabel mSkillTips;

    public GameObject mSkillViewWnd;

    public UISprite mVs;

    // 存储选择宠物的Rid
    [HideInInspector]
    public List<string> mSelectRidList = new List<string>();

    public List<int> mSelectClassIdList = new List<int>();

    [HideInInspector]
    public List<PetItemWnd> arrPet = new List<PetItemWnd>();

    // 长按宠物的rid
    [HideInInspector]
    public Property mPressOb;

    // 副本的配置信息
    List<LPCMapping> petInfoList = new List<LPCMapping>();

    List<Property> mArchiveFormation = new List<Property>();

    // 可出战的宠物数量
    [HideInInspector]
    public int amount = 0;

    // 是否是选择队长状态
    bool mIsSetLeader = false;

    bool mNoSelectLeaderSkill = false;

    //队伍中是否有使用有队长技能增益
    bool mIsHasLeaderSkill = false;

    string mWndType = string.Empty;

    // 进入副本的消耗
    LPCMapping mCost = new LPCMapping();

    LPCMapping mDynamicData = LPCMapping.Empty;

    // 消耗字段
    string field = string.Empty;

    // 副本站位信息
    [HideInInspector]
    public LPCMapping mFormation = LPCMapping.Empty;

    // 是否调用
    bool mIsInvoke = true;

    // 是否使用好友宠物
    bool mIsFriendPet = false;

    // 是否循环战斗
    [HideInInspector]
    public bool mIsLoopFight = false;

    // 是否全部满级
    bool mIsAllMaxLevel = true;

    List<Property> mCacheList = new List<Property>();

    // 收集的战斗列表
    LPCMapping mFightMap = LPCMapping.Empty;

    // 缓存克隆的物件对象
    [HideInInspector]
    public List<Property> mCacheCreateOb = new List<Property>();

    [HideInInspector]
    public DragDropPetItem mDragDropPetItem;

    // 玩家阵容对应的位置信息
    Dictionary<string, int> mPos = new Dictionary<string, int>();

    Dictionary<int, Property> mSkillTarget = new Dictionary<int, Property>();

    /// <summary>
    /// 正在延迟关闭中
    /// </summary>
    private bool IsDelayClosing { set; get; }

    bool mIsCountDown = false;
    float mLastTime = 0;

    int mRemainTime = 0;


    // 当前窗口对象
    GameObject mCurWnd;

    int mMapType = 0;

    int mAutoRemain = 0;

    #endregion

    #region 内部函数

    void OnDestroy()
    {
        // 析构克隆的物件
        DestroyProperty();

        // 销毁可能存在的窗口
        if (WindowMgr.GetWindow(PetInfoWnd.WndType) != null)
            WindowMgr.DestroyWindow(PetInfoWnd.WndType);

        if (WindowMgr.GetWindow(PetSimpleInfoWnd.WndType) != null)
            WindowMgr.DestroyWindow(PetSimpleInfoWnd.WndType);

        if (WindowMgr.GetWindow(BossInfoWnd.WndType) != null)
            WindowMgr.DestroyWindow(BossInfoWnd.WndType);

        if (WindowMgr.GetWindow(QuickMarketWnd.WndType) != null)
            WindowMgr.DestroyWindow(QuickMarketWnd.WndType);

        Coroutine.StopCoroutine("LimitPosInScreen");
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    void OnDisable()
    {
        // 取消调用
        if (IsInvoking("TowerRefreshTime"))
            CancelInvoke("TowerRefreshTime");

        // 取消消息关注
        MsgMgr.RemoveDoneHook("MSG_ENTER_INSTANCE", "SelectFighterWnd");

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("SelectFighterWnd");
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    void OnEnable()
    {
        // 取消验证关闭标识
        IsDelayClosing = false;

        // 关注MSG_ENTER_INSTANCE消息
        MsgMgr.RemoveDoneHook("MSG_ENTER_INSTANCE", "SelectFighterWnd");
        MsgMgr.RegisterDoneHook("MSG_ENTER_INSTANCE", "SelectFighterWnd", OnMsgEnterInstance);

        if (ME.user == null)
            return;

        ME.user.dbase.RegisterTriggerField("SelectFighterWnd", new string[] { "auto_dungeons", "auto_tower" }, new global::CallBack(OnFieldsChange));
    }

    void OnFieldsChange(object para, params object[] param)
    {
        // 刷新自动战斗面板
        ShowAutoPanel();
    }

    void Start()
    {
        mVs.spriteName = ConfigMgr.IsCN ? "vs":"vs_en";

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        // 缓存当前窗口对象
        mCurWnd = this.gameObject;

        float scale = Game.CalcWndScale();
        transform.localScale = new Vector3(scale, scale, scale);
    }

    void Update()
    {
        if (mIsCountDown)
        {
            if (Time.realtimeSinceStartup > mLastTime + 1)
            {
                mLastTime = Time.realtimeSinceStartup;
                CountDown();
            }
        }
    }

    /// <summary>
    /// 循环战斗倒计时
    /// </summary>
    void CountDown()
    {
        if (mRemainTime < 1)
        {
            // 结束调用本方法
            mIsCountDown = false;

            // 执行进入副本
            DoEnterInstance();

            return;
        }

        mCountDown.text = string.Format(LocalizationMgr.Get("SelectFighterWnd_38"), mRemainTime);

        mRemainTime--;
    }

    /// <summary>
    ///初始化窗口
    /// </summary>
    void Redraw()
    {
        //注册事件;
        RegisterEvent();

        LeaderSkillEffectDesc(null);

        //初始化玩家阵容格子;
        InitPlayerGrid();

        // 初始化玩家阵容信息
        InitPlayerFormationInfo();

        //初始化敌方阵容格子;
        InitEnemyGrid();

        //初始化label内容;
        SetLabelContent();

        // 获取进入副本的开销数据;
        mCost = InstanceMgr.GetInstanceCostMap(ME.user, mInstanceId, mDynamicData);

        // 消耗属性的字段
        field = FieldsMgr.GetFieldInMapping(mCost);

        //显示进入副本开销的值
        mPower.text = mCost.GetValue<int>(field).ToString();

        mCostIcon.spriteName = FieldsMgr.GetFieldIcon(field);

        if (mSetLeaderMask.activeSelf)
            mSetLeaderMask.SetActive(false);

        // 地图类型
        mMapType = InstanceMgr.GetInstanceMapType(mInstanceId);

        // 当前副本是否可以自动战斗
        bool isLoopfight = InstanceMgr.IsAutoLoopFight(mInstanceId);
        if (isLoopfight)
        {
            mAutoFight.value = mIsLoopFight;

            cancleBtn.transform.localPosition = new Vector3(21,
                cancleBtn.transform.localPosition.y, cancleBtn.transform.localPosition.z);
            enterBtn.transform.localPosition = new Vector3(504,
                enterBtn.transform.localPosition.y, enterBtn.transform.localPosition.z);

            // 显示自动战斗面板
            ShowAutoPanel();
        }
        else
        {
            cancleBtn.SetActive(true);

            mAutoPanel.SetActive(false);

            cancleBtn.transform.localPosition = new Vector3(98,
                cancleBtn.transform.localPosition.y, cancleBtn.transform.localPosition.z);
            enterBtn.transform.localPosition = new Vector3(430,
                enterBtn.transform.localPosition.y, enterBtn.transform.localPosition.z);
        }

        // TODO 暂时屏蔽
        mTaskBtn.SetActive(false);

        if (mMapType.Equals(MapConst.ARENA_MAP)
            || mMapType.Equals(MapConst.ARENA_NPC_MAP)
            || mMapType.Equals(MapConst.ARENA_REVENGE_MAP))
        {
            mLifeWnd.SetActive(false);
            mApWnd.SetActive(true);
        }
        else
        {
            mLifeWnd.SetActive(true);
            mApWnd.SetActive(false);
        }
    }

    /// <summary>
    /// 显示自动战斗面板
    /// </summary>
    void ShowAutoPanel()
    {
        CancelInvoke("TowerRefreshTime");
        UIEventListener.Get(mAutoRemainTime.gameObject).onClick -= OnClickBuyMonthCard;
        if (mMapType.Equals(MapConst.DUNGEONS_MAP_2))
        {
            mAutoRemainTime.gameObject.SetActive(true);

            cancleBtn.SetActive(false);

            int dunEndTime = 0;

            // 圣域自动战斗结束时间
            LPCValue dungeons_value = ME.user.Query<LPCValue>("auto_dungeons");
            if (dungeons_value != null && dungeons_value.IsInt)
                dunEndTime = dungeons_value.AsInt;

            if (dunEndTime == 0)
            {
                // 圣域自动战斗没有开启
                if(!mAutoPanel.activeSelf)
                    mAutoPanel.SetActive(true);

                UIEventListener.Get(mAutoRemainTime.gameObject).onClick = OnClickBuyMonthCard;

                mAutoTips1.text = LocalizationMgr.Get("SelectFighterWnd_48");
                mAutoTips2.text = LocalizationMgr.Get("SelectFighterWnd_43");
                mCountDown.text = LocalizationMgr.Get("SelectFighterWnd_45");
                mAutoRemainTime.text = LocalizationMgr.Get("SelectFighterWnd_60");
            }
            else if (dunEndTime - TimeMgr.GetServerTime() <= 0)
            {
                // 圣域自动战斗已过期
                mAutoTips1.text = LocalizationMgr.Get("SelectFighterWnd_48");
                mAutoTips2.text = LocalizationMgr.Get("SelectFighterWnd_52");
                mCountDown.text = LocalizationMgr.Get("SelectFighterWnd_51");
                mAutoRemainTime.text = LocalizationMgr.Get("SelectFighterWnd_46");
            }
            else
            {
                mAutoTips1.text = LocalizationMgr.Get("SelectFighterWnd_41");
                mAutoTips2.text = LocalizationMgr.Get("SelectFighterWnd_43");
                mCountDown.text = LocalizationMgr.Get("SelectFighterWnd_27");

                mAutoRemain = dunEndTime - TimeMgr.GetServerTime();

                // 每秒循环一次
                InvokeRepeating("TowerRefreshTime", 0, 1.0f);
            }
        }
        else if (mMapType.Equals(MapConst.TOWER_MAP))
        {
            mAutoRemainTime.gameObject.SetActive(true);

            mAutoTips1.text = LocalizationMgr.Get("SelectFighterWnd_42");
            mAutoTips2.text = LocalizationMgr.Get("SelectFighterWnd_44");
            mCountDown.text = LocalizationMgr.Get("SelectFighterWnd_58");

            int towerEndTime = 0;

            // 通天塔自动战斗结束时间
            LPCValue tower_value = ME.user.Query<LPCValue>("auto_tower");
            if (tower_value != null && tower_value.IsInt)
                towerEndTime = tower_value.AsInt;

            mAutoRemain = 0;

            if (towerEndTime == 0)
            {
                // 通天塔自动战斗没有开启
                mAutoPanel.SetActive(false);

                cancleBtn.SetActive(true);
            }
            else if (towerEndTime - TimeMgr.GetServerTime() <= 0)
            {
                // 通天塔自动战斗已过期
                mAutoTips1.text = LocalizationMgr.Get("SelectFighterWnd_49");
                mCountDown.text = LocalizationMgr.Get("SelectFighterWnd_59");

                mAutoRemainTime.text = LocalizationMgr.Get("SelectFighterWnd_46");
                mAutoTips2.text = LocalizationMgr.Get("SelectFighterWnd_54");

                cancleBtn.SetActive(false);
            }
            else
            {
                mAutoPanel.SetActive(true);
                cancleBtn.SetActive(false);

                mAutoRemain = towerEndTime - TimeMgr.GetServerTime();

                // 每秒循环一次
                InvokeRepeating("TowerRefreshTime", 0, 1.0f);
            }
        }
        else
        {
            mAutoRemainTime.gameObject.SetActive(false);

            mAutoTips1.text = LocalizationMgr.Get("SelectFighterWnd_26");
            mAutoTips2.text = LocalizationMgr.Get("SelectFighterWnd_28");

            mCountDown.text = LocalizationMgr.Get("SelectFighterWnd_27");
        }
    }

    void TowerRefreshTime()
    {
        if (mAutoRemain <= 0)
        {
            // 刷新自动爬塔面板
            ShowAutoPanel();

            // 结束调用
            if (IsInvoking("TowerRefreshTime"))
                CancelInvoke("TowerRefreshTime");

            return;
        }

        mAutoRemain--;

        if(mAutoRemain < 60)
            mAutoRemainTime.text = string.Format(LocalizationMgr.Get("SelectFighterWnd_57"), mAutoRemain);
        else if (mAutoRemain < 3600)
            mAutoRemainTime.text = string.Format(LocalizationMgr.Get("SelectFighterWnd_56"), mAutoRemain / 60);
        else if(mAutoRemain < 86400)
            mAutoRemainTime.text = string.Format(LocalizationMgr.Get("SelectFighterWnd_55"), mAutoRemain / 3600);
        else
            mAutoRemainTime.text = string.Format(LocalizationMgr.Get("SelectFighterWnd_47"), mAutoRemain / 86400);
    }

    /// <summary>
    ///注册事件
    /// </summary>
    void RegisterEvent()
    {
        //注册按钮的点击事件;
        UIEventListener.Get(closeBtn).onClick += ClickCloseUI;
        UIEventListener.Get(enterBtn).onClick += ClickEnterCombatScene;
        UIEventListener.Get(cancleBtn).onClick += ClickCloseUI;
        UIEventListener.Get(mTaskBtn).onClick += OnClickTaskBtn;
        UIEventListener.Get(mSetLeaderBtn).onClick = OnClickSetLeaderBtn;

        // 注册开关状态变化回调
        EventDelegate.Add(mAutoFight.onChange, OnClickLoopFightBtn);
    }

    /// <summary>
    /// 月卡购买点击回调
    /// </summary>
    void OnClickBuyMonthCard(GameObject go)
    {
        // 显示月卡界面
        ShowMonthCardWnd();
    }

    /// <summary>
    /// 进入副本
    /// </summary>
    void OnMsgEnterInstance(string cmd, LPCValue para)
    {
        LPCMapping args = para.AsMapping;
        if (args == null)
            return;

        string rid = args.GetValue<string>("rid");
        if (string.IsNullOrEmpty(rid))
        {
            // 打开主场景
            SceneMgr.LoadScene("Main", SceneConst.SCENE_WORLD_MAP, new CallBack(DoCloseWnd));
        }

        // 标识正在关闭中
        IsDelayClosing = true;

        // 延迟到下一帧调用隐藏自己;
        MergeExecuteMgr.DispatchExecute(DoDestroy);
    }

    /// <summary>
    /// 销毁自己
    /// </summary>
    void DoDestroy()
    {
        if (this == null)
            return;

        // 销毁自己
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    ///初始化玩家阵容格子
    /// </summary>
    void InitPlayerGrid()
    {
        //获取玩家副本出战宠物的最大数量;
        if (string.IsNullOrEmpty(mInstanceId))
            return;

        amount = InstanceMgr.GetMaxCombatPetAmount(mInstanceId);

        // 获取副本配置信息
        LPCMapping instanceData = InstanceMgr.GetInstanceInfo(mInstanceId);

        // 没有获取到该副本的配置信息
        if (instanceData == null || instanceData.Count == 0)
            return;

        // 副本站位信息
        mFormation = instanceData.GetValue<LPCMapping>("formation");

        // 获取副本站位信息
        if (mFormation == null || mFormation.Count == 0)
            return;

        for (int i = 0; i < SelectNormalParent.Length; i++)
            SelectNormalParent[i].SetActive(false);

        if (mFormation.ContainsKey(FormationConst.RAW_NONE))
        {
            // 不区分前后排
            InitNormalFormationGrid();
            mSetLeaderBtn.SetActive(false);

            mLeaderSkillDesc.transform.localPosition = new Vector3(263, 246, 0);
            mLeaderSkillDesc.alignment = NGUIText.Alignment.Center;
            mSpecial.SetActive(false);
        }
        else
        {
            // 区分前后排
            InitSpecialFormationGrid();
            mSetLeaderBtn.SetActive(true);

            mLeaderSkillDesc.transform.localPosition = new Vector3(294, 268, 0);
            mLeaderSkillDesc.alignment = NGUIText.Alignment.Left;
            mSpecial.SetActive(true);
        }
    }

    /// <summary>
    /// 初始化普通站位玩家阵容格子,不区分前后排
    /// </summary>
    void InitNormalFormationGrid()
    {
        for (int i = 0; i < SelectNormalParent.Length; i++)
        {
            if(SelectNormalParent[i].activeSelf)
                SelectNormalParent[i].SetActive(false);
        }

        if (amount - 2 < 0)
            return;

        GameObject parent = SelectNormalParent[amount - 2];
        if (parent == null)
            return;

        parent.SetActive(true);

        for (int i = 0; i < parent.transform.childCount; i++)
        {
            GameObject go = parent.transform.GetChild(i).gameObject;
            if (go == null)
                return;

            UIPanel panel = go.GetComponent<UIPanel>();
            if (panel != null)
                panel.depth += 2;

            DragDropPetItem drag = go.AddComponent<DragDropPetItem>();

            drag.SetCallBack(new CallBack(CallBack));

            //添加宠物的点击事件，点击取消选择宠物;
            UIEventListener.Get(go).onClick = ClickCancelFighterPet;

            //添加玩家宠物的 长按 事件,长按显示宠物信息;
            UIEventListener.Get(go).onPress = OnPressShowPetInfo;

            go.SetActive(true);
            mPlayerSelectList.Add(go.GetComponent<PetItemWnd>());
            go.GetComponent<PetItemWnd>().ShowLeaderIcon(false);

            // 设置宠物格子标签
            SetFormationPetItemTag(go);

            mPos.Remove(go.name);

            mPos.Add(go.name, i);
        }

        UpdateShadow();
    }

    /// <summary>
    /// 初始化特殊站位玩家阵容格子, 区分前后排
    /// </summary>
    void InitSpecialFormationGrid()
    {
        // 前排宠物数量
        int frontAmount = mFormation.GetValue<int>(FormationConst.RAW_FRONT);

        // 后排宠物数量
        int backAmount = mFormation.GetValue<int>(FormationConst.RAW_BACK);

        // item的间隔
        float space = 14;

        // item的大小
        Vector3 itemSize = SelectFighterTemplte.GetComponent<BoxCollider>().size;

        // 显示一列item需要的高度包括间隔
        float showItemHeight = mFrontParent.transform.localPosition.y + itemSize.y / 2.0f
            - mFrontbg.transform.localPosition.y + mFrontbg.height / 2.0f - 15;
        Transform parent = null;
        float frontRemainSpace = (showItemHeight - frontAmount * itemSize.y - (frontAmount - 1) * space) / 2.0f;
        float backRemainSpace = (showItemHeight - backAmount * itemSize.y - (backAmount - 1) * space) / 2.0f;

        int index = 0;
        for (int i = 0; i < backAmount + frontAmount; i++)
        {
            if (i + 1 <= frontAmount)
                parent = mFrontParent.transform;
            else
                parent = mBackParent.transform;

            GameObject go = GameObject.Instantiate(SelectFighterTemplte);
            go.transform.SetParent(parent);
            go.name = "PlayerFighter" + i;
            go.transform.localScale = Vector3.one;
            UIPanel panel = go.GetComponent<UIPanel>();
            if (panel != null)
                panel.depth += 2;

            if (i + 1 <= frontAmount)
            {
                frontRemainSpace = frontRemainSpace > 0 ? frontRemainSpace : 0;
                go.transform.localPosition = new Vector3(0,
                    0 - i * (itemSize.y + 15)  - frontRemainSpace,
                    go.transform.localPosition.z);
            }
            else
            {
                backRemainSpace = backRemainSpace > 0 ? backRemainSpace : 0;
                go.transform.localPosition = new Vector3(0,
                    0 - index * (itemSize.y + 15) - backRemainSpace,
                    go.transform.localPosition.z);
                index ++;
            }

            //添加宠物的点击事件，点击取消选择宠物;
            UIEventListener.Get(go).onClick = ClickCancelFighterPet;

            //添加玩家宠物的 长按 事件,长按显示宠物信息;
            UIEventListener.Get(go).onPress = OnPressShowPetInfo;

            go.SetActive(true);

            mPlayerSelectList.Add(go.GetComponent<PetItemWnd>());

            DragDropPetItem drag = go.AddComponent<DragDropPetItem>();

            drag.SetCallBack(new CallBack(CallBack));

            // 设置宠物格子标签
            SetFormationPetItemTag(go);

            mPos.Remove(go.name);

            mPos.Add(go.name, i);
        }

        UpdateShadow();
    }

    /// <summary>
    /// 初始化玩家缓存的阵容信息
    /// </summary>
    void InitPlayerFormationInfo()
    {
        int leaderPos = GetCacheLeaderPos();

        for (int i = 0; i < mPlayerSelectList.Count; i++)
        {
            PetItemWnd item = mPlayerSelectList[i];
            if(i + 1 > mArchiveFormation.Count)
                break;

            if (i == leaderPos)
                item.ShowLeaderSkill(true);

            Property ob = mArchiveFormation[i];
            if (ob == null)
            {
                item.SetBind(null);
                continue;
            }

            // 该副本不能出现相同宠物
            if (! InstanceMgr.IsAllowSamePet(mInstanceId) &&
                mSelectClassIdList.Contains(ob.GetClassID()))
                continue;

            if (mFriendPetWnd.mData.ContainsKey(ob.GetRid()))
                continue;

            /// 材料宠物不允许出战， 部分副本不允许选择仓库中的宠物
            if (!InstanceMgr.IsFillLevelLimit(ob.GetLevel(), mInstanceId)
                || MonsterMgr.IsMaterialMonster(ob.GetClassID())
                || (! InstanceMgr.IsSelectStorePet(mInstanceId) && MonsterMgr.IsStorePet(ob)))
            {
                continue;
            }

            item.ShowLeaderText(string.Empty);

            item.ShowMaxLevel(true);

            item.SetBind(ob);

            // 设置宠物格子标签
            SetFormationPetItemTag(item.gameObject);

            if (!mSelectRidList.Contains(ob.GetRid()))
            {
                mSelectRidList.Add(ob.GetRid());

                mSelectClassIdList.Add(ob.GetClassID());
            }
        }

        if (mFormation.ContainsKey(FormationConst.RAW_NONE))
        {
            if (leaderPos >= mPlayerSelectList.Count)
                leaderPos = 0;

            mPlayerSelectList[leaderPos].SetLeader(true);
        }
        else
        {
            if (leaderPos >= mPlayerSelectList.Count)
                leaderPos = 0;

            mPlayerSelectList[leaderPos].ShowLeaderIcon(true);
        }

        // 刷新队长技能
        RefreshLeaderDesc();

        UpdateShadow();
    }

    /// <summary>
    /// 缓存队长位置
    /// </summary>
    void CacheLeaderPos(int pos)
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        LPCMapping data = LPCMapping.Empty;

        LPCValue v = OptionMgr.GetOption(ME.user, "leader_pos");
        if (v != null && v.IsMapping && v.AsMapping != null)
            data = v.AsMapping;

        // 获取副本配置信息
        LPCMapping instanceData = InstanceMgr.GetInstanceInfo(mInstanceId);
        if (instanceData == null)
            return;

        data.Add(instanceData.GetValue<int>("map_id"), pos);

        OptionMgr.SetOption(ME.user, "leader_pos", LPCValue.Create(data));
    }

    /// <summary>
    /// 获取缓存的队长位置信息
    /// </summary>
    int GetCacheLeaderPos()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return 0;

        LPCMapping data = LPCMapping.Empty;

        LPCValue v = OptionMgr.GetOption(ME.user, "leader_pos");
        if (v != null)
            data = v.AsMapping;
        else
            return 0;

        if (data == null)
            return 0;

        // 获取副本配置信息
        LPCMapping instanceData = InstanceMgr.GetInstanceInfo(mInstanceId);
        if (instanceData == null)
            return 0;

        return data.GetValue<int>(instanceData.GetValue<int>("map_id"));
    }

    /// <summary>
    ///初始化敌方阵容格子
    /// </summary>
    void InitEnemyGrid()
    {
        if (petInfoList == null)
            return;

        int bossAmount = 0;

        for (int i = 0; i < petInfoList.Count; i++)
        {
            if (petInfoList[i].GetValue<int>("is_boss") == 1)
                bossAmount++;
        }

        if (bossAmount > 0)
        {
            // 绘制boss副本宠物的阵容
            RedrawBossPetLineUp(bossAmount);
        }
        else
        {
            // 绘制普通副本的宠物阵容
            RedrawNormalPetLineUp();
        }
    }

    /// <summary>
    /// 绘制附加技能
    /// </summary>
    void RedrawAppendSkill(List<Property> petList)
    {
        LPCArray appendSkill = LPCArray.Empty;

        for (int i = 0; i < petList.Count; i++)
        {
            Property ob = petList[i];
            if (ob == null)
                continue;

            if (ob.Query<int>("is_boss") != 1)
                continue;

            LPCArray skills = SkillMgr.GetAppendSkill(ob);

            for (int j = 0; j < skills.Count; j++)
            {
                int id = skills[j].AsInt;

                if(!mSkillTarget.ContainsKey(id))
                    mSkillTarget.Add(id, ob);
            }

            // 收集附加技能
            appendSkill.Append(skills);
        }

        mAppendSkillItem.SetActive(false);

        if (appendSkill == null || appendSkill.Count == 0)
        {
            mSkillTips.gameObject.SetActive(false);
            return;
        }

        mSkillTips.gameObject.SetActive(true);

        float startX = 0;

        for (int i = 0; i < appendSkill.Count; i++)
        {
            int skillId = appendSkill[i].AsInt;

            GameObject go = Instantiate(mAppendSkillItem);
            if (go == null)
                continue;

            go.transform.SetParent(mAppendSkillItem.transform.parent);
            go.transform.localPosition = Vector3.zero;
            go.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            if (appendSkill.Count % 2 == 1)
                startX = (i - (appendSkill.Count / 2)) * 65;
            else
                startX = (i - ((appendSkill.Count - 1) / 2f)) * 65;

            go.transform.localPosition = new Vector3(startX, mAppendSkillItem.transform.localPosition.y, mAppendSkillItem.transform.localPosition.z);

            go.SetActive(true);

            SkillItem script = go.GetComponent<SkillItem>();
            if (script == null)
                continue;

            // 绑定数据
            script.SetBind(skillId);

            // 注册按钮点击事件
            UIEventListener.Get(go).onPress = OnPressSkillItem;
        }
    }

    /// <summary>
    /// 技能格子点击回调
    /// </summary>
    void OnPressSkillItem(GameObject go, bool isPress)
    {
        SkillItem skillItem = go.GetComponent<SkillItem>();
        if (skillItem == null)
            return;

        SkillViewWnd script = mSkillViewWnd.GetComponent<SkillViewWnd>();
        if (script == null)
            return;

        //按下
        if (isPress)
        {
            if (skillItem.mSkillId <= 0)
                return;

            skillItem.SetSelected(true);

            // 显示悬浮窗口
            script.ShowView(skillItem.mSkillId, mSkillTarget[skillItem.mSkillId], true);

            BoxCollider box = go.GetComponent<BoxCollider>();

            Vector3 boxPos= box.transform.localPosition;

            mSkillViewWnd.transform.localPosition = new Vector3 (boxPos.x, boxPos.y + box.size.y / 2, boxPos.z);

            Coroutine.DispatchService(LimitPosInScreen(script), "LimitPosInScreen");
        }
        else
        {
            skillItem.SetSelected(false);

            // 隐藏悬浮窗口
            script.HideView();
        }
    }

    IEnumerator LimitPosInScreen(SkillViewWnd script)
    {
        yield return null;

        if (script == null)
            yield break;

        // 限制悬浮窗口在屏幕范围内
        script.LimitPosInScreen();
    }

    /// <summary>
    /// 绘制地下城界面宠物阵容
    /// </summary>
    void RedrawBossPetLineUp(int bossAmount)
    {
        mDungeonsBossBg.SetActive(true);

        if (petInfoList.Count < 1)
            return;

        int index = 0;

        int bossIndex = 0;

        List<Property> list = new List<Property>();

        UIPanel itemPanel = SelectFighterTemplte.GetComponent<UIPanel>();
        foreach (LPCMapping data in petInfoList)
        {
            if (data == null)
                continue;

            // 创建一个敌方宠物
            Property enemy = PropertyMgr.CreateProperty(data);
            AddCacheOb(enemy);

            GameObject go = Instantiate(SelectFighterTemplte);
            go.SetActive(true);
            PetItemWnd scriptOb = go.GetComponent<PetItemWnd>();

            //添加玩家宠物的 长按 事件;
            UIEventListener.Get(go).onClick = OnClickShowPetInfo;

            list.Add(enemy);

            if (enemy.Query<int>("is_boss").Equals(1))
            {
                go.transform.SetParent(mBossItem.transform);
                go.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
                go.transform.localPosition = Vector3.zero;

                float height = go.transform.Find("bg").GetComponent<UISprite>().height * 1.2f;

                float Y = 0f;

                if (bossAmount % 2 == 1)
                    Y = (bossIndex - (bossAmount / 2)) * height;
                else
                    Y = (bossIndex - ((bossAmount - 1) / 2f)) * height;

                go.transform.localPosition = new Vector3(0, Y, 0);

                scriptOb.ShowLeaderSkill(true);

                bossIndex++;
            }
            else
            {
                go.transform.SetParent(mDungeonsPetParent.transform);
                go.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
                go.GetComponent<UIPanel>().depth = itemPanel.depth - 2;

                float height = go.transform.Find("bg").GetComponent<UISprite>().height * 0.7f;
                float start_x = mBossItem.transform.localPosition.y + height * (petInfoList.Count - 2) / (float)2;
                go.transform.localPosition = new Vector3(-3, start_x - height * index, 0);

                scriptOb.ShowLevel(false);
                scriptOb.ShowLeaderSkill(false);
                index++;
            }

            // 绑定数据
            scriptOb.SetBind(enemy);
        }

        // 绘制附加技能
        RedrawAppendSkill(list);
    }

    /// <summary>
    /// 绘制普通副本的阵容
    /// </summary>
    void RedrawNormalPetLineUp()
    {
        mDungeonsBossBg.SetActive(false);

        GameObject go = null;

        if (petInfoList.Count < 1)
            return;

        List<Property> list = new List<Property>();

        UIPanel itemPanel = SelectFighterTemplte.GetComponent<UIPanel>();

        int index = 0;
        for (int i = 0; i < SelectEnemyPos.Length; i++)
        {
            if (petInfoList.Count == 4)
            {
                if (i == petInfoList.Count)
                    continue;
            }
            else if (petInfoList.Count == 3)
            {
                if (i == 1 || i == petInfoList.Count + 1)
                    continue;
            }
            else if (petInfoList.Count == 2)
            {
                if (i > 1)
                    continue;
            }
            else if (petInfoList.Count == 1)
            {
                if (i != 0)
                    break;
            }

            //实例化格子;
            go = GameObject.Instantiate(SelectFighterTemplte);

            //设置父级;
            go.transform.SetParent(SelectEnemyPos[i].transform);
            go.name = "EnemyFighter" + i;

            //设置相对位置和大小;
            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
            go.GetComponent<UIPanel>().depth = itemPanel.depth - 2;

            //添加玩家宠物的 长按 事件;
            UIEventListener.Get(go).onClick = OnClickShowPetInfo;

            LPCMapping data = petInfoList[index];

            if (mDynamicData != null && mDynamicData.ContainsKey("pet_id"))
                data.Add("class_id", mDynamicData.GetValue<int>("pet_id"));

            if (mDynamicData != null && mDynamicData.ContainsKey("rank"))
                data["rank"] = mDynamicData.GetValue<LPCValue>("rank");

            if (data != null && data.Count > 0)
            {
                // 创建一个敌方宠物
                Property enemy = PropertyMgr.CreateProperty(data);
                AddCacheOb(enemy);

                list.Add(enemy);

                if (i == 0)
                    go.GetComponent<PetItemWnd>().ShowLeaderSkill(true);

                go.GetComponent<PetItemWnd>().SetBind(enemy);
            }

            go.SetActive(true);
            index++;
        }

        if (petInfoList.Count == 3)
            mEnemyFormationParent.transform.localPosition = new Vector3(0, -50, 0);
        else if (petInfoList.Count == 2)
            mEnemyFormationParent.transform.localPosition = new Vector3(50, 0, 0);
        else if (petInfoList.Count == 1)
        {
            go.transform.localPosition = new Vector3(60, -50, 0);
            go.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            mBossBg.SetActive(true);
        }

        // 绘制附加技能
        RedrawAppendSkill(list);
    }

    /// <summary>
    /// 设置队长技能按钮点击事件
    /// </summary>
    void OnClickSetLeaderBtn(GameObject go)
    {
        // 正在等待服务器的回执消息
        // 或者正在延迟关闭中
        if (VerifyCmdMgr.IsVerifyCmd("CMD_ENTER_INSTANCE") || IsDelayClosing)
            return;

        mSetLeaderMask.SetActive(true);
        mIsSetLeader = true;
    }

    /// <summary>
    ///点击关闭窗口按钮
    /// </summary>
    void ClickCloseUI(GameObject go)
    {
        // 正在等待服务器的回执消息
        // 或者正在延迟关闭中
        if (VerifyCmdMgr.IsVerifyCmd("CMD_ENTER_INSTANCE") || IsDelayClosing)
            return;

        if (mIsLoopFight)
        {
            // 取消倒计时
            mIsCountDown = false;

            mAutoFight.value = false;
            mIsLoopFight = false;
        }

        // 继续指引
        if (GuideMgr.IsGuiding())
            EventMgr.FireEvent(EventMgrEventType.EVENT_GUIDE_RETUEN_OPERATE, MixedValue.NewMixedValue<int>(GuideConst.RETURN_SELECT_INSTANCE), true);

        // 显示主场景
        if (string.Equals(SceneMgr.MainScene, SceneConst.SCENE_WORLD_MAP))
            Coroutine.DispatchService(DoLoadScene(SceneConst.SCENE_WORLD_MAP));
        else
            Coroutine.DispatchService(DoLoadScene(SceneConst.SCENE_MAIN_CITY));
    }

    /// <summary>
    /// 执行载场景
    /// </summary>
    IEnumerator DoLoadScene(string scene)
    {
        yield return null;

        // 打开主场景
        SceneMgr.LoadScene("Main", scene, new CallBack(DoCloseWnd));

        yield return null;

        // 关闭当前窗口
        WindowMgr.DestroyWindow(SelectFighterWnd.WndType);
    }

    /// <summary>
    /// 执行关闭窗口
    /// </summary>
    void DoCloseWnd(object para, object[] param)
    {
        if (string.IsNullOrEmpty(mWndType))
            return;

        //获得获得需要显示的窗口
        GameObject wnd = WindowMgr.OpenWnd(mWndType);

        // 创建窗口失败
        if (wnd == null)
            return;

        if (mWndType.Equals(DungeonsWnd.WndType))
        {
            LPCMapping config = InstanceMgr.GetInstanceInfo(mInstanceId);
            wnd.GetComponent<DungeonsWnd>().Bind(mInstanceId, config.GetValue<int>("map_id"), SceneMgr.SceneCameraFromPos);
        }
        else if (mWndType.Equals(TowerWnd.WndType))
        {
            CsvRow row = TowerMgr.GetTowerInfoByInstance(mInstanceId);
            if (row == null)
                return;

            // 构建参数
            LPCMapping data = LPCMapping.Empty;

            data.Add("difficulty", row.Query<int>("difficulty"));

            data.Add("instance_id", mInstanceId);

            data.Add("is_play_forward", 0);

            // 抛出打开通天塔事件
            EventMgr.FireEvent(EventMgrEventType.EVENT_OPEN_TOWER_SCENE, MixedValue.NewMixedValue<LPCMapping>(data));

            wnd.GetComponent<TowerWnd>().Bind(row.Query<int>("difficulty"), false, mInstanceId);
        }
        else if (mWndType.Equals(SelectInstanceWnd.WndType))
        {
            LPCMapping instanceData = InstanceMgr.GetInstanceInfo(mInstanceId);

            if (instanceData == null)
                return;

            // 重新打开SelectInstanceWnd
            wnd.GetComponent<SelectInstanceWnd>().Bind(instanceData.GetValue<int>("map_id"), SceneMgr.SceneCameraFromPos);
        }
        else
        {
        }
    }

    /// <summary>
    /// 任务按钮点击事件
    /// </summary>
    void OnClickTaskBtn(GameObject go)
    {
        // 正在等待服务器的回执消息
        // 或者正在延迟关闭中
        if (VerifyCmdMgr.IsVerifyCmd("CMD_ENTER_INSTANCE") || IsDelayClosing)
            return;

        // 创建窗口
        GameObject wnd = WindowMgr.OpenWnd(InstanceChallengeTaskWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<InstanceChallengeTaskWnd>().Bind(mInstanceId);
    }

    /// <summary>
    /// 能否开启循环战斗
    /// </summary>
    bool CanOpenLoopFight()
    {
        if (ME.user == null)
            return false;

        if (mMapType.Equals(MapConst.DUNGEONS_MAP_2))
        {
            int dunEndTime = 0;

            // 圣域自动战斗结束时间
            LPCValue dungeons_value = ME.user.Query<LPCValue>("auto_dungeons");
            if (dungeons_value != null && dungeons_value.IsInt)
                dunEndTime = dungeons_value.AsInt;

            if (dunEndTime == 0 || dunEndTime - TimeMgr.GetServerTime() <= 0)
            {
                // 圣域自动战斗已过期
                DialogMgr.ShowDailog(
                    new global::CallBack(OnClickBuyCallBack),
                    LocalizationMgr.Get("SelectFighterWnd_52"),
                    string.Empty, LocalizationMgr.Get("SelectFighterWnd_53"));

                // 显示自动战斗面板
                ShowAutoPanel();

                return false;
            }
        }
        else if (mMapType.Equals(MapConst.TOWER_MAP))
        {
            int towerEndTime = 0;

            // 通天塔自动战斗结束时间
            LPCValue tower_value = ME.user.Query<LPCValue>("auto_tower");
            if (tower_value != null && tower_value.IsInt)
                towerEndTime = tower_value.AsInt;

            if (towerEndTime == 0 || towerEndTime - TimeMgr.GetServerTime() <= 0)
            {
                // 通天塔自动战斗已过期
                DialogMgr.ShowDailog(
                    new global::CallBack(OnClickBuyCallBack),
                    LocalizationMgr.Get("SelectFighterWnd_54"),
                    string.Empty, LocalizationMgr.Get("SelectFighterWnd_53"));

                // 显示自动战斗面板
                ShowAutoPanel();

                return false;
            }
        }
        else
        {
            if (!InstanceMgr.IsAutoLoopFight(mInstanceId))
            {
                LogMgr.Trace("当前副本不允许开启循环战斗");
                return false;
            }

            // 循环战斗功能没有开启
            int canLoop = ME.user.Query<int>("new_function/loop_fight");
            if (canLoop != 1)
            {
                // 打开提示窗口
                WindowMgr.OpenWnd(LoopFightUnlockWnd.WndType);

                return false;
            }
        }

        // 玩家没有选择宠物;
        if (mSelectRidList.Count <= 0)
        {
            DialogMgr.Notify(LocalizationMgr.Get("SelectFighterWnd_3"));
            return false;
        }

        // 副本没有通关
        if (! mMapType.Equals(MapConst.TOWER_MAP) && !InstanceMgr.IsClearanced(ME.user, mInstanceId))
        {
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("SelectFighterWnd_30"),
                string.Empty,
                string.Empty,
                true,
                this.transform
            );
            return false;
        }

        if (mMapType.Equals(MapConst.INSTANCE_MAP_1))
        {
            // 所有攻方宠物全部满级
            if (mIsAllMaxLevel)
            {
                DialogMgr.ShowSingleBtnDailog(
                    null,
                    LocalizationMgr.Get("SelectFighterWnd_31"),
                    LocalizationMgr.Get("SelectFighterWnd_26"),
                    string.Empty,
                    true,
                    this.transform
                );
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// 立即购买点击回调
    /// </summary>
    void OnClickBuyCallBack(object para, params object[] param)
    {
        if (!(bool)param[0])
            return;

        ShowMonthCardWnd();
    }

    /// <summary>
    /// 显示月卡界面
    /// </summary>
    void ShowMonthCardWnd()
    {
        // 打开月卡窗口
        GameObject wnd = WindowMgr.OpenWnd(MonthCardWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return;

        MonthCardWnd script = wnd.GetComponent<MonthCardWnd>();

        // 绑定数据
        script.Bind(ShopConfig.MONTH_CARD_ID);
    }

    /// <summary>
    /// 能否进入副本
    /// </summary>
    bool CanEnterInstance()
    {
        // 没有副本id
        if (string.IsNullOrEmpty(mInstanceId))
            return false;

        // 获取副本配置信息
        LPCMapping instanceInfo = InstanceMgr.GetInstanceInfo(mInstanceId);
        if (instanceInfo == null)
            return false;

        // 地图配置信息
        CsvRow mapConfig = MapMgr.GetMapConfig(instanceInfo.GetValue<int>("map_id"));
        if (mapConfig == null)
            return false;

        // 获取地图类型
        int mapType = mapConfig.Query<int>("map_type");

        // 副本没有解锁
        if (! InstanceMgr.CanEnterInstance(ME.user, mInstanceId, LPCMapping.Empty))
        {
            // 通天塔的提示需要单独提示
            if (mapType == MapConst.TOWER_MAP)
                DialogMgr.Notify(LocalizationMgr.Get("SelectFighterWnd_39"));
            else
                DialogMgr.Notify(LocalizationMgr.Get("SelectFighterWnd_36"));

            return false;
        }

        // 装备包裹已满，无法开始战斗, 只有地下城副本才做包裹格子剩余数量的判断
        if (mapType == MapConst.DUNGEONS_MAP_2 &&
            ContainerMgr.GetFreePosCount(ME.user as Container, ContainerConfig.POS_ITEM_GROUP) <= 0)
        {
            // 显示一个提示框
            DialogMgr.ShowSingleBtnDailog(
                null,
                LocalizationMgr.Get("SelectFighterWnd_35"),
                LocalizationMgr.Get("SelectFighterWnd_34"),
                string.Empty,
                true,
                this.transform
            );

            return false;
        }

        if (mapType.Equals(MapConst.SECRET_DUNGEONS_MAP))
        {
            // 判断是否能够进入秘密圣域
            if (! CanEnterSecretDungeonsMap())
            {
                DialogMgr.ShowSingleBtnDailog(
                    null,
                    LocalizationMgr.Get("DungeonsWnd_27"),
                    string.Empty,
                    string.Empty,
                    true,
                    this.transform
                );
                return false;
            }
        }

        // 提示消耗不足
        if (ME.user.Query<int>(field) < mCost.GetValue<int>(field))
        {
            DialogMgr.ShowDailog(
                new CallBack(CostTipsDialogCallBack),
                LocalizationMgr.Get("SelectFighterWnd_33"),
                string.Format(LocalizationMgr.Get("SelectFighterWnd_32"), FieldsMgr.GetFieldName(field)),
                string.Empty,
                string.Empty,
                true,
                this.transform
            );

            return false;
        }

        return true;
    }

    /// <summary>
    /// 判断是否允许进入秘密地下城
    /// </summary>
    private bool CanEnterSecretDungeonsMap()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return false;

        // 如果地图没有owner标识不需要占有所有人都可以
        string owner = mDynamicData.GetValue<string>("owner");
        if (string.IsNullOrEmpty(owner))
            return true;

        // 判断是自己的圣域, 可以进入
        string userRid = ME.user.GetRid();
        if (string.Equals(userRid, owner))
            return true;

        // 获取holder信息
        // 如果玩家已经占有过该秘密地下城
        LPCArray holder = mDynamicData.GetValue<LPCArray>("holder");
        if (holder == null || holder.IndexOf(userRid) != -1)
            return true;

        // 如果列表已满不允许进入
        LPCMapping maxHolder = GameSettingMgr.GetSetting<LPCMapping>("max_dynamic_map_holder");
        if (holder.Count >= maxHolder.GetValue<int>(mDynamicData.GetValue<int>("type")))
            return false;

        // 可以进入
        return true;
    }

    /// <summary>
    /// 检测出战列表
    /// </summary>
    bool CheckFightList(LPCMapping fightMap)
    {
        if (fightMap == null || fightMap.Count == 0)
            return false;

        if (mFormation.ContainsKey(FormationConst.RAW_NONE))
        {
            if (fightMap[FormationConst.RAW_NONE].AsArray.Count > amount)
            {
                DialogMgr.Notify(LocalizationMgr.Get("SelectFighterWnd_25"));
                return false;
            }
        }
        else
        {
            int frontAmount = fightMap[FormationConst.RAW_FRONT].AsArray.Count;

            int backAmount = fightMap[FormationConst.RAW_BACK].AsArray.Count;

            if (frontAmount + backAmount > amount
                || frontAmount > mFormation[FormationConst.RAW_FRONT].AsInt
                || backAmount > mFormation[FormationConst.RAW_BACK].AsInt)
            {
                DialogMgr.Notify(LocalizationMgr.Get("SelectFighterWnd_25"));

                return false;
            }
        }

        // 玩家没有选择宠物;
        if (mSelectRidList.Count <= 0)
        {
            DialogMgr.Notify(LocalizationMgr.Get("SelectFighterWnd_3"));
            return false;
        }

        // 不能只选择好友宠物
        if (mSelectRidList.Count == 1 && mIsFriendPet)
        {
            DialogMgr.Notify(LocalizationMgr.Get("SelectFighterWnd_3"));
            return false;
        }

        for (int i = 0; i < mCacheList.Count; i++)
        {
            Property ob = mCacheList[i];
            if (ob == null)
                continue;

            /// 材料宠物不允许出战， 部分副本不允许选择仓库中的宠物
            if (!InstanceMgr.IsFillLevelLimit(ob.GetLevel(), mInstanceId)
                || MonsterMgr.IsMaterialMonster(ob.GetClassID())
                || (! InstanceMgr.IsSelectStorePet(mInstanceId) && MonsterMgr.IsStorePet(ob)))
            {
                DialogMgr.Notify(string.Format(LocalizationMgr.Get("SelectFighterWnd_25")));
                return false;
            }
        }

        return true;
    }

    /// <summary>
    ///点击进入战斗场景
    /// </summary>
    void ClickEnterCombatScene(GameObject go)
    {
        // 正在等待服务器的回执消息
        // 或者正在延迟关闭中
        if (VerifyCmdMgr.IsVerifyCmd("CMD_ENTER_INSTANCE") || IsDelayClosing)
            return;

        if (mIsLoopFight)
            mIsCountDown = false;

        // 执行进入副本操作
        DoEnterInstance();
    }

    /// <summary>
    /// 执行进入副本
    /// </summary>
    void DoEnterInstance()
    {
        // 正在等待服务器的回执消息
        // 或者正在延迟关闭中
        if (VerifyCmdMgr.IsVerifyCmd("CMD_ENTER_INSTANCE") || IsDelayClosing)
            return;

        // 不能进入副本
        if (!mIsLoopFight && !CanEnterInstance())
            return;

        // 获取副本地图类型
        int mapType = InstanceMgr.GetMapTypeByInstanceId(mInstanceId);

        // 反击以cookie作为一条数据的唯一id，对战则以玩家rid作为唯一id
        string opponentId = string.Empty;
        if (mapType == MapConst.ARENA_REVENGE_MAP)
            opponentId = mDefenseData.GetValue<string>("cookie", string.Empty);
        else
            opponentId = mDefenseData.GetValue<string>("rid", string.Empty);

        LPCMapping extraPara = LPCMapping.Empty;

        // 添加竞技场对手信息
        if (! string.IsNullOrEmpty(opponentId))
            extraPara.Add("opponent_id", opponentId);

        // 增加动态副本dynamic_id
        if (mDynamicData != null && mDynamicData.ContainsKey("dynamic_id"))
            extraPara.Add("dynamic_id", mDynamicData.GetValue<string>("dynamic_id"));

        if (mapType.Equals(MapConst.INSTANCE_MAP_1))
        {
            // 副本难度
            int difficulty = InstanceMgr.GetInstanceInfo(mInstanceId).GetValue<int>("difficulty");

            // 对应的地图id
            int mapId = InstanceMgr.GetInstanceInfo(mInstanceId).GetValue<int>("map_id");
            LPCValue v = OptionMgr.GetOption(ME.user, "instance_difficulty");
            LPCMapping data = LPCMapping.Empty;

            if (v != null && v.IsMapping)
                data = v.AsMapping;

            data.Add(mapId, difficulty);

            // 缓存副本难度到本地
            OptionMgr.SetOption(ME.user, "instance_difficulty", LPCValue.Create(data));
        }

        // 收集出战宠物
        mFightMap = DoGatherFighter();

        // 检测出战列表
        if (!CheckFightList(mFightMap))
        {
            if (mIsLoopFight)
            {
                mAutoFight.value = false;

                // 标识停止循环战斗
                mIsCountDown = false;

                // 恢复循环战斗提示信息
                ShowAutoPanel();
            }

            return;
        }

        //检测阵容是否改变
        if (CheckTeamChange())
            InstanceMgr.IsTipsLeaderSkill = false;

        // 有队长技能，但是队长位置没有队长技能(1.循环战斗不提示 2.新手引导不提示 3.阵容改变需要重新提示)
        if (!InstanceMgr.IsTipsLeaderSkill && mIsHasLeaderSkill && !mNoSelectLeaderSkill && !mIsLoopFight && !GuideMgr.IsGuiding())
        {
            // 构造数据
            LPCMapping para = LPCMapping.Empty;
            para.Add("fight_map", mFightMap);
            para.Add("extra_para", extraPara);

            // 提示玩家没有选择队长
            DialogMgr.ShowDailog(
                new CallBack(OnLeaderTipsDialogCallBack, para),
                LocalizationMgr.Get("SelectFighterWnd_22"),
                string.Empty,
                LocalizationMgr.Get("SelectFighterWnd_23"),
                string.Empty,
                true,
                this.transform
            );

            return;
        }

        // 打开副本Load界面
        InstanceMgr.OpenInstance(ME.user, mInstanceId, Rid.New());

        // 进入副本
        InstanceMgr.EnterInstance(ME.user, mInstanceId, GetLeaderRid(), mIsLoopFight, mFightMap, extraPara);

        // 设置阵容缓存信息
        FormationMgr.SetArchiveFormation(mInstanceId, mCacheList);

        return;
    }

    /// <summary>
    /// 检测阵容是否改变
    /// </summary>
    private bool CheckTeamChange()
    {
        //阵容改变需要重新开启队长技能提示
        bool isChange = false;
        for (int i = 0; i < mCacheList.Count; i++)
        {
            if (mCacheList[i] == null)
            {
                if (mArchiveFormation.Count <= i || mArchiveFormation[i] != null)
                {
                    isChange = true;
                    break;
                }
            }
            else
            {
                if (mArchiveFormation.Count <= i || mArchiveFormation[i] == null || !mArchiveFormation[i].GetRid().Equals(mCacheList[i].GetRid()))
                {
                    isChange = true;
                    break;
                }
            }
        }

        return isChange;
    }

    string GetLeaderRid()
    {
        for (int i = 0; i < mPlayerSelectList.Count; i++)
        {
            PetItemWnd script = mPlayerSelectList[i];
            if (script == null)
                continue;

            if (script.mIsLeader)
                return script.item_ob == null ? string.Empty : script.item_ob.GetRid();
        }

        return string.Empty;
    }

    /// <summary>
    /// 消耗不足提示弹框回调
    /// </summary>
    void CostTipsDialogCallBack(object para, params object[] param)
    {
        if (! (bool)param[0])
            return;

        // 通关兰达平原普通所有副本
        if (! GuideMgr.IsGuided(4))
        {
            DialogMgr.Notify(LocalizationMgr.Get("GuideWnd_1"));

            return;
        }

        // 打开快捷购买窗口
        GameObject wnd = WindowMgr.OpenWnd(QuickMarketWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        LPCMapping instanceConfig = InstanceMgr.GetInstanceInfo(mInstanceId);
        if (instanceConfig == null)
            return;

        CsvRow mapConfig = MapMgr.GetMapConfig(instanceConfig.GetValue<int>("map_id"));
        if (mapConfig == null)
            return;

        // 地图类型
        int mapType = mapConfig.Query<int>("map_type");

        string group = string.Empty;

        if (mapType.Equals(MapConst.ARENA_MAP)
            || mapType.Equals(MapConst.ARENA_NPC_MAP)
            || mapType.Equals(MapConst.ARENA_REVENGE_MAP))
        {
            group = ShopConfig.AP_GROUP;
        }
        else
        {
            group = ShopConfig.LIFE_GROUP;
        }

        wnd.GetComponent<QuickMarketWnd>().Bind(group);
    }

    void OnLeaderTipsDialogCallBack(object para, params object[] param)
    {
        //取消
        if (!(bool)param[0])
        {
            if (mIsLoopFight)
            {
                mAutoFight.value = false;

                mIsCountDown = false;

                // 显示自动战斗面板
                ShowAutoPanel();
            }

            return;
        }

        // 标记已经提示过
        InstanceMgr.IsTipsLeaderSkill = true;

        LPCMapping extraPara = para as LPCMapping;
        if (extraPara == null)
            return;

        // 打开副本Load界面
        InstanceMgr.OpenInstance(ME.user, mInstanceId, Rid.New());

        // 进入副本
        InstanceMgr.EnterInstance(ME.user, mInstanceId, GetLeaderRid(), mIsLoopFight, extraPara.GetValue<LPCMapping>("fight_map"), extraPara.GetValue<LPCMapping>("extra_para"));

        // 设置阵容缓存信息
        FormationMgr.SetArchiveFormation(mInstanceId, mCacheList);
    }

    /// <summary>
    /// 收集出战宠物
    /// </summary>
    LPCMapping DoGatherFighter()
    {
        // 是否是前排的
        bool isFront = false;

        // 是否区分前后排
        bool isRawNone = true;

        mIsFriendPet = false;

        LPCMapping fightMap = LPCMapping.Empty;

        // 清空列表
        mCacheList.Clear();

        if (!mFormation.ContainsKey(FormationConst.RAW_NONE))
        {
            fightMap.Add(FormationConst.RAW_FRONT, LPCArray.Empty);
            fightMap.Add(FormationConst.RAW_BACK, LPCArray.Empty);

            isRawNone = false;
        }
        else
        {
            fightMap.Add(FormationConst.RAW_NONE, LPCArray.Empty);

            isRawNone = true;
        }

        mIsHasLeaderSkill = false;

        for (int i = 0; i < mPlayerSelectList.Count; i++)
        {
            Property ob = mPlayerSelectList[i].item_ob;

            // 区分前后
            if (! isRawNone)
            {
                if (i + 1 <= mFormation.GetValue<int>(FormationConst.RAW_FRONT))
                    isFront = true;
                else
                    isFront = false;
            }

            //没有宠物数据;
            if (ob == null)
            {
                mCacheList.Add(null);

                if (isRawNone)
                {
                    fightMap[FormationConst.RAW_NONE].AsArray.Add(string.Empty);
                }
                else
                {
                    if (isFront)
                        fightMap[FormationConst.RAW_FRONT].AsArray.Add(string.Empty);
                    else
                        fightMap[FormationConst.RAW_BACK].AsArray.Add(string.Empty);
                }

                continue;
            }

            if (!MonsterMgr.IsMaxLevel(ob))
                mIsAllMaxLevel = false;

            string petRid = ob.GetRid();
            if (mFriendPetWnd.mData.ContainsKey(petRid))
            {
                mCacheList.Add(null);

                LPCMapping sharePetData = LPCMapping.Empty;

                mIsFriendPet = true;

                if (mFriendPetWnd.mData.ContainsKey(petRid))
                    sharePetData.Add("friend_rid", mFriendPetWnd.mData[petRid].GetValue<string>("rid"));

                if (isRawNone)
                {
                    fightMap[FormationConst.RAW_NONE].AsArray.Add(sharePetData);
                }
                else
                {
                    if (isFront)
                        fightMap[FormationConst.RAW_FRONT].AsArray.Add(sharePetData);
                    else
                        fightMap[FormationConst.RAW_BACK].AsArray.Add(sharePetData);
                }

                // 好友宠物不需要缓存出站记录
                continue;
            }
            else
            {
                if (isRawNone)
                {
                    fightMap[FormationConst.RAW_NONE].AsArray.Add(petRid);
                }
                else
                {
                    if (isFront)
                        fightMap[FormationConst.RAW_FRONT].AsArray.Add(petRid);
                    else
                        fightMap[FormationConst.RAW_BACK].AsArray.Add(petRid);
                }
            }

            if (!mIsHasLeaderSkill)
            {
                // 获取队长技能
                LPCMapping skillData = SkillMgr.GetLeaderSkill(ob);
                if (skillData != null && skillData.Count != 0)
                    mIsHasLeaderSkill = true;
            }

            mCacheList.Add(ob);
        }

        return fightMap;
    }

    /// <summary>
    ///显示宠物信息
    /// </summary>
    void ShowInfo()
    {
        // 当前处于滑动列表中
        if (mPlayerPetWnd.ScrollView.isDragging || mFriendPetWnd.ScrollView.isDragging)
            return;

        // 没有选择对象
        if (mPressOb == null)
            return;

        // 副本配置信息
        LPCMapping configInstance = InstanceMgr.GetInstanceInfo(mInstanceId);
        if (configInstance == null)
            return;

        // 地图配置信息
        CsvRow mapConfig = MapMgr.GetMapConfig(configInstance.GetValue<int>("map_id"));
        if (mapConfig == null)
            return;

        if (mDragDropPetItem != null)
            mDragDropPetItem.enabled = false;

        if (mapConfig.Query<int>("map_type").Equals(MapConst.DUNGEONS_MAP_2))
        {
            if (mPressOb.Query<int>("is_boss") == 1)
            {
                // 显示boss的宠物信息界面
                ShowBossInfo();
            }
            else
            {
                ShowPetInfo();
            }
        }
        else
        {
            ShowPetInfo();
        }
    }

    void ShowPetInfo()
    {
        // 玩家的宠物
        if (IsUserPet())
        {
            ShowPetInfo(ME.user.GetName(), ME.user.GetLevel());
        }
        else
        {
            if (IsFriendPet())
            {
                // 好友的信息
                LPCMapping friendData = mFriendPetWnd.mData[mPressOb.GetRid()];
                ShowPetInfo(friendData.GetValue<string>("name"), friendData.GetValue<int>("level"));
            }
            else
            {
                // 显示简单的宠物信息
                ShowSimplePetInfo();
            }
        }
    }

    bool IsUserPet()
    {
        foreach (Property item in mPlayerPetWnd.mPetData)
        {
            if (item == null)
                continue;

            if (item.GetRid().Equals(mPressOb.GetRid()))
                return true;
        }

        return false;
    }

    /// <summary>
    /// 是否是好友宠物
    /// </summary>
    bool IsFriendPet()
    {
        foreach (Property item in mFriendPetWnd.mPetData)
        {
            if (item == null)
                continue;

            if (item.GetRid().Equals(mPressOb.GetRid()))
                return true;
        }

        return false;
    }

    void ShowPetInfo(string name, int level)
    {
        // 获得宠物信息窗口
        GameObject wnd = WindowMgr.OpenWnd(PetInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);

        if (wnd == null)
            return;

        PetInfoWnd script = wnd.GetComponent<PetInfoWnd>();
        if (script == null)
            return;

        script.Bind(mPressOb.GetRid(), name, level);

        script.SetCallBack(new CallBack(OnDragCallBack));
    }

    /// <summary>
    /// 显示简单的宠物信息界面
    /// </summary>
    void ShowSimplePetInfo()
    {
        // 获得宠物信息窗口
        GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();
        if (script == null)
            return;

        script.Bind(mPressOb);

        script.ShowBtn(true);

        wnd.transform.localPosition = Vector3.zero;
    }

    void OnDragCallBack(object para, params object[] _params)
    {
        if (mCurWnd == null)
            return;

        if (mDragDropPetItem == null)
            return;

        mDragDropPetItem.enabled = true;
    }

    /// <summary>
    /// 显示boss信息
    /// </summary>
    void ShowBossInfo()
    {
        GameObject wnd = WindowMgr.OpenWnd(BossInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<BossInfoWnd>().Bind(mPressOb);
    }

    /// <summary>
    ///设置宠物格子的背景图片，
    /// 目的是为了只存在一个格子
    /// 有透明度动画的效果,
    /// 其他格子为默认的背景，没有动画效果
    /// </summary>
    void SetSpriteEffect(int index)
    {
        //设置背景的透明度动画;
        mPlayerSelectList[index].mBg.gameObject.GetComponent<TweenAlpha>().enabled = true;

        mPlayerSelectList[index].mBg.gameObject.GetComponent<TweenAlpha>().ResetToBeginning();

        for (int j = 0; j < mPlayerSelectList.Count; j++)
        {
            if (mPlayerSelectList[j].item_ob != null)
            {
                mPlayerSelectList[j].mBg.spriteName = "PetIconBg";
                mPlayerSelectList[j].mBg.gameObject.GetComponent<TweenAlpha>().enabled = false;

                //设置alpha值为255,表示图片不透明;
                mPlayerSelectList[j].mBg.GetComponent<UISprite>().alpha = 255;
                continue;
            }

            if (index == j)
                continue;

            mPlayerSelectList[j].mBg.spriteName = "PetIconBg";
            mPlayerSelectList[j].mBg.gameObject.GetComponent<TweenAlpha>().enabled = false;

            //设置alpha值为255,表示图片不透明;
            mPlayerSelectList[j].mBg.GetComponent<UISprite>().alpha = 255;
        }
    }

    /// <summary>
    ///初始化界面上label的内容
    /// </summary>
    void SetLabelContent()
    {
        mLeaveLabel.text = LocalizationMgr.Get("SelectFighterWnd_2");
        mTaskLabel.text = LocalizationMgr.Get("SelectFighterWnd_6");
        mStartFighter.text = LocalizationMgr.Get("SelectFighterWnd_1");
        mSetLeaderBtnLb.text = LocalizationMgr.Get("SelectFighterWnd_18");
        mFrontLb.text = LocalizationMgr.Get("SelectFighterWnd_19");
        mBackLb.text = LocalizationMgr.Get("SelectFighterWnd_20");
        mSetLeaderDesc.text = LocalizationMgr.Get("SelectFighterWnd_21");

        //获取副本所有的配置信息;
        LPCMapping instance = InstanceMgr.GetInstanceInfo(mInstanceId);

        //没有该副本信息;
        if (instance == null)
            return;

        if (mDynamicData != null && mDynamicData.Count > 0)
        {
            if (mDynamicData.ContainsKey("owner_name"))
                mInstanceName.text = string.Format(LocalizationMgr.Get("SelectFighterWnd_14"), mDynamicData.GetValue<string>("owner_name"));
            else
                mInstanceName.text = InstanceMgr.GetInstanceName(mInstanceId, mDynamicData);
        }
        else
            mInstanceName.text = string.Format(LocalizationMgr.Get("SelectFighterWnd_12"), LocalizationMgr.Get(instance["name"].AsString));

        if (!mInstanceName.gameObject.activeSelf)
            mInstanceName.gameObject.SetActive(true);
    }

    #endregion

    #region 外部接口

    public void CancelLoopFight()
    {
        mAutoFight.value = false;
    }

    /// <summary>
    /// 循环战斗按钮点击事件
    /// </summary>
    public void OnClickLoopFightBtn()
    {
        mIsLoopFight = mAutoFight.value;

        mIsAllMaxLevel = true;

        // 取消自动循环
        if (!mIsLoopFight)
        {
            mIsCountDown = false;

            // 显示自动战斗面板
            ShowAutoPanel();

            return;
        }

        // 收集战斗列表
        mFightMap = DoGatherFighter();

        // 不能开启循环战斗或者不能进入副本
        if (! CanOpenLoopFight() || ! CanEnterInstance())
        {
            mAutoFight.value = false;
            mIsLoopFight = mAutoFight.value;
            return;
        }

        mIsCountDown = true;

        // 倒计时开启循环战斗
        mRemainTime = GameSettingMgr.GetSettingInt("loop_fight_count_down");
    }

    /// <summary>
    /// 设置玩家阵容中宠物格子的标签
    /// </summary>
    public void SetFormationPetItemTag(GameObject petItem)
    {
        PetItemWnd scriptOb = petItem.GetComponent<PetItemWnd>();
        if (scriptOb == null)
            return;

        if (scriptOb.item_ob == null)
            petItem.tag = "NullGrid";
        else
            scriptOb.tag = "FormationGrid";
    }

    /// <summary>
    /// 点击取消阵容中的宠物
    /// </summary>
    public void ClickCancelFighterPet(GameObject go)
    {
        PetItemWnd petItemWnd = go.GetComponent<PetItemWnd>();
        if (mIsSetLeader)
        {
            mIsSetLeader = false;

            mSetLeaderMask.SetActive(false);

            for (int i = 0; i < mPlayerSelectList.Count; i++)
                mPlayerSelectList[i].ShowLeaderIcon(false);

            // 刷新队长技能描述
            LeaderSkillEffectDesc(go.GetComponent<PetItemWnd>().item_ob);

            petItemWnd.ShowLeaderIcon(true);

            if (mPos.ContainsKey(go.name))
                CacheLeaderPos(mPos[go.name]);

            return;
        }

        // 没有宠物数据;
        if (petItemWnd.item_ob == null)
            return;

        // 获取点击物体的Rid;
        string rid = petItemWnd.item_ob.GetRid();

        for (int i = 0; i < mPlayerSelectList.Count; i++)
        {
            // 没有宠物数据;
            if (mPlayerSelectList[i].item_ob == null)
                continue;

            if (!mPlayerSelectList[i].item_ob.GetRid().Equals(rid))
                continue;

            if (mSelectRidList != null && mSelectRidList.Count > 0)
            {
                // 移除缓存中取消的宠物rid
                if (mSelectRidList.Contains(rid))
                    mSelectRidList.Remove(rid);

                mSelectClassIdList.Remove(petItemWnd.item_ob.GetClassID());
            }

            // 设置队长技能效果描述;
            if (i == 0 && mFormation.ContainsKey(FormationConst.RAW_NONE))
                LeaderSkillEffectDesc(null);

            mPlayerSelectList[i].SetBind(null);

            // 设置宠物格子标签
            SetFormationPetItemTag(mPlayerSelectList[i].gameObject);

            UpdateShadow();
            break;
        }

        if (mIsLoopFight)
            OnClickLoopFightBtn();

        if (mFriendPetWnd.mData.ContainsKey(rid))
            mFriendPetWnd.mFightAmount--;

        if (amount == mSelectRidList.Count + 1)
        {
            // 刷新数据
            mPlayerPetWnd.RefreshData();
            mFriendPetWnd.RefreshData();
        }

        for (int i = 0; i < arrPet.Count; i++)
        {
            // 没有宠物数据;
            if (arrPet[i].item_ob == null)
                continue;

            // 取消选中状态;
            if (!arrPet[i].item_ob.GetRid().Equals(rid))
                continue;

            arrPet[i].SetSelected(false, false);

            if (mFriendPetWnd.mData.ContainsKey(rid))
                UIEventListener.Get(arrPet[i].gameObject).onClick = mFriendPetWnd.ClickSelectPet;
            else
                UIEventListener.Get(arrPet[i].gameObject).onClick = mPlayerPetWnd.ClickSelectPet;
        }

        // 刷新队长技能描述
        RefreshLeaderDesc();
    }

    public void OnClickShowPetInfo(GameObject go)
    {
        //没有宠物;
        if (go.GetComponent<PetItemWnd>().item_ob == null)
            return;

        mPressOb = go.GetComponent<PetItemWnd>().item_ob;

        // 显示宠物信息
        ShowInfo();
    }

    /// <summary>
    ///长按显示宠物信息
    /// </summary>
    public void OnPressShowPetInfo(GameObject go, bool isPress)
    {
        //玩家正在滑动宠物列表;
        if (mPlayerPetWnd.ScrollView.isDragging || mFriendPetWnd.ScrollView.isDragging)
            return;

        mDragDropPetItem = go.GetComponent<DragDropPetItem>();

        //手指抬起时
        if (!isPress)
        {
            mPressOb = null;
            return;
        }

        //没有宠物;
        if (go.GetComponent<PetItemWnd>().item_ob == null)
            return;

        mPressOb = go.GetComponent<PetItemWnd>().item_ob;
        CancelInvoke("ShowInfo");

        if (mIsInvoke)
        {
            //0.5秒后显示宠物信息界面;
            Invoke("ShowInfo", 0.5f);
        }
    }

    /// <summary>
    ///设置阵容中下一个位置没有英雄时的效果
    /// </summary>
    public void UpdateShadow()
    {
        for (int i = 0; i < mPlayerSelectList.Count; i++)
        {
            // 此位置有宠物;
            if (mPlayerSelectList[i].item_ob != null)
            {
                mPlayerSelectList[i].mBg.spriteName = "PetIconBg";
                mPlayerSelectList[i].mBg.gameObject.GetComponent<TweenAlpha>().enabled = false;

                mPlayerSelectList[i].ShowLeaderText(string.Empty);

                continue;
            }

            if (i == 0)
            {
                mPlayerSelectList[i].mBg.spriteName = "PetSelectBg";

                // 设置队长位置图标
                if (mFormation.ContainsKey(FormationConst.RAW_NONE))
                    mPlayerSelectList[i].ShowLeaderText(LocalizationMgr.Get("SkillItemWnd_1"));

                SetSpriteEffect(i);
            }
            else
            {
                mPlayerSelectList[i].mBg.spriteName = "PetSelectBg";
                SetSpriteEffect(i);
            }
            break;
        }
    }

    /// <summary>
    ///设置宠物的选中状态
    /// </summary>
    public void SetPetSelectState(GameObject go)
    {
        PetItemWnd petItem = go.GetComponent<PetItemWnd>();

        // 没有宠物信息;
        if (petItem == null ||
            petItem.item_ob == null)
            return;

        // 设置选中状态;
        if (petItem.isSelected)
            petItem.SetSelected(false, false);
        else
            petItem.SetSelected(true);
    }

    /// <summary>
    /// 刷新队长技能描述
    /// </summary>
    public void RefreshLeaderDesc()
    {
        for (int i = 0; i < mPlayerSelectList.Count; i++)
        {
            PetItemWnd script = mPlayerSelectList[i];

            if (script == null)
                continue;

            if (!script.mIsLeader)
                continue;

            // 设置队长技能描述
            LeaderSkillEffectDesc(script.item_ob);
        }
    }

    /// <summary>
    ///显示队长技能效果描述
    /// </summary>
    public void LeaderSkillEffectDesc(Property pro)
    {
        string desc = string.Empty;

        if (string.IsNullOrEmpty(SkillMgr.GetLeaderSkillDesc(pro)))
            desc = string.Format("[adffa4]{0}[-][ffffff]{1}[-]", LocalizationMgr.Get("SelectFighterWnd_10"), LocalizationMgr.Get("SelectFighterWnd_11"));
        else
            desc = string.Format("[adffa4]{0}{1}[-]", LocalizationMgr.Get("SelectFighterWnd_10"), SkillMgr.GetLeaderSkillDesc(pro));

        mLeaderSkillDesc.text = desc;

        // 获取队长技能
        LPCMapping skillData = SkillMgr.GetLeaderSkill(pro);
        if (skillData == null || skillData.Count == 0)
            mNoSelectLeaderSkill = false;
        else
            mNoSelectLeaderSkill = true;
    }

    /// <summary>
    /// 添加克隆的物件对象
    /// </summary>
    public void AddCacheOb(Property ob)
    {
        if (ob == null)
            return;

        if (mCacheCreateOb == null)
            mCacheCreateOb = new List<Property>();

        if (mCacheCreateOb.Contains(ob))
            mCacheCreateOb.Remove(ob);

        mCacheCreateOb.Add(ob);
    }

    /// <summary>
    /// 析构克隆的宠物对象
    /// </summary>
    public void DestroyProperty()
    {
        if (mCacheCreateOb == null)
            return;
        for (int i = 0; i < mCacheCreateOb.Count; i++)
        {
            if (mCacheCreateOb[i] == null)
                continue;

            // 析构创建的宠物对象
            mCacheCreateOb[i].Destroy();
        }
    }

    public void CallBack(object para, params object[] param)
    {
        if ((bool)param[0])
        {
            CancelInvoke("ShowInfo");
            mIsInvoke = false;
        }
        else
        {
            mIsInvoke = true;
        }
    }

    /// <summary>
    /// 绑定数据,不需要的参数直接传null
    /// </summary>
    public void Bind(string WndName, string InstanceId, LPCMapping data = null, bool isAutoLoop = false)
    {
        mWndType = WndName;

        this.mInstanceId = InstanceId;

        LPCMapping instanceInfo = InstanceMgr.GetInstanceInfo(InstanceId);
        if (instanceInfo == null)
            return;

        CsvRow mapInfo = MapMgr.GetMapConfig(instanceInfo.GetValue<int>("map_id"));
        if (mapInfo == null)
            return;

        // 地图类型
        int mapType = mapInfo.Query<int>("map_type");

        // 获取缓存的阵容信息;
        mArchiveFormation = FormationMgr.GetArchiveFormation(InstanceId);

        petInfoList.Clear();

        mPlayerSelectList.Clear();

        // 竞技场地图
        if (mapType.Equals(MapConst.ARENA_MAP) || mapType.Equals(MapConst.ARENA_NPC_MAP) || mapType.Equals(MapConst.ARENA_REVENGE_MAP))
        {
            mDefenseData = data;

            // 获取防守方宠物列表
            LPCArray defenseList = data.GetValue<LPCArray>("defense_list");
            if (defenseList != null || defenseList.Count > 0)
            {
                foreach (LPCValue item in defenseList.Values)
                    petInfoList.Add(item.AsMapping);
            }
        }
        else if (mapType.Equals(MapConst.PET_DUNGEONS_MAP))
        {
            List<CsvRow> instanceConfig = InstanceMgr.GetInstanceFormation(mInstanceId);
            if (instanceConfig == null)
                return;

            if (data == null)
                return;

            // 记录动态副本数据
            mDynamicData = data;

            foreach (CsvRow item in instanceConfig)
            {
                LPCMapping para = new LPCMapping();
                para.Add("rid", Rid.New());

                // 获取始化参数;
                int initScript = item.Query<int>("init_script");
                LPCMapping initArgs = ScriptMgr.Call(initScript, ME.user.GetLevel(),
                    item.Query<LPCValue>("init_script_args"), para) as LPCMapping;

                // 构建宠物属性
                para.Append(initArgs);
                para.Append(InstanceMgr.GetPetDungeonAttrib(
                    data.GetValue<int>("pet_id"),
                    mInstanceId,
                    initArgs.GetValue<int>("batch"),
                    initArgs.GetValue<int>("pos")));

                // 添加列表
                petInfoList.Add(para);
            }
        }
        else if (mapType.Equals(MapConst.SECRET_DUNGEONS_MAP))
        {
            List<CsvRow> instanceConfig = InstanceMgr.GetInstanceFormation(mInstanceId);
            if (instanceConfig == null)
                return;

            if (data == null)
                return;

            // 记录动态副本数据
            mDynamicData = data;

            foreach (CsvRow item in instanceConfig)
            {
                LPCMapping para = new LPCMapping();

                // 调用脚本参数计算怪物class_id;
                int classIdScript = item.Query<int>("class_id_script");
                int classId = (int) ScriptMgr.Call(classIdScript, ME.user.GetLevel(), data.GetValue<LPCValue>("pet_id"));

                para.Add("rid", Rid.New());
                para.Add("class_id", classId);

                // 获取始化参数;
                int initScript = item.Query<int>("init_script");
                LPCMapping initArgs = ScriptMgr.Call(initScript, ME.user.GetLevel(),
                    item.Query<LPCValue>("init_script_args"), para) as LPCMapping;

                // 获取始化参数
                para.Append(initArgs);

                // 添加列表
                petInfoList.Add(para);
            }
        }
        else
        {
            //获取副本配置信息;
            List<CsvRow> instanceConfig = InstanceMgr.GetInstanceFormation(mInstanceId);
            if (instanceConfig == null)
                return;

            foreach (CsvRow item in instanceConfig)
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
                petInfoList.Add(para);
            }
        }

        mIsLoopFight = isAutoLoop;

        //初始化窗口;
        Redraw();

        mPlayerPetWnd.gameObject.SetActive(true);
        mFriendPetWnd.gameObject.SetActive(true);
    }

    /// <summary>
    /// 通天之塔绑定数据
    /// </summary>
    public void TowerBind(string WndName, string InstanceId, int layer, int difficulty, bool isAutoLoop = false)
    {
        mWndType = WndName;
        this.mInstanceId = InstanceId;

        petInfoList.Clear();

        mPlayerSelectList.Clear();

        // 获取缓存的阵容信息;
        mArchiveFormation = FormationMgr.GetArchiveFormation(InstanceId);

        int batch = 0;

        //获取副本配置信息;
        List<CsvRow> resources = TowerMgr.GetTowerBossLevelResources(difficulty, layer, out batch);

        if (resources != null)
        {
            foreach (CsvRow item in resources)
            {
                LPCMapping para = new LPCMapping();

                // 调用脚本参数计算怪物class_id;
                int classIdScript = item.Query<int>("class_id_script");
                int classId = (int) ScriptMgr.Call(classIdScript, ME.user.GetLevel(),
                    item.Query<LPCValue>("class_id_args"));

                para.Add("rid", Rid.New());
                para.Add("class_id", classId);
                para.Add("difficulty", difficulty);
                para.Add("layer", layer);
                para.Add("batch", batch);

                // 获取始化参数;
                int initScript = item.Query<int>("init_script");
                LPCMapping initArgs = ScriptMgr.Call(initScript, ME.user.GetLevel(),
                    item.Query<LPCValue>("init_script_args"), para) as LPCMapping;

                // 获取始化参数
                para.Append(initArgs);

                // 替换掉初始化规则
                para.Add("init_rule", "tower_monster_show");

                // 添加列表
                petInfoList.Add(para);
            }
        }

        mIsLoopFight = isAutoLoop;

        //初始化窗口;
        Redraw();

        mPlayerPetWnd.gameObject.SetActive(true);
        mFriendPetWnd.gameObject.SetActive(true);
    }

    // 指引玩家进入战斗
    public void GuideClickEnterBattle()
    {
        ClickEnterCombatScene(enterBtn);
    }

    #endregion
}
