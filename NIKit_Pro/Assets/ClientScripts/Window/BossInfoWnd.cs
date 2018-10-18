/// <summary>
/// BossInfoWnd.cs
/// Created by fengsc 2017/01/05
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class BossInfoWnd : WindowBase<BossInfoWnd>
{
    #region 成员变量

    // 窗口关闭按钮
    public GameObject mCloseBtn;

    // 宠物名称
    public UILabel mName;

    public UITexture mIcon;

    // 宠物元素图标
    public UISprite mElementIcon;

    // 元素名称
    public UILabel mElementName;

    public UILabel mLevelTips;

    public UILabel mLevel;

    public UILabel mSkillTips;

    public UISprite[] mStars;

    // 描述信息
    public UILabel mDesc;

    // 技能格子数组
    public GameObject[] mSkills;

    // 确认按钮
    public GameObject mConfirmBtn;
    public UILabel mConfirmBtnLb;

    public GameObject mSkillView;

    public GameObject mMask;

    public TweenAlpha mTweenAlpha;

    public TweenScale mTweenScale;

    // 宠物对象
    Property mOb;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 注册事件
        RegisterEvent();

        InitLocaText();

        if (mTweenAlpha == null || mTweenScale == null)
            return;

        // 播放动画
        mTweenAlpha.PlayForward();

        mTweenScale.PlayForward();

        // 重置动画组件
        mTweenAlpha.ResetToBeginning();

        mTweenScale.ResetToBeginning();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    void InitLocaText()
    {
        mConfirmBtnLb.text = LocalizationMgr.Get("BossInfoWnd_7");

        mLevelTips.text = LocalizationMgr.Get("BossInfoWnd_8");

        mSkillTips.text = LocalizationMgr.Get("BossInfoWnd_10");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mCloseBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mConfirmBtn).onClick = OnClickCloseBtn;
        UIEventListener.Get(mMask).onClick = OnClickCloseBtn;

        if (mTweenScale == null)
            return;

        EventDelegate.Add(mTweenScale.onFinished, OnTweenFinish);
    }

    /// <summary>
    /// tween动画播放完后回调
    /// </summary>
    void OnTweenFinish()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 宠物名称
        mName.text = mOb.Short();

        // 显示元素图标
        mElementIcon.spriteName = PetMgr.GetElementIconName(MonsterMgr.GetElement(mOb.GetClassID()));

        mLevel.text = string.Format(LocalizationMgr.Get("BossInfoWnd_9"), mOb.GetLevel());

        for (int i = 0; i < mStars.Length; i++)
            mStars[i].gameObject.SetActive(false);

        string starName = PetMgr.GetStarName(mOb.GetRank());

        for (int i = 0; i < mOb.GetStar(); i++)
        {
            mStars[i].spriteName = starName;
            mStars[i].gameObject.SetActive(true);
        }

        string iconName = MonsterMgr.GetIcon(mOb.GetClassID(), mOb.GetRank());

        string path = string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconName);

        mIcon.mainTexture = ResourceMgr.LoadTexture(path);

        // 获取宠物的所有技能
        LPCArray skills = mOb.GetAllSkills();

        for (int i = 0; i < mSkills.Length; i++)
        {
            mSkills[i].GetComponent<SkillItem>().SetBind(-1);
            mSkills[i].GetComponent<SkillItem>().SetSelected(false);

            mSkills[i].SetActive(true);
        }

        // 遍历各个技能
        foreach (LPCValue mks in skills.Values)
        {
            // 获取技能类型
            int skillId = mks.AsArray[0].AsInt;
            int type = SkillMgr.GetSkillPosType(skillId);

            if (type <= 0 || type > mSkills.Length)
                continue;

            GameObject go = mSkills[type - 1];
            if (go == null)
                continue;

            go.SetActive(true);

            SkillItem item = go.GetComponent<SkillItem>();

            //获取技能等级;
            int level = mOb.GetSkillLevel(skillId);

            item.SetBind(skillId);

            if (! SkillMgr.IsLeaderSkill(skillId))
                item.SetMaxLevel(level);

            item.SetLeader(false);

            item.SetSelected(false);

            //添加点击事件;
            UIEventListener.Get(mSkills[type - 1]).onPress = OnPressSkillItem;
        }

        if (skills.Count < 5)
        {
            for (int i = 0; i < mSkills.Length; i++)
            {
                if (i < 4)
                {
                    mSkills[i].transform.localPosition = new Vector3(
                        mSkills[i].transform.localPosition.x,
                        -60,
                        mSkills[i].transform.localPosition.z);
                }
                else
                {
                    mSkills[i].SetActive(false);
                }
            }
        }
    }

    /// <summary>
    /// 窗口关闭按钮点击事件
    /// </summary>
    void OnClickCloseBtn(GameObject go)
    {
        // 销毁当前窗口
        WindowMgr.DestroyWindow(gameObject.name);
    }

    /// <summary>
    /// 技能格子点击事件
    /// </summary>
    void OnPressSkillItem(GameObject go, bool isPress)
    {
        SkillItem data = go.GetComponent<SkillItem>();
        if (data == null)
            return;

        SkillViewWnd script = mSkillView.GetComponent<SkillViewWnd>();
        if (script == null)
            return;

        //按下
        if (isPress)
        {
            if (data.mSkillId <= 0)
                return;

            data.SetSelected(true);

            BoxCollider box = go.GetComponent<BoxCollider>();

            Vector3 boxPos = box.transform.localPosition;

            mSkillView.transform.localPosition = new Vector3(boxPos.x, boxPos.y + box.size.y / 2, boxPos.z);

            mSkillView.SetActive(true);

            // 显示悬浮窗口
            script.ShowView(data.mSkillId, mOb);

            // 限制悬浮窗口在屏幕范围内
            script.LimitPosInScreen();
        }
        else
        {
            data.SetSelected(false);

            // 隐藏悬浮窗口
            script.HideView();
        }
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
   public void Bind(Property ob)
   {
        if (ob == null)
            return;

        mOb = ob;

        // 绘制窗口
        Redraw();
    }
}
