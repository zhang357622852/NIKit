/// <summary>
/// DetailedPetInfoWnd.cs
/// Created by fengsc 2016/07/18
///战斗结算奖励宠物信息窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class DetailedPetInfoWnd : WindowBase<DetailedPetInfoWnd>
{
    #region 成员变量

    /// <summary>
    ///宠物元素图片
    /// </summary>
    public UISprite mElement;

    /// <summary>
    ///宠物名字
    /// </summary>
    public UILabel mPetName;

    /// <summary>
    ///宠物星级
    /// </summary>
    public GameObject[] mStars;

    /// <summary>
    ///宠物技能
    /// </summary>
    public GameObject[] mSkills;

    /// <summary>
    ///关闭按钮
    /// </summary>
    public GameObject mCloseBtn;

    /// <summary>
    ///玩家评价按钮
    /// </summary>
    public GameObject mEvaluateBtn;
    public UILabel mEvaluateLabel;

    /// <summary>
    ///合成信息按钮
    /// </summary>
    public GameObject mCompoundBtn;
    public UILabel mCompoundLabel;

    /// <summary>
    ///宠物种族
    /// </summary>
    public UILabel mRace;
    public UILabel mRaceTips;

    /// <summary>
    ///宠物类型
    /// </summary>
    public UILabel mType;
    public UILabel mTypeTips;

    /// <summary>
    ///宠物最大等级
    /// </summary>
    public UILabel mMaxLevel;
    public UILabel mMaxLevelTips;

    /// <summary>
    ///宠物体力值
    /// </summary>
    public UILabel mPower;
    public UILabel mPowerTips;

    /// <summary>
    ///宠物攻击力
    /// </summary>
    public UILabel mAttack;
    public UILabel mAttackTips;

    /// <summary>
    ///宠物防御力
    /// </summary>
    public UILabel mDefence;
    public UILabel mDefenceTips;

    /// <summary>
    ///宠物攻击速度
    /// </summary>
    public UILabel mAgility;
    public UILabel mAgilityTips;

    public UILabel mEvolveChangeTips;

    public UILabel mEvolveTips;

    /// <summary>
    ///技能悬浮窗口
    /// </summary>
    public GameObject mSkillView;

    /// <summary>
    ///宠物模型
    /// </summary>
    public GameObject mPetModel;

    public UIToggle mEvolveToggle;
    public GameObject mEvolveIcon;
    public UITexture mRankIcon;

    public UIToggle mNormalToggle;
    public GameObject mIcon;
    public UITexture mNormalIcon;

    public GameObject mRichTextContent;

    public UISprite mMask;

    public UISprite mTitleBg;

    public GameObject mNameAndStar;

    public TweenAlpha mTweenAlpha;

    public TweenScale mTweenScale;

    Property mOriginalPet = null;

    Property mClonePet = null;

    Property mViewPet = null;

    // 第一次打开时待窗口动画完成后再加载模型
    private bool isTweenOver = false;

    private State mCurrentState;

    private enum State
    {
        RANK = 0,
        NORMAL = 1,
    }

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 注册事件;
        RegisterEvent();

        SelectEffect();

        InitLabel();

        if (mTweenAlpha == null || mTweenScale == null)
            return;

        // 播放动画
        mTweenAlpha.PlayForward();

        mTweenScale.PlayForward();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("DetailedPetInfoWnd");

        if (mClonePet != null)
            mClonePet.Destroy();
    }

    /// <summary>
    ///刷新宠物信息
    /// </summary>
    void RedrawPetInfo(Property ob)
    {
        //没有宠物数据
        if (ob == null)
            return;

        // 非合成材料不显示合成按钮
//        mCompoundBtn.SetActive(MonsterMgr.IsSyntheMaterial(ob.GetClassID()));

        int classId = ob.GetClassID();

        //获取宠物种族;
        mRace.text = MonsterConst.MonsterRaceTypeMap[MonsterMgr.GetRace(classId)];

        //获取宠物类型;
        mType.text = MonsterConst.MonsterStyleTypeMap[MonsterMgr.GetType(classId)];

        //获取宠物当前星级的最大等级;
        mMaxLevel.text = MonsterMgr.GetMaxLevel(ob).ToString();

        //获取宠物的名字;
        mPetName.text = string.Format("{0}{1}[-]", PetMgr.GetAwakeColor(ob.GetRank()), ob.Short());

        //获取宠物的攻击力
        mAttack.text = ob.QueryAttrib("attack").ToString();

        //获取宠物体力
        mPower.text = ob.QueryAttrib("max_hp").ToString();

        //获取宠物防御力;
        mDefence.text = ob.QueryAttrib("defense").ToString();

        // 敏捷;
        mAgility.text = ob.QueryAttrib("agility").ToString();

        //获取宠物元素图标;
        mElement.spriteName = PetMgr.GetElementIconName(MonsterMgr.GetElement(classId));

        // 获取星级
        int star = ob.GetStar();
        int count = star < mStars.Length ? star : mStars.Length;
        string IconName = PetMgr.GetStarName(ob.GetRank());

        for (int i = 0; i < mStars.Length; i++)
            mStars[i].gameObject.SetActive(false);

        for (int i = 0; i < count; i++)
        {
            mStars[i].GetComponent<UISprite>().spriteName = IconName;
            mStars[i].gameObject.SetActive(true);
        }

        float starX = mStars[0].GetComponent<UISprite>().localSize.x;

        mNameAndStar.transform.localPosition = new Vector3(
            0,
            mNameAndStar.transform.localPosition.y,
            mNameAndStar.transform.localPosition.z);

        mNameAndStar.transform.localPosition = new Vector3(
            mNameAndStar.transform.localPosition.x + (mStars.Length - count) * 0.5f * starX,
            mNameAndStar.transform.localPosition.y,
            mNameAndStar.transform.localPosition.z);

        string desc = MonsterMgr.GetEvolutionDesc(ob);

        RichTextContent content = mRichTextContent.GetComponent<RichTextContent>();

        if (content == null)
            return;

        // 清空组件的内容
        content.clearContent();

        content.ParseValue(desc);
    }

    /// <summary>
    ///注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mEvaluateBtn).onClick = OnClickEvaluateBtn;
        UIEventListener.Get(mCompoundBtn).onClick = OnClickCompundBtn;
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mMask.gameObject).onClick = OnClickCloseBtn;
        UIEventListener.Get(mEvolveToggle.gameObject).onClick = OnClickEvolveToggle;
        UIEventListener.Get(mNormalToggle.gameObject).onClick = OnClickNormalToggle;

        //注册进化图标点击事件;
        EventMgr.RegisterEvent("DetailedPetInfoWnd", EventMgrEventType.EVENT_CLICK_PICTURE, WhenShowHover);

        if (mTweenScale == null)
            return;

        // 注册动画播放完成回调
        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinished);

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    /// <summary>
    /// TweenAlpha结束的回调
    /// </summary>
    private void OnTweenFinished()
    {
        isTweenOver = true;
        ShowModel(mOriginalPet);

        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 显示悬浮
    /// </summary>
    void WhenShowHover(int eventId, MixedValue para)
    {
        List<object> data = para.GetValue<List<object>>();

        if (data == null || data.Count < 1)
            return;

        bool isPress = (bool)data[0];

        int skillId = (int)data[1];

        Vector3 iconPos = (Vector3)data[2];

        if (mSkillView == null)
            return;

        SkillViewWnd script = mSkillView.GetComponent<SkillViewWnd>();
        if (script == null)
            return;

        //鼠标按下
        if (isPress)
        {
            // 显示悬浮
            script.ShowView(skillId, mOriginalPet, true);

            // 设置悬浮的位置
            mSkillView.transform.position = iconPos;

            // 限制悬浮在屏幕范围内
            script.LimitPosInScreen();
        }
        else
        {
            // 隐藏悬浮窗口
            script.HideView();
        }
    }

    /// <summary>
    ///玩家评价按钮点击事件回调
    /// </summary>
    void OnClickEvaluateBtn(GameObject go)
    {
        // 打开玩家评价窗口
        GameObject wnd = WindowMgr.OpenWnd(AppraiseWnd.WndType);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<AppraiseWnd>().Bind(mViewPet.GetClassID(), mViewPet.GetRank(), mViewPet.GetStar());
    }

    /// <summary>
    ///合成信息按钮点击事件
    /// </summary>
    void OnClickCompundBtn(GameObject go)
    {
        GameObject wnd = WindowMgr.OpenWnd ("PetSynthesisViewWnd");

        wnd.GetComponent<PetSynthesisViewWnd> ().BindData (mOriginalPet.GetClassID());
    }

    /// <summary>
    ///关闭按钮点击事件回调
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        WindowMgr.DestroyWindow(this.gameObject.name);
    }

    void OnClickEvolveToggle(GameObject go)
    {
        if (mCurrentState == State.RANK)
            return;

        SelectEffect();

        mViewPet = mOriginalPet.GetRank() > 1 ? mOriginalPet : mClonePet;

        //rank=2表示觉醒
        AccordingRankDisplayData(mViewPet);
    }

    void OnClickNormalToggle(GameObject go)
    {
        if (mCurrentState == State.NORMAL)
            return;

        SelectEffect();

        mViewPet = mOriginalPet.GetRank() > 1 ? mClonePet : mOriginalPet;

        //rank <= 1 表示没有觉醒;
        AccordingRankDisplayData(mViewPet);
    }

    /// <summary>
    ///单选按钮选中后的显示效果
    /// </summary>
    void SelectEffect()
    {
        if (mEvolveToggle == null || mNormalToggle == null)
            return;

        if (mEvolveToggle.value)
        {
            mEvolveIcon.transform.localPosition = new Vector3 (mEvolveIcon.transform.localPosition.x, -7,
                mEvolveIcon.transform.localPosition.z);
            mIcon.transform.localPosition = new Vector3 (mIcon.transform.localPosition.x, 0,
                mIcon.transform.localPosition.z);
        }
        if (mNormalToggle.value)
        {
            mEvolveIcon.transform.localPosition = new Vector3 (mEvolveIcon.transform.localPosition.x, 0,
                mEvolveIcon.transform.localPosition.z);
            mIcon.transform.localPosition = new Vector3 (mIcon.transform.localPosition.x, -7,
                mIcon.transform.localPosition.z);
        }
    }

    /// <summary>
    ///初始化本地化文本
    /// </summary>
    void InitLabel()
    {
        mEvaluateLabel.text = LocalizationMgr.Get("RewardPetInfoWnd_1");
        mCompoundLabel.text = LocalizationMgr.Get("RewardPetInfoWnd_2");
        mRaceTips.text = LocalizationMgr.Get("RewardPetInfoWnd_3");
        mTypeTips.text = LocalizationMgr.Get("RewardPetInfoWnd_4");
        mMaxLevelTips.text = LocalizationMgr.Get("RewardPetInfoWnd_5");
        mPowerTips.text = LocalizationMgr.Get("RewardPetInfoWnd_6");
        mAttackTips.text = LocalizationMgr.Get("RewardPetInfoWnd_7");
        mDefenceTips.text = LocalizationMgr.Get("RewardPetInfoWnd_8");
        mAgilityTips.text = LocalizationMgr.Get("RewardPetInfoWnd_9");
        mEvolveChangeTips.text = LocalizationMgr.Get("RewardPetInfoWnd_10");
        mEvolveTips.text = LocalizationMgr.Get("RewardPetInfoWnd_11");
    }

    /// <summary>
    ///初始化技能
    /// </summary>
    void RedrawSkill(Property ob)
    {
        // 获取绑定宠物的技能
        LPCArray skills = ob.GetAllSkills();

        for (int i = 0; i < mSkills.Length; i++)
        {
            mSkills[i].GetComponent<SkillItem>().SetBind(-1);
            mSkills[i].GetComponent<SkillItem>().SetSelected(false);

            mSkills[i].SetActive(true);
        }

        // 遍历技能列表
        foreach (LPCValue mks in skills.Values)
        {
            // 获取技能类型
            int skillId = mks.AsArray[0].AsInt;
            int type = SkillMgr.GetSkillPosType(skillId);

            if (type <= 0 || type > mSkills.Length)
                continue;

            SkillItem item = mSkills[type - 1].GetComponent<SkillItem>();

            //获取技能等级;
            int level = ob.GetSkillLevel(skillId);

            item.SetBind(skillId);

            if (!SkillMgr.IsLeaderSkill(skillId))
                item.SetMaxLevel(level);
            else
                item.SetLeader(true);

            item.SetSelected(false);

            //添加点击事件;
            UIEventListener.Get(mSkills[type - 1]).onPress = ClickShowHoverWnd;
        }
    }

    /// <summary>
    ///按下显示悬浮窗口
    /// </summary>
    void ClickShowHoverWnd(GameObject go, bool isPress)
    {
        if (mViewPet == null)
            return;

        SkillItem data = go.GetComponent<SkillItem>();
        if (data == null || mSkillView == null)
            return;

        SkillViewWnd script = mSkillView.GetComponent<SkillViewWnd>();
        if (script == null)
            return;

        //按下
        if (isPress)
        {
            if(data.mSkillId <= 0)
                return;

            data.SetSelected(true);

            script.ShowView(data.mSkillId, mViewPet);

            BoxCollider box = go.GetComponent<BoxCollider>();

            Vector3 boxPos= box.transform.localPosition;

            mSkillView.transform.localPosition = new Vector3 (boxPos.x, boxPos.y + box.size.y / 2, boxPos.z);

            // 限制悬浮在屏幕范围内
            script.LimitPosInScreen();
        }
        else
        {
            data.SetSelected(false);

            // 隐藏悬浮窗口
            script.HideView();
        }
    }

    void CreateMonster()
    {
        //构建参数;
        LPCMapping para = new LPCMapping ();
        para.Add("level", mOriginalPet.Query<int>("level"));
        para.Add("star", mOriginalPet.Query<int>("star"));
        para.Add("rid", Rid.New());
        para.Add("class_id", mOriginalPet.GetClassID());

        int rank = mOriginalPet.GetRank() <= 1 ? 2 : 1;

        para.Add("rank", rank);

        if (mClonePet != null)
            mClonePet.Destroy();

        // 创建一个宠物对象;
        mClonePet = PropertyMgr.CreateProperty(para);
    }

    /// <summary>
    ///根据是否觉醒显示模型和相关数据
    /// </summary>
    void AccordingRankDisplayData(Property ob)
    {
        if (ob == null)
            return;

        //刷新宠物属性信息;
        RedrawPetInfo(ob);

        // 刷新技能信息
        RedrawSkill(ob);

        // 设置绑定宠物模型
        if(isTweenOver)
            ShowModel(ob);

        if (ob.GetRank() <= 1)
        {
            mCurrentState = State.NORMAL;
            mNormalToggle.startsActive = true;
            mEvolveToggle.startsActive = false;
        }
        else
        {
            mCurrentState = State.RANK;
            mNormalToggle.startsActive = false;
            mEvolveToggle.startsActive = true;
        }
    }

    /// <summary>
    /// 显示模型
    /// </summary>
    /// <param name="ob">Ob.</param>
    void ShowModel(Property ob)
    {
        // 获取窗口绑定的模型窗口组件
        ModelWnd pmc = mPetModel.GetComponent<ModelWnd>();
        if (pmc == null)
            return;

        // 载入模型
        pmc.LoadModel(ob, LayerMask.NameToLayer("UI"));
    }

    #region 外部接口

    /// <summary>
    ///绑定数据
    /// </summary>
    public void Bind(Property data, bool isShowMask = true)
    {
        if (data == null)
            return;

        mOriginalPet = data;

        mViewPet = mOriginalPet;

        // 判断怪物是否可以评论
        // 不能显示在图鉴中就不能显示参与评论
        mEvaluateBtn.SetActive((mOriginalPet.Query<int>("show_in_manual") == 1));

        if (data.GetRank() != 0)
        {
            CreateMonster();
        }
        else
        {
            mEvolveToggle.gameObject.SetActive(false);
        }

        AccordingRankDisplayData(mOriginalPet);

        mRankIcon.gameObject.SetActive(true);
        mRankIcon.mainTexture = MonsterMgr.GetTexture(mOriginalPet.GetClassID(), 2);

        mNormalIcon.gameObject.SetActive(true);
        mNormalIcon.mainTexture = MonsterMgr.GetTexture(mOriginalPet.GetClassID(), 0);

        mMask.alpha = isShowMask ? 0.5f : 0.01f;
    }

    #endregion
}
