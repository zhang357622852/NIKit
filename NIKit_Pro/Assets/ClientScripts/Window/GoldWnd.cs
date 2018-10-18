/// <summary>
///GoldWnd.cs
/// Create By fengsc 2016/07/21
/// 玩家金币显示窗口
/// </summary>
using UnityEngine;
using System.Collections;

public class GoldWnd : WindowBase<GoldWnd>
{

    #region 成员变量

    /// <summary>
    ///点击显示购买金币窗口的按钮
    /// </summary>
    public GameObject mAddBtn;

    public UILabel mValue;

    string eventName = string.Empty;

    // 用来记录取消监听时的属性值
    int fixedMonely = 0;

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        if (mAddBtn != null)
            UIEventListener.Get(mAddBtn).onClick = OnClickAddBtn;

        //初始化金钱数量
        RedrawMoneyAmount();
    }

    void OnEnable()
    {
        //注册事件
        RegisterEvent();

        //初始化金钱数量
        RedrawMoneyAmount();
    }

    void RegisterEvent()
    {
        if (ME.user == null)
            return;

        eventName = Game.GetUniqueName("GoldWnd") + "_money";

        ME.user.dbase.RegisterTriggerField(eventName, new string[] { "money" }, new CallBack(OnMoneyChange));
    }

    void UnRegisterEvent()
    {
        // 没有关注的属性
        if (string.IsNullOrEmpty(eventName))
            return;

        if (ME.user == null)
            return;

        // 移除属性字段关注回调
        ME.user.dbase.RemoveTriggerField(eventName);

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

        // 打开快捷购买窗口
        GameObject wnd = WindowMgr.OpenWnd(QuickMarketWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
        if (wnd == null)
            return;
        wnd.GetComponent<QuickMarketWnd>().Bind(ShopConfig.MONEY_GROUP);
    }

    /// <summary>
    ///刷新金钱数量
    /// </summary>
    void RedrawMoneyAmount()
    {
        //玩家对象不存在;
        if (ME.user == null)
            return;

        //获取玩家的金钱数量;
        int amount = ME.user.Query<int>("money");

        if (mValue == null)
            return;

        //以特定的格式显示玩家金钱数量;
        mValue.text = Game.SetMoneyShowFormat(amount);
    }

    /// <summary>
    /// 刷新修正后的金钱数量
    /// </summary>
    void RedrawFixedMoneyAmount()
    {
        //以特定的格式显示玩家金钱数量;
        mValue.text = Game.SetMoneyShowFormat(fixedMonely);
    }

    /// <summary>
    ///玩家金钱数量变化回调
    /// </summary>
    void OnMoneyChange(object param, params object[] paramEx)
    {
        //延迟刷新金钱数量;
        MergeExecuteMgr.DispatchExecute(RedrawMoneyAmount);
    }

    /// <summary>
    /// Raises the disable event.
    /// </summary>
    void OnDisable()
    {
        UnRegisterEvent ();
    }

    #endregion

    #region 外部接口

    /// <summary>
    /// 取消关注属性变化
    /// </summary>
    public void UnRegisterField()
    {
        UnRegisterEvent ();

        if (ME.user == null)
            return;

        // 记录属性值
        fixedMonely = ME.user.Query<int>("money");
    }

    /// <summary>
    /// 重新开始关注属性变化
    /// </summary>
    public void RestartRegisterField()
    {
        RegisterEvent ();

        // 修正水晶数量显示
        RedrawMoneyAmount();
    }

    /// <summary>
    /// 扣除消耗(外部需要判断是增加还是减少金钱量)
    /// </summary>
    public void ChangeNumber(int number)
    {
        fixedMonely += number;

        //延迟刷新金钱数量;
        MergeExecuteMgr.DispatchExecute(RedrawFixedMoneyAmount);
    }

    #endregion

}
