/// <summary>
/// CombatWindow.cs
/// Created by xuhd Nov/13/2014
/// 战斗场景的窗口，包含三个子窗口
/// 负责提供获取子窗口的接口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

/// <summary>
/// 战斗场景的窗口，包含三个子窗口
/// </summary>
public class CombatWnd : WindowBase<CombatWnd>
{
    #region 成员变量

    // 窗口子控件
    public UILabel mPetInfo;
    public UISprite[] mPetMpGroup;
    public UISprite mSkillBg;
    public GameObject[] mSkillGroup;
    public GameObject mTarget;
    public GameObject mTargetTip;
    public GameObject mSkillViewWnd;
    public GameObject mCombatSet;
    public GameObject mPause;
    public GameObject mAddSpeed;
    public UISprite spAdd;
    public UISprite spPause;
    public GameObject mSkillWnd;

    public GameObject mPauseWnd;

    public CombatSetWnd mCombatSetWnd;

    public GameObject mInstanceDescWnd;
    public UILabel mInstanceDesc;

    // 回合提示动画
    public TweenScale mRoundTipsScale;
    public UIPanel mRoundTipsWnd;

    // 回合提示
    public UILabel mRoundTips;
    public UISprite mRoundTipsBg;

    public UILabel mMp;

    public GameObject mCombatInfo;

    private TweenAlpha[] mRoundTipsAlphas;

    private string petRid = string.Empty;

    // 回合行动参数
    private LPCMapping roundArgs = LPCMapping.Empty;

    private Char target;

    private Vector3 curTipPos = Vector3.one;

    // 选中的技能id
    private int selskillId = -1;

    // 记录点击相关数据
    private Vector3 mTouchPosition = Vector3.zero;
    private bool mIsTouched = false;

#if (UNITY_ANDROID || UNITY_IPHONE) && ! UNITY_EDITOR
    private int curTouchId = -1;
#endif

    // 技能选中提示列表
    private List<GameObject> TargetList = new List<GameObject>();

    // 技能窗口宽度和大小信息
    private int mSkillBgLength = 785;
    private int mSkillItemSize = 120;

    private bool mIsPlayback = false;

    private LPCMapping mInstance;

    bool mIsLoopFight = false;

    private string mInstanceId;

    // 窗口绑定对象
    public Property OwnerOb { get; private set; }

    #endregion

    /// <summary>
    /// Awake this instance.
    /// </summary>
    void Awake()
    {
        // 注册等待指令事件
        EventMgr.RegisterEvent("CombatWnd", EventMgrEventType.EVENT_WAIT_COMMAND, WhenRoundCombat);

        // 获取所有的TweenAlpha组件
        mRoundTipsAlphas = mRoundTipsWnd.gameObject.GetComponents<TweenAlpha>();
    }

    /// <summary>
    /// Raises the enable event.
    /// </summary>
    void OnEnable()
    {
        //// 如果这个时候MainWnd存在处于显示中
        //// 定位bug用PQTC-868
        GameObject gameOb = WindowMgr.GetWindow(MainWnd.WndType);

        // 如果当前窗口处于显示状态
        if (gameOb == null || ! gameOb.activeSelf)
            return;

        // 战斗场景中关闭，自动检查关闭主界面
        WindowMgr.HideWindow(gameOb);

        // 获取GameObject MainWnd组件
        MainWnd wnd = gameOb.GetComponent<MainWnd>();
        if (wnd == null)
            return;

        // 向服务器上传MainWnd打开是调用栈信息
        LogMgr.Error("Mainwnd in Combat Scene (CombatWnd)\n{0}", wnd.OnEnableStackTrace);
    }

    // Use this for initialization
    void Start()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCombatSet).onClick += OnCombatSet;
        UIEventListener.Get(mAddSpeed).onClick += OnAddSpeed;
        UIEventListener.Get(mPause).onClick += OnAutoCombat;

        // 注册战斗回放结束事件
        EventMgr.RegisterEvent("CombatWnd", EventMgrEventType.EVENT_PLAYBACK_END, OnPlaybackEnd);

        //注册副本通关事件;
        EventMgr.RegisterEvent("CombatWnd", EventMgrEventType.EVENT_INSTANCE_CLEARANCE, OnInstanceClearance);

        //注册副本是否复活提示事件;
        EventMgr.RegisterEvent("CombatWnd", EventMgrEventType.EVENT_REVIVE_TIPS, OnReviveTips);

        // 注册战斗回合开始事件
        EventMgr.RegisterEvent("CombatWnd", EventMgrEventType.EVENT_COMBAT_ROUND_START, OnRoundStart);

        // 监听msg_arena_challenge_bonus
        MsgMgr.RegisterDoneHook("MSG_ARENA_CHALLENGE_BONUS", "CombatWnd", OnArenaChallengeBonusMsg);

        // 注册按钮事件
        for (int i = 0; i < mSkillGroup.Length; i++)
            UIEventListener.Get(mSkillGroup[i]).onPress = OnSkillItemPress;

        // 绘制窗口
        Redraw();

        // 当前窗口打开抛出事件
        EventMgr.FireEvent(EventMgrEventType.EVENT_OPEN_COMBATWND, null);

        mMp.text = LocalizationMgr.Get("CombatWnd_6");
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        //解注册副本通关事件;
        EventMgr.UnregisterEvent("CombatWnd");

        // 取消暂停
        TimeMgr.DoContinueCombatLogic("CombatSetPause");

        // 移除消息监听
        MsgMgr.RemoveDoneHook("MSG_ARENA_CHALLENGE_BONUS", "CombatWnd");

        // 移除字段关注
        if (target != null)
        {
            target.dbase.RemoveTriggerField("CombatWnd");
            return;
        }

        // 关注属性字段变化
        if (OwnerOb != null)
        {
            // 注册属性变化回调
            OwnerOb.tempdbase.RemoveTriggerField("CombatWnd");
        }
    }

