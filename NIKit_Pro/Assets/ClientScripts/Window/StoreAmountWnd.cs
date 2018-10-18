/// <summary>
/// StoreAmountWnd.cs
/// Create By lic 2016/11/16
/// 仓库格子数量显示窗口
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class StoreAmountWnd : WindowBase<StoreAmountWnd>
{
    #region 成员变量

    /// <summary>
    ///添加按钮
    /// </summary>
    public GameObject mAddBtn;

    /// <summary>
    ///显示仓库宠物数量
    /// </summary>
    public UILabel mValue;

    #endregion

    #region 私有变量

    #endregion

    // Use this for initialization
    void Start ()
    {
        Redraw();

        UIEventListener.Get(mAddBtn).onClick = OnClickAddBtn;
    }

    void OnEnable()
    {
        // 注册事件
        RegisterEvent();
    }

    void OnDisable()
    {
        // 玩家对象不存在
        if(ME.user == null)
            return;

        // 解注册副本通关事件;
        EventMgr.UnregisterEvent("StoreAmountWnd");

        // 注销玩家包裹变化回调
        ME.user.baggage.eventCarryChange -= RedrawPetAmount;
    }

    /// <summary>
    ///注册事件
    /// </summary>
    void RegisterEvent()
    {
        // 玩家对象不存在
        if(ME.user == null)
            return;

        //注册包裹变化回调;
        ME.user.baggage.eventCarryChange += RedrawPetAmount;

        // 注册玩家包裹变化回调
        ME.user.dbase.RemoveTriggerField("StoreAmountWnd");
        ME.user.dbase.RegisterTriggerField("StoreAmountWnd", new string[] { "container_size" }, new CallBack(OnContainerSizeChange));
    }

    /// <summary>
    ///添加按钮点击事件
    /// </summary>
    void OnClickAddBtn(GameObject go)
    {
        // 能否升级仓库格子
        if(! BaggageMgr.CheckCanUpgradeBaggage(ME.user, ContainerConfig.POS_STORE_GROUP))
        {
            DialogMgr.Notify(LocalizationMgr.Get("StoreWnd_6"));
            return;
        }

        GameObject wnd = WindowMgr.OpenWnd("BuyStorageWnd", null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;

        wnd.GetComponent<BuyStorageWnd>().BindData(ContainerConfig.POS_STORE_GROUP);
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
        if(ME.user == null)
            return;

        int petAmount = BaggageMgr.GetItemsByPage(ME.user, ContainerConfig.POS_STORE_GROUP).Count;

        //获取玩家的背包格子数量;
        int containerSize = ME.user.baggage.ContainerSize[ContainerConfig.POS_STORE_GROUP].AsInt;

        mValue.text = string.Format("{0}{1}{2}", petAmount, "/", containerSize);
    }
}
