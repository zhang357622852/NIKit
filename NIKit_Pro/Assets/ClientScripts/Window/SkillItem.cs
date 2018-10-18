/// <summary>
/// SkillItem.cs
/// Created by lic 7/7/2016
/// 技能格子
/// </summary>

using UnityEngine;
using System.Collections;

public class SkillItem : WindowBase<SkillItem>
{

    public UITexture mIcon;
    public GameObject mCover;
    public UILabel  mCdLb;
    public UILabel  mLevel;
    public UILabel  mLeader;
    public UISprite mBg;
    public GameObject[] mMp;
    public UISpriteAnimation mPassiveAni;

    #region 属性

    /// <summary>
    /// 窗口绑定对象
    /// </summary>
    /// <value>The item ob.</value>
    public int mSkillId { get; private set; }

    /// <summary>
    /// 窗口选择状态
    /// </summary>
    public bool IsSelected { get; private set; }

    public delegate void ItemClickedDelegate(int skillId);     // 技能项被选中事件

    public ItemClickedDelegate mOnItemClicked = null;

    #endregion

    #region 内部函数

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
       
    }

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        mLeader.text = LocalizationMgr.Get("SkillItemWnd_1");
    }

    /// <summary>
    /// 刷新窗口
    /// 为了将skillItem做的尽量通用，
    /// 此处只设置技能图标，其他等级
    /// CD等由各个界面自行调用接口显示
    /// </summary>
    private void Redraw()
    {
        mCover.SetActive(false);
        mCdLb.gameObject.SetActive(false);
        mLevel.gameObject.SetActive(false);
        mLeader.gameObject.SetActive(false);

        for(int i = 0; i< mMp.Length; i++)
        {
            mMp[i].SetActive(false);
        }

        if(mSkillId <= 0)
        {
            string path = SkillMgr.GetIconResPath("emptySkillItem");
            mIcon.mainTexture = ResourceMgr.LoadTexture(path);

            mLevel.gameObject.SetActive(false);
            mLeader.gameObject.SetActive(false);
            mPassiveAni.gameObject.SetActive(false);
            return;
        }

        // 取得技能对应的icon
        mIcon.mainTexture = SkillMgr.GetTexture(mSkillId);

        // 如果是被动技能，显示光效
        if(SkillMgr.IsPassiveSkill(mSkillId))
        {
            // 队长技能不显示光效
            if (!SkillMgr.IsLeaderSkill(mSkillId))
            {
                mPassiveAni.gameObject.SetActive(true);
                mPassiveAni.ResetToBeginning();
            }
        }
        else
            mPassiveAni.gameObject.SetActive(false);
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 设置选中
    /// </summary>
    public void SetSelected(bool is_selected)
    {
        IsSelected = is_selected;

        // 设置选中高光,同时播放动画
        if (IsSelected)
            mBg.spriteName = "skill_select";
        else
            mBg.spriteName = "skill";
    }

    ///<summary>
    /// 设置CD时间
    ///</summary>
    public void SetCd(int cd)
    {
        if(cd <= 0)
        {
            mCdLb.gameObject.SetActive(false);
            return;
        }
            
        mCdLb.gameObject.SetActive(true);
        mCdLb.text = cd.ToString();
    }

    /// <summary>
    /// 设置Mp
    /// </summary>
    /// <param name="mp">Mp.</param>
    public void SetMp(int mp, bool isMpEnough = true)
    {
        if(mp < 0)
            mp = 0;

        if(mp > mMp.Length)
            mp = mMp.Length;

        if(mp%2 == 0)
            mMp[0].transform.parent.localPosition = new Vector3(10.5f, 0f, 0f);
        else
            mMp[0].transform.parent.localPosition = new Vector3(0f, 0f, 0f);

        for(int i = 0; i< mMp.Length; i++)
        {
            if(i < mp)
                mMp[i].SetActive(true);
            else
                mMp[i].SetActive(false);
        }
    }

    /// <summary>
    /// 设置遮罩
    /// </summary>
    /// <param name="isCover">If set to <c>true</c> is cover.</param>
    public void SetCover(bool isCover)
    {
        if(isCover)
            mCover.SetActive(true);
        else
            mCover.SetActive(false);
    }

    /// <summary>
    /// 显示等级(队长技能显示leader)
    /// </summary>
    public void SetLevel(int level)
    {
        // 必须先赋值再显示等级
        if(mSkillId <= 0)
        {
            mLevel.gameObject.SetActive(false);
            return;
        }

        if(level <= 0)
            return;

        mLevel.text = level.ToString();

        mLevel.gameObject.SetActive(true);
    }

    /// <summary>
    /// 显示leader
    /// </summary>
    public void SetLeader(bool isShow)
    {
        mLeader.gameObject.SetActive(isShow);
    }

    /// <summary>
    /// 显示等级以及最大等级
    /// </summary>
    public void SetMaxLevel(int level)
    {
        // 技能的最大等级
        int maxLevel = SkillMgr.GetSkillMaxLevel(mSkillId);

        mLevel.text = string.Format("{0}/{1}", level, maxLevel);

        mLevel.gameObject.SetActive(true);
    }

    /// <summary>
    /// 窗口绑定实体
    /// </summary>
    public void SetBind(int skillID)
    {
        // 重置绑定对象
        mSkillId = skillID;

        // 重绘窗口
        Redraw();
    }

    public void OnItemClick(GameObject ob)
    {
        if (mOnItemClicked != null)
        {
            mOnItemClicked(mSkillId);
        }
    }

    public ItemClickedDelegate Callback
    {
        get { return mOnItemClicked; }
        set { mOnItemClicked = value; }
    }

    #endregion
}
