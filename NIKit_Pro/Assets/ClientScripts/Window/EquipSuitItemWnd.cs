using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipSuitItemWnd : MonoBehaviour
{
    #region 成员变量

    /// <summary>
    /// 套装名称
    /// </summary>
    public UILabel mSuitName;

    /// <summary>
    /// 套装图标
    /// </summary>
    public UITexture mSuitIcon;

    /// <summary>
    /// 物品数量
    /// </summary>
    public UILabel mAmount;

    /// <summary>
    /// 小红点
    /// </summary>
    public GameObject mAmountGo;

    /// <summary>
    /// 背景
    /// </summary>
    public UISprite mBg;

    /// <summary>
    /// 选中时的状态
    /// </summary>
    public GameObject mSelectState;

    public UISpriteAnimation mNewTips;

    private CsvRow mSuitData;

    private List<Property> mEquipList;

    private bool mIsSelect = false;

    private bool mIsTrigger = true;

    public delegate void ClickCallBack(int suitId, bool isSelect);

    #endregion

    #region 属性

    public ClickCallBack CallBack { get; set; }

    #endregion

    #region 内部接口

    void Start()
    {
        if (mNewTips != null)
            mNewTips.namePrefix = ConfigMgr.IsCN ? "cnew" : "new";

        UIEventListener.Get(gameObject).onClick = OnClickSuit;
    }

    /// <summary>
    /// 刷新窗口数据
    /// </summary>
    private void Redraw()
    {
        //设置套装的类型名字;
        mSuitName.text = LocalizationMgr.Get(mSuitData.Query<string>("name"));

        //设置套装类型icon;
        mSuitIcon.mainTexture = EquipMgr.GetSuitTexture(mSuitData.Query<int>("suit_id"));

        if (mIsTrigger)
            SetSelect(mIsSelect);
        else
            SetSelectNoTrriger(mIsSelect);

        RefreshAmount();

        DoCheckNewTips();
    }

    /// <summary>
    /// 刷新状态
    /// </summary>
    private void RefreshState()
    {
        if (mSuitData == null)
            return;

        if (mIsSelect)
            mBg.alpha = mSuitIcon.alpha = mSuitName.alpha = 1f;
        else
        {
            mBg.alpha = 0.2f;
            mSuitIcon.alpha = 0.5f;
            mSuitName.alpha = 0.5f;
        }

        mSelectState.SetActive(mIsSelect);
    }

    /// <summary>
    /// 触发事件
    /// </summary>
    private void TriggerEvent()
    {
        if (CallBack == null)
            return;

        CallBack(mSuitData.Query<int>("suit_id"), mIsSelect);
    }

    /// <summary>
    /// 刷新拥有数目
    /// </summary>
    private void RefreshAmount()
    {
        if (mEquipList == null || mEquipList.Count == 0)
        {
            mAmountGo.SetActive(false);
            return;
        }

        // 1. 绘制套装数量
        mAmount.text = mEquipList.Count.ToString();
        mAmountGo.SetActive(true);
    }

    /// <summary>
    /// 检测新装备
    /// </summary>
    private void DoCheckNewTips()
    {
        if (mNewTips == null || ME.user == null)
        {
            mNewTips.gameObject.SetActive(false);
            return;
        }

        // 有新物品
        if (BaggageMgr.HasNewItem(mEquipList))
        {
            mNewTips.gameObject.SetActive(true);

            mNewTips.ResetToBeginning();
        }
        else
        {
            mNewTips.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 窗口的点击事件
    /// </summary>
    private void OnClickSuit(GameObject go)
    {
        SetSelect(mIsSelect ? false : true);
    }

    #endregion

    #region 公共接口

    /// <summary>
    /// 绑定重绘窗口的数据
    /// </summary>
    public void BindData(CsvRow data, List<Property> equipList, bool isTrigger = true)
    {
        // 绑定数据
        mSuitData = data;

        mEquipList = equipList;

        mIsTrigger = isTrigger;

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 设置状态， 并且触发事件
    /// </summary>
    /// <param name="isSelect"></param>
    public void SetSelect(bool isSelect)
    {
        mIsSelect = isSelect;

        RefreshState();

        TriggerEvent();
    }

    /// <summary>
    /// 设置状态， 不触发事件
    /// </summary>
    /// <param name="isSelect"></param>
    public void SetSelectNoTrriger(bool isSelect)
    {
        mIsSelect = isSelect;

        RefreshState();
    }
    #endregion
}
