/// <summary>
/// CustomPetItemWnd.cs
/// Created by lic 2016-6-17
/// 宠物格子
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;

public class CustomPetItemWnd : WindowBase<CustomPetItemWnd>
{
    #region 成员变量
    public UISprite[] mStars;              // 星级
    public UITexture mIcon;                // 宠物
    public UILabel mLevel;                 // 等级
    public UISprite mSelect;               // 选中状态
    public UISprite mLeaderSkillBg;        // 队长技能
    public UITexture mLeaderSkillIcon;     // 技能图标
    public UISprite mBg;                   // 背景
    public UISprite mLock;                 // 宠物当前状态
    public UISprite mCover;                // 遮盖
    public UISprite mMax;                  // 满
    public UISprite mLeaderIcon;           // 队长图标
    public UISprite mSynthesis;            // 合成
    public UISpriteAnimation mAnima;       // 动画
    public UISpriteAnimation mNewPetTips;  // 新宠物提示

    public UILabel mLeaderLb;

    [HideInInspector]
    public Property
    item_ob;

    [HideInInspector]
    public bool
    isSelected = false;

    [HideInInspector]
    public bool mIsLeader = false;

    [HideInInspector]
    public bool mIsAttack = false;

    //默认icon
    private string iconName = "emptypet";

    // 是否显示最大等级
    private bool isShowMaxLeve = false;

    // 是否显示领导技能
    private bool isShowLeaderSkill = false;

    // 是否显示可合成
    private bool isShowSynthetic = false;

    // 是否显示宠物等级
    private bool isShowLevel = true;

    private bool mIsShowLeaderIcon = false;

    // 窗口唯一标识
    private string instanceID = string.Empty;

    #endregion

    #region 内部函数

    void Awake()
    {
        instanceID = gameObject.GetInstanceID().ToString();
    }

