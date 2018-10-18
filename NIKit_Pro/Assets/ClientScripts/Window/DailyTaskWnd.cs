/// <summary>
/// DailyTaskWnd.cs
/// Created by fengsc 2018/07/24
/// 每日任务界面
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyTaskWnd : WindowBase<DailyTaskWnd>
{
    public UIScrollView mScrollView;

    public UIWrapContent mWrapContent;

    public GameObject mTaskItem;

    public UIPanel mPanel;

    // 创建item个数
    int mColumnCount = 7;

    // 当前页面的数据
    List<int> mData = new List<int>();

    // 当前显示数据的index与实际数据的对应关系
    Dictionary<int, int> indexMap = new Dictionary<int, int>();

    // 起始位置
    Dictionary<GameObject,Vector3> rePosition = new Dictionary<GameObject,Vector3>();

    // taskItem
    List<GameObject> taskItemObList = new List<GameObject>();

    void OnEnable()
    {
        // 注册事件
        RegisterEvent();

        Redraw(true);
    }

    // Use this for initialization
    void Start()
    {
        mTaskItem.SetActive (false);

        // 刷新格子
        CreateItem();
    }

    void OnDisable()
    {
        WindowMgr.RemoveOpenWnd(gameObject, WindowOpenGroup.SINGLE_OPEN_WND);

        mWrapContent.onInitializeItem -= OnUpdateItem;

        // 解注册事件
        if (ME.user == null)
            return;

        ME.user.dbase.RemoveTriggerField("DailyTaskWnd");
    }

    /// <summary>
    /// 注册事件
    /// </summary>
    private void RegisterEvent()
    {
        mWrapContent.onInitializeItem = OnUpdateItem;

        // 解注册事件
        ME.user.dbase.RemoveTriggerField("DailyTaskWnd");
        ME.user.dbase.RegisterTriggerField("DailyTaskWnd", new string[] {"task"}, new CallBack (OnTaskChange));
        ME.user.dbase.RegisterTriggerField("DailyTaskWnd", new string[] {"daily_task"}, new CallBack (OnDailyTaskChange));
    }

    /// <summary>
    /// Raises the destroy event.
    /// </summary>
    void OnDestroy()
    {
        // 解注册事件
        EventMgr.UnregisterEvent("DailyTaskWnd");

        // 玩家对象不存在
        if (ME.user == null)
            return;

        // 移除属性字段关注回调
        ME.user.dbase.RemoveTriggerField("DailyTaskWnd");
    }

    /// <summary>
    /// 创建任务格子(动态复用)
    /// </summary>
    void CreateItem()
    {
        for(int i = 0; i < mColumnCount; i++)
        {
            GameObject item = Instantiate (mTaskItem) as GameObject;
            item.transform.parent = mWrapContent.transform;
            item.name = string.Format("task_item_{0}", i);
            item.transform.localScale = Vector3.one;
            item.transform.localPosition = new Vector3 (0, - i * 110, 0);

            item.SetActive(true);

            // 注册点击事件
            UIEventListener.Get(item).onClick = OnTaskItemClicked;

            // 记录item初始位置
            rePosition.Add(item, item.transform.localPosition);

            taskItemObList.Add (item);
        }
    }

    /// <summary>
    /// 任务item被点击
    /// </summary>
    void OnTaskItemClicked(GameObject ob)
    {
        GameObject wnd = WindowMgr.GetWindow(TaskInfoWnd.WndType);

        if (wnd == null)
            wnd = WindowMgr.CreateWindow(TaskInfoWnd.WndType, TaskInfoWnd.PrefebResource);

        if (wnd == null)
        {
            LogMgr.Trace("TaskInfoWnd打开失败");
            return;
        }

        // 显示邮件窗口
        WindowMgr.ShowWindow(wnd);

        wnd.GetComponent<TaskInfoWnd>().BindData(ob.GetComponent<TaskItemWnd>().TaskId);
    }

    /// <summary>
    /// task字段变化回调
    /// </summary>
    void OnTaskChange(object para, params object[] _params)
    {
        Redraw();
    }

    /// <summary>
    /// DailyTask字段变化回调
    /// </summary>
    void OnDailyTaskChange(object para, params object[] _params)
    {
        Redraw();
    }

    /// <summary>
    /// 签到被点击
    /// </summary>
    /// <param name="ob">Ob.</param>
    void OnSignBtn(GameObject ob)
    {
        GameObject wnd = WindowMgr.GetWindow(SignWnd.WndType);

        if (wnd == null)
            wnd = WindowMgr.CreateWindow(SignWnd.WndType, SignWnd.PrefebResource);

        if (wnd == null)
        {
            LogMgr.Trace("SignWnd打开失败");
            return;
        }

        // 显示邮件窗口
        WindowMgr.ShowWindow(wnd);
    }

    /// <summary>
    /// 刷新当前页面
    /// </summary>
    /// <param name="type">Type.</param>
    void Redraw(bool resetPosition = false)
    {
        // 刷新数据
        InitData();

        // 需要复位格子位置
        if(resetPosition)
        {
            foreach(GameObject item in rePosition.Keys)
                item.transform.localPosition = rePosition[item];

            // 此处是要还原scrollview的位置，
            // 但是没找到scrollview具体可用的接口
            mPanel.clipOffset = new Vector2(0f, 0f);
            mScrollView.transform.localPosition = new Vector3(0f, 0f, 0f);

            if(indexMap != null)
                indexMap.Clear();

            for(int i = 0; i < mColumnCount; i++)
            {
                indexMap.Add(i, -i);
            }
        }

        // 填充数据
        foreach(KeyValuePair<int, int> kv in indexMap)
        {
            FillData(kv.Key, kv.Value);
        }
    }

    /// <summary>
    /// 初始化数据.
    /// </summary>
    void InitData()
    {
        mData = TaskMgr.GetStandardTasksByType(TaskConst.DAILY_TASK, ME.user);

        int rowAmount = mData.Count > mColumnCount ? mData.Count : mColumnCount;

        //固定滚动条
        mScrollView.DisableSpring();

        // 从0开始
        mWrapContent.maxIndex = 0;

        mWrapContent.minIndex = -(rowAmount - 1);
    }

    /// <summary>
    ///设置滑动列表时复用宠物格子更改数据
    /// </summary>
    void OnUpdateItem(GameObject go, int index, int realIndex)
    {
        // 将index与realindex对应关系记录下来
        if(!indexMap.ContainsKey(index))
            indexMap.Add(index, realIndex);
        else
            indexMap[index] = realIndex;

        FillData(index, realIndex);
    }

    /// <summary>
    /// 填充数据
    /// </summary>
    /// <param name="index">Index.</param>
    /// <param name="realIndex">Real index.</param>
    void FillData(int index, int realIndex)
    {
        if (index >= taskItemObList.Count)
            return;

        GameObject taskItemWnd = taskItemObList[index];

        if(taskItemWnd == null)
            return;

        TaskItemWnd item = taskItemWnd.GetComponent<TaskItemWnd>();

        if(item == null)
            return;

        int dataIndex =  System.Math.Abs(realIndex);

        if(dataIndex < mData.Count)
        {
            item.BindData(mData[dataIndex]);
        }
    }
}
