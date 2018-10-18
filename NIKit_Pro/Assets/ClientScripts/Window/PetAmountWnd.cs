/// <summary>
/// PetAmountWnd.cs
/// Create By fengsc 2016/07/22
/// 玩家数量显示窗口
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class PetAmountWnd : WindowBase<PetAmountWnd>
{
    #region 成员变量

    /// <summary>
    ///添加按钮
    /// </summary>
    public GameObject mAddBtn;

    /// <summary>
    ///显示宠物数量
    /// </summary>
    public UILabel mValue;

    #endregion

    // Use this for initialization
    void Start()
    {
        UIEventListener.Get(mAddBtn).onClick = OnClickAddBtn;
    }

    void OnEnable()
    {
        Redraw();

        // 注册事件
        RegisterEvent();
    }

    void OnDisable()
    {
        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 移除属性字段关注回调
        ME.user.dbase.RemoveTriggerField("PetAmountWnd");
        ME.user.baggage.eventCarryChange -= RedrawPetAmount;
    }

    /// <summary>
    ///注册事件
    /// </summary>
    void RegisterEvent()
    {
        //顽疾对象不存在
        if (ME.user == null)
            return;

        //注册包裹变化回调;
        ME.user.baggage.eventCarryChange += RedrawPetAmount;

        ME.user.dbase.RemoveTriggerField("PetAmountWnd");
        ME.user.dbase.RegisterTriggerField("PetAmountWnd", new string[] { "container_size" }, new CallBack(OnContainerSizeChange));
    }

    /// <summary>
    ///添加按钮点击事件
    /// </summary>
    void OnClickAddBtn(GameObject go)
    {
        // 通关兰达平原普通所有副本
        if (! GuideMgr.IsGuided(4))
        {
            DialogMgr.Notify(LocalizationMgr.Get("GuideWnd_1"));

            return;
        }

        // 检测宠物格子数量
        if(!BaggageMgr.CheckCanUpgradeBaggage(ME.user, ContainerConfig.POS_PET_GROUP))
        {
            DialogMgr.Notify(LocalizationMgr.Get("StoreWnd_5"));
            return;
        }

        GameObject wnd = WindowMgr.OpenWnd("BuyStorageWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        wnd.GetComponent<BuyStorageWnd>().BindData(ContainerConfig.POS_PET_GROUP);
    }

    /// <summary>
    ///宠物数量变化回调
    /// </summary>
    void RedrawPetAmount(string[] pos)
    {
        Redraw();
    }

    void OnContainerSizeChange(object para, params object[] param)
    {
        // 刷新界面
        Redraw();
    }

    /// <summary>
    ///刷新宠物数量
    /// </summary>
    void Redraw()
    {

        //玩家对象不存在;
        if (ME.user == null)
            return;

        //获取玩家宠物个数
        int petAmount = BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_PET_GROUP).Count;

        //获取玩家的背包格子数量;
        int containerSize = ME.user.baggage.ContainerSize[ContainerConfig.POS_PET_GROUP].AsInt;

        mValue.text = string.Format("{0}{1}{2}", petAmount, "/", containerSize);
    }
}
