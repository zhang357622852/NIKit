/// <summary>
/// AttributeWnd.cs
/// Created by lic 2016-7-02
/// 宠物属性界面
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class AttributeWnd : MonoBehaviour
{
    #region 成员变量

    // 本地化文字
    public UILabel mExp;
    public UILabel mLevel;
    public UILabel mStrength;
    public UILabel mAttack;
    public UILabel mDefense;
    public UILabel mSpeed;
    public UILabel mCriticalRate;
    public UILabel mCriticalHurt;
    public UILabel mEffectHit;
    public UILabel mEffectDefense;
    public UILabel mEvaluate;

    public UILabel mExp_value;
    public UILabel mLevel_value;
    public UILabel mStrength_value;
    public UILabel mAttack_value;
    public UILabel mDefense_value;
    public UILabel mSpeed_value;
    public UILabel mCriticalRate_value;
    public UILabel mCriticalHurt_value;
    public UILabel mEffectHit_value;
    public UILabel mEffectDefense_value;
    public UILabel mStrength_value_add;
    public UILabel mAttack_value_add;
    public UILabel mDefense_value_add;
    public UILabel mSpeed_value_add;
    public UILabel mType;
    public UILabel mRace;

    public UISlider mExpSlider;
    public GameObject mEvaluate_btn;
    public UIToggle mPetLock_mark;
    public UIToggle mShare_mark;
    public GameObject mLockPet;
    public GameObject mShare;

    public GameObject mChatShareBtn;
    public UILabel mChatShareBtnLb;

    // 绑定的宠物对象
    private Property item_ob = null;

    private bool mIsMeBaggage = false;

    #endregion

    #region 内部函数

    void Start()
    {
        // 初始化界面
        InitWnd();

        // 注册事件
        RegisterEvent();

        // 刷新界面
        Redraw();
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    void OnDisable()
    {
        // 对象不存在
        if (item_ob == null)
            return;

        // 取消关注
        item_ob.tempdbase.RemoveTriggerField("AttributeWnd");
    }

    /// <summary>
    /// 初始化窗口.
    /// </summary>
    void InitWnd()
    {
        mExp.text = LocalizationMgr.Get("AttributeWnd_1");
        mLevel.text = LocalizationMgr.Get("AttributeWnd_2");
        mStrength.text = LocalizationMgr.Get("AttributeWnd_3");
        mAttack.text = LocalizationMgr.Get("AttributeWnd_4");
        mDefense.text = LocalizationMgr.Get("AttributeWnd_5");
        mSpeed.text = LocalizationMgr.Get("AttributeWnd_6");
        mCriticalRate.text = LocalizationMgr.Get("AttributeWnd_8");
        mCriticalHurt.text = LocalizationMgr.Get("AttributeWnd_9");
        mEffectHit.text = LocalizationMgr.Get("AttributeWnd_10");
        mEffectDefense.text = LocalizationMgr.Get("AttributeWnd_11");
        mLockPet.GetComponent<UILabel>().text = LocalizationMgr.Get("AttributeWnd_12");
        mShare.GetComponent<UILabel>().text = LocalizationMgr.Get("AttributeWnd_13");
        mChatShareBtnLb.text = LocalizationMgr.Get("AttributeWnd_18");
        mEvaluate.text = LocalizationMgr.Get("AttributeWnd_14");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mShare).onClick += OnShareClicked;
        UIEventListener.Get(mLockPet).onClick += OnLockPetClicked;
        UIEventListener.Get(mPetLock_mark.gameObject).onClick += OnLockPetClicked;
        UIEventListener.Get(mShare_mark.gameObject).onClick += OnShareClicked;
        UIEventListener.Get(mChatShareBtn).onClick += OnClickChatShareBtn;
        UIEventListener.Get(mEvaluate_btn).onClick = OnClickEvaluateBtn;
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        bool isNotEmpty = (item_ob != null ? true : false);

        mExp_value.gameObject.SetActive(isNotEmpty);
        mLevel_value.gameObject.SetActive(isNotEmpty);
        mStrength_value.gameObject.SetActive(isNotEmpty);
        mAttack_value.gameObject.SetActive(isNotEmpty);
        mDefense_value.gameObject.SetActive(isNotEmpty);
        mSpeed_value.gameObject.SetActive(isNotEmpty);
        mCriticalRate_value.gameObject.SetActive(isNotEmpty);
        mCriticalHurt_value.gameObject.SetActive(isNotEmpty);
        mEffectHit_value.gameObject.SetActive(isNotEmpty);
        mEffectDefense_value.gameObject.SetActive(isNotEmpty);
        mStrength_value_add.gameObject.SetActive(isNotEmpty);
        mAttack_value_add.gameObject.SetActive(isNotEmpty);
        mDefense_value_add.gameObject.SetActive(isNotEmpty);
        mSpeed_value_add.gameObject.SetActive(isNotEmpty);
        mType.gameObject.SetActive(isNotEmpty);
        mRace.gameObject.SetActive(isNotEmpty);

        mPetLock_mark.gameObject.SetActive(mIsMeBaggage);
        mShare_mark.gameObject.SetActive(mIsMeBaggage);
        mShare.SetActive(mIsMeBaggage);
        mLockPet.gameObject.SetActive(mIsMeBaggage);
        mChatShareBtn.SetActive(mIsMeBaggage);

        mExpSlider.value = 0f;

        mPetLock_mark.value = false;
        mShare_mark.value = false;

        if (!isNotEmpty)
            return;

        if (ME.user == null)
            return;

        if (PetMgr.IsSharePet(item_ob.GetRid()))
            mShare_mark.value = true;

        if (PetMgr.IsLockPet(item_ob))
            mPetLock_mark.value = true;

        // 宠物当前等级
        int level = item_ob.GetLevel();

        // 宠物最大等级
        int maxLevel = MonsterMgr.GetMaxLevel(item_ob);

        // 宠物当前经验值
        int exp = item_ob.Query<int>("exp");

        // 获取宠物下一级所需经验
        int nextExp = StdMgr.GetPetStdExp(level + 1 , item_ob.GetStar());

        mExp_value.text =  exp + "/" + nextExp;

        if(nextExp == 0)
            mExpSlider.value = 0f;
        else
            mExpSlider.value = exp/(float)nextExp;

        mLevel_value.text = level.ToString() + "/" + maxLevel.ToString();

        mStrength_value.text = item_ob.Query<int>("max_hp").ToString();

        mAttack_value.text = item_ob.Query<int>("attack").ToString();

        mDefense_value.text = item_ob.Query<int>("defense").ToString();

        // 敏捷
        mSpeed_value.text = item_ob.Query<int>("agility").ToString();

        mCriticalRate_value.text = item_ob.QueryAttrib("crt_rate") / 10 + "%";

        mCriticalHurt_value.text = item_ob.QueryAttrib("crt_dmg_rate") / 10 + "%";

        mEffectHit_value.text = item_ob.QueryAttrib("accuracy_rate") / 10 + "%";

        mEffectDefense_value.text = item_ob.QueryAttrib("resist_rate") / 10 + "%";

        // 体力增加值
        int strength_add = item_ob.QueryAttrib("max_hp") - item_ob.Query<int>("max_hp");
        if (strength_add <= 0)
            mStrength_value_add.text = "";
        else
            mStrength_value_add.text = "+" + strength_add.ToString();

        // 攻击力增加值
        int attack_add = item_ob.QueryAttrib("attack") - item_ob.Query<int>("attack");
        if (attack_add <= 0)
            mAttack_value_add.text = "";
        else
            mAttack_value_add.text = "+" + attack_add.ToString();

        // 防御力增加值
        int defense_add = item_ob.QueryAttrib("defense") - item_ob.Query<int>("defense");
        if (defense_add <= 0)
            mDefense_value_add.text = "";
        else
            mDefense_value_add.text = "+" + defense_add.ToString();

        // 敏捷增加值
        int speed_add = item_ob.QueryAttrib("agility") - item_ob.Query<int>("agility");
        if (speed_add <= 0)
            mSpeed_value_add.text = "";
        else
            mSpeed_value_add.text = "+" + speed_add.ToString();

        // 获取宠物类型
        int type = MonsterMgr.GetType(item_ob.GetClassID());
        string typeText = MonsterConst.MonsterStyleTypeMap.ContainsKey(type) ?
            MonsterConst.MonsterStyleTypeMap[type] : MonsterConst.MonsterStyleTypeMap[0];
        mType.text = typeText;

        // 宠物种族
        int race = MonsterMgr.GetRace(item_ob.GetClassID());
        string raceText = MonsterConst.MonsterRaceTypeMap.ContainsKey(race) ?
            MonsterConst.MonsterRaceTypeMap[race] : MonsterConst.MonsterRaceTypeMap[0];
        mRace.text = raceText;
    }

    /// <summary>
    /// 属性刷新完成事件回调
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    private void RefreshAffectEnd(object param, params object[] paramEx)
    {
        // 当前界面没有绑定宠物不处理
        if (item_ob == null)
            return;

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 分享按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnShareClicked(GameObject ob)
    {
        mShare_mark.value = !mShare_mark.value;
        if(item_ob == null)
            return;

        if(PetMgr.IsSharePet(item_ob.GetRid()))
            return;

        // 判断是否是材料宠物
        if (MonsterMgr.IsMaterialMonster(item_ob.GetClassID()))
        {
            // 材料宠物不能分享
            DialogMgr.ShowSingleBtnDailog(
                null,
                string.Format(LocalizationMgr.Get("AttributeWnd_19")),
                string.Empty,
                string.Empty,
                true,
                WindowMgr.GetWindow(BaggageWnd.WndType).transform
            );

            return;
        }

        // 显示二次确认弹框
        DialogMgr.ShowDailog(
            new CallBack(ShareSecondDialogCallBack),
            string.Format(LocalizationMgr.Get("AttributeWnd_15"), LocalizationMgr.Get(item_ob.Query<string>("name"))),
            string.Empty,
            string.Empty,
            string.Empty,
            true,
            WindowMgr.GetWindow(BaggageWnd.WndType).transform
        );
    }

    /// <summary>
    /// 共享宠物二次弹框按钮点击事件
    /// </summary>
    void ShareSecondDialogCallBack(object para, params object[] param)
    {
        // 确认按钮点击事件
        if ((bool) param[0])
        {
            // 通知服务器设置主要魔灵
            Operation.CmdSetSharePet.Go(item_ob.GetRid());

            mShare_mark.value = true;
        }
        // 取消按钮点击事件
        else
            // 不做操作
            mShare_mark.value = false;
    }

    /// <summary>
    /// 聊天共享宠物按钮点击事件
    /// </summary>
    void OnClickChatShareBtn(GameObject go)
    {
        if (item_ob == null)
            return;

        // 打开聊天界面
        GameObject wnd = WindowMgr.OpenWnd(ChatWnd.WndType);

        if (wnd == null)
        {
            LogMgr.Trace("ChatWnd窗口创建失败");
            return;
        }

        string wndName = string.Empty;

        // 不在副本中才打开主城界面
        if (!InstanceMgr.IsInInstance(ME.user))
            wndName = MainWnd.WndType;

        // 绑定数据
        wnd.GetComponent<ChatWnd>().BindPublish(item_ob);
        wnd.GetComponent<ChatWnd>().Bind(wndName, null);

        // 关闭包裹界面
        WindowMgr.DestroyWindow(BaggageWnd.WndType);
    }

    /// <summary>
    /// 锁定宠物按钮被点击
    /// </summary>
    void OnLockPetClicked(GameObject ob)
    {
        mPetLock_mark.value = !mPetLock_mark.value;

        if(item_ob == null)
            return;

        int isLock = !mPetLock_mark.value == true ? 1 : 0;

        if (!mPetLock_mark.value)
        {
            // 显示二次确认弹框
            DialogMgr.ShowDailog(
                new CallBack(LockPetSecondDialogCallBack, isLock),
                string.Format(LocalizationMgr.Get("AttributeWnd_16"), LocalizationMgr.Get(item_ob.Query<string>("name"))),
                string.Empty,
                string.Empty,
                string.Empty,
                true,
                WindowMgr.GetWindow(BaggageWnd.WndType).transform
            );
        }
        else
        {
            // 显示二次确认弹框
            DialogMgr.ShowDailog(new CallBack(LockPetSecondDialogCallBack, isLock),
                string.Format(LocalizationMgr.Get("AttributeWnd_17"), LocalizationMgr.Get(item_ob.Query<string>("name"))),
                string.Empty,
                string.Empty,
                string.Empty,
                true,
                WindowMgr.GetWindow(BaggageWnd.WndType).transform
            );
        }
    }

    /// <summary>
    /// 玩家评价按钮点击事件
    /// </summary>
    void OnClickEvaluateBtn(GameObject go)
    {
        // 当前选择对象不存在
        if (item_ob == null)
            return;

        // 打开玩家评价窗口
        GameObject wnd = WindowMgr.OpenWnd(AppraiseWnd.WndType);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<AppraiseWnd>().Bind(item_ob.GetClassID(), item_ob.GetRank(), item_ob.GetStar());
    }

    /// <summary>
    /// 锁定宠物二次弹框按钮点击事件
    /// </summary>
    void LockPetSecondDialogCallBack(object para, params object[] param)
    {
        // 确认按钮点击事件
        if (!(bool) param[0])
            return;

        mPetLock_mark.value = (int) para == 1 ? true : false;

        // 通知服务器锁定魔灵
        Operation.CmdLockPet.Go(item_ob.GetRid(), (int) para);
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 窗口绑定实体
    /// </summary>
    public void SetBind(Property ob, bool isMeBaggage = true)
    {
        // 取消关注
        if (item_ob != null)
            item_ob.tempdbase.RemoveTriggerField("AttributeWnd");

        // 重置绑定对象
        item_ob = ob;

        // 判断怪物是否可以评论
        // 不能显示在图鉴中就不能显示参与评论
        if (item_ob != null)
            mEvaluate_btn.SetActive((item_ob.Query<int>("show_in_manual") == 1));
        else
            mEvaluate_btn.SetActive(false);

        // 注册improvement
        if (item_ob != null)
            item_ob.tempdbase.RegisterTriggerField("AttributeWnd", new string[]
                {
                    "improvement"
                }, new CallBack(RefreshAffectEnd, null));

        mIsMeBaggage = isMeBaggage;

        // 重绘窗口
        Redraw();
    }

    #endregion
}
