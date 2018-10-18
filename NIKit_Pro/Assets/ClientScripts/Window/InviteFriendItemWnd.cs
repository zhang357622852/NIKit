/// <summary>
/// InviteFriendItemWnd.cs
/// Created by zhangwm 2018/06/25
/// 邀请好友
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class InviteFriendItemWnd : WindowBase<InviteFriendItemWnd>
{
    #region 成员变量
    public UILabel mTitle;

    // 奖励
    public GameObject mRewardItemPrefab;
    public UIGrid mRewardGrid;

    public GameObject mMaskGo;

    //状态
    public GameObject mCompletePart;
    public GameObject mUncompletePart;
    public GameObject mReciveBtn;
    #endregion

    #region 私有变量
    public int TaskId { get; private set; }
    private int mRewardIndex = 0;
    private Property mPropOb = null;
    #endregion

    #region 内部函数
    void Start()
    {
        InitText();
        RegisterEvent();
    }

    private void OnDestroy()
    {
        if (mPropOb != null)
            mPropOb.Destroy();
    }

    /// <summary>
    /// 初始化本地化文本
    /// </summary>
    private void InitText()
    {
        mCompletePart.GetComponentInChildren<UILabel>().text = LocalizationMgr.Get("ResearchRewardWnd_5");
        mUncompletePart.GetComponentInChildren<UILabel>().text = LocalizationMgr.Get("ResearchRewardItemWnd_1");
        mReciveBtn.GetComponentInChildren<UILabel>().text = LocalizationMgr.Get("DungeonsRewardWnd_5");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    void RegisterEvent()
    {
        UIEventListener.Get(mReciveBtn).onClick = OnReceiveBtn;
    }

    /// <summary>
    /// 领取奖励按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnReceiveBtn(GameObject ob)
    {
        if (TaskMgr.GetTaskInfo(TaskId) == null)
            return;

        // 已完成
        if (TaskMgr.IsCompleted(ME.user, TaskId))
        {
            // 已领取
            if (TaskMgr.isBounsReceived(ME.user, TaskId))
                return;

            if (!TaskMgr.CheckCanReceiveBonus(ME.user, TaskId))
                return;

            // 服务器领取奖励
            Operation.CmdReceiveBonus.Go(TaskId);
        }
    }

    private Transform GetItem()
    {
        Transform tran = mRewardGrid.transform.Find(mRewardIndex.ToString());
        if (tran == null)
        {
            GameObject go = NGUITools.AddChild(mRewardGrid.gameObject, mRewardItemPrefab);
            tran = go.transform;
            tran.name = mRewardIndex.ToString();
            mRewardItemPrefab.SetActive(false);
        }
        mRewardIndex++;
        tran.gameObject.SetActive(true);

        return tran;
    }

    private void RecycleItems()
    {
        mRewardIndex = 0;
        for (int i = 0; i < mRewardGrid.transform.childCount; i++)
        {
            Transform tran = mRewardGrid.transform.GetChild(i);
            tran.gameObject.SetActive(false);
        }
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
    #endregion

    #region 外部接口
    /// <summary>
    /// 刷新数据
    /// </summary>
    public void Redraw()
    {
        if (TaskId <= 0 || ME.user == null)
            return;

        CsvRow item = TaskMgr.GetTaskInfo(TaskId);
        if (item == null)
            return;

        //描述
        mTitle.text = TaskMgr.GetTaskDesc(ME.user, TaskId);

        //  任务已完成
        if (TaskMgr.IsCompleted(ME.user, TaskId))
        {
            // 奖励还未领取
            if (!TaskMgr.isBounsReceived(ME.user, TaskId))
            {
                mMaskGo.SetActive(false);
                mReciveBtn.SetActive(true);
                mReciveBtn.GetComponentInChildren<UISpriteAnimation>().enabled = true;
                mCompletePart.SetActive(false);
                mUncompletePart.SetActive(false);
            }
            else
            {
                mMaskGo.SetActive(true);
                mReciveBtn.SetActive(false);
                mReciveBtn.GetComponentInChildren<UISpriteAnimation>().enabled = false;
                mCompletePart.SetActive(true);
                mUncompletePart.SetActive(false);
            }
        }
        else
        {
            mMaskGo.SetActive(false);
            mReciveBtn.SetActive(false);
            mReciveBtn.GetComponentInChildren<UISpriteAnimation>().enabled = false;
            mCompletePart.SetActive(false);
            mUncompletePart.SetActive(true);
        }

        LPCArray bonusList = TaskMgr.GetBonus(ME.user, TaskId);
        // 显示奖励
        RecycleItems();
        for (int i = 0; i < bonusList.Count; i++)
        {
            LPCMapping bonusItem = bonusList[i].AsMapping;

            if (bonusItem.Count == 0)
                continue;

            Transform tran = GetItem();
            tran.GetComponent<SignItemWnd>().Bind(bonusItem, string.Empty, false, false, -1, string.Empty);
            UIEventListener.Get(tran.gameObject).onClick = OnItemBtn;
        }
        mRewardGrid.Reposition();
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    /// <param name="itemId">Item identifier.</param>
    public void BindData(int task_id)
    {
        TaskId = task_id;

        Redraw();
    }
    #endregion
}
