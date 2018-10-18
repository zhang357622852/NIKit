/// <summary>
/// WindowMgr.cs
/// Created by xuhd Oct/20/2014
/// 窗口管理器
/// </summary>

using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using QCCommonSDK;
using LPC;

// 选中图标位置
public enum SelectPos
{
    TopLeftCorner,
    TopRightCorner,
    Center,
    BottomLeftCorner,
    BottonRightCorner,
}

public static class WindowMgr
{
    #region 属性

    /// <summary>
    /// NGUI 的根对象
    /// </summary>
    /// <value>设置场景中的UI根对象</value>
    public static Transform UIRoot { get; set; }

    /// <summary>
    /// 全场窗口对象的实例索引
    /// </summary>
    /// <value>窗口对象名：窗口对象句柄</value>
    private static Dictionary<string, GameObject> WindowObjectMap = new Dictionary<string, GameObject>();

    /// <summary>
    /// 大窗口的有序列表，后打开的加到末尾
    /// </summary>
    /// <value>The window list.</value>

    private static List<WindowBaseSource> windowList = new List<WindowBaseSource> ();
    public static List<WindowBaseSource> WindowList
    {
        get { return windowList;}

        set { windowList = value;}
    }

    // 缓存互斥窗口组
    private static Dictionary<string, ArrayList> mMutexGroup = new Dictionary<string, ArrayList>();

    // 正在打开的窗口列表
    private static Dictionary<string, List<GameObject>> mOpenWndGroup = new Dictionary<string, List<GameObject>>();

    // 自定义窗口配置信息
    private static CsvFile mCustomWindowCsv;

    #endregion

    /// <summary>
    /// Init this instance.
    /// </summary>
    public static void Init()
    {
        // 载入自定义窗口配置表
        mCustomWindowCsv = CsvFileMgr.Load("custom_window");
    }

    /// <summary>
    /// Gets the name of the custom window.
    /// </summary>
    /// <returns>The custom window name.</returns>
    public static string GetCustomWindowName(string wndName)
    {
        // 没有配置信息
        if (mCustomWindowCsv == null)
            return wndName;

        // 获取自定义配置信息
        CsvRow data = mCustomWindowCsv.FindByKey(wndName);

        // 没有自定义窗口配置
        if (data == null)
            return wndName;

        // 获取当前设备型号
        string device = QCCommonSDK.QCCommonSDK.GetPhoneDeviceVersion();

        // 该设备不需要自定义窗口，直接放回原始窗口
        if (! data.Contains(device))
            return wndName;

        // 该设备不需要自定义窗口，直接放回原始窗口
        string customName = data.Query<string>(device);
        if (string.IsNullOrEmpty(customName))
            return wndName;

        // 返回自定义窗口
        return customName;
    }

    /// <summary>
    /// 根据窗口对象实例名称获取窗口
    /// </summary>
    /// <returns>如果存在，返回对象的实例；否则返回null</returns>
    /// <param name="name">窗口对象实例名.</param>
    public static GameObject GetWindow(string name)
    {
        if (WindowObjectMap == null)
            return null;

        // 从缓存列表中直接查找
        if (WindowObjectMap.ContainsKey(name))
            return WindowObjectMap[name];
        else
            return null;
    }

    /// <summary>
    /// 创建窗口对象实例
    /// </summary>
    /// <returns>窗口对象GameObject实例</returns>
    /// <param name="name">窗口实例名</param>
    /// <param name="perfebResource">预设资源完整路径</param>
    public static GameObject CreateWindow(string name, string perfebResource, Transform parentWnd = null, float scare = 1.0f, bool isDontUnload = false)
    {
        // 不应该存在同名窗口，调用者自己控制逻辑
        if (GetWindow(name) != null)
        {
            LogMgr.Trace("名为 {0} 的窗口已存在.", name);
            return null;
        }

        if (WindowObjectMap == null)
            WindowObjectMap = new Dictionary<string, GameObject>();

        if (WindowObjectMap.ContainsKey(name))
            WindowObjectMap.Remove(name);

        // 加载窗口对象实例
        GameObject wndOb = ResourceMgr.Load(perfebResource, isDontUnload) as GameObject;

        if (wndOb == null)
        {
            LogMgr.Trace("不存在预设{0}", perfebResource);
            return null;
        }

        // 没有传父窗口，取根窗口
        if (parentWnd == null)
            parentWnd = UIRoot;

        if (parentWnd == null)
        {
            LogMgr.Trace("取不到父窗口。");
            return null;
        }

        GameObject wnd = GameObject.Instantiate(wndOb, wndOb.transform.localPosition, Quaternion.identity) as GameObject;
        wnd.name = name;

        // 挂到UIRoot下
        Transform ts = wnd.transform;
        ts.SetParent(parentWnd);
        ts.localPosition = wndOb.transform.localPosition;
        ts.localScale = new Vector3(scare, scare, scare);

        WindowObjectMap.Add(name, wnd);
        return wnd;
    }

