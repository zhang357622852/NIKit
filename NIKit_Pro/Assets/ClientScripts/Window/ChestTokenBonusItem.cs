/// <summary>
/// ChestTokenBonusItem.cs
///  Created by zhaozy 2018/05/30
/// 进击宝箱怪的呼啦啦代币活动奖励item
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class ChestTokenBonusItem : WindowBase<ChestTokenBonusItem>
{
    public SignItemWnd[] mSignItem;

    // 领取按钮
    public GameObject mReceiveBtn;
    public UISprite mReceiveBtnIcon;
    public UILabel mReceiveBtnLb;
    public UILabel mReceiveLimitLb;
    public GameObject mDisableSprite;

    public UILabel mReceiveDesc;

    private LPCMapping mActivityData;

    Property mPropOb = null;

    int mBonusId = 0;

    // 是否可以领取奖励
    bool mIsReceiveBonus = false;

    // 奖励信息
    LPCMapping mBonusData = LPCMapping.Empty;

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

        // 获取已经领取列表
        LPCMapping receiveMap = mActivityData.GetValue<LPCMapping>("receive_map");
        if (receiveMap == null)
            receiveMap = LPCMapping.Empty;

        // 先隐藏奖励按钮
        for (int i = 0; i < mSignItem.Length; i++)
            mSignItem[i].gameObject.SetActive(false);

        // 设置奖励列表位置
        LPCArray bonusList = mBonusData.GetValue<LPCArray>("bonus");
        if (bonusList.Count > 1)
        {
            mSignItem[0].transform.localPosition = new Vector3(195, 4, 0);
            mSignItem[1].transform.localPosition = new Vector3(287, 4, 0);
        }
        else
        {
            mSignItem[0].transform.localPosition = new Vector3(240, 4, 0);
        }

        // 填充奖励数据
        for (int i = 0; i < bonusList.Count; i++)
        {
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

        // 获取活动领取限购类型
        int limitType = mBonusData.GetValue<int>("limit_type");
        int limitTimes = mBonusData.GetValue<int>("limit");

        // 获取当前兑换次数
        int curTimes = receiveMap.GetValue<int>(mBonusId);

        // 如果是每天限购, 设置兑换次数信息
        if (limitType == 0)
            mReceiveDesc.text = string.Format(LocalizationMgr.Get("ChestTokenWnd_8"), curTimes, limitTimes);
        else
            mReceiveDesc.text = string.Format(LocalizationMgr.Get("ChestTokenWnd_9"), curTimes, limitTimes);

        // 已经达到了领取上限，不能在领取了
        if (curTimes >= limitTimes)
        {
            // 奖励已经领取
            mDisableSprite.SetActive(true);
            mReceiveBtnIcon.alpha = 0;
            mReceiveBtnLb.alpha = 0;
            mReceiveLimitLb.text = LocalizationMgr.Get("ChestTokenWnd_10");
            mIsReceiveBonus = false;
            return;
        }

        // 显示控件
        mReceiveBtnIcon.alpha = 1f;
        mReceiveBtnLb.alpha = 1f;
        mReceiveLimitLb.text = string.Empty;

        // 如果当前积分不能兑换
        int score = mBonusData.GetValue<int>("score");
        if (mBonusData.GetValue<int>("score") > mActivityData.GetValue<int>("score"))
        {
            // 无法领取
            mDisableSprite.SetActive(true);
            mReceiveBtnLb.text = score.ToString();
            mIsReceiveBonus = false;
            return;
        }

        // 可以兑换
        mDisableSprite.SetActive(false);
        mReceiveBtnLb.text = score.ToString();
        mIsReceiveBonus = true;
    }

    /// <summary>
    /// 领取按钮点击事件
    /// </summary>
    void OnClickReceiveBtn(GameObject go)
    {
        // 不能领取奖励
        if (!mIsReceiveBonus)
            return;

        // 领取任务奖励
        ActivityMgr.ReceiveActivityBonus(mActivityData.GetValue<string>("cookie"), LPCValue.Create(mBonusId));
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
