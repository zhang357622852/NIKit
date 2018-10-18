/// <summary>
/// ReceiveBonusItem.cs
/// Created by lic 2017/06/07
/// 领取奖励项
/// </summary>
/// 
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ReceiveBonusItem : WindowBase<ReceiveBonusItem>
{
    #region 成员变量

    public SignItemWnd mItem;
    public GameObject mBtn;
    public UILabel mDesc;
    public UILabel mTimes;

    #endregion

    #region 私有变量

    LPCMapping mData;
    int mNum;
    int mType;
    CallBack mTask;
    Property mPropOb = null;

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();
    }

    void OnDestroy()
    {
        if (mPropOb != null)
            mPropOb.Destroy();
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mItem.gameObject).onClick = OnItemBtn;
    }

    /// <summary>
    /// 物体被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnItemBtn(GameObject ob)
    {
        // 显示信息弹框
        ShowInfo();
    }

    /// <summary>
    /// 领取按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnReceiveBtn(GameObject ob)
    {
        if (mTask != null)
            mTask.Go(mType, mData);
    }

    /// <summary>
    /// 显示道具查看窗口
    /// </summary>
    void ShowInfo()
    {
        // 获取奖励数据
        LPCMapping itemData = mData.GetValue<LPCMapping>("bonus");
        if (itemData == null)
            return;

        if (itemData.ContainsKey("class_id"))
        {
            int classId = itemData.GetValue<int>("class_id");

            // 构造参数
            LPCMapping dbase = LPCMapping.Empty;
            dbase.Append(itemData);
            dbase.Add("rid", Rid.New());

            // 克隆物件对象
            if (mPropOb != null)
                mPropOb.Destroy();

            // 克隆物件对象
            mPropOb = PropertyMgr.CreateProperty(dbase);

            // 创建对象失败
            if (mPropOb == null)
                return;

            if (MonsterMgr.IsMonster(classId))
            {
                // 显示宠物悬浮窗口
                GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();

                // 获取组件失败
                if (script == null)
                    return;

                script.Bind(mPropOb);
                script.ShowBtn(true, false, false);
            }
            else if (EquipMgr.IsEquipment(classId))
            {
                GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

                // 获取组件失败
                if (script == null)
                    return;

                script.SetEquipData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));
                script.SetMask(true);
            }
            else
            {
                GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

                // 获取组件失败
                if (script == null)
                    return;

                script.SetPropData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));
                script.SetMask(true);
            }
        }
        else
        {
            string fields = FieldsMgr.GetFieldInMapping(itemData);

            int classId = FieldsMgr.GetClassIdByAttrib(fields);

            // 构造参数
            LPCMapping dbase = LPCMapping.Empty;
            dbase.Add("class_id", classId);
            dbase.Add("amount", itemData.GetValue<int>(fields));
            dbase.Add("rid", Rid.New());

            if (mPropOb != null)
                mPropOb.Destroy();

            // 克隆物件对象
            mPropOb = PropertyMgr.CreateProperty(dbase);

            // 创建对象失败
            if (mPropOb == null)
                return;

            GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (wnd == null)
                return;

            RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

            // 获取组件失败
            if (script == null)
                return;

            script.SetPropData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));
            script.SetMask(true);
        }
    }

    /// <summary>
    /// 刷新界面
    /// </summary>
    void Redraw()
    {
        int limit = mData.GetValue<int>("limit");

        int cost = mData.GetValue<int>("cost");

        LPCMapping item = mData.GetValue<LPCMapping>("bonus");

        string color = string.Empty;

        string numColor = string.Empty;

        if (mNum >= limit)
        {
            // 不能领取
            UIEventListener.Get(mBtn).onClick -= OnReceiveBtn;

            // 红色
            color = "[FF0000]";

            numColor = "[FF0000]";
        }
        else
        {
            // 可以领取
            UIEventListener.Get(mBtn).onClick = OnReceiveBtn;

            // 黑色
            color = "[ffffff]";

            numColor = "[000000]";
        }

        mTimes.text = string.Format("{0}{1}/{2}{3}[-]", color, mNum, limit, LocalizationMgr.Get("AccumulateScoreWnd_25"));

        string desc = mNum < limit ?  (mType == 1 ? string.Format(LocalizationMgr.Get("AccumulateScoreWnd_9"), cost) : string.Format(LocalizationMgr.Get("AccumulateScoreWnd_24"), cost)): 
            (mType == 1 ? LocalizationMgr.Get("AccumulateScoreWnd_22") : LocalizationMgr.Get("AccumulateScoreWnd_23"));

        mDesc.text = string.Format("{0}{1}[-]", numColor, desc);

        if (item.ContainsKey("class_id") && ItemMgr.IsDoubleExpItem(item.GetValue<int>("class_id")))
            mItem.ShowAmount(false);
        else
            mItem.ShowAmount(true);

        mItem.Bind(item, "", false, false, -1, mType == 1 ? "small_icon_bg" : "big_icon_bg");
    }

        
    #endregion

    #region 公共函数

    public void BindData(LPCMapping data, int num, int type, CallBack task)
    {
        if (data == null)
            return;

        mData = data;
        mNum = num;
        mType = type;
        mTask = task;

        Redraw();
    }

    #endregion
}