    /// <summary>
    /// 打开副本闪屏界面
    /// </summary>
    public static void PlaySplashWnd(int splashType)
    {
        // 获取已经打开的窗口界面
        string wndName = "SplashWnd";
        GameObject wnd = WindowMgr.GetWindow(wndName);

        // 需要重新创建窗口
        if (wnd == null)
        {
            // 创建窗口
            wnd = WindowMgr.CreateWindow(wndName,
                string.Format("Assets/Prefabs/Window/{0}.prefab", WindowMgr.GetCustomWindowName(wndName)));
        }

        // 创建窗口失败
        if (wnd == null)
        {
            LogMgr.Trace("创建窗口{0}失败", wndName);
            return;
        }

        // 抛出副本闪屏事件
        wnd.GetComponent<SplashWnd>().SetSplashCurve(splashType);

        // 消息窗口
        WindowMgr.ShowWindow(wnd);
    }

    /// <summary>
    /// 销毁指定名称的对象
    /// </summary>
    /// <param name="name">窗口对象名</param>
    public static void DestroyWindow(string name)
    {
        if (name.Equals(MainWnd.WndType))
        {
            LogMgr.Trace("MainWnd不允许销毁!");
            return;
        }

        // 销毁窗口
        GameObject wndObj = GetWindow(name);

        if (wndObj != null)
            UnityEngine.Object.DestroyImmediate(wndObj);

        // 不包含该窗口
        if (WindowObjectMap == null || !WindowObjectMap.ContainsKey(name))
            return;

        // 移除窗口
        WindowObjectMap.Remove(name);

        // 通知关闭此界面消息
        EventMgr.FireEvent(EventMgrEventType.EVENT_DESTROY_WINDOW, MixedValue.NewMixedValue<string>(name));
    }

    /// <summary>
    /// 控制窗口的显示，调整depth确保后显示的窗口显示在上面
    /// </summary>
    /// <param name="wnd">窗口的GameObject.</param>
    public static void ShowWindow(GameObject wnd, bool sortByDepth = true)
    {
        // 关闭同组其他窗口
        CloseMutexGroup(wnd);

        // 激活窗口
        if (! wnd.activeSelf)
            wnd.SetActive(true);

        // 通知窗口开启
        UIEventShowAndHide script = wnd.GetComponent<UIEventShowAndHide>();
        if (script != null)
            script.Show();

        // 初始化窗口，记录顺序
        wnd.SendMessage("_InitWnd", SendMessageOptions.DontRequireReceiver);
    }

    /// <summary>
    /// 控制窗口的隐藏，
    /// </summary>
    /// <param name="wnd">窗口的GameObject.</param>
    public static void HideWindow(string wndName)
    {
        // 销毁窗口
        GameObject wnd = GetWindow(wndName);

        // 窗口对象不存在
        if (wnd == null)
            return;

        // 通知窗口隐藏
        UIEventShowAndHide script = wnd.GetComponent<UIEventShowAndHide>();
        if (script != null)
            script.Hide();

        // 清除绑定
        wnd.SendMessage("_DetachWnd", SendMessageOptions.DontRequireReceiver);

        // 取消激活状态
        wnd.SetActive(false);
    }

    /// <summary>
    /// 控制窗口的隐藏，
    /// </summary>
    /// <param name="wnd">窗口的GameObject.</param>
    public static void HideWindow(GameObject wnd)
    {
        // 窗口对象不存在
        if (wnd == null)
            return;

        // 通知窗口隐藏
        UIEventShowAndHide script = wnd.GetComponent<UIEventShowAndHide>();
        if (script != null)
            script.Hide();

        // 清除绑定
        wnd.SendMessage("_DetachWnd", SendMessageOptions.DontRequireReceiver);

        // 取消激活状态
        wnd.SetActive(false);
    }

    /// <summary>
    /// 打开主界面
    /// </summary>
    public static GameObject OpenMainWnd()
    {
        GameObject mainWnd = WindowMgr.GetWindow(MainWnd.WndType);
        if (mainWnd == null)
        {
            mainWnd = WindowMgr.CreateWindow(MainWnd.WndType, MainWnd.PrefebResource);

            if (mainWnd == null)
            {
                LogMgr.Trace("打开MainWnd失败");
                return null;
            }
        }

        // 显示主窗口
        WindowMgr.ShowWindow(mainWnd);

        return mainWnd;
    }

    /// <summary>
    /// 隐藏主窗口
    /// </summary>
    public static GameObject HideMainWnd()
    {
        GameObject mainWnd = WindowMgr.GetWindow(MainWnd.WndType);
        if (mainWnd == null)
            return null;

        // 隐藏窗口
        WindowMgr.HideWindow(mainWnd);

        return mainWnd;
    }

