/// <summary>
/// FrankensteinTrainingExtraBonusItem.cs
///  Created by zhangwm 2018/09/18
/// 科学怪人的秘密特训活动额外奖励item
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class FrankensteinTrainingExtraBonusItem : WindowBase<FrankensteinTrainingExtraBonusItem>
{
    public UIGrid mItemGrid;

    public SignItemWnd[] mSignItem;

    // 领取按钮
    public GameObject mReceiveBtn;
    public UILabel mReceiveBtnLb;
    public GameObject mReceiveCover;

    private int mBonusId = 0;

    // 是否可以领取奖励
    bool mIsReceiveBonus = false;

    // 活动信息
    private LPCMapping mActivityData;

    // 临时记录物品pro
    private Property mPropOb = null;

    // 奖励信息
    LPCArray mBonusData;

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        // 析构临时对象
        if (mPropOb != null)
            mPropOb.Destroy();
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        // 没有绑定数据不处理
        if (mBonusData == null)
            return;

        // 注册领取按钮点击事件
        UIEventListener.Get(mReceiveBtn.gameObject).onClick = OnClickReceiveBtn;

        // 先隐藏奖励
        for (int i = 0; i < mSignItem.Length; i++)
            mSignItem[i].gameObject.SetActive(false);

        // 填充奖励数据
        for (int i = 0; i < mBonusData.Count; i++)
        {
            if (mSignItem[i] == null)
                continue;

            mSignItem[i].gameObject.SetActive(true);

            LPCMapping data = mBonusData[i].AsMapping;

            if (data.ContainsKey("class_id") && ItemMgr.IsDoubleExpItem(data.GetValue<int>("class_id")))
                mSignItem[i].ShowAmount(false);
            else
                mSignItem[i].ShowAmount(true);

            // 绑定数据
            mSignItem[i].Bind(data, "", false, false, -1, "small_icon_bg");

            // 注册点击事件
            UIEventListener.Get(mSignItem[i].gameObject).onClick = OnItemBtn;
        }

        mItemGrid.Reposition();

        LPCArray receiveExtraList = LPCArray.Empty;

        // 获取已领取奖励
        LPCValue v = mActivityData.GetValue<LPCValue>("receive_extra_list");
        if (v != null && v.IsArray)
            receiveExtraList = v.AsArray;

        if (receiveExtraList.IndexOf(mBonusId) != -1)
        {
            // 奖励已经领取
            mReceiveCover.SetActive(true);

            mReceiveBtnLb.text = LocalizationMgr.Get("FrankensteinTrainingWnd_2");

            mIsReceiveBonus = false;
        }
        else
        {
            if (mBonusId > mActivityData.GetValue<int>("exchange_times"))
            {
                // 无法领取 分数不够
                mReceiveCover.SetActive(true);

                mReceiveBtnLb.text = string.Format(LocalizationMgr.Get("FrankensteinTrainingWnd_12"), mBonusId);

                mIsReceiveBonus = false;
            }
            else
            {
                // 奖励未领取
                mReceiveCover.SetActive(false);

                mReceiveBtnLb.text = LocalizationMgr.Get("FrankensteinTrainingWnd_4");

                mIsReceiveBonus = true;
            }
        }
    }

    /// <summary>
    /// 领取按钮点击事件
    /// </summary>
    void OnClickReceiveBtn(GameObject go)
    {
        // 不能领取奖励
        if (!mIsReceiveBonus)
            return;

        LPCValue arg = LPCValue.CreateMapping();
        arg.AsMapping.Add("type", ActivityBonusType.EXTRA_BONUS);
        arg.AsMapping.Add("bonus_id", mBonusId);

        // 领取任务奖励
        ActivityMgr.ReceiveActivityBonus(mActivityData.GetValue<string>("cookie"), arg);
    }

    /// <summary>
    /// 物体被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnItemBtn(GameObject ob)
    {
        // 获取奖励数据
        LPCMapping itemData = ob.GetComponent<SignItemWnd>().mData;
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

            mPropOb = PropertyMgr.CreateProperty(dbase);

            if (MonsterMgr.IsMonster(classId))
            {
                // 显示宠物悬浮窗口
                GameObject wnd = WindowMgr.OpenWnd(PetSimpleInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                PetSimpleInfoWnd script = wnd.GetComponent<PetSimpleInfoWnd>();

                script.Bind(mPropOb);
                script.ShowBtn(true, false, false);
            }
            else if (EquipMgr.IsEquipment(classId))
            {
                GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

                script.SetEquipData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));

                script.SetMask(true);
            }
            else
            {
                GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
                if (wnd == null)
                    return;

                RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

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

            mPropOb = PropertyMgr.CreateProperty(dbase);

            GameObject wnd = WindowMgr.OpenWnd(RewardItemInfoWnd.WndType, null, WindowOpenGroup.SINGLE_OPEN_WND);
            if (wnd == null)
                return;

            RewardItemInfoWnd script = wnd.GetComponent<RewardItemInfoWnd>();

            script.SetPropData(mPropOb, true, false, LocalizationMgr.Get("MessageBoxWnd_2"));

            script.SetMask(true);
        }
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(int bonusId, LPCArray bonusData, LPCMapping activityData)
    {
        // 获取奖励信息
        mBonusData = bonusData;

        // 获取任务数据
        mActivityData = activityData;

        // 记录奖励id
        mBonusId = bonusId;

        // 绘制窗口
        Redraw();
    }
}
