/// <summary>
/// EquipItemWnd.cs
/// Created by lic 2016-6-23
/// 装备格子
/// </summary>

using UnityEngine;
using System.Collections;
using LPC;
using System.Collections.Generic;

public class EquipItemWnd : WindowBase<EquipItemWnd>
{
    #region 成员变量

    // 星级
    public GameObject[] mStars;

    // 宠物
    public UITexture mIcon;

    // 等级
    public UILabel mLevel;

    // 新装备
    public GameObject mNew;

    // 套装图标
    public UITexture mSuitIcon;

    // 背景
    public UISprite mBg;

    // 选中图标
    public GameObject mCheck;

    public UISpriteAnimation mNewequipTips;

    // 装备类型
    public int equipType = -1;

    // 窗口唯一标识
    private string instanceID = string.Empty;

    // 装备类型对应的图标
    private Dictionary<int, string> typeDict = new Dictionary<int, string>()
    {
        { EquipConst.WEAPON, "weapon" },
        { EquipConst.ARMOR, "suit" },
        { EquipConst.SHOES, "glove" },
        { EquipConst.AMULET, "shileld" },
        { EquipConst.NECKLACE, "necklace" },
        { EquipConst.RING, "ring" },
    };

    #endregion

    #region 属性

    /// <summary>
    /// 窗口绑定对象
    /// </summary>
    /// <value>The item ob.</value>
    public Property ItemOb { get; private set; }

    /// <summary>
    /// 窗口选择状态
    /// </summary>
    public bool IsSelected { get; private set; }

    public bool mIsCheck { get; private set; }

    #endregion

    #region 内部函数

    /// <summary>
    /// 注册事件
    /// </summary>

    // Use this for initialization
    void Start()
    {
        if(mNewequipTips != null)
            mNewequipTips.namePrefix = ConfigMgr.IsCN ? "cnew" : "new";

        // 初始化窗口
        Redraw();
    }

    void Awake()
    {
        instanceID = gameObject.GetInstanceID().ToString();
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    void OnDestroy()
    {
        // 对象不存在
        if (ItemOb == null)
            return;

        // 取消关注
        ItemOb.dbase.RemoveTriggerField(instanceID);
    }

    /// <summary>
    /// 绑定宠物事件回调
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    private void OnItemChange(object param, params object[] paramEx)
    {
        // 当前界面没有绑定宠物不处理
        if (ItemOb == null)
            return;

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 刷新窗口
    /// </summary>
    private void Redraw()
    {
        // 初始化窗口扣减
        mLevel.gameObject.SetActive(false);
        mNew.SetActive(false);
        mSuitIcon.gameObject.SetActive(false);

        if (mNewequipTips != null)
            mNewequipTips.gameObject.SetActive(false);

        // 初始化星级
        for (int i = 0; i < mStars.Length; i++)
            mStars[i].SetActive(false);

        // 没有指定类型，显示为空格子
        if (equipType < 0 && ItemOb == null)
        {
            mIcon.gameObject.SetActive(false);
            return;
        }

        // 如果没有绑定道具，直接设置默认图片
        if (equipType >= 0 && ItemOb == null)
        {
            mIcon.mainTexture = EquipMgr.LoadTexture(typeDict[equipType]);
            return;
        }

        mIcon.mainTexture = EquipMgr.GetTexture(ItemOb.GetClassID(), ItemOb.GetRarity());
        mIcon.gameObject.SetActive(true);

        // 绘制装备星级
        int star = ItemOb.Query<int>("star");
        int count = star < mStars.Length ? star : mStars.Length;
        for (int i = 0; i < count; i++)
            mStars[i].SetActive(true);

        // 获取装备的强化等级
        int rank = ItemOb.GetRank();

        // 未强化(level = 0)不显示等级
        if (rank > 0)
        {
            mLevel.text = string.Format("+{0}", rank);
            mLevel.gameObject.SetActive(true); 
        }

        // 是否为新装备
        mNew.SetActive(BaggageMgr.IsNew(ItemOb));

        mSuitIcon.mainTexture = EquipMgr.GetSuitTexture(ItemOb.Query<int>("suit_id"));
        mSuitIcon.gameObject.SetActive(true);
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 设置选中
    /// </summary>
    public void SetSelected(bool is_selected)
    {
        IsSelected = is_selected;

        mBg.spriteName = is_selected?"selectBg":"equipItemBg";
    }

    /// <summary>
    /// 设置勾选
    /// </summary>
    public void SetCheck(bool is_selected)
    {
        mIsCheck = is_selected;

        if(mCheck != null)
            mCheck.SetActive(is_selected);
    }

    /// <summary>
    /// 窗口绑定实体
    /// </summary>
    public void SetBind(Property ob, bool isAutoRefresh = true)
    {
        if (string.IsNullOrEmpty(instanceID))
            instanceID = gameObject.GetInstanceID().ToString();

        if (ItemOb != null && isAutoRefresh)
            ItemOb.dbase.RemoveTriggerField(instanceID);

        if(ob != null && isAutoRefresh)
            ob.dbase.RegisterTriggerField(instanceID, new string[]
            {
                "rank"
            }, new CallBack(OnItemChange));

        // 重置绑定对象
        ItemOb = ob;

        // 重绘窗口
        Redraw();
    }

    /// <summary>
    /// 设置装备部位
    /// </summary>
    public void SetType(int type)
    {
        // 记录类型
        equipType = type; 

        // 类型不存在
        if (!typeDict.ContainsKey(type))
            return;

        // 设置默认的图片
        mIcon.mainTexture = EquipMgr.LoadTexture(typeDict[type]);
    }

    /// <summary>
    /// 设置新物品提示
    /// </summary>
    public void SetNewTips(Property ob)
    {
        if (mNewequipTips == null)
            return;

        if (ob == null)
        {
            mNewequipTips.gameObject.SetActive(false);
            return;
        }

        // 播放序列帧动画
        if (BaggageMgr.IsNew(ob))
        {
            mNewequipTips.gameObject.SetActive(true);
            mNewequipTips.ResetToBeginning();
        }
        else
        {
            mNewequipTips.gameObject.SetActive(false);
        }
    }

    #endregion
}