    /// <summary>
    /// 打开指定窗口(使用此接口前提是窗口唯一)
    /// </summary>
    /// <param name="wndName">Window name.</param>
    public static GameObject OpenWnd(string wndName, Transform parent = null, string group = "")
    {
        // 当前有窗口正在打开
        if (CheckIsOpenWnd(group))
            return null;

        // 打开合成界面
        GameObject wnd = WindowMgr.GetWindow(wndName);

        // 窗口对象不存在则创建一个
        if (wnd == null)
            wnd = WindowMgr.CreateWindow(wndName,
                string.Format("Assets/Prefabs/Window/{0}.prefab", WindowMgr.GetCustomWindowName(wndName)),
                parent);

        // 创建窗口失败
        if (wnd == null)
        {
            LogMgr.Trace("创建窗口{0}失败", wndName);
            return null;
        }

        // 添加到正在打开的窗口列表中
        if (! string.IsNullOrEmpty(group))
            AddToOpenWndList(wnd, group);

        // 消息窗口
        WindowMgr.ShowWindow(wnd);

        return wnd;
    }

    /// <summary>
    /// 打开战斗窗口
    /// </summary>
    public static void OpenCombatWnd(Property ownerOb)
    {
        // 看看窗口是否已经打开过了，如果没有打开则直接创建
        GameObject mCombatWnd = WindowMgr.GetWindow(CombatWnd.WndType);
        if (mCombatWnd == null)
        {
            // 创建窗口
            mCombatWnd = WindowMgr.CreateWindow(CombatWnd.WndType, CombatWnd.PrefebResource);
            if (mCombatWnd == null)
            {
                LogMgr.Trace("打开mCombatWnd失败");
                return;
            }
        }

        // 获取窗口挂接组件
        CombatWnd mwnd = mCombatWnd.GetComponent<CombatWnd>();
        if (mwnd == null)
        {
            LogMgr.Trace("预置上的脚本丢失");
            return;
        }

        // 显示窗口
        WindowMgr.ShowWindow(mCombatWnd);

        // 绑定窗口
        mwnd.Bind(ownerOb);
    }

    /// <summary>
    /// 加入互斥窗口组
    /// </summary>
    /// <param name="group">要加入的组名</param>
    /// <param name="ob">窗口对象</param>
    public static void AddInMuTextGroup(string group, GameObject wnd_ob)
    {
        ArrayList list = new ArrayList();

        if (!mMutexGroup.ContainsKey(group))
            mMutexGroup.Add(group, new ArrayList());

        mMutexGroup.TryGetValue(group, out list);
        list.Add(wnd_ob);
        mMutexGroup[group] = list;

        // 如果此时窗口是打开的，关闭其他互斥窗口
        if (wnd_ob.activeSelf)
            CloseMutexGroup(wnd_ob);
    }

    /// <summary>
    /// 关闭某组所有窗口
    /// </summary>
    /// <param name="gruop">Gruop.</param>
    public static void HideMutexGroup(string group)
    {
        if (!mMutexGroup.ContainsKey(group))
            return;

        foreach (GameObject ob in mMutexGroup[group])
        {
            if (ob == null ||
                !ob.activeSelf)
                continue;

            HideWindow(ob);
        }
    }

    /// <summary>
    /// 隐藏所有的窗口
    /// </summary>
    public static void HideAllWnd()
    {
        foreach (GameObject wnd in WindowObjectMap.Values)
        {
            if (wnd == null)
                continue;

            if (wnd.activeSelf || wnd.activeInHierarchy)
                HideWindow(wnd);
        }
    }

    /// <summary>
    /// 返回窗口的父窗口，UI Root的下一层
    /// </summary>
    /// <returns>The parent window.</returns>
    /// <param name="wnd">Window.</param>
    public static GameObject GetParentWnd(GameObject wnd)
    {
        Transform wnd_trans = wnd.transform;

        while (wnd_trans.parent != null && wnd_trans.parent.name != "UI Root")
        {
            wnd_trans = wnd_trans.parent;
        }

        return wnd_trans.gameObject;
    }

    /// <summary>
    /// 添加一个通信中的提示窗口.
    /// </summary>
    /// <param name="tips">Tips.</param>
    public static void AddWaittingWnd(string cookie, string tips = "")
    {
        // 获取窗口
        GameObject wnd = WindowMgr.GetWindow(WaittingWnd.WndType);
        if (wnd == null)
            wnd = WindowMgr.CreateWindow(WaittingWnd.WndType, WaittingWnd.PrefebResource);

        // 创建失败
        if (wnd == null)
        {
            LogMgr.Trace("没有窗口prefab，创建失败");
            return;
        }

        if (string.IsNullOrEmpty(tips))
            tips = LocalizationMgr.Get("WindowMgr_1");

        WindowMgr.ShowWindow(wnd, false);

        wnd.GetComponent<WaittingWnd>().SetTipMsg(tips);
    }

