/// <summary>
/// TrialBonusItem.cs
///  Created by fengsc 2018/03/21
/// 试炼活动奖励item
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class TrialBonusItem : WindowBase<TrialBonusItem>
{
    public SignItemWnd[] mSignItem;

    // 领取按钮
    public UISprite mReceiveBtn;
    public UILabel mReceiveBtnLb;

    private LPCMapping mActivityData;

    Property mPropOb = null;

    int mNeedScore = 0;

    // 是否可以领取奖励
    bool mIsReceiveBonus = false;

    // 奖励列表
    LPCArray mBonusList = LPCArray.Empty;

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
        if (mBonusList == null)
            return;

        // 注册领取按钮点击事件
        UIEventListener.Get(mReceiveBtn.gameObject).onClick = OnClickReceiveBtn;

        LPCArray receiveList = LPCArray.Empty;

        LPCValue v = mActivityData.GetValue<LPCValue>("receive_list");
        if (v != null && v.IsArray)
            receiveList = v.AsArray;

        if (receiveList.IndexOf(mNeedScore) != -1)
        {
            // 奖励已经领取
            mReceiveBtn.alpha = 0.35f;

            mReceiveBtnLb.text = LocalizationMgr.Get("TrialActivityWnd_23");

            mIsReceiveBonus = false;
        }
        else
        {
            if (mNeedScore > mActivityData.GetValue<int>("score"))
            {
                // 无法领取
                mReceiveBtn.alpha = 0.35f;

                mReceiveBtnLb.text = mNeedScore.ToString();

                mIsReceiveBonus = false;
            }
            else
            {
                // 奖励未领取
                mReceiveBtn.alpha = 1.0f;

                mReceiveBtnLb.text = LocalizationMgr.Get("TrialActivityWnd_24");

                mIsReceiveBonus = true;
            }
        }

        if (mBonusList.Count > 1)
        {
            mSignItem[0].transform.localPosition = new Vector3(-25, 4, 0);
            mSignItem[1].transform.localPosition = new Vector3(21, 50, 0);
        }
        else
        {
            mSignItem[0].transform.localPosition = new Vector3(0, 15, 0);
        }

        for (int i = 0; i < mSignItem.Length; i++)
            mSignItem[i].gameObject.SetActive(false);

        for (int i = 0; i < mBonusList.Count; i++)
        {
            mSignItem[i].gameObject.SetActive(true);

            LPCMapping data = mBonusList[i].AsMapping;

            if (data.ContainsKey("class_id") && ItemMgr.IsDoubleExpItem(data.GetValue<int>("class_id")))
                mSignItem[i].ShowAmount(false);
            else
                mSignItem[i].ShowAmount(true);

            // 绑定数据
            mSignItem[i].Bind(data, "", false, false, -1, "small_icon_bg");

            // 注册点击事件
            UIEventListener.Get(mSignItem[i].gameObject).onClick = OnItemBtn;
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

        if (mNeedScore > mActivityData.GetValue<int>("score"))
        {
            // 积分不足
            return;
        }
        else
        {
            LPCArray receiveList = LPCArray.Empty;

            LPCValue v = mActivityData.GetValue<LPCValue>("receive_list");
            if (v != null && v.IsArray)
                receiveList = v.AsArray;

            if (receiveList.IndexOf(mNeedScore) != -1)
            {
                // 奖励已经领取

                return;
            }
        }

        // 领取任务奖励
        ActivityMgr.ReceiveActivityBonus(mActivityData.GetValue<string>("cookie"), LPCValue.Create(mNeedScore));
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
    public void Bind(LPCArray bonus, int needScore, LPCMapping activityData)
    {
        mBonusList = bonus;

        mActivityData = activityData;

        // 领取奖励需要的积分
        mNeedScore = needScore;

        // 绘制窗口
        Redraw();
    }
}
