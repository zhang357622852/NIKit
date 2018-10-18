using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

// 选中图标位置
public enum ClearanceBonusType
{
    PET,
    SUIT,
    ITEM
}

public class ClearanceBonusWnd : WindowBase<ClearanceBonusWnd>
{
    #region 成员变量

    // 标题
    public UILabel mTitle;

    public UILabel mPetName;
    public UILabel mItemName;
    public UILabel mSuitName;
    public UILabel mAttribName;

    public GameObject mPetIconOb;
    public GameObject mItemIconOb;
    public GameObject mStarAndElementOb;

    public UITexture mPetIcon;
    public UITexture mItemIcon;
    public UITexture mSuitIcon;

    public UISprite mElement;
    public UISprite[] mStars;

    #endregion

    #region 私有变量

    int mMapId;
    LPCMapping mBonusData;

    private List<float> star_x = new List<float>();

    #endregion

    #region 内部函数

    void Awake()
    {
        for (int i = 0; i < mStars.Length; i++)
            star_x.Add(mStars[i].transform.localPosition.x);
    }

    // Use this for initialization
    void Redraw()
    {
        // 获取地图信息,没有配置的类型
        CsvRow mapData = MapMgr.GetMapConfig(mMapId);
        if (mapData == null)
            return;

        mTitle.text = string.Format(LocalizationMgr.Get("ClearanceBonusWnd_1"),
            LocalizationMgr.Get(mapData.Query<string>("name")));

        // 非属性奖励
        if (!mBonusData.ContainsKey("class_id"))
        {
            SetModel(ClearanceBonusType.ITEM);
            // 显示属性奖励
            string field = FieldsMgr.GetFieldInMapping(mBonusData);

            mItemIcon.mainTexture = ItemMgr.GetTexture(FieldsMgr.GetFieldTexture(field));

            mItemName.text = string.Format("{0}×{1}", FieldsMgr.GetFieldName(field), mBonusData[field].AsInt);

            return;
        }

        // 非属性奖励必须要有class_id和amount两个参数
        int classId = mBonusData.GetValue<int>("class_id");

        // 如果是道具
        if (ItemMgr.IsItem(classId))
        {
            // 非套装箱子道具
            if (!mBonusData.ContainsKey("suit_id"))
            {
                SetModel(ClearanceBonusType.ITEM);

                mItemIcon.mainTexture = ItemMgr.GetTexture(classId, true);

                mItemName.text = string.Format("{0}×{1}", ItemMgr.GetName(classId), mBonusData.GetValue<int>("amount"));

                return;
            }

            int suitId = mBonusData.GetValue<int>("suit_id");

            // 套装
            SetModel(ClearanceBonusType.SUIT);
            mItemIcon.mainTexture = ItemMgr.GetTexture(classId);
            mSuitName.text = ItemMgr.GetName(classId);

            //套装类型图标;
            mSuitIcon.mainTexture = EquipMgr.GetSuitTexture(suitId);

            // 获取属性描述
            mAttribName.text = EquipMgr.GetSuitPropDesc(suitId);

            return;
        }

        if (MonsterMgr.IsMonster(classId))
        {
            SetModel(ClearanceBonusType.PET);

            // 没有配置表示未觉醒
            int rank = mBonusData.ContainsKey("rank") ?
                mBonusData.GetValue<int>("rank") : 1;

            int stars = mBonusData.GetValue<int>("star");

            mPetIcon.mainTexture = MonsterMgr.GetTexture(classId, rank);
            mPetName.text = MonsterMgr.GetName(classId, rank);

            int offset = (mStars.Length - stars) * 12;

           for (int i = 0; i < mStars.Length; i++)
            {
                if(i >= stars)
                {
                    mStars[i].gameObject.SetActive(false);
                    continue;
                }

                mStars[i].GetComponent<UISprite>().spriteName = PetMgr.GetStarName(rank);
                mStars[i].gameObject.SetActive(true);

                mStars[i].transform.localPosition = new Vector3(star_x[i] + offset,
                    mStars[i].transform.localPosition.y,
                    mStars[i].transform.localPosition.z);
            }

            //根据class_id获取宠物元素;
            mElement.spriteName = PetMgr.GetElementIconName(MonsterMgr.GetElement(classId));

            return;
        }

        LogMgr.Error("主线任务通关暂时不支持配置此类型奖励{0}", classId);
    }

    /// <summary>
    /// 设置模式
    /// </summary>
    /// <param name="type">Type.</param>
    private void SetModel(ClearanceBonusType type)
    {
        switch (type)
        {
            case ClearanceBonusType.ITEM:
              
                mItemIconOb.SetActive(true);
                mItemName.gameObject.SetActive(true);

                mPetIconOb.SetActive(false);
                mPetName.gameObject.SetActive(false);
                mSuitName.gameObject.SetActive(false);
                mSuitIcon.gameObject.SetActive(false);
                mStarAndElementOb.SetActive(false);
                mAttribName.gameObject.SetActive(false);
                break;
            case ClearanceBonusType.PET:

                mPetIconOb.SetActive(true);
                mPetName.gameObject.SetActive(true);
                mStarAndElementOb.SetActive(true);

                mItemIconOb.SetActive(false);
                mAttribName.gameObject.SetActive(false);
                mItemName.gameObject.SetActive(false);
                mSuitName.gameObject.SetActive(false);
                mSuitIcon.gameObject.SetActive(false);
                break;
            case ClearanceBonusType.SUIT:

                mItemIconOb.SetActive(true);
                mSuitName.gameObject.SetActive(true);
                mSuitIcon.gameObject.SetActive(true);
                mAttribName.gameObject.SetActive(true);

                mPetIconOb.SetActive(false);
                mPetName.gameObject.SetActive(false);
                mStarAndElementOb.SetActive(false);
                mItemName.gameObject.SetActive(false);
                break;
        }
    }

    #endregion

    #region 外部接口

    public void BindData(int mapId, LPCMapping bonusData)
    {
        mMapId = mapId;

        mBonusData = bonusData;

        Redraw();
    }

    #endregion
}