    /// <summary>
    /// 将窗口添加到窗口组
    /// </summary>
    /// <param name="name">Name.</param>
    /// <param name="wnd">Window.</param>
    public static void AddToWindowMap(string name, GameObject wnd)
    {
        if (string.IsNullOrEmpty(name))
        {
            LogMgr.Trace("窗口名称不能为空");
            return;
        }

        if (wnd == null)
        {
            LogMgr.Trace("窗口不能为空");
            return;
        }

        if (WindowObjectMap.ContainsKey(name))
            return;

        WindowObjectMap.Add(name, wnd);
    }

    /// <summary>
    /// 将窗口在窗口组张移除
    /// </summary>
    public static void RemoveWindowMapByName(string name)
    {
        // WindowObjectMap为null
        if (WindowObjectMap == null)
            return;

        // 不在缓存中
        if (!WindowObjectMap.ContainsKey(name))
            return;

        // 删除数据
        WindowObjectMap.Remove(name);
    }

    /// <summary>
    /// 将窗口添加到正在打开的窗口列表中
    /// </summary>
    public static void AddToOpenWndList(GameObject wnd, string group)
    {
        if (wnd == null || string.IsNullOrEmpty(group))
            return;

        List<GameObject> list = new List<GameObject>();

        if (! mOpenWndGroup.TryGetValue(group, out list))
            list = new List<GameObject>();

        if (! list.Contains(wnd))
            list.Add(wnd);

        mOpenWndGroup[group] = list;
    }

    /// <summary>
    /// 移除正在打开的窗口列表中的窗口
    /// </summary>
    public static void RemoveOpenWnd(GameObject wnd, string group)
    {
        if (wnd == null || string.IsNullOrEmpty(group))
            return;

        List<GameObject> list = new List<GameObject>();

        if (! mOpenWndGroup.TryGetValue(group, out list))
            list = new List<GameObject>();

        // 移除缓存窗口
        list.Remove(wnd);

        mOpenWndGroup[group] = list;
    }

    #region 内部函数

    /// <summary>
    /// 判断是否有窗口正在打开
    /// </summary>
    private static bool CheckIsOpenWnd(string group)
    {
        if (string.IsNullOrEmpty(group))
            return false;

        List<GameObject> list = new List<GameObject>();

        // 当前是多个窗口存在的分组
        if (group.Equals(WindowOpenGroup.MULTIPLE_OPEN_WND))
        {
            // 尝试获取单个窗口的分组列表
            if (!mOpenWndGroup.TryGetValue(WindowOpenGroup.SINGLE_OPEN_WND, out list))
                list = new List<GameObject>();

            if (list.Count == 0)
                return false;
            else
                return true;
        }
        else
        {
            // 尝试获取多个窗口存在的分组
            if (!mOpenWndGroup.TryGetValue(WindowOpenGroup.MULTIPLE_OPEN_WND, out list))
                list = new List<GameObject>();

            if (list.Count == 0)
            {
                if (!mOpenWndGroup.TryGetValue(WindowOpenGroup.SINGLE_OPEN_WND, out list))
                    list = new List<GameObject>();

                if (list.Count == 0)
                    return false;
                else
                    return true;
            }
            else
            {
                return true;
            }
        }
    }

    /// 关闭互斥组其他打开的窗口
    /// </summary>
    private static void CloseMutexGroup(GameObject wnd_ob)
    {
        // 取得所在的互斥组
        ArrayList groupList = GetMutexGroup(wnd_ob);
        if (groupList.Count <= 0)
            return;

        foreach (string group in groupList)
        {
            foreach (GameObject ob in mMutexGroup[group])
            {
                if (ob == null ||
                    !ob.activeSelf ||
                    ob == wnd_ob)
                    continue;

                HideWindow(ob);
            }
        }
    }

    /// <summary>
    /// 取得所在的互斥窗口组
    /// </summary>
    /// <returns>The mutex group.</returns>
    /// <param name="wnd_ob">Wnd_ob.</param>
    private static ArrayList GetMutexGroup(GameObject wnd_ob)
    {
        ArrayList groupList = new ArrayList();

        if (mMutexGroup == null ||
            mMutexGroup.Count <= 0)
            return groupList;

        foreach (string group in mMutexGroup.Keys)
        {
            foreach (GameObject ob in mMutexGroup[group])
            {
                if (ob == null || ob != wnd_ob)
                    continue;

                groupList.Add(group);
                break;
            }
        }

        return groupList;
    }

    #endregion
}
