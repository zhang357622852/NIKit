/// <summary>
/// ManualAttribWnd.cs
/// Created by fengsc 2018/01/02
/// 使魔图鉴属性面板
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class ManualAttribWnd : WindowBase<ManualAttribWnd>
{
    #region 成员变量

    // 名称
    public UILabel mNameLb;
    public UILabel mName;

    // 星级
    public UILabel mStarLb;
    public UISprite[] mStars;

    // 属性
    public UILabel mElementLb;
    public UISprite mElement;

    // 种族
    public UILabel mRaceLb;
    public UILabel mRace;

    // 类型
    public UILabel mTypeLb;
    public UILabel mType;

    // 最大等级
    public UILabel mMaxLevelLb;
    public UILabel mMaxLevel;

    // 生命
    public UILabel mLifeLb;
    public UILabel mLife;

    // 攻击力
    public UILabel mAttackLb;
    public UILabel mAttack;

    // 防御力
    public UILabel mDefenceLb;
    public UILabel mDefence;

    // 敏捷
    public UILabel mAgilityLb;
    public UILabel mAgility;

    public GameObject mModelSprite;

    // 使魔模型
    public ModelWnd mModel;

    // 评价
    public GameObject mEvaluateBtn;
    public UILabel mEvaluateBtnLb;

    // 使魔技能
    public GameObject[] mSkills;

    public GameObject mAwakeChangeBtn;
    public UILabel mAwakeChangeBtnLb;

    public ManualPetItemWnd mPetItemWnd;

    public RichTextContent mRichTextContent;

    public UILabel mNoAwake;

    public SkillViewWnd mSkillView;

    public ElementSelectWnd mElementSelectWnd;

    // 宠物数据
    LPCMapping mPetData;

    // 宠物对象
    Property mAwakeOb;

    Property mNoAwakeOb;

    // 当前宠物对象
    Property mCurrentOb;

    #endregion

    // Use this for initialization
    void Start ()
    {
        // 初始化文本
        InitLabel();

        // 注册事件
        RegisterEvent();
    }

    void OnDestroy()
    {
        // 卸载宠物对象
        if (mAwakeOb != null)
            mAwakeOb.Destroy();

        // 卸载宠物对象
        if (mNoAwakeOb != null)
            mNoAwakeOb.Destroy();

        if (mCurrentOb != null)
            mCurrentOb.Destroy();

        EventMgr.UnregisterEvent("ManualAttribWnd");
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    void InitLabel()
    {
        mNameLb.text = LocalizationMgr.Get("PetManualWnd_10");
        mStarLb.text = LocalizationMgr.Get("PetManualWnd_11");
        mElementLb.text = LocalizationMgr.Get("PetManualWnd_12");
        mRaceLb.text = LocalizationMgr.Get("PetManualWnd_13");
        mTypeLb.text = LocalizationMgr.Get("PetManualWnd_14");
        mMaxLevelLb.text = LocalizationMgr.Get("PetManualWnd_15");
        mLifeLb.text = LocalizationMgr.Get("PetManualWnd_16");
        mAttackLb.text = LocalizationMgr.Get("PetManualWnd_17");
        mDefenceLb.text = LocalizationMgr.Get("PetManualWnd_18");
        mAgilityLb.text = LocalizationMgr.Get("PetManualWnd_19");

        mEvaluateBtnLb.text = LocalizationMgr.Get("PetManualWnd_21");
        mAwakeChangeBtnLb.text = LocalizationMgr.Get("PetManualWnd_22");

        mNoAwake.text = LocalizationMgr.Get("PetManualWnd_23");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mEvaluateBtn).onClick = OnClickEvaluateBtn;
        UIEventListener.Get(mAwakeChangeBtn).onClick = OnClickAwakeChangeBtn;

        for (int i = 0; i < mSkills.Length; i++)
            UIEventListener.Get(mSkills[i]).onPress = OnSkillBtn;

        EventMgr.RegisterEvent("ManualAttribWnd", EventMgrEventType.EVENT_CLICK_PICTURE, WhenClickPicture);
    }

    /// <summary>
    /// 设置选中
    /// </summary>
    private void OnSkillBtn(GameObject go, bool isPress)
    {
        if (mCurrentOb == null)
            return;

        SkillItem item = go.GetComponent<SkillItem>();
        if (item == null)
            return;

        int skillId = item.mSkillId;

        if (skillId <= 0)
            return;

        // 默认先把所有的技能格子全部取消选中
        for (int i = 0; i < mSkills.Length; i++)
            mSkills[i].GetComponent<SkillItem>().SetSelected(false);

        //按下
        if (isPress)
        {
            // 设置选中
            item.SetSelected(true);

            BoxCollider box = go.GetComponent<BoxCollider>();

            // box相对this.transform位置
            Vector3 relativePos = transform.InverseTransformPoint(box.transform.position);

            mSkillView.transform.localPosition = new Vector3(relativePos.x, relativePos.y + box.size.y / 2, relativePos.z);

            // 限制悬浮在屏幕内
            mSkillView.LimitPosInScreen();

            // 显示悬浮窗口
            mSkillView.ShowView(skillId, mCurrentOb, true);
        }
        else
        {
            item.SetSelected(false);

            // 隐藏悬浮窗口
            mSkillView.HideView();
        }
    }

    /// <summary>
    /// 图片点击回调
    /// </summary>
    void WhenClickPicture(int eventId, MixedValue para)
    {
        List<object> args = para.GetValue<List<object>>();
        if (args == null)
            return;

        int skillID = (int)args[1];
        if (skillID <= 0)
            return;

        bool isPress = (bool)args[0];

        if (mSkillView == null)
            return;

        if (isPress)
        {
            // 显示悬浮窗口
            mSkillView.ShowView(skillID, mCurrentOb, true);

            // box相对this.transform位置
            Vector3 relativePos = transform.InverseTransformPoint((Vector3) args[2]);

            Vector3 boxSize = (Vector3) args[3];

            mSkillView.transform.localPosition = new Vector3(relativePos.x, relativePos.y + boxSize.y / 2, relativePos.z);

            // 限制悬浮窗口在屏幕范围内
            mSkillView.LimitPosInScreen();
        }
        else
        {
            // 隐藏悬浮窗口
            mSkillView.HideView();
        }
    }

    /// <summary>
    /// 玩家评价按钮点击事件
    /// </summary>
    void OnClickEvaluateBtn(GameObject go)
    {
        // 打开玩家评价窗口
        GameObject wnd = WindowMgr.OpenWnd(AppraiseWnd.WndType);
        if (wnd == null)
            return;

        // 绑定数据
        wnd.GetComponent<AppraiseWnd>().Bind(mCurrentOb.GetClassID(), mCurrentOb.GetRank(), mCurrentOb.GetStar());
    }

    /// <summary>
    /// 觉醒变化按钮点击事件
    /// </summary>
    void OnClickAwakeChangeBtn(GameObject go)
    {
        if (mCurrentOb == null)
            return;

        // 该使魔不能觉醒
        if (mPetData.GetValue<int>("rank") == MonsterConst.RANK_UNABLEAWAKE)
        {
            DialogMgr.Notify(LocalizationMgr.Get("PetManualWnd_23") + "。");

            return;
        }

        // 还未收集到该使魔，无法查看其觉醒变化。
        if (mPetData.GetValue<int>("is_user") != 1)
        {
            DialogMgr.Notify(LocalizationMgr.Get("PetManualWnd_24"));
            return;
        }

        if (mCurrentOb.GetRank() == MonsterConst.RANK_AWAKED)
        {
            mCurrentOb = mNoAwakeOb;
        }
        else
        {
            mCurrentOb = mAwakeOb;
        }

        // 重新绘制属性面板
        Redraw();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        if (mCurrentOb == null)
            return;

        // 使魔名称
        mName.text = LocalizationMgr.Get(mCurrentOb.GetName());

        for (int i = 0; i < mStars.Length; i++)
            mStars[i].gameObject.SetActive(false);

        int rank = mCurrentOb.GetRank();

        string starName = PetMgr.GetStarName(rank);

        int star = mCurrentOb.GetStar();

        for (int i = 0; i < star; i++)
        {
            mStars[i].gameObject.SetActive(true);

            mStars[i].spriteName = starName;
        }

        int classId = mCurrentOb.GetClassID();

        mElementSelectWnd.Bind(classId, mCurrentOb.Query<int>("element"), new CallBack(OnCallBack));

        int isUser = mPetData.GetValue<int>("is_user");

        // 属性
        mElement.spriteName = PetMgr.GetElementIconName(MonsterMgr.GetElement(classId));

        // 种族
        mRace.text = MonsterConst.MonsterRaceTypeMap[MonsterMgr.GetRace(classId)];

        // 类型
        if (isUser == 1)
            mType.text = MonsterConst.MonsterStyleTypeMap[MonsterMgr.GetType(classId)];
        else
            mType.text = LocalizationMgr.Get("PetManualWnd_20");

        // 最大等级
        mMaxLevel.text = MonsterMgr.GetMaxLevel(star).ToString();

        // 生命
        if (isUser == 1)
            mLife.text = mCurrentOb.QueryAttrib("max_hp").ToString();
        else
            mLife.text = LocalizationMgr.Get("PetManualWnd_20");

        // 攻击
        if (isUser == 1)
            mAttack.text = mCurrentOb.QueryAttrib("attack").ToString();
        else
            mAttack.text = LocalizationMgr.Get("PetManualWnd_20");

        // 防御
        if (isUser == 1)
            mDefence.text = mCurrentOb.QueryAttrib("defense").ToString();
        else
            mDefence.text = LocalizationMgr.Get("PetManualWnd_20");

        // 敏捷
        if (isUser == 1)
            mAgility.text = mCurrentOb.QueryAttrib("agility").ToString();
        else
            mAgility.text = LocalizationMgr.Get("PetManualWnd_20");

        // 绘制宠物技能
        RedrawSkill();

        if (rank != MonsterConst.RANK_UNABLEAWAKE)
        {
            // 觉醒描述
            string desc = MonsterMgr.GetEvolutionDesc(mCurrentOb);

            mRichTextContent.clearContent();

            mRichTextContent.ParseValue(desc);

            mRichTextContent.gameObject.SetActive(true);

            mNoAwake.gameObject.SetActive(false);
        }
        else
        {
            mNoAwake.gameObject.SetActive(true);

            mRichTextContent.gameObject.SetActive(false);
        }

        LPCMapping para = LPCMapping.Empty;

        para.Append(mPetData);

        para["rank"] = LPCValue.Create(rank);

        // 绑定宠物数据
        mPetItemWnd.Bind(para);

        mModel.UnLoadModel();

        if (isUser == 1)
        {
            // 载入模型
            mModel.LoadModel(mCurrentOb, LayerMask.NameToLayer("UI"));

            mModelSprite.SetActive(false);
        }
        else
        {
            mModelSprite.SetActive(true);
        }
    }

    void OnCallBack(object para, params object[] param)
    {
        // 刷新窗口
        int classId = (int) param[0];

        LPCMapping data = LPCMapping.Empty;

        data.Append(mPetData);

        data["class_id"] = LPCValue.Create(classId);

        data["rid"] = LPCValue.Create(Rid.New());

        int rank = mCurrentOb.GetRank();

        data["rank"] = LPCValue.Create(rank);

        data["is_user"] = LPCValue.Create(ManualMgr.IsCompleted(ME.user, classId, rank) ? 1 : 0);

        // 绑定数据刷新窗口
        Bind(data);
    }

    /// <summary>
    /// 绘制宠物技能
    /// </summary>
    void RedrawSkill()
    {
        // 默认先把所有的技能格子全部置空
        for (int i = 0; i < mSkills.Length; i++)
        {
            mSkills[i].GetComponent<SkillItem>().SetBind(-1);
            mSkills[i].GetComponent<SkillItem>().SetSelected(false);
        }

        // 当前绑定宠物为空
        if(mCurrentOb == null)
            return;

        // 获取绑定宠物的技能
        LPCArray skillInfo = mCurrentOb.GetAllSkills();

        // 对字典按key（skillid）进行排序
        foreach (LPCValue mks in skillInfo.Values)
        {
            // 获取技能类型
            int skillId = mks.AsArray[0].AsInt;
            int type = SkillMgr.GetSkillPosType(skillId);

            if (type <= 0 || type > mSkills.Length)
                continue;

            SkillItem item = mSkills[type - 1].GetComponent<SkillItem>();

            item.SetBind(skillId);

            // 判断是否为队长技能
            if (!SkillMgr.IsLeaderSkill(skillId))
            {
                item.SetLevel(mCurrentOb.GetSkillLevel(skillId));
                item.SetLeader(false);
            }
            else
            {
                item.SetLevel(-1);
                item.SetLeader(true);
            }
        }
    }

    /// <summary>
    /// 创建宠物对象
    /// </summary>
    void CreateProperty()
    {
        // 卸载宠物对象
        if (mAwakeOb != null)
        {
            mAwakeOb.Destroy();

            mAwakeOb = null;
        }

        // 卸载宠物对象
        if (mNoAwakeOb != null)
        {
            mNoAwakeOb.Destroy();

            mNoAwakeOb = null;
        }

        // 卸载宠物对象
        if (mCurrentOb != null)
        {
            mCurrentOb.Destroy();

            mCurrentOb = null;
        }

        LPCMapping para = LPCMapping.Empty;

        para.Append(mPetData);

        int rank = mPetData.GetValue<int>("rank");

        if (rank == MonsterConst.RANK_AWAKED)
        {
            mAwakeOb = PropertyMgr.CreateProperty(mPetData);

            para["rid"] = LPCValue.Create(Rid.New());
            para["rank"] = LPCValue.Create(MonsterConst.RANK_UNAWAKE);

            mNoAwakeOb = PropertyMgr.CreateProperty(para);

            mCurrentOb = mAwakeOb;
        }
        else if(rank == MonsterConst.RANK_UNAWAKE)
        {
            mNoAwakeOb = PropertyMgr.CreateProperty(mPetData);

            para["rid"] = LPCValue.Create(Rid.New());
            para["rank"] = LPCValue.Create(MonsterConst.RANK_AWAKED);

            mAwakeOb = PropertyMgr.CreateProperty(para);

            mCurrentOb = mNoAwakeOb;
        }
        else
        {
            mCurrentOb = PropertyMgr.CreateProperty(mPetData);
        }
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data)
    {
        if (data == null || data.Count == 0)
            return;

        mPetData = data;

        // 创建宠物对象
        CreateProperty();

        // 判断怪物是否可以评论
        // 不能显示在图鉴中就不能显示参与评论
        if (mCurrentOb != null)
            mEvaluateBtn.SetActive((mCurrentOb.Query<int>("show_in_manual") == 1));
        else
            mEvaluateBtn.SetActive(false);

        // 重绘窗口
        Redraw();
    }
}
