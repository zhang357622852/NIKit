/// <summary>
/// BossHPBarWnd.cs
/// Created by fengsc 2016/10/27
/// Boss血条
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class BossHPBarWnd : WindowBase<BossHPBarWnd>
{
    #region 成员变量

    // 能量滑动条
    public UISlider mMpSlider;

    // 血量滑动条
    public UISlider mHpSlider;

    public UIWidget mGreenFG;
    public UIWidget mVioletFG;
    public UIWidget mYellowFG;

    // boss的重生次数
    public UILabel mRebornTimes;

    // 血条高光滑动条
    public UISlider mLightSlider;

    // buff的父节点
    public Transform mBuffParent;

    // skill的父节点
    public Transform mSkillParent;

    // 技能悬浮窗口对象
    public Transform mSkillView;

    // Boss的名称
    public UILabel mName;

    // 前景框Tween动画
    public TweenPosition mFgFramePos;

    // 血条和能量条动画组件
    public TweenPosition mSliderPos;

    // 血条高亮alpha动画
    public TweenAlpha mHpLightAlpha;

    public TweenAlpha mFgFrameAlpha;

    // buff基础对象
    public GameObject buffItem;

    // 技能基础控件
    public GameObject skillItem;
    private List<GameObject> mSkillWndList = new List<GameObject>();

    // 对象rid
    private string mRid = string.Empty;

    // 宠物对象
    private Property mOb;

    private float mHP;
    private float mMaxHp;

    private int mMp;
    private int mMaxMp;

    private bool mIsRedraw = true;

    private int mTimes = 0;

    #endregion

    #region 内部函数

    void Start()
    {
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("BossHpBar" + mRid);

        if (mOb == null)
            return;

        // 取消关注角色属性字段变化
        mOb.dbase.RemoveTriggerField("BossHpBarHp");
        mOb.dbase.RemoveTriggerField("BossHpBarMp");
        mOb.dbase.RemoveTriggerField("BossHpBarRebornTimes");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        string listenerId = "BossHpBar" + mRid;

        // 注册附加状态
        EventMgr.RegisterEvent(listenerId, EventMgrEventType.EVENT_APPLY_STATUS, WhenStatusChange);

        // 注册清除状态
        EventMgr.RegisterEvent(listenerId, EventMgrEventType.EVENT_CLEAR_STATUS, WhenStatusClear);

        // 注册玩家回合状态消失
        EventMgr.RegisterEvent(listenerId, EventMgrEventType.EVENT_STATUS_ROUND_UPDATE, WhenRoundStatusChange);

        // 注册boss死亡事件
        EventMgr.RegisterEvent(listenerId, EventMgrEventType.EVENT_DIE, WhenBossDie);

        // 注册战斗结束
        EventMgr.RegisterEvent(listenerId, EventMgrEventType.EVENT_ROUND_COMBAT_END, WhenRoundCombatEnd);

        // 注册首创结束事件
        EventMgr.RegisterEvent(listenerId, EventMgrEventType.EVENT_RECEIVE_DAMAGE, WhenRecevivDamage);

        if (mOb == null)
            return;

        // 关注血条变化
        mOb.dbase.RemoveTriggerField("BossHpBarMp");
        mOb.dbase.RegisterTriggerField("BossHpBarMp", new string[]
            {
                "mp",
                "max_mp"
            }, new CallBack(WhenMpChange));

        // 蓝条变化
        mOb.dbase.RemoveTriggerField("BossHpBarHp");
        mOb.dbase.RegisterTriggerField("BossHpBarHp", new string[]
            {
                "hp",
                "max_hp"
            }, new CallBack(WhenHpChange));

        // 复活次数限制
        mOb.dbase.RemoveTriggerField("BossHpBarRebornTimes");
        mOb.dbase.RegisterTriggerField("BossHpBarRebornTimes", new string[]
            {
                "reborn_times",
            }, new CallBack(WhenRebornTimesChange));
        
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 绘制boss身上的buff
        RedrawBuff();

        // 绘制boss技能
        RedrawSkills();

        // 绘制重生次数
        RedrawRebornTimes();

        // 绘制血量
        RedrawHP();

        // 绘制蓝量
        RedrawMP();

        // 刷新技能显示
        RedrawSkills();
    }

    /// <summary>
    /// 汇总重生次数
    /// </summary>
    void RedrawRebornTimes()
    {
        int rebornTime = mOb.Query<int>("reborn_times");
        if (mTimes > 1)
            mRebornTimes.text = "×" + (rebornTime + 1);
        else
            mRebornTimes.gameObject.SetActive(false);

        switch (rebornTime + 1)
        {
            case 0 :
            case 1 :
                mHpSlider.foregroundWidget = mYellowFG;
                mVioletFG.gameObject.SetActive(false);
                mGreenFG.gameObject.SetActive(false);
                break;
            case 2 :
                mHpSlider.foregroundWidget = mVioletFG;
                mGreenFG.gameObject.SetActive(false);
                break;
            case 3 :
                mHpSlider.foregroundWidget = mGreenFG;
                break;
            default : 
                break;
        }
    }

    /// <summary>
    /// 绘制boss血量
    /// </summary>
    void RedrawHP()
    {
        // 获取max_hp
        int maxHp = mOb.QueryAttrib("max_hp");
        if (maxHp <= 0)
            return;

        // 获取当前hp
        int hp = mOb.Query<int>("hp");
        if (hp <= 0)
            hp = 0;

        // 血量和最大血量都没有发生变化，不作处理
        if (maxHp == mMaxHp && hp == mHP)
            return;

        // 计算血量百分比
        float value = hp * 1.0f / maxHp;

        // 此处的修正为了保证value的值过小界面上无法看见血条
        if (value > 0 && value < 0.003f)
            value = 0.003f;

        // 设置血条进度
        mHpSlider.value = value;

        if (mIsRedraw)
        {
            mLightSlider.value = value;
            mIsRedraw = false;
        }

        // 记录此时的血量和最大血量
        mHP = hp;
        mMaxHp = maxHp;
    }

    /// <summary>
    /// 绘制蓝量
    /// </summary>
    void RedrawMP()
    {
        // 获取max_mp
        int maxMp = mOb.QueryAttrib("max_mp");
        if (maxMp <= 0)
            return;

        // 获取当前mp
        int mp = mOb.Query<int>("mp");
        if (mp <= 0)
            mp = 0;

        // 蓝量没有发生变化不作处理
        if (mp == mMp && maxMp == mMaxMp)
            return;

        // 设置滑动条的值
        mMpSlider.value = mp * 1.0f / maxMp;

        // 记录此时的蓝量
        mMp = mp;
        mMaxMp = maxMp;
    }

    /// <summary>
    /// 绘制boos的buff
    /// </summary>
    void RedrawBuff()
    {
        // boss处于死亡状态不作处理
        if (mOb.CheckStatus("DIDE"))
            return;

        // 获取boss的所有状态
        List<LPCMapping> statusList = mOb.GetAllStatus();
        int statusId = -1;
        int childId = 0;
        GameObject wnd;

        // 填充数据
        for (int i = 0; i < statusList.Count; i++)
        {
            // 获取状态id
            statusId = statusList[i].GetValue<int>("status_id");

            // 无效状态id或者不需要显示的状态
            if (statusId <= 0 || ! StatusMgr.IsStatusShow(statusId))
                continue;

            // 如果没有窗口就创建
            if (childId >= mBuffParent.transform.childCount)
            {
                // 将新的消息添加到列表的末尾
                wnd = Instantiate(buffItem);
                wnd.transform.SetParent(mBuffParent.transform);
                wnd.transform.localScale = Vector3.one;
                wnd.transform.localPosition = Vector3.zero;
                wnd.name = string.Format("BuffBtn{0}_{1}", mOb.GetRid(), childId);
            }
            else
            {
                wnd = mBuffParent.transform.GetChild(childId).gameObject;
            }

            // 显示图标
            if (! wnd.activeSelf)
                wnd.SetActive(true);

            // 绑定数据,状态id，持续的回合
            wnd.GetComponent<BuffBtn>().SetType(statusId, statusList[i].GetValue<int>("round"));

            // childId++
            childId++;
        }

        // 隐藏多余的图标
        for (int i = childId; i < mBuffParent.childCount; i++)
        {
            // 获取子控件
            wnd = mBuffParent.transform.GetChild(i).gameObject;

            // 如果本身不是显示状态
            if (wnd == null || ! wnd.activeSelf)
                continue;

            // 隐藏控件
            wnd.SetActive(false);
        }

        // 激活排序组件
        mBuffParent.GetComponent<UIGrid>().enabled = true;
    }

    /// <summary>
    /// 绘制boss的技能图标
    /// </summary>
    void RedrawSkills()
    {
        // 获取boss的所有技能信息
        LPCArray skills = mOb.GetAllSkills();

        // 没有技能不处理
        if (skills == null || skills.Count < 1)
            return;

        Dictionary<int,int> skillTypeMap = new Dictionary<int,int>();

        List<int> skillTypeArr = new List<int>();

        // 收集技能
        foreach (LPCValue mks in skills.Values)
        {
            // 获取技能id
            int skillId = mks.AsArray[0].AsInt;
            if (skillId <= 0)
                continue;

            // 获取技能类型
            int type = SkillMgr.GetSkillPosType(skillId);

            if (type <= 0)
                continue;

            if (!SkillMgr.CanShowSkill(mOb, skillId))
                continue;

            skillTypeArr.Add(type);

            skillTypeMap.Add(type, skillId);
        }

        skillTypeArr.Sort();

        GameObject wnd;

        // 显示图标
        for (int i = 0; i < skillTypeArr.Count; i++)
        {
            int skillId = skillTypeMap[skillTypeArr[i]];

            // 如果没有窗口就创建
            if (i >= mSkillWndList.Count)
            {
                // 将新的消息添加到列表的末尾
                wnd = Instantiate(skillItem);
                wnd.transform.SetParent(mSkillParent);
                wnd.transform.localScale = new Vector3(0.45f, 0.45f, 0.45f);
                wnd.transform.localPosition = Vector3.zero;
                wnd.name = string.Format("skill_{0}_{1}", mRid, skillId);

                // 添加到缓存列表中
                mSkillWndList.Add(wnd);
            } else
            {
                wnd = mSkillWndList[i];
            }

            // 设置窗口位置
            wnd.transform.localPosition = new Vector3(0 - (skillTypeArr.Count - i - 1) * 60, 0, 0);

            // 获取SkillItem组件
            SkillItem item = wnd.GetComponent<SkillItem>();

            // 绑定技能数据
            item.SetBind(skillId);

            // 取得蓝耗的值
            LPCMapping mpMap = SkillMgr.GetCasTCost(mOb, skillId);

            int skillMp = mpMap.ContainsKey("mp") ? mpMap.GetValue<int>("mp") : 0;

            bool isMpEnough = skillMp > mOb.Query<int>("mp") ? false : true;

            // 设置技能蓝量
            item.SetMp(skillMp, isMpEnough);

            // 判断技能是否在cd中
            if (CdMgr.SkillIsCooldown(mOb, skillId))
            {
                // 技能cd时间
                int cd = CdMgr.GetSkillCdRemainRounds(mOb, skillId);

                if (cd > 0)
                    item.SetCd(cd);
            }

            // 不能被选中,并且不是被动技能
            if (!SkillMgr.IsValidSkill(mOb, skillId))
                wnd.GetComponent<SkillItem>().SetCover(true);
            else
                wnd.GetComponent<SkillItem>().SetCover(false);

            item.SetSelected(false);

            // 显示图标
            if (! wnd.activeSelf)
                wnd.SetActive(true);

            // 添加点击事件;
            UIEventListener.Get(wnd).onPress = OnClickSkillItem;
        }
    }

    /// <summary>
    /// 技能格子点击事件
    /// </summary>
    void OnClickSkillItem(GameObject go, bool isPress)
    {
        SkillItem data = go.GetComponent<SkillItem>();

        //按下
        if (isPress)
        {
            if (data.mSkillId <= 0)
                return;

            data.SetSelected(true);

            // 开启协程
            Coroutine.DispatchService(SetSkillViewPos(data.mSkillId, go));
        }
        else
        {
            data.SetSelected(false);
            mSkillView.GetComponent<SkillViewWnd>().HideView();
        }
    }

    IEnumerator SetSkillViewPos(int skillId, GameObject go)
    {
        if (mSkillView == null)
            yield break;

        SkillViewWnd script = mSkillView.GetComponent<SkillViewWnd>();
        if (script == null)
            yield break;

        script.ShowView(skillId, mOb);

        // 等待一帧设置悬浮的位置
        yield return null;

        // 获取技能格子的碰撞盒
        BoxCollider box = go.GetComponent<BoxCollider>();

        float skillView_X = go.transform.localPosition.x - box.size.x * 0.5f * 0.45f - script.mBg.localSize.x * 0.5f;

        float skillView_Y = go.transform.localPosition.y - box.size.y * 0.5f * 0.45f - script.mBg.localSize.y;

        mSkillView.transform.localPosition = new Vector3(skillView_X, skillView_Y, 0);

        // 限定悬浮的位置在屏幕范围内
        script.LimitPosInScreen();
    }

    /// <summary>
    /// 播放boss受创的血条动画
    /// </summary>
    void WhenRecevivDamage(int eventId, MixedValue para)
    {
        // 转换数据测试
        LPCMapping args = para.GetValue<LPCMapping>();

        // 不是血条绑定对象id
        if (! string.Equals(args.GetValue<string>("rid"), mRid))
            return;

        // 重置动画
        mFgFramePos.ResetToBeginning();
        mSliderPos.ResetToBeginning();
        mHpLightAlpha.ResetToBeginning();
        mFgFrameAlpha.ResetToBeginning();

        mFgFramePos.PlayForward();
        mFgFrameAlpha.PlayForward();
        mSliderPos.PlayForward();
        mHpLightAlpha.PlayForward();
    }

    /// <summary>
    /// boss状态发生变化事件回调
    /// </summary>
    void WhenStatusChange(int eventId, MixedValue para)
    {
        // 数据格式转换
        LPCMapping args = para.GetValue<LPCMapping>();

        // 不是血条绑定对象id
        if (! string.Equals(args.GetValue<string>("rid"), mRid))
            return;

        // 延迟到下一帧调用;
        MergeExecuteMgr.DispatchExecute(DoDelayedRedrawBuff);
    }

    /// <summary>
    /// 清除附加状态事件回调
    /// </summary>
    void WhenStatusClear(int eventId, MixedValue para)
    {
        // 数据格式转换
        LPCMapping args = para.GetValue<LPCMapping>();

        // 不是血条绑定对象id
        if (! string.Equals(args.GetValue<string>("rid"), mRid))
            return;

        // 判断角色是否是清除死亡状态
        LPCArray statusList = args.GetValue<LPCArray>("status_list");

        // 如果是清除死亡状态, 需要显示血条
        if (statusList.IndexOf(StatusMgr.GetStatusIndex("DIED")) != -1)
            mOb.Actor.ShowHp(!mOb.CheckStatus("DIED"));

        // 延迟到下一帧调用;
        MergeExecuteMgr.DispatchExecute(DoDelayedRedrawBuff);
    }

    /// <summary>
    /// 回状态改变
    /// </summary>
    void WhenRoundStatusChange(int eventId, MixedValue para)
    {
        // 不是血条绑定对象id
        if (! string.Equals(para.GetValue<string>(), mRid))
            return;

        // 延迟到下一帧调用;
        MergeExecuteMgr.DispatchExecute(DoDelayedRedrawBuff);
    }

    /// <summary>
    /// 延迟重回buff窗口
    /// </summary>
    void DoDelayedRedrawBuff()
    {
        // 窗口已经析构，不在重绘
        if (this == null)
            return;

        // 重回buff窗口
        RedrawBuff();
    }

    /// <summary>
    /// boss事件回调
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    void WhenBossDie(int eventId, MixedValue para)
    {
        Property ob = para.GetValue<Property>();

        if (ob == null)
            return;

        // 绑定目标不一致
        if (! string.Equals(ob.GetRid(), mRid))
            return;

        // 隐藏血条窗口
        WindowMgr.HideWindow(this.gameObject);
    }

    /// <summary>
    /// 战斗结束事件回调
    /// </summary>
    void WhenRoundCombatEnd(int eventId, MixedValue para)
    {
        // 隐藏战斗血条
        WindowMgr.HideWindow(this.gameObject);
    }

    /// <summary>
    /// 能量变化事件回调
    /// </summary>
    void WhenMpChange(object para, params object[] param)
    {
        // 绘制蓝条
        RedrawMP();

        // 绘制技能
        RedrawSkills();
    }

    /// <summary>
    /// 血量变化事件回调
    /// </summary>
    void WhenHpChange(object para, params object[] param)
    {
        // 刷新血条
        RedrawHP();
    }

    /// <summary>
    /// Whens the reborn times change.
    /// </summary>
    /// <param name="para">Para.</param>
    /// <param name="param">Parameter.</param>
    void WhenRebornTimesChange(object para, params object[] param)
    {
        // 刷新重生次数
        RedrawRebornTimes();
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(string rid)
    {
        // 窗口绑定rid
        mRid = rid;

        // 获取绑定对象
        mOb = Rid.FindObjectByRid(mRid) as Char;

        if (mOb == null)
        {
            LogMgr.Trace("没有获取到boss对象");
            return;
        }

        // 绑定名称
        if (mName != null)
            mName.text = mOb.Short();

        // 注册事件
        RegisterEvent();

        if (mTimes == 0)
            mTimes = mOb.Query<int>("reborn_times") + 1;

        // 绘制窗口
        Redraw();
    }

    #endregion
}
