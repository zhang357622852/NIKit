/// <summary>
/// ManualPetItemWnd.cs
/// Created by fengsc 2017/12/28
/// 使魔图鉴宠物基础格子
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class ManualPetItemWnd : WindowBase<ManualPetItemWnd>
{
    #region 成员变量

    // 宠物头像
    public UITexture mIcon;

    // 宠物星级
    public UISprite[] mStar;

    // 新宠物提示
    public GameObject mNewTips;

    public GameObject mQuestionMark;

    public GameObject mBonus;

    // 奖励物品
    public UILabel mBonusDesc;

    // 奖励物品图标
    public UISprite mBonusIcon;

    public UISprite mSelect;

    // 宠物对象
    [HideInInspector]
    public LPCMapping mPetData;

    [HideInInspector]
    public bool mIsBonus;

    #endregion

    // Use this for initialization
    void Start ()
    {
        if(mNewTips != null)
            mNewTips.GetComponent<UISpriteAnimation>().namePrefix = ConfigMgr.IsCN ? "cnew" : "new";
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        for (int i = 0; i < mStar.Length; i++)
            mStar[i].gameObject.SetActive(false);

        int rank = mPetData.GetValue<int>("rank");

        int classId = mPetData.GetValue<int>("class_id");

        string starName = PetMgr.GetStarName(rank);

        for (int i = 0; i < mPetData.GetValue<int>("star"); i++)
        {
            mStar[i].spriteName = starName;
            mStar[i].gameObject.SetActive(true);
        }

        Texture2D tex = MonsterMgr.GetTexture(classId, rank);

        mIcon.mainTexture = tex;

        LPCArray array = ManualMgr.GetBonusList(ME.user, rank);

        mIsBonus = false;

        // 玩家持有的宠物
        if (mPetData.GetValue<int>("is_user") == 1)
        {
            mQuestionMark.SetActive(false);


            if (mNewTips != null)
                mNewTips.SetActive(ManualMgr.IsNewManual(ME.user, rank, classId));

            if (array != null && array.IndexOf(classId) != -1)
            {
                CsvRow row = MonsterMgr.GetRow(classId);
                if (row == null)
                    return;

                LPCMapping data = row.Query<LPCMapping>("manual_bonus");

                LPCMapping manualBonus = data.GetValue<LPCMapping>(rank);

                string fields = FieldsMgr.GetFieldInMapping(manualBonus);

                if (mBonus != null)
                {
                    mBonus.SetActive(true);

                    mBonusDesc.text = string.Format(LocalizationMgr.Get("PetManualWnd_1"), manualBonus.GetValue<int>(fields));

                    mBonusIcon.spriteName = FieldsMgr.GetFieldIcon(fields);
                }

                mIsBonus = true;
            }
            else
            {
                if (mBonus != null)
                    mBonus.SetActive(false);
            }
        }
        else
        {
            mQuestionMark.SetActive(true);

            if (mBonus != null)
                mBonus.SetActive(false);

            if (mNewTips != null)
                mNewTips.SetActive(false);
        }
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping petData)
    {
        if (petData == null || petData.Count == 0)
            return;

        mPetData = petData;

        Redraw();
    }

    public void Select(bool select)
    {
        if (select)
        {
            mSelect.GetComponent<TweenAlpha>().ResetToBeginning();
            mSelect.GetComponent<TweenAlpha>().PlayForward();

            mSelect.spriteName = "PetSelectBg";
        }
        else
        {
            mSelect.spriteName = "PetIconBg";
        }
    }
}
