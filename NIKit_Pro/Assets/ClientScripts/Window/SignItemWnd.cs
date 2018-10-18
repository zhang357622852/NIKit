/// <summary>
/// SignItemWnd.cs
/// Created by fengsc 2016/11/02
/// 签到基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class SignItemWnd : WindowBase<SignItemWnd>
{
    #region 成员变量

    // 签到成功勾选图标
    public GameObject mYes;

    // 签到成功提示
    public UILabel mSignOkTips;

    public GameObject mSignEffect;

    // 签到奖励物品图标
    public UITexture mIcon;

    // 奖励物品的数量
    public UILabel mAmount;

    // 第几天
    public UILabel mDays;

    public TweenScale mTipsScaleAnim;

    public TweenAlpha mTipsAlphaAnim;

    // 背景框
    public UISprite mBG;

    // 选中背景框
    public GameObject mSelectBg;

    public GameObject mSelectSprite;

    // 套装图标
    public UITexture mSuitIcon;

    // 星级
    public UISprite[] mStars;

    public GameObject mPiece;

    [HideInInspector]
    public LPCMapping mData = LPCMapping.Empty;

    public GameObject mMax;

    public UILabel mLevel;

    // 邮件附件是否可以全部领取
    bool mIsAllReceive = true;

    bool mIsShow = false;

    string mBgName = "";

    bool mIsShowAmount = true;

    bool mIsOnlyShowAttribAmount = true;

    Property mPetOb;

    #endregion

    public bool mSelect{ get; private set; }

    void OnDestroy()
    {
        if (mPetOb != null)
            mPetOb.Destroy();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        if (mYes != null)
            mYes.SetActive(false);

        if (mSignOkTips != null)
        {
            mSignOkTips.gameObject.SetActive(false);
        }

        if (mSignEffect != null)
            mSignEffect.SetActive(false);

        if (mData == null)
            return;

        // 绘制道具基本信息
        RedrawItemInfo(mData);

        if (mIsShow)
        {
            mYes.SetActive(true);

            mSignOkTips.alpha = 1;

            mSignOkTips.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// 绘制格子的基本信息
    /// </summary>
    void RedrawItemInfo(LPCMapping data)
    {
        // 初始化星级
        foreach (UISprite item in mStars)
            item.gameObject.SetActive(false);

        string desc = string.Empty;

        if (mSelectBg != null)
            mSelectBg.SetActive(false);

        if (mSelectSprite != null)
            mSelectSprite.SetActive(false);

        if (mMax != null)
            mMax.SetActive(false);

        if (mLevel != null)
            mLevel.gameObject.SetActive(false);

        if (mPiece != null)
            mPiece.SetActive(false);

        string iconName = string.Empty;
        string starName = string.Empty;
        string path = string.Empty;

        // 套装id
        int suitId = 0;
        // 物品的星级
        int star = 0;
        if (data.ContainsKey("pet_id"))
        {
            desc = "×" + data.GetValue<int>("amount");

            int petId = data.GetValue<int>("pet_id");

            iconName = MonsterMgr.GetIcon(petId, MonsterMgr.GetDefaultRank(petId));

            path = string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconName);

            if (mPiece != null)
            {
                mPiece.transform.localPosition = new Vector3(-23.3f, -23.7f, 0);

                mPiece.transform.localRotation = Quaternion.Euler(new Vector3(0, -180, 0));

                mPiece.SetActive(true);
            }

            mAmount.alignment = NGUIText.Alignment.Right;
        }
        else if (data.ContainsKey("class_id"))
        {
            if (data.ContainsKey("amount"))
                desc = "×" + data.GetValue<int>("amount");

            int classId = data.GetValue<int>("class_id");

            if (EquipMgr.IsEquipment(classId))
            {
                iconName = EquipMgr.GetIcon(classId, data.GetValue<int>("rarity"));
                path = string.Format("Assets/Art/UI/Icon/equipment/{0}.png", iconName);

                // 装备显示强化等级
                int rank = data.GetValue<int>("rank");
                if (rank <= 0)
                    desc = string.Empty;
                else
                    desc = "+" + rank;

                star = data.GetValue<int>("star");
                suitId = data.GetValue<int>("suit_id");
            }
            else if (ItemMgr.IsItem(classId))
            {
                iconName = ItemMgr.GetClearIcon(classId);
                path = string.Format("Assets/Art/UI/Icon/item/{0}.png", iconName);

                if (data.ContainsKey("star"))
                {
                    // 套装箱子不显示数量
                    desc = string.Empty;
                    star = data.GetValue<int>("star");
                    suitId = data.GetValue<int>("suit_id");
                }
            }
            else
            {
                LPCMapping para = new LPCMapping();

                if (data.ContainsKey("star"))
                    para.Add("star", data.GetValue<int>("star"));

                if (data.ContainsKey("rank"))
                    para.Add("rank", data.GetValue<int>("rank"));

                if (data.ContainsKey("level"))
                    para.Add("level", data.GetValue<int>("level"));
                else
                    para.Add("level", 1);

                para.Add("class_id", data.GetValue<int>("class_id"));

                para.Add("rid", Rid.New());

                if (mPetOb != null)
                    mPetOb.Destroy();

                mPetOb = PropertyMgr.CreateProperty(para);
                iconName = MonsterMgr.GetIcon(classId, mPetOb.Query<int>("rank"));
                path = string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconName);
                starName = PetMgr.GetStarName(mPetOb.Query<int>("rank"));

                if (MonsterMgr.IsMaxLevel(mPetOb))
                    mMax.gameObject.SetActive(true);
                else
                    mMax.gameObject.SetActive(false);

                desc = string.Empty;

                if (mLevel != null)
                {
                    // 显示宠物等级
                    mLevel.text = GET_LEVEL_ALIAS.Call(mPetOb.GetLevel().ToString());
                    mLevel.gameObject.SetActive(true);
                }

                star = mPetOb.Query<int>("star");
            }

            mAmount.alignment = NGUIText.Alignment.Right;
        }
        else
        {
            string fields = FieldsMgr.GetFieldInMapping(data);

            iconName = ItemMgr.GetClearIcon(FieldsMgr.GetFieldItemClassId(fields));

            path = string.Format("Assets/Art/UI/Icon/item/{0}.png", iconName);


            if (mIsOnlyShowAttribAmount)
            {

                desc = data.GetValue<int>(fields).ToString();

                mAmount.alignment = NGUIText.Alignment.Center;

            }
            else
            {
                desc = "×" + data.GetValue<int>(fields).ToString();

                mAmount.alignment = NGUIText.Alignment.Right;
            }
        }

        Texture2D res = ResourceMgr.LoadTexture(path);
        if (res != null)
            mIcon.mainTexture = res;

        // 显示星级
        for (int i = 0; i < star; i++)
        {
            if (!string.IsNullOrEmpty(starName))
                mStars[i].spriteName = starName;

            mStars[i].gameObject.SetActive(true);
        }

        if (suitId != 0)
        {
            // 获取套装类型图标
            mSuitIcon.mainTexture = EquipMgr.GetSuitTexture(suitId);

            mSuitIcon.gameObject.SetActive(true);
        }
        else
            mSuitIcon.gameObject.SetActive(false);

        if (!mIsAllReceive)
            mBG.spriteName = string.IsNullOrEmpty(mBgName) ? "PetIconBg" : mBgName;
        else
            mBG.spriteName = string.IsNullOrEmpty(mBgName) ? "StrengthenPetIconBg" : mBgName;

        mAmount.text = desc;

        mAmount.gameObject.SetActive(mIsShowAmount);
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping data, string finishTips, bool isShowEffect, bool isShowSelect,  int day = -1, string bgName = "")
    {
        mData = data;

        mIsShow = isShowSelect;

        mBgName = bgName;

        // 绘制窗口
        Redraw();

        if (day < 0)
            mDays.gameObject.SetActive(false);
        else
        {
            mDays.text = string.Format(LocalizationMgr.Get("SignWnd_2"), day);
            mDays.gameObject.SetActive(true);
        }
        mSignOkTips.text = finishTips;

        mSignEffect.SetActive(isShowEffect);
    }

    /// <summary>
    /// 是否显示数量
    /// </summary>
    public void ShowAmount(bool isShow)
    {
        mIsShowAmount = isShow;
    }

    public void OnlyShowAttribAmount(bool mIsShow)
    {
        mIsOnlyShowAttribAmount = mIsShow;
    }

    public void ShowTipsEffect()
    {
        mYes.SetActive(true);

        mSignOkTips.gameObject.SetActive(true);

        mSignEffect.SetActive(true);
        mTipsScaleAnim.PlayForward();
        mTipsAlphaAnim.PlayForward();
    }

    /// <summary>
    /// 普通格子绑定数据
    /// </summary>
    public void NormalItemBind(LPCMapping data, bool isAllReceive)
    {
        if (data == null)
            return;

        mData = data;

        mIsAllReceive = isAllReceive;

        RedrawItemInfo(data);
    }

    /// <summary>
    /// 设置选中状态
    /// </summary>
    public void Select(bool isSelect)
    {
        mSelectBg.SetActive(isSelect);

        mSelectSprite.SetActive(isSelect);

        mSelect = isSelect;
    }
}