#if (UNITY_ANDROID || UNITY_IPHONE) && !UNITY_EDITOR
    /// </summary>
    /// Update this instance.
    /// </summary>
    void Update()
    {
        // 只是相应单指点击
        if (Input.touchCount != 1)
        {
            mIsTouched = false;
            mTouchPosition = Vector3.zero;
            curTouchId = -1;

            return;
        }

        // 判断当前手指touch状态
        switch (Input.GetTouch(0).phase)
        {
            case TouchPhase.Began:

                // 获取当前Touch
                Touch touch = Input.GetTouch(0);

                // 如果当前ui相机发生碰撞了优先处理UI碰撞
                if (UICamera.Raycast(touch.position))
                {
                    // 重置mIsTouched标识
                    mIsTouched = false;
                    curTouchId = -1;
                    mTouchPosition = Vector3.zero;
                    return;
                }

                // 标识mIsTouched
                mIsTouched = true;
                curTouchId = touch.fingerId;
                mTouchPosition = touch.position;

                // 开始
                break;

            case TouchPhase.Ended:
            case TouchPhase.Canceled:

                // 获取当前Touch
                Touch endedTouch = Input.GetTouch(0);

                // 如果当前ui相机发生碰撞了优先处理UI碰撞
                if (UICamera.Raycast(endedTouch.position))
                {
                    // 重置mIsTouched标识
                    mIsTouched = false;
                    curTouchId = -1;
                    mTouchPosition = Vector3.zero;
                    return;
                }

                Vector3 curPos = endedTouch.position;
                Vector3 delta = curPos - mTouchPosition;

                // 如果没有mIsTouched
                // curTouchId发生了变化
                // 位置变化太大了也不触发点击事件
                if (! mIsTouched ||
                    curTouchId != endedTouch.fingerId ||
                    Game.convertDistanceFromPointToInch(delta.magnitude) > ConstantValue.MOVE_INCH)
                    break;

                // 重置mIsTouched标识
                mIsTouched = false;
                curTouchId = -1;
                mTouchPosition = Vector3.zero;

                // 执行碰撞
                DoRayCast(curPos);

                break;

            default:
                break;
        }
    }
#else

    /// <summary>
    /// Update this instance.
    /// </summary>
    void Update()
    {
        // 如果是鼠标右键Down事件
        if (Input.GetMouseButtonDown(0))
        {
            // 如果当前ui相机发生碰撞了优先处理UI碰撞
            if (UICamera.Raycast(Input.mousePosition))
            {
                // 重置mIsTouched标识和mTouchPosition
                mIsTouched = false;
                mTouchPosition = Vector3.zero;
                return;
            }

            // 记录当前鼠标位置
            mTouchPosition = Input.mousePosition;
            mIsTouched = true;
            return;
        }

        // 如果是鼠标右键up事件
        if (Input.GetMouseButtonUp(0))
        {
            // 获取鼠标当前位置
            Vector3 curPos = Input.mousePosition;

            // 如果当前ui相机发生碰撞了优先处理UI碰撞
            if (UICamera.Raycast(curPos))
            {
                // 重置mIsTouched标识和mTouchPosition
                mIsTouched = false;
                mTouchPosition = Vector3.zero;
                return;
            }

            // 如果鼠标位置有变化不处理
            // 如果mIsTouched为false表示没有点击
            Vector3 delta = curPos - mTouchPosition;
            if (Game.convertDistanceFromPointToInch(delta.magnitude) > ConstantValue.MOVE_INCH ||
                ! mIsTouched)
            {
                // 重置mIsTouched标识和mMousePosition
                mIsTouched = false;
                mTouchPosition = Vector3.zero;
                return;
            }

            // 重置mIsTouched标识和mTouchPosition
            mIsTouched = false;
            mTouchPosition = Vector3.zero;

            // 执行碰撞
            DoRayCast(curPos);

            return;
        }

        // 如果是其他鼠标按键事件
        if (Input.GetMouseButton(1) ||
            Input.GetMouseButton(2) ||
            Input.GetAxis("Mouse ScrollWheel") > 0)
        {
            mIsTouched = false;
            mTouchPosition = Vector3.zero;
        }
    }

