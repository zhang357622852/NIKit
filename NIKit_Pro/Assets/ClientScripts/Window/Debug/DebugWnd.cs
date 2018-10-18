/// <summary>
/// DebugWindow.cs
/// Created by xuhd Sec/04/2014
/// 调试窗口
/// </summary>
using UnityEngine;
using System.Collections;

public class DebugWnd : MonoBehaviour
{
    #region 公共字段

    public GameObject mDebugBtn;
    // 调试按钮
    public TweenPosition tp;
    // 动画组件

    #endregion

    #region 私有字段

    private const string mPrefabPath = "Assets/Prefabs/Window/Debug/DebugOptionsWnd.prefab";

    private GameObject mDebugOptionsWnd = null;
    private Vector3 mLeft = new Vector3(-550, 230, 0);
    private Vector3 mRight = new Vector3(-550, 230, 0);

    #endregion

    // Use this for initialization
    void Start()
    {
        RegisterEvent();
    }

    #region 外部接口

    /// <summary>
    /// 隐藏窗口
    /// </summary>
    public void Hide()
    {
        if (mDebugOptionsWnd != null)
        {
            Destroy(mDebugOptionsWnd);
        }
        gameObject.SetActive(false);
    }

    #endregion

    #region 内部方法

    /// <summary>
    /// 注册窗口事件
    /// </summary>
    private void RegisterEvent()
    {
        UIEventListener.Get(mDebugBtn).onClick += OnBtnClick;
        EventMgr.RegisterEvent(string.Format("DebugWnd_{0}", gameObject.GetInstanceID()), EventMgrEventType.EVENT_DEBUG_OPTION_WND_WIDTH_CHANGE, OnOptionWndWidthChange);
    }

    /// <summary>
    /// 点击Debug按钮
    /// </summary>
    /// <param name="go">Go.</param>
    private void OnBtnClick(GameObject go)
    {
        GameObject wndOb;
        if (mDebugOptionsWnd == null)
        {
            // 加载窗口对象实例
            wndOb = ResourceMgr.Load(mPrefabPath) as GameObject;

            if (wndOb == null)
            {
                LogMgr.Trace("加载DebugOptionWnd失败");
                return;
            }

            GameObject wnd = GameObject.Instantiate(wndOb, wndOb.transform.localPosition, Quaternion.identity) as GameObject;
            wnd.name = wndOb.name;

            // 挂到UIRoot下
            Transform ts = wnd.transform;
            ts.parent = WindowMgr.UIRoot;
            ts.localPosition = wndOb.transform.localPosition;
            ts.localScale = Vector3.one;
            mDebugOptionsWnd = wnd;
            mDebugOptionsWnd.SetActive(true);
            mDebugOptionsWnd.GetComponent<DebugOptionsWnd>().OpenWnd();
            OnOpen();
        }
        else
        {
            if (mDebugOptionsWnd.GetComponent<DebugOptionsWnd>().mIsOpen)
            {
                mDebugOptionsWnd.GetComponent<DebugOptionsWnd>().HideWnd();
                OnHide();
            }
            else
            {
                mDebugOptionsWnd.GetComponent<DebugOptionsWnd>().OpenWnd();
                OnOpen();
            }
        }
    }

    private void OnOpen()
    {
        tp.from = mLeft;
        tp.to = mRight;
        tp.enabled = true;
        tp.ResetToBeginning();
    }

    private void OnHide()
    {
        tp.from = mRight;
        tp.to = mLeft;
        tp.enabled = true;
        tp.ResetToBeginning();
    }

    /// <summary>
    /// 选项窗口宽度改变了，需调整DebugWnd位置
    /// </summary>
    /// <param name="eventId">Event identifier.</param>
    /// <param name="para">Para.</param>
    private void OnOptionWndWidthChange(int eventId, MixedValue para)
    {
        return;
        //int deltaWidth = para.GetValue<int>();

        //Vector3 pos = transform.localPosition;
        //transform.localPosition = new Vector3(pos.x + deltaWidth, pos.y, pos.z);
    }

    #endregion
}