    // Use this for initialization
    void Start()
    {
        if(mNewPetTips != null)
            mNewPetTips.namePrefix = ConfigMgr.IsCN ? "cnew" : "new";

        // 初始化窗口
        Redraw();
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    void OnDestroy()
    {
        // 对象不存在
        if (item_ob == null)
            return;

        // 取消关注
        item_ob.dbase.RemoveTriggerField(instanceID);
    }

    /// <summary>
    /// 绑定宠物事件回调
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    private void OnPetItemChange(object param, params object[] paramEx)
    {
        // 当前界面没有绑定宠物不处理
        if (item_ob == null)
            return;

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        if (mLeaderIcon != null)
            mLeaderIcon.alpha = (mIsShowLeaderIcon ? 1f : 0f);

        if (item_ob == null)
        {
            mLevel.alpha = 0f;
            mLeaderSkillBg.alpha = 0f;
            mMax.alpha = 0f;

            for(int i = 0; i < mStars.Length; i++)
                mStars[i].alpha = 0f;

            // 显示头像
            mIcon.alpha = 1f;

            string resPath = MonsterMgr.GetIconResPath(iconName);
            mIcon.mainTexture = ResourceMgr.LoadTexture(resPath);

            return;
        }

        int classId = item_ob.GetClassID();

        mIcon.alpha = 1f; 
        mIcon.mainTexture = MonsterMgr.GetTexture(classId, item_ob.GetRank());

        int star = item_ob.GetStar();
        string StarName = PetMgr.GetStarName(item_ob.GetRank());

        // 遍历星星逐个设置显示
        for (int i = 0; i < mStars.Length; i++)
        {
            // 隐藏星级
            if (i >= star)
            {
                // 隐藏控件
                mStars[i].alpha = 0f;
                continue;
            }

            // 设置星星图片
            mStars[i].spriteName = StarName;
            mStars[i].alpha = 1f;
        }

        // 等级需用图文混排的形式
        int Level = item_ob.GetLevel();
        mLevel.alpha = (isShowLevel ? 1f : 0f);

        if(MonsterMgr.IsMaxLevel(item_ob) && isShowMaxLeve)
            mMax.alpha = 1f;
        else
            mMax.alpha = 0f;

        mLevel.text = GET_LEVEL_ALIAS.Call(Level.ToString());

        // 判断是否需要显示mSynthesis
        if (mSynthesis != null)
            mSynthesis.alpha = ((isShowSynthetic && MonsterMgr.IsSyntheMaterial(classId)) ? 1f : 0f);

        if (isShowLeaderSkill)
        {
            // 取宠物身上的队长技能
            LPCMapping leaderSkill = SkillMgr.GetLeaderSkill(item_ob);

            if (leaderSkill.Count == 0)
            {
                // 隐藏控件
                mLeaderSkillBg.alpha = 0f;
                mLeaderSkillIcon.alpha = 0f;
                return;
            }

            // 遍历玩家技能数据
            int leaderSkillId = 0;
            foreach (int skillId in leaderSkill.Keys)
            {
                if (skillId != 0)
                    leaderSkillId = skillId;
            }

            // 设置图片资源
            mLeaderSkillIcon.mainTexture = SkillMgr.GetTexture(leaderSkillId);

            // 显示控件
            mLeaderSkillBg.alpha = 1f;
            mLeaderSkillIcon.alpha = 1f;
        }
        else
        {
            // 隐藏控件
            mLeaderSkillBg.alpha = 0f;
            mLeaderSkillIcon.alpha = 0f;
        }
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 设置选中(isShow表示是否显示选中“√”)
    /// </summary>
    public void SetSelected(bool is_selected, bool isShow = true)
    {
        isSelected = is_selected;

        // 设置选中高光,同时播放动画
        if(is_selected)
        {
            mBg.spriteName = "PetSelectBg";
            mBg.gameObject.GetComponent<TweenAlpha>().enabled = true;
            mBg.gameObject.GetComponent<TweenAlpha>().ResetToBeginning();
            mSelect.alpha = isShow ? 1f : 0f;
        }
        else
        {
            mBg.spriteName = "PetIconBg";
            mBg.gameObject.GetComponent<TweenAlpha>().enabled = false;
            mBg.alpha = 1.0f;

            mSelect.alpha = 0f;
        }
    }

    /// <summary>
    /// 设置新宠物提示
    /// </summary>
    public void SetNewPetTips(Property ob)
    {
        if (mNewPetTips == null)
            return;

        if (ob == null)
        {
            mNewPetTips.gameObject.SetActive(false);
            return;
        }

        // 播放序列帧动画
        if (BaggageMgr.IsNew(ob))
        {
            mNewPetTips.gameObject.SetActive(true);
            mNewPetTips.ResetToBeginning();
        }
        else
        {
            mNewPetTips.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 是否显示领导技能(默认是显示)
    /// </summary>
    /// <param name="isShow">If set to <c>true</c> is show.</param>
    public void ShowLeaderSkill(bool isShow)
    {
        isShowLeaderSkill = isShow;
    }

    /// <summary>
    /// 是否显示合成(默认是不显示)
    /// </summary>
    /// <param name="isShow">If set to <c>true</c> is show.</param>
    public void ShowSynthetic(bool isShow)
    {
        isShowSynthetic = isShow;
    }

    /// <summary>
    /// 显示当前宠物状态（同时遮盖）
    /// </summary>
    /// <param name="iconName">Icon name.</param>
    public void SetLock(string icon, bool isShowLockCover = false)
    {
        if(string.IsNullOrEmpty(icon))
        {
            mLock.alpha = 0f;
            mCover.alpha = 0f;
        }
        else
        {
            mLock.alpha = 1f;
            mLock.spriteName = icon;
            mCover.alpha = (isShowLockCover ? 0.65f : 0f);
        }
    }

    /// <summary>
    /// 是否显示遮盖
    /// </summary>
    /// <param name="isShow">If set to <c>true</c> is show.</param>
    public void ShowCover(bool isShow)
    {
        mCover.alpha = (isShow ? 0.65f : 0f);
    }

    /// <summary>
    /// 是否显示队长图标
    /// </summary>
    public void ShowLeaderIcon(bool isShow)
    {
        mIsShowLeaderIcon = isShow;
        mLeaderIcon.alpha = (isShow ? 1f : 0f);

        mIsLeader = isShow;
    }

    public void SetLeader(bool isLeader)
    {
        mIsLeader = isLeader;
    }

    /// <summary>
    /// 是否显示最大等级(默认是不显示)
    /// </summary>
    /// <param name="isShow">If set to <c>true</c> is show.</param>
    public void ShowMaxLevel(bool isShow)
    {
        isShowMaxLeve = isShow;
    }

    /// <summary>
    /// 窗口绑定实体
    /// </summary>
    public void SetBind(Property ob)
    {
        if (string.IsNullOrEmpty(instanceID))
            instanceID = gameObject.GetInstanceID().ToString();

        // 取消关注
        if (item_ob != null)
            item_ob.dbase.RemoveTriggerField(instanceID);

        if(ob != null)
            ob.dbase.RegisterTriggerField(instanceID, new string[]
            {
                "level",
                "star",
                "rank"
            }, new CallBack(OnPetItemChange));

        // 重置绑定对象
        item_ob = ob;

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 设置未填充图标
    /// </summary>
    public void SetIcon(string spriteName)
    {
        // 为空或null表示将图标滞空
        if(string.IsNullOrEmpty(spriteName))
            spriteName = "emptypet";
        else
            iconName = spriteName;

        // 显示图片
        mIcon.alpha = 1f;

        string resPath = MonsterMgr.GetIconResPath(spriteName);
        mIcon.mainTexture = ResourceMgr.LoadTexture(resPath);
    }

    public void ShowLeaderText(string content)
    {
        if (mLeaderLb == null)
            return;

        if (string.IsNullOrEmpty(content))
        {
            mLeaderLb.alpha = 0f;
            return;
        }

        // 设置文本信息
        mLeaderLb.text = content;
        mLeaderLb.alpha = 1f;
    }

    /// <summary>
    /// 是否显示宠物等级(默认是显示)
    /// </summary>
    public void ShowLevel(bool isShow)
    {
        isShowLevel = isShow;
    }

    /// <summary>
    /// 设置选中图标位置(默认为右上角)
    /// </summary>
    /// <param name="pos">Position.</param>
    public void SetSelectPos(SelectPos pos)
    {
        Vector3 position = new Vector3 (0f, 0f, 0f);

        switch (pos) 
        {
            case SelectPos.BottonRightCorner:
                 position = new Vector3 (35f, -32f, 0f);
                 break;
            case SelectPos.BottomLeftCorner:
                position = new Vector3 (-35f, -32f, 0f);
                break;
            case SelectPos.Center:
                position = new Vector3 (0f, 0f, 0f);
                break;
            case SelectPos.TopLeftCorner:
                position = new Vector3 (-35f, 39.2f, 0f);
                break;
            default:
                 position = new Vector3 (35f, 39.2f, 0f);
                 break;
        }

        mSelect.transform.localPosition = position;
    }

    /// <summary>
    /// 设置动画
    /// </summary>
    /// <param name="isOpen">If set to <c>true</c> is open.</param>
    public void SetAnima(bool isOpen)
    {
        if (isOpen)
        {
            mAnima.gameObject.SetActive (true);
            mAnima.enabled = true;
        } else
            mAnima.gameObject.SetActive (false);
    }

    #endregion
}