#endif

    /// <summary>
    /// Raises the application pause event.
    /// </summary>
    void OnApplicationPause(bool pauseStatus)
    {
#if (UNITY_ANDROID || UNITY_IPHONE) && ! UNITY_EDITOR
        curTouchId = -1;
#endif

        mIsTouched = false;
        mTouchPosition = Vector3.zero;
    }

    #region 外部接口

    /// <summary>
    /// 绑定窗口
    /// </summary>
    public void Bind(Property ownerOb)
    {
        // 绘制设置窗口
        RedrawSetWnd();

        // 先取消原来对象的关注事件
        if (OwnerOb != null)
            OwnerOb.tempdbase.RemoveTriggerField("CombatWnd");

        // 重置绑定窗口对象
        OwnerOb = ownerOb;

        // 关注属性字段变化
        OwnerOb.tempdbase.RegisterTriggerField("CombatWnd", new string[]
            {
                "is_cross_map",
            }, new CallBack(WhenCrossMap, null));
    }

    /// <summary>
    /// 绘制设置窗口
    /// </summary>
    public void RedrawSetWnd()
    {
        if (mIsPlayback)
        {
            spPause.spriteName = "pause";

            // 设置加速信息
            spAdd.spriteName = string.Format("x{0}", TimeMgr.GetScaleMultiple(true));
        }
        else
        {
            // 设置当前自动战斗icon
            spPause.spriteName = AutoCombatMgr.IsAutoCombat() ? "pause" : "auto";

            // 设置加速信息
            spAdd.spriteName = string.Format("x{0}", TimeMgr.GetScaleMultiple(mIsLoopFight));
        }
    }

    // 指引选择技能
    public void SelectSkill(int index)
    {
        SkillItem item = mSkillGroup[index].GetComponent<SkillItem>();

        // 取得技能格子对应的技能id
        int skillId = item.mSkillId;
        if (skillId <= 0)
            return;

        item.SetSelected(true);

        // 取消其他格子的选中状态
        for (int i = 0; i < mSkillGroup.Length; i++)
        {
            if (!mSkillGroup[i].activeInHierarchy)
                continue;

            int itemId = mSkillGroup[i].GetComponent<SkillItem>().mSkillId;

            if (itemId != skillId)
                mSkillGroup[i].GetComponent<SkillItem>().SetSelected(false);
        }

        // 设置选中技能id
        this.selskillId = skillId;

        // 设置技能施法对象提示
        SetTargetTip();
    }

    /// <summary>
    /// 指引释放技能
    /// </summary>
    public void GuideDoSkill(string targetRid)
    {
        // 获取战斗角色对象
        CombatActor actor = CombatActorMgr.GetCombatActor(targetRid);

        // 角色实体没有被激活或者Alpha为0
        if (actor == null ||
            !actor.isActive() ||
            Game.FloatEqual(actor.GetAlpha(), 0))
            return;

        // 检测目标是否合法
        if (! CheckRidValidity(targetRid))
            return;

        // 执行回合
        DoTactics(targetRid);
    }

    #endregion

    #region 内部方法

    /// <summary>
    /// Redraw this instance.
    /// </summary>
    private void Redraw()
    {
        mInstance = ME.user.Query<LPCMapping>("instance");

        mInstanceId = mInstance.GetValue<string>("id");

        mIsLoopFight = InstanceMgr.GetLoopFightByInstanceId(mInstanceId);

        mIsPlayback = (mInstance.GetValue<int>("playback") == 1);

        if (mIsPlayback)
        {
            mCombatInfo.SetActive(true);
            mSkillWnd.SetActive(false);
        }
        else
        {
            // 绘制技能窗口
            RedrawSkillWnd();

            mCombatInfo.SetActive(false);
        }

        // 绘制设置窗口
        RedrawSetWnd();

        // 刷新副本信息显示
        RedrawInstanceInfoWnd(mInstanceId);
    }

    /// <summary>
    /// 副本信息显示窗口
    /// </summary>
    private void RedrawInstanceInfoWnd(string instanceId)
    {
        int mapType = InstanceMgr.GetInstanceMapType(instanceId);

        if (mapType != MapConst.TOWER_MAP) 
        {
            mInstanceDescWnd.SetActive(false);
            return;
        }

        CsvRow row = TowerMgr.GetTowerInfoByInstance(instanceId);

        if (row == null)
        {
            mInstanceDescWnd.SetActive (false);
            return;
        }

        mInstanceDescWnd.SetActive (true);

        int difficult = row.Query<int>("difficulty");

        int layer = row.Query<int>("layer");

        mInstanceDesc.text = string.Format(LocalizationMgr.Get("CombatWnd_1"), layer + 1,
            (difficult == TowerConst.EASY_TOWER) ? LocalizationMgr.Get("CombatWnd_2"):LocalizationMgr.Get("CombatWnd_3"));
    }


    /// <summary>
    /// 绘制技能窗口
    /// </summary>
    private void RedrawSkillWnd()
    {
        // 如果当前处于自动战斗中
        // target object对象不存在
        if (target == null || AutoCombatMgr.IsAutoCombat() || mIsPlayback)
        {
            // 隐藏技能窗口（包括提示箭头）
            mSkillWnd.SetActive(false);
            return;
        }

        // 显示技能窗口
        mSkillWnd.SetActive(true);

        // 初始化选中
        selskillId = -1;
        mTarget.SetActive(false);

        // 显示宠物信息
        mPetInfo.text = string.Format(LocalizationMgr.Get("CombatWnd_5"), target.GetLevel(), target.Short());

        // 显示宠物蓝量
        int mp = target.Query<int>("mp");

        if (mp < 0)
            mp = 0;

        if (mp > mPetMpGroup.Length)
            mp = mPetMpGroup.Length;

        for (int i = 0; i < mPetMpGroup.Length; i++)
        {
            if (i < mp)
                mPetMpGroup[i].spriteName = "full";
            else
                mPetMpGroup[i].spriteName = "empty";
        }

        // 先初始化一把技能信息
        foreach (GameObject item in mSkillGroup)
        {
            item.GetComponent<SkillItem>().SetBind(-1);
            item.GetComponent<SkillItem>().SetSelected(false);
            item.GetComponent<SkillItem>().SetCd(-1);
            item.GetComponent<SkillItem>().SetMp(0);
            item.GetComponent<SkillItem>().SetCover(false);
            item.SetActive(false);
        }

        // 获取宠物技能
        LPCArray skills = target.GetAllSkills();

        // 没有技能不处理
        if (skills.Count == 0)
            return;

        Dictionary<int,int> skillTypeMap = new Dictionary<int,int>();

        List<int> skillTypeArr = new List<int>();

        foreach (LPCValue mks in skills.Values)
        {
            // 获取技能对应的类型
            int skillId = mks.AsArray[0].AsInt;
            int type = SkillMgr.GetSkillPosType(skillId);

            // 类型必须为大于0的整数
            if (type <= 0)
                continue;

            if (!SkillMgr.CanShowSkill(target as Property, skillId))
                continue;

            skillTypeArr.Add(type);

            skillTypeMap.Add(type, skillId);
        }

        // 对skillTypeArr进行排序
        int temp = 0;
        for (int i = skillTypeArr.Count; i > 0; i--)
        {
            for (int j = 0; j < i - 1; j++)
            {
                if (skillTypeArr[j] > skillTypeArr[j + 1])
                {
                    temp = skillTypeArr[j];
                    skillTypeArr[j] = skillTypeArr[j + 1];
                    skillTypeArr[j + 1] = temp;
                }
            }
        }

        // 填充数据
        for (int i = 0; i < skillTypeArr.Count; i++)
        {
            // 对当前index进行修正
            int index = i + (mSkillGroup.Length - skillTypeArr.Count);

            // 获取type对应的skillid
            int skillId = skillTypeMap[skillTypeArr[i]];

            if (skillId <= 0)
                continue;

            mSkillGroup[index].SetActive(true);

            // 填充数据
            mSkillGroup[index].GetComponent<SkillItem>().SetBind(skillId);

            // 取得蓝耗的值
            LPCMapping mpMap = SkillMgr.GetCasTCost(target as Property, skillId);

            int skillMp = mpMap.ContainsKey("mp") ? mpMap.GetValue<int>("mp") : 0;

            bool isMpEnough = skillMp > target.Query<int>("mp") ? false : true;

            // 设置技能蓝耗
            mSkillGroup[index].GetComponent<SkillItem>().SetMp(skillMp, isMpEnough);

            // 设置技能cd
            if (CdMgr.SkillIsCooldown(target, skillId))
            {
                int cd = CdMgr.GetSkillCdRemainRounds(target, skillId);

                if (cd > 0)
                    mSkillGroup[index].GetComponent<SkillItem>().SetCd(cd);
            }

            // 检测能否被选中
            if (!SkillMgr.IsValidSkill(target as Property, skillId))
            {
                mSkillGroup[index].GetComponent<SkillItem>().SetCover(true);
            }

            // 设置默认选中
            if (selskillId == -1 && SkillMgr.CanApplySkill(target as Property, skillId))
            {
                selskillId = skillId;

                mSkillGroup[index].GetComponent<SkillItem>().SetSelected(true);

                // 选择施法对象提示
                SetTargetTip();
            }

        }

        // 动态调整背景大小
        mSkillBg.width = mSkillBgLength - (mSkillGroup.Length - skillTypeArr.Count) * mSkillItemSize;
    }

    /// <summary>
    /// 属性刷新完成事件回调
    /// </summary>
    private void WhenMpChanged(object param, params object[] paramEx)
    {
        // 当前界面没有绑定宠物不处理
        if (target == null)
            return;

        // 重绘窗口
        RedrawSkillWnd();
    }

    /// <summary>
    /// Dos the auto combat.
    /// </summary>
    private void DoAutoCombat()
    {
        // 将死亡宠物隐藏起来
        List<Property> allPetList = RoundCombatMgr.GetPropertyList();
        foreach (Property petOb in allPetList)
        {
            // 没有死亡不处理
            if (! petOb.CheckStatus("DIED"))
                continue;

            CombatActor actor = petOb.Actor;
            if (actor == null)
                continue;

            // 将已经死亡的角色隐藏起来
            actor.SetTweenAlpha(0f, 0.1f);
        }

        // 如果存在施法对象
        if (target != null)
        {
            // 构建参数
            LPCMapping para = new LPCMapping();
            para.Append(roundArgs);              // 获取当前回合相关参数
            para.Add("rid", target.GetRid());    // 释放技能角色rid

            // 取消之前选中状态
            target.Actor.CancelActionSetByName("select");

            // 移除施法对象属性监听
            target.dbase.RemoveTriggerField("CombatWnd");

            // 执行回合
            TacticsMgr.DoTactics(target, TacticsConst.TACTICS_TYPE_ATTACK, para);

            // 隐藏提示
            mTarget.SetActive(false);
        }

        // 移除标记的技能
        selskillId = -1;

        // 移除施法对象
        target = null;

        // 隐藏本窗口
        RedrawSkillWnd();
    }

    /// <summary>
    /// 执行射线相交检测,执行策略
    /// </summary>
    private void DoRayCast(Vector3 position)
    {
        // 场景相机不存在
        if (SceneMgr.SceneCamera == null)
            return;

        // 发出射线
        Ray ray = SceneMgr.SceneCamera.ScreenPointToRay(position);

        // 执行碰撞检测
        RaycastHit[] hitList = Physics.RaycastAll(ray, SceneMgr.SceneCamera.farClipPlane);

        // 没有碰撞到任何实体
        if (hitList.Length == 0)
            return;

        // 选择实体rid
        string selectRid = string.Empty;

        // 遍历各个实体
        foreach(RaycastHit hit in hitList)
        {
            // 碰撞角色rid
            string rid = hit.transform.name;

            // 获取战斗角色对象
            CombatActor actor = CombatActorMgr.GetCombatActor(rid);

            // 角色实体没有被激活或者Alpha为0
            if (actor == null ||
                ! actor.isActive() ||
                Game.FloatEqual(actor.GetAlpha(), 0))
                continue;

            // 设置rid
            selectRid = rid;
            break;
        }

        // 如果当前处于自动战斗中
        if (AutoCombatMgr.IsAutoCombat())
        {
            // 设置锁定目标
            AutoCombatMgr.LockCombatTarget(Rid.FindObjectByRid(selectRid));

            return;
        }

        // 检测目标是否合法
        if (! CheckRidValidity(selectRid))
            return;

        // 执行回合
        DoTactics(selectRid);
    }

    ///<summary>
    /// 判断对象是否合法
    /// </summary>
    private bool CheckRidValidity(string rid)
    {
        // 获取rid失败, 或者当前没有选择技能
        if (string.IsNullOrEmpty(rid) ||
            selskillId == -1)
            return false;

        // 获取技能可选择目标列表
        List<Property> selList = SkillMgr.GetCanSelectList(target, selskillId);

        // 技能选择目标失败
        if (selList == null || selList.Count == 0)
            return false;

        // 查看对象是否在列表中
        foreach (Property item in selList)
        {
            // 检测是否可以施放技能
            if (item.GetRid().Equals(rid))
                return true;
        }

        // 返回无效
        return false;
    }

    ///<summary>
    /// 执行策略
    ///</summary>
    private void DoTactics(string pickRid)
    {
        // 构建参数
        LPCMapping para = new LPCMapping();
        para.Add("skill_id", selskillId);    // 释放技能
        para.Add("pick_rid", pickRid);       // 技能作用目标
        para.Append(roundArgs);              // 获取当前回合相关参数
        para.Add("rid", target.GetRid());    // 释放技能角色rid

        // 取消之前选中状态
        target.Actor.CancelActionSetByName("select");

        // 取消目标自动战斗
        AutoCombatMgr.AutoCombat = false;

        // 将死亡宠物隐藏起来
        List<Property> allPetList = RoundCombatMgr.GetPropertyList();
        foreach (Property petOb in allPetList)
        {
            // 没有死亡不处理
            if (! petOb.CheckStatus("DIED"))
                continue;

            CombatActor actor = petOb.Actor;
            if (actor == null)
                continue;

            // 将已经死亡的角色隐藏起来
            actor.SetTweenAlpha(0f, 0.1f);
        }

        // 移除施法对象属性监听
        target.dbase.RemoveTriggerField("CombatWnd");

        // 执行回合
        TacticsMgr.DoTactics(target, TacticsConst.TACTICS_TYPE_ATTACK, para);

        // 隐藏提示
        mTarget.SetActive(false);

        // 移除标记的技能
        selskillId = -1;

        // 移除施法对象
        target = null;

        // 隐藏本窗口
        RedrawSkillWnd();
    }

    /// <summary>
    /// 行动回合
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    private void WhenRoundCombat(int eventId, MixedValue para)
    {
        // 获取执行回合数据
        roundArgs = para.GetValue<LPCMapping>();

        // 获取执行回合的宠物rid
        petRid = roundArgs.GetValue<string>("rid");

        // 没有绑定owner
        if (string.IsNullOrEmpty(petRid))
            return;

        // 查找对象
        target = Rid.FindObjectByRid(petRid) as Char;
        if (target == null)
            return;

        // 注册属性变化回调
        target.dbase.RemoveTriggerField("CombatWnd");
        target.dbase.RegisterTriggerField("CombatWnd", new string[]
            {
                "mp",
                "max_mp"
            }, new CallBack(WhenMpChanged, null));

        // 重绘界面
        RedrawSkillWnd();

        // 设置宠物提示位置
        SetRoundSelect(Rid.FindObjectByRid(petRid));
    }

    /// <summary>
    /// Whens the cross map.
    /// </summary>
    /// <param name="param">Parameter.</param>
    /// <param name="paramEx">Parameter ex.</param>
    private void WhenCrossMap(object param, params object[] paramEx)
    {
        // 窗口绑定对象不存在
        if (OwnerOb == null)
            return;

        // 获取玩家的过图标识
        int isCrossMap = OwnerOb.QueryTemp<int>("is_cross_map");

        // 没有在过图中
        if (isCrossMap == 0)
            return;

        // 如果打开了战斗设置窗口需要关闭
        if (mCombatSetWnd != null && mCombatSetWnd.mIsopen)
        {
            // SetActive false战斗设置窗口
            mCombatSetWnd.gameObject.SetActive(false);

            // 隐藏暂停窗口
            WindowMgr.HideWindow(mPauseWnd);

            // 取消战斗暂停
            TimeMgr.DoContinueCombatLogic("CombatSetPause");
        }
    }

    /// <summary>
    /// 设置当前回合选中光效
    /// </summary>
    /// <param name="pet">Pet.</param>
    private void SetRoundSelect(Property pet)
    {
        if (pet == null || pet.CheckStatus("DIED"))
            return;

        CombatActor actor = pet.Actor;
        if (actor == null)
            return;

        // 添加到ActionRound
        string cookie = Game.NewCookie(pet.GetRid());
        RoundCombatMgr.AddRoundAction(cookie, pet);

        // 角色播放选中action
        actor.DoActionSet("select", cookie, LPCMapping.Empty);
    }

    /// <summary>
    /// 按压按钮
    /// </summary>
    /// <param name="ob">Ob.</param>
    private void OnSkillItemPress(GameObject ob, bool isPress)
    {
        SkillItem item = ob.GetComponent<SkillItem>();

        // 取得技能格子对应的技能id
        int skillId = item.mSkillId;

        if (!isPress)
        {
            mSkillViewWnd.GetComponent<SkillViewWnd>().HideView(true);

            // 如果技能处于冷却状态或次技能为被动技能等,取消选中状态
            if (!SkillMgr.CanApplySkill(target as Property, skillId))
            {
                item.SetSelected(false);

                // 选中第一个处于非冷却状态下的技能
                for (int i = 0; i < mSkillGroup.Length; i++)
                {
                    if (!mSkillGroup[i].activeInHierarchy)
                        continue;

                    int itemId = mSkillGroup[i].GetComponent<SkillItem>().mSkillId;

                    if (SkillMgr.IsValidSkill(target as Property, itemId))
                    {
                        mSkillGroup[i].GetComponent<SkillItem>().SetSelected(true);

                        // 设置选中技能id
                        this.selskillId = itemId;

                        // 显示悬浮
                        mSkillViewWnd.GetComponent<SkillViewWnd>().ShowView(itemId, target as Property, false, true);
                        break;
                    }
                }

            }
            else
            {
                // 设置选中技能id
                this.selskillId = skillId;
            }

            // 设置技能施法对象提示
            SetTargetTip();

            return;
        }

        // 显示技能悬浮
        mSkillViewWnd.GetComponent<SkillViewWnd>().ShowView(skillId, target as Property);

        // 如果已经处于选中状态
        if (item.IsSelected)
        {
            return;
        }

        item.SetSelected(true);

        // 取消其他格子的选中状态
        for (int i = 0; i < mSkillGroup.Length; i++)
        {
            if (!mSkillGroup[i].activeInHierarchy)
                continue;

            int itemId = mSkillGroup[i].GetComponent<SkillItem>().mSkillId;

            if (itemId != skillId)
                mSkillGroup[i].GetComponent<SkillItem>().SetSelected(false);
        }

    }

    ///<summary>
    /// 选择施法对象提示
    ///</summary>
    public void SetTargetTip()
    {
        List<Property> targetList = SkillMgr.GetCanSelectList(target, selskillId);
        mTarget.SetActive(true);

        // 将死亡宠物隐藏起来
        List<Property> allPetList = RoundCombatMgr.GetPropertyList();

        foreach (Property item in allPetList)
        {
            // 如果没有死亡不处理
            if (! item.CheckStatus("DIED"))
                continue;

            // 没有Actor对象不处理
            CombatActor actor = item.Actor;
            if (actor == null)
                continue;

            // 尝试显示模型
            if (targetList.Contains(item))
            {
                // 显示目标角色
                actor.SetTweenAlpha(0.5f);

                // 隐藏占位目标对象
                foreach (Property occupierOb in item.GetOccupierList())
                {
                    // 如果没有Actor对象
                    if (occupierOb.Actor == null)
                        continue;

                    // 隐藏目标对象
                    occupierOb.Actor.ShowHp(false);
                    occupierOb.Actor.SetTweenAlpha(0f);
                }
            }
            else
            {
                // 隐藏目标角色
                actor.SetTweenAlpha(0f);

                // 显示占位目标对象
                foreach (Property occupierOb in item.GetOccupierList())
                {
                    // 如果没有Actor对象
                    if (occupierOb.Actor == null)
                        continue;

                    // 显示目标对象
                    occupierOb.Actor.ShowHp(true);
                    occupierOb.Actor.SetTweenAlpha(1f);
                }
            }
        }

        // 先隐藏起来
        for (int i = 0; i < TargetList.Count; i++)
        {
            TargetList[i].transform.GetComponent<TweenPosition>().ResetToBeginning();
            TargetList[i].SetActive(false);
        }

        // 缓存列表
        List<GameObject> tmpList = new List<GameObject>();

        GameObject ob;

        int index = 0;

        for (index = 0; index < targetList.Count; index++)
        {
            string name = targetList[index].GetRid();
            if (index < TargetList.Count)
            {
                ob = TargetList[index];
            }
            else
            {
                ob = GameObject.Instantiate(mTargetTip) as GameObject;
                tmpList.Add(ob);
            }

            ob.transform.parent = mTarget.transform;
            ob.transform.localScale = Vector3.one;
            ob.name = name;
            SetSelectTips(ob, targetList[index]);
        }

        // 将新创建的列表项加入缓存
        for (int i = 0; i < tmpList.Count; ++i)
            TargetList.Add(tmpList[i]);

        // 隐藏多余的项
        for (int i = index; i < TargetList.Count; i++)
        {
            TargetList[i].transform.GetComponent<TweenPosition>().ResetToBeginning();
            TargetList[i].SetActive(false);
        }
    }

    /// <summary>
    /// 回放暂停
    /// </summary>
    public void PlaybackPause()
    {
        if (! TimeMgr.PauseCombatLogic)
        {
            spPause.spriteName = "auto";
            TimeMgr.DoPauseCombatLogic("CombatSetPause");
        }
        else
        {
            TimeMgr.DoContinueCombatLogic("CombatSetPause");
            spPause.spriteName = "pause";
        }
    }

    /// <summary>
    /// 设置选择目标提示位置
    /// </summary>
    /// <param name="ob">Ob.</param>
    /// <param name="pet">Pet.</param>
    private void SetSelectTips(GameObject ob, Property pet)
    {
        CombatActor actor = pet.Actor;
        if (actor == null)
            return;

        // 计算当前位置
        Vector3 actorPos = actor.GetPosition();
        curTipPos.x = actorPos.x;
        curTipPos.y = actorPos.y + actor.GetHpbarOffestY();
        curTipPos.z = actorPos.z;

        ob.transform.position = Game.WorldToUI(curTipPos);

        Vector3 localPos = ob.transform.localPosition;

        // 取得当前宠物的位置偏移
        float arrowOffset = MonsterMgr.GetArrowOffset(pet.GetClassID());

        // 对提示位置做偏移 
        ob.transform.localPosition = new Vector3(localPos.x, localPos.y + arrowOffset + 60f, localPos.z);

        ob.transform.GetComponent<TweenPosition>().from = ob.transform.localPosition;

        Vector3 localPosOffset = ob.transform.localPosition;

        ob.transform.GetComponent<TweenPosition>().to = new Vector3(localPosOffset.x, localPosOffset.y + 30f, localPosOffset.z);

        // 判断pet与target是否在同一阵营
        List<Property> attackList = RoundCombatMgr.GetPropertyList(CampConst.CAMP_TYPE_ATTACK);

        if (attackList.Contains(target) && attackList.Contains(pet))
        {
            ob.GetComponent<UISprite>().spriteName = "GreenDri";
        }
        else
        {
            int element_restrain = ElementMgr.GetMonsterCounter(target, pet);

            switch (element_restrain)
            {
                case ElementConst.ELEMENT_ADVANTAGE:
                    ob.GetComponent<UISprite>().spriteName = "GreenDri";
                    break;
                case ElementConst.ELEMENT_NEUTRAL:
                    ob.GetComponent<UISprite>().spriteName = "YellowDri";
                    break;
                case ElementConst.ELEMENT_DISADVANTAGE:
                    ob.GetComponent<UISprite>().spriteName = "Reddri";
                    break;
            }
        }

        ob.SetActive(true);
    }

    /// <summary>
    /// 战斗设置
    /// </summary>
    /// <param name="ob">Ob.</param>
    private void OnCombatSet(GameObject ob)
    {
        // 战斗设置窗口对象不存在
        if (mCombatSetWnd == null)
            return;

        // 获取玩家的过图标识, 如果玩家正在过图中
        int isCrossMap = OwnerOb.QueryTemp<int>("is_cross_map");
        if (isCrossMap != 0)
            return;

        // 如果是打开窗口，则执行关闭操作
        if (mCombatSetWnd.mIsopen)
        {
            mCombatSetWnd.gameObject.SetActive(false);

            // 隐藏暂停窗口
            WindowMgr.HideWindow(mPauseWnd);

            TimeMgr.DoContinueCombatLogic("CombatSetPause");
        }
        else
        {
            // 暂停游戏
            TimeMgr.DoPauseCombatLogic("CombatSetPause");

            mCombatSetWnd.gameObject.SetActive(true);

            // 显示暂停窗口
            WindowMgr.ShowWindow(mPauseWnd);

            if (mIsPlayback)
            {
                if (TimeMgr.PauseCombatLogic)
                    spPause.spriteName = "auto";
                else
                    spPause.spriteName = "pause";

                mPauseWnd.GetComponent<PauseWnd>().Bind(new CallBack(OnPauseCallBack));
            }
        }
    }

    /// <summary>
    /// Raises the add speed event.
    /// </summary>
    /// <param name="ob">Ob.</param>
    private void OnAddSpeed(GameObject ob)
    {
        // 是否是临时tempTimeScale
        bool tempTimeScale = mIsPlayback ? true : mIsLoopFight;

        // 获取当前的缩放倍速
        int multiple = TimeMgr.GetScaleMultiple(tempTimeScale);

        // 已经超过了MaxMultiple
        if (multiple == TimeMgr.MaxMultiple)
            multiple = TimeMgr.MinMultiple;
        else
            multiple += 1;

        // 设置缩放比例
        if (tempTimeScale)
            TimeMgr.TempTimeScale = TimeMgr.GetScale(multiple);
        else
            TimeMgr.TimeScale = TimeMgr.GetScale(multiple);

        // 设置加速信息
        spAdd.spriteName = string.Format("x{0}", multiple);
    }

    /// <summary>
    /// 自动战斗
    /// </summary>
    /// <param name="ob">Ob.</param>
    private void OnAutoCombat(GameObject ob)
    {
        if (mIsPlayback)
        {
            PlaybackPause();

            PauseWnd script = mPauseWnd.GetComponent<PauseWnd>();

            if (mPauseWnd.activeSelf)
            {
                WindowMgr.HideWindow(mPauseWnd);
            }
            else
            {
                WindowMgr.ShowWindow(mPauseWnd);
                script.Bind(new CallBack(OnPauseCallBack));
            }
        }
        else
        {
            // 设置自动战斗
            // 如果当前处于自动战斗，则需要取消自动战斗
            if (AutoCombatMgr.IsAutoCombat())
            {
                // 取消自动战斗
                AutoCombatMgr.SetAutoCombat(false, mIsLoopFight);

                // 更换图片
                spPause.spriteName = "auto";

                return;
            }

            // 自动战斗
            AutoCombatMgr.SetAutoCombat(true, mIsLoopFight);

            // 更换图片
            spPause.spriteName = "pause";

            // 执行自动战斗
            DoAutoCombat();
        }
    }

    void OnPauseCallBack(object para, params object[] param)
    {
        if (TimeMgr.PauseCombatLogic)
            spPause.spriteName = "auto";
        else
            spPause.spriteName = "pause";
    }

    private void OnArenaChallengeBonusMsg(string cmd, LPCValue para)
    {
        LPCMapping data = para.AsMapping;

        GameObject wnd = WindowMgr.OpenWnd(ShowBonusWnd.WndType);
        if (wnd == null)
            return;

        wnd.GetComponent<ShowBonusWnd>().Bind(data.GetValue<LPCMapping>("bonus"));
    }

    /// <summary>
    /// 回放结束
    /// </summary>
    private void OnPlaybackEnd(int eventId, MixedValue para)
    {
        // 打开回放详细信息详细界面
        GameObject wnd = WindowMgr.OpenWnd(PlaybackInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        LPCMapping data = para.GetValue<LPCMapping>();

        // 绑定数据
        wnd.GetComponent<PlaybackInfoWnd>().Bind(data.GetValue<string>("id"), null, true, false);

        // 关闭聊天窗口
        WindowMgr.DestroyWindow(ChatWnd.WndType);
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 副本通关回调
    /// </summary>
    private void OnInstanceClearance(int eventId, MixedValue para)
    {
        LPCMapping data = para.GetValue<LPCMapping>();

        if (data == null)
            return;

        string instanceId = data.GetValue<string>("instance_id");

        // 获取副本配置信息
        LPCMapping instanceConfig = InstanceMgr.GetInstanceInfo(instanceId);

        // 没有获取到该副本配置信息
        if (instanceConfig == null)
            return;

        // 获取该副本对应的地图配置
        CsvRow mapConfig = MapMgr.GetMapConfig(instanceConfig.GetValue<int>("map_id"));

        if (mapConfig == null)
            return;

        mIsLoopFight = InstanceMgr.GetLoopFightByInstanceId(instanceId);

        // 获取地图类型
        int mapType = mapConfig.Query<int>("map_type");

        if (MapConst.ARENA_MAP == mapType || mapType == MapConst.ARENA_NPC_MAP || mapType == MapConst.ARENA_REVENGE_MAP)
        {
            GameObject wnd = WindowMgr.OpenWnd("ArenaFightSettlementWnd");

            if (wnd == null)
                return;

            if (mapType == MapConst.ARENA_NPC_MAP)
            {
                LPCMapping map = data.GetValue<LPCMapping>("bonus_map");

                LPCMapping opponentData = LPCMapping.Empty;
                opponentData.Add("name", LocalizationMgr.Get(instanceConfig.GetValue<string>("name")));
                opponentData.Add("level", instanceConfig.GetValue<int>("level"));
                opponentData.Add("icon", instanceConfig.GetValue<string>("icon"));
                opponentData.Add("is_npc", 1);

                map.Add("opponent_top", opponentData);

                data["bonus_map"].AsMapping = map;
            }

            wnd.GetComponent<ArenaFightSettlementWnd>().Bind(data);

            //result = 1 代表副本通关;
            if (data.GetValue<int>("result") == 1)
                wnd.GetComponent<ArenaFightSettlementWnd>().FightVectory(mapType);
            else
                wnd.GetComponent<ArenaFightSettlementWnd>().FightFail(mapType);
        }
        else
        {
            GameObject wnd = WindowMgr.OpenWnd("FightSettlementWnd");

            if (wnd == null)
                return;

            wnd.GetComponent<FightSettlementWnd>().Bind(data);

            //result = 1 代表副本通关;
            if (data.GetValue<int>("result") == 1)
                wnd.GetComponent<FightSettlementWnd>().InstanceClearance(mIsLoopFight);
            else
                wnd.GetComponent<FightSettlementWnd>().InstanceClearanceFailed(mIsLoopFight);
        }

        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 副本复活提示回调
    /// </summary>
    void OnReviveTips(int eventId, MixedValue para)
    {
        GameObject wnd = WindowMgr.OpenWnd(ReviveWnd.WndType);

        //创建窗口失败;
        if (wnd == null)
            return;

        ReviveWnd res = wnd.GetComponent<ReviveWnd>();

        //没有挂载脚本;
        if (res == null)
            return;

        res.Bind(para.GetValue<LPCMapping>());
    }

    /// <summary>
    /// 回合开始事件回调
    /// </summary>
    void OnRoundStart(int eventId, MixedValue para)
    {
        // 刷新战斗回合提示
        RefreshCombatRoundTips();
    }

    /// <summary>
    /// 刷新战斗回合提示
    /// </summary>
    void RefreshCombatRoundTips()
    {
        // 剩余战斗回合
        int remainRound = GameSettingMgr.GetSettingInt("max_combat_rounds") - InstanceMgr.GetRoundCount(ME.user) + 1;
        if (remainRound > InstanceConst.FIRST_STAGE || remainRound < 1)
            return;

        // 显示剩余回合数量
        mRoundTips.text = string.Format(LocalizationMgr.Get("CombatWnd_4"), remainRound);

        // 偏移量
        int widthOffset = 90;

        mRoundTipsBg.width = mRoundTips.width + widthOffset;

        if (remainRound == InstanceConst.FIRST_STAGE
            || remainRound == InstanceConst.SECOND_STAGE
            || remainRound == InstanceConst.THIRD_STAGE
            || remainRound <= InstanceConst.FOURTH_STAGE)
        {
            PlayAnimation();
        }
    }

    /// <summary>
    /// 播放动画
    /// </summary>
    void PlayAnimation()
    {
        // 重置动画
        mRoundTipsAlphas[0].ResetToBeginning();

        mRoundTipsAlphas[1].from = 1;
        mRoundTipsAlphas[1].to = 0;

        mRoundTipsScale.ResetToBeginning();

        // 开始动画
        for (int i = 0; i < mRoundTipsAlphas.Length; i++)
        {
            mRoundTipsAlphas[i].PlayForward();
        }

        mRoundTipsScale.PlayForward();
    }

    #endregion
}
