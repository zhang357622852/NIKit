/// <summary>
/// FrankensteinTrainingBonusItem.cs
///  Created by zhangwm 2018/09/18
/// 科学怪人的秘密特训活动奖励item
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class FrankensteinTrainingBonusItem : WindowBase<FrankensteinTrainingBonusItem>
{
    public UIGrid mItemGrid;

    public SignItemWnd[] mSignItem;

    // 领取按钮
    public GameObject mReceiveBtn;
    public UILabel mReceiveBtnLb;
    public GameObject mReceiveCover;

    // 临时记录物品pro
    private Property mPropOb = null;

    private int mBonusId = 0;

    // 是否可以领取奖励
    bool mIsReceiveBonus = false;

    // 活动信息
    private LPCMapping mActivityData;

    // 奖励信息
    LPCMapping mBonusData;

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

        // 获取奖励物品
        LPCArray bonusList = mBonusData.GetValue<LPCArray>("bonus");

        if (bonusList == null || bonusList.Count <= 0)
            return;

        // 填充奖励数据
        for (int i = 0; i < bonusList.Count; i++)
        {
            if (i >= mSignItem.Length)
                break;

            if (mSignItem[i] == null)
                continue;

            mSignItem[i].gameObject.SetActive(true);

            LPCMapping data = bonusList[i].AsMapping;

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

        LPCMapping receiveMap = LPCMapping.Empty;

        // 获取已领取奖励
        LPCValue v = mActivityData.GetValue<LPCValue>("receive_map");

        if (v != null && v.IsMapping)
            receiveMap = v.AsMapping;

        int limitTimes = mBonusData.GetValue<int>("limit");

        // 获取当前兑换次数
        int curTimes = receiveMap.GetValue<int>(mBonusId);

        int needScore = mBonusData.GetValue<int>("score");

        if (curTimes >= limitTimes)
        {
            // 奖励已经领取
            mReceiveCover.SetActive(true);

            mReceiveBtnLb.text = LocalizationMgr.Get("FrankensteinTrainingWnd_2");

            mIsReceiveBonus = false;
        }
        else
        {
            if (needScore > mActivityData.GetValue<int>("score"))
            {
                // 无法领取 分数不够
                mReceiveCover.SetActive(true);

                mIsReceiveBonus = false;
            }
            else
            {
                // 奖励未领取
                mReceiveCover.SetActive(false);

                mIsReceiveBonus = true;
            }

            mReceiveBtnLb.text = string.Format(LocalizationMgr.Get("FrankensteinTrainingWnd_3"), needScore);
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
        arg.AsMapping.Add("type", ActivityBonusType.NORMAL_BONUS);
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
    public void Bind(int bonusId, LPCMapping bonusData, LPCMapping activityData)
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
