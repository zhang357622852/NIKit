/// <summary>
/// ActivityTaskWnd.cs
/// Created by zhaozy 2018/06/01
/// 活动任务631.5
/// </summary>

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class ActivityTaskWnd : WindowBase<ActivityTaskWnd>
{
    #region 成员变量

    // 标题
    public UILabel mtaskDesc;

    // 使魔元素图标
    public GameObject mReceiveBtn;
    public UISprite mReceiveBtnSprite;
    public UILabel mReceiveLabel;
    public GameObject mDisableSprite;

    // 奖励信息
    public GameObject mItemWnd;
    public UIGrid mItemWndGrid;
    public GameObject mBonusScrollView;

    // 奖励显示框可以显示数量
    public int mShowBonusItemNum = 0;

    #endregion

    #region 私有变量

    private Property mUser;
    private string mActivityCookie = string.Empty;
    private CsvRow mTaskData;

    /// <summary>
    /// The m cache sign item window.
    /// </summary>
    private List<SignItemWnd> mCacheSignItemWnd = new List<SignItemWnd>();

    /// <summary>
    /// The m property ob.
    /// </summary>
    Property mPropOb = null;

    #endregion

    #region 内部函数

    /// <summary>
    /// Start this instance.
    /// </summary>
    void Start()
    {
        // 注册按钮点击事件
        UIEventListener.Get(mReceiveBtn).onClick = OnClickReceiveBtn;
    }

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
    /// 刷新数据
    /// </summary>
    void Redraw()
    {
        // 没有任务数据
        if (mUser == null || mTaskData == null)
            return;

        // 获取子任务ID
        int taskId = mTaskData.Query<int>("task_id");

        // 获取任务描述
        mtaskDesc.text = ActivityMgr.GetActivityTaskDesc(mUser, mActivityCookie, taskId);

        // 任务未完成
        if (!ActivityMgr.IsCompletedActivityTask(mUser, mActivityCookie, taskId))
        {
            mReceiveLabel.text = LocalizationMgr.Get("ActivityTaskWnd_1"); 
            mReceiveBtn.GetComponent<BoxCollider>().enabled = false;
            mDisableSprite.SetActive(true);
        }
        else if (ActivityMgr.HasActivityTaskBonus(mUser, mActivityCookie, taskId))
        {
            mReceiveLabel.text = LocalizationMgr.Get("ActivityTaskWnd_2"); 
            mReceiveBtn.GetComponent<BoxCollider>().enabled = true;
            mDisableSprite.SetActive(false);
        }
        else
        {
            mReceiveLabel.text = LocalizationMgr.Get("ActivityTaskWnd_3"); 
            mReceiveBtn.GetComponent<BoxCollider>().enabled = false;
            mDisableSprite.SetActive(true);
        }

        // 获取任务
        LPCArray bonusList = ActivityMgr.GetActivityTaskBonus(taskId);
        if (bonusList == null || bonusList.Count == 0)
            return;

        // 填充数据
        for (int i = 0; i < bonusList.Count; i++)
        {
            // 获取一个控件
            SignItemWnd wndOb = GetSignItemWnd(i);
            if (wndOb == null)
                continue;

            // 绑定数据
            wndOb.NormalItemBind(bonusList[i].AsMapping, false);
            wndOb.ShowAmount(true);

            // 注册点击事件
            UIEventListener.Get(wndOb.gameObject).onClick = OnItemBtn;
        }

        // 重置位置
        mItemWndGrid.Reposition();

        // 重置UIScrollView位置
        if (bonusList.Count > mShowBonusItemNum)
            mBonusScrollView.GetComponent<UIScrollView>().ResetPosition();
    }

    /// <summary>
    /// 关闭按钮点击事件
    /// </summary>
    void OnClickReceiveBtn(GameObject go)
    {
        // 没有任务数据
        if (mUser == null || mTaskData == null)
            return;

        // 领取奖励
        ActivityMgr.ReceiveActivityTaskBonus(mUser,
            mActivityCookie,
            mTaskData.Query<int>("task_id"));
    }

    /// <summary>
    /// Gets the sign item window.
    /// </summary>
    /// <returns>The sign item window.</returns>
    /// <param name="index">Index.</param>
    private SignItemWnd GetSignItemWnd(int index)
    {
        // 直接获取缓存数据
        if (mCacheSignItemWnd.Count > index &&
            mCacheSignItemWnd[index] != null)
            return mCacheSignItemWnd[index];

        // 创建新对象
        GameObject item = Instantiate(mItemWnd);
        item.transform.SetParent(mItemWndGrid.transform);
        item.transform.localPosition = Vector3.zero;
        item.transform.localScale = Vector3.one;
        item.name = "item_" + index;
        item.SetActive(true);

        // 添加到缓存列表中
        SignItemWnd signItemWnd = item.GetComponent<SignItemWnd>();
        mCacheSignItemWnd.Add(signItemWnd);
        return signItemWnd;
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
    /// 绑定数据
    /// </summary>
    public void Bind(Property user, string activityCookie, string activityId, CsvRow taskData)
    {
        // 绑定数据
        mUser = user;
        mActivityCookie = activityCookie;
        mTaskData = taskData;

        // 重回窗口
        Redraw();
    }

    #endregion
}
