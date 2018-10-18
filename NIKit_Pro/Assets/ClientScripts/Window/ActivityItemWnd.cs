/// <summary>
/// ActivityItemWnd.cs
/// Created by lic 06/05/2017
/// 活动格子
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class ActivityItemWnd : WindowBase<ActivityItemWnd>
{
    #region 成员变量

    // 背景图片
    public UITexture mBg;
    public GameObject mNew;

    // 活动的开始时间
    public UILabel mActivityTime;

    // 标题
    public UILabel mTitle;
    public UILabel mShadowTitle;

    // 提示标题
    public UILabel mPreTitle;

    // 副标题
    public UILabel mSubTitle;

    // 使魔元素图标
    public UISprite mElement;

    // 使魔头像
    public UITexture mPetIcon;

    // 星级
    public UILabel[] mStars;

    CsvRow mRow = null;

    #endregion

    #region 私有变量

    public LPCMapping ActivityInfo { get; private set; }

    #endregion

    #region 内部函数 

    void Start()
    {
        mNew.GetComponent<UISpriteAnimation>().namePrefix = ConfigMgr.IsCN ? "cnew" : "new";
    }

    /// <summary>
    /// 刷新数据
    /// </summary>
    void Redraw()
    {
        // 初始化控件
        mElement.gameObject.SetActive(false);

        mPetIcon.gameObject.SetActive(false);

        // 前置标题
        if (mPreTitle != null)
            mPreTitle.text = string.Empty;

        mTitle.text = string.Empty;
        mSubTitle.text = string.Empty;
        mActivityTime.text = string.Empty;

        if (mShadowTitle != null)
            mShadowTitle.text = string.Empty;

        if (mRow == null)
            return;

        string path = string.Format("Assets/Art/UI/Activity/Background/{0}.png", mRow.Query<string>("icon"));

        mBg.mainTexture = ResourceMgr.LoadTexture(path);

        string activityId = mRow.Query<string>("activity_id");

        if (mActivityTime != null)
        {
            // 有效时间段列表
            LPCArray validPeriod = ActivityInfo.GetValue<LPCArray>("valid_period");

            // 活动时间描述
            mActivityTime.text = ActivityMgr.GetActivityTimeDesc(activityId, validPeriod);
        }

        LPCMapping extraPara = ActivityInfo.GetValue<LPCMapping>("extra_para");
        if (extraPara == null)
            extraPara = LPCMapping.Empty;

        string title = ActivityMgr.GetActivityTitle(activityId, extraPara);

        // 前置标题
        if (mPreTitle != null)
            mPreTitle.text = ActivityMgr.GetActivityPreTitle(activityId, extraPara);

        // 标题
        if (mTitle != null)
            mTitle.text = title;

        if (mShadowTitle != null)
            mShadowTitle.text = title;

        // 副标题
        if (mSubTitle != null)
            mSubTitle.text = ActivityMgr.GetActivitySubTitle(activityId, extraPara);

        if (extraPara.ContainsKey("pet_id"))
        {
            int classId = extraPara.GetValue<int>("pet_id");

            if (mElement != null)
            {
                // 显示使魔元素图标
                mElement.spriteName = PetMgr.GetElementIconName(MonsterMgr.GetElement(classId));

                mElement.gameObject.SetActive(true);
            }

            if (mPetIcon != null)
            {
                // 加载宠物头像
                mPetIcon.mainTexture = MonsterMgr.GetTexture(classId, MonsterMgr.GetDefaultRank(classId));

                mPetIcon.gameObject.SetActive(true);
            }

            mBg.color = MonsterConst.MonsterElementRGBColorMap[MonsterMgr.GetElement(classId)];

            // 精英圣域
            if (string.Equals(activityId, "pet_dungeon"))
            {
                // 使魔配置数据
                CsvRow row = MonsterMgr.GetRow(classId);
                if (row == null)
                    return;
                int star = row.Query<int>("star");
                for (int i = 0; i < star; i++)
                {
                    mStars[i].gameObject.SetActive(true);
                    mStars[i].transform.localPosition = new Vector3(mStars[i].width * (i - (star - 1) * 0.5f), 0, 0);
                }
            }
        }

        SetNew();
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="itemId">Item identifier.</param>
    public void BindData(LPCMapping activityInfo, CsvRow row)
    {
        if (activityInfo == null)
            return;

        ActivityInfo = activityInfo;

        mRow = row;

        Redraw();
    }

    /// <summary>
    /// Determines whether this instance cancel new.
    /// </summary>
    /// <returns><c>true</c> if this instance cancel new; otherwise, <c>false</c>.</returns>
    public void SetNew()
    {
        if (ActivityMgr.IsNewActivity(ME.user, ActivityInfo.GetValue<string>("cookie")))
        {
            mNew.SetActive(true);
            mNew.GetComponent<UISpriteAnimation>().ResetToBeginning();
        }
        else
            mNew.SetActive(false);
    }

    #endregion
}
