/// <summary>
/// ActivityWnd.cs
/// Created by lic 06/05/2017
/// 活动界面
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ActivityWnd : WindowBase<ActivityWnd>
{
    #region 成员变量

    public GameObject mCloseBtn;

    public UIScrollView mScrollView;

    public UIGrid mGrid;

    public UILabel mNoActivityDesc;

    #endregion

    #region 私有变量

    // 有序的活动列表
    List<LPCMapping> mActivityList = new List<LPCMapping>();

    #endregion

    #region 内部函数

    // Use this for initialization
    void Start()
    {
        // 注册事件
        RegisterEvent();

        //初始化窗口
        InitWnd();

        // 初始化数据
        InitData();

        TweenScale mTweenScale = transform.GetComponent<TweenScale>();

        if (mTweenScale == null)
            return;

        float scale = Game.CalcWndScale();
        mTweenScale.to = new Vector3(scale, scale, scale);
    }

    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("ActivityWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mCloseBtn).onClick = OnCloseBtn;

        // 注册活动列表变化事件
        EventMgr.RegisterEvent("ActivityWnd", EventMgrEventType.EVENT_NOTIFY_ACTIVITY_LIST, OnNotifyActivityList);
    }

    /// <summary>
    /// 活动列表通知事件回调
    /// </summary>
    void OnNotifyActivityList(int eventID, MixedValue value)
    {
        // 销毁子物体
        NGUITools.DestroyChildren(mGrid.transform);

        // 刷新界面
        InitData();
    }

    /// <summary>
    /// 初始化窗口
    /// </summary>
    private void InitWnd()
    {
        mNoActivityDesc.text = LocalizationMgr.Get("ActivityWnd_1");
    }

    /// <summary>
    /// 初始化数据.
    /// </summary>
    void InitData()
    {
        // 获取有序的活动列表
        mActivityList = ActivityMgr.GetShowActivityList(ActivityMgr.GetOrderActivityList());

        mNoActivityDesc.gameObject.SetActive(mActivityList.Count <= 2);

        for(int i = 0; i < mActivityList.Count; i++)
        {
            LPCMapping activityData = mActivityList[i];

            string activityId = activityData.GetValue<string>("activity_id");

            CsvRow row = ActivityMgr.ActivityCsv.FindByKey(activityId);

            // 没有配置数据
            if (row == null)
                continue;

            // 获取资源路径
            string path = string.Format("Assets/Prefabs/Window/{0}.prefab",
                WindowMgr.GetCustomWindowName(row.Query<string>("wnd_name")));

            // 加载窗口对象实例
            GameObject wndOb = ResourceMgr.Load(path) as GameObject;

            if (wndOb == null)
            {
                LogMgr.Trace("不存在预设{0}", path);
                continue;
            }

            GameObject item = GameObject.Instantiate(wndOb);

            // 设置父级
            item.transform.SetParent(mGrid.transform);

            item.name = activityId;

            // 初始化
            item.transform.localScale = Vector3.one;

            item.transform.localPosition = Vector3.zero;

            item.SetActive(true);

            // 注册点击事件
            UIEventListener.Get(item).onClick = OnItemClicked;

            // 绑定数据
            item.GetComponent<ActivityItemWnd>().BindData(activityData, row);

            item.GetComponent<UIDragScrollView>().scrollView = mScrollView;
        }

        // 排序子物体
        mGrid.Reposition();

        //固定滚动条
        mScrollView.DisableSpring();

    }

    /// <summary>
    /// item被点击
    /// </summary>
    void OnItemClicked(GameObject ob)
    {
        ActivityItemWnd item = ob.GetComponent<ActivityItemWnd>();

        if (item == null || item.ActivityInfo == null)
            return;

        // 取消新活动标识
        if (ActivityMgr.IsNewActivity(ME.user, item.ActivityInfo.GetValue<string>("cookie")))
        {
            ActivityMgr.CancelActivityNewState(ME.user, item.ActivityInfo.GetValue<string>("cookie"));
            item.SetNew();
        }

        string activityId = item.ActivityInfo.GetValue<string>("activity_id");

        LPCValue click_script = ActivityMgr.ActivityCsv.FindByKey(activityId).Query<LPCValue>("click_script");

        if (click_script == null || !click_script.IsInt)
            return;

        // 点击参数
        LPCMapping click_args = ActivityMgr.ActivityCsv.FindByKey(activityId).Query<LPCMapping>("click_args");

        // 调用脚本处理点击事件
        ScriptMgr.Call(click_script.AsInt, click_args, item.ActivityInfo);
    }

    /// <summary>
    /// 关闭按钮被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnCloseBtn(GameObject ob)
    {
        WindowMgr.DestroyWindow(gameObject.name);
    }

    #endregion
}
