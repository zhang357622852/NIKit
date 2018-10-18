/// <summary>
/// SystemFunctionItemWnd.cs
/// Created by fengsc 2017/03/31
/// 系统功能基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class SystemFunctionItemWnd : WindowBase<SystemFunctionItemWnd>
{
    public UITexture mIcon;

    public UILabel mDescLb;

    public TweenRotation mEffect;

    // 红点提示
    public GameObject mRedPoint;

    public GameObject mNewTips;

    public LPCMapping mData = LPCMapping.Empty;

    [HideInInspector]
    public int mIndex = 0;

    string mDesc = string.Empty;

    int mRemainTime = -1;

    bool mEnableCountDown = false;

    float mLastTime = 0f;

    void OnEnable()
    {
        // 刷新剩余时间
        RefreshRemainTime();

        mLastTime = 0f;
    }

    void Update()
    {
        if (mEnableCountDown)
        {
            if (Time.realtimeSinceStartup > mLastTime + 1)
            {
                mLastTime = Time.realtimeSinceStartup;

                // 倒计时
                CountDown();
            }
        }
    }

    /// <summary>
    /// 刷新剩余时间
    /// </summary>
    void RefreshRemainTime()
    {
        if (mData == null || mData.Count == 0)
            return;

        // 获取结束时间
        int endTime = mData.GetValue<int>("end_time");

        // 计算剩余时间
        mRemainTime = Mathf.Max(endTime -TimeMgr.GetServerTime(), 0);

        // 标识倒计时开始
        mEnableCountDown = mRemainTime > 0;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        string iconName = string.Empty;
        string name = string.Empty;

        if (mData.ContainsKey("task_id"))
        {
            CsvRow taskData = TaskMgr.GetTaskInfo(mData.GetValue<int>("task_id"));

            iconName = taskData.Query<string>("icon");
            name = LocalizationMgr.Get(taskData.Query<string>("title"));
        }
        else
        {
            LPCValue script = mData.GetValue<LPCValue>("show_icon_script");
            if (script.IsInt && script.AsInt != 0)
            {
                object ret = ScriptMgr.Call(script.AsInt, ME.user, mData);
                iconName = ret.ToString();
            }
            name = LocalizationMgr.Get(mData.GetValue<string>("name"));
        }

        string resPath = string.Format("Assets/Art/UI/Icon/item/{0}.png", iconName);

        mIcon.mainTexture = ResourceMgr.LoadTexture(resPath);

        mIcon.width = 80;
        mIcon.height = 80;

        // 提示信息显示判断脚本
        LPCValue showScript = mData.GetValue<LPCValue>("show_tips_script");

        if (showScript.IsInt && showScript.AsInt != 0)
        {
            object ret = ScriptMgr.Call(showScript.AsInt, ME.user, mData);

            LPCMapping showArgs = mData.GetValue<LPCMapping>("show_tips_args");

            if (!(bool)ret)
            {
                SetEffect(false);

                SetRedPoint(false, showArgs.GetValue<int>("type"));
            }
            else
            {
                SetEffect(true);

                SetRedPoint(true, showArgs.GetValue<int>("type"));
            }
        }
        else
        {
            SetEffect(false);

            SetRedPoint(false, SystemFunctionConst.RED_POINT);
        }

        // 刷新剩余时间
        RefreshRemainTime();

        if (!mEnableCountDown)
            mDescLb.text = string.IsNullOrEmpty(mDesc) ? name : mDesc;
    }

    /// <summary>
    /// 倒计时
    /// </summary>
    void CountDown()
    {
        if (mRemainTime <= 0)
        {
            mDescLb.text = 0.ToString();

            // 抛出活动列表事件, 刷新列表
            EventMgr.FireEvent(EventMgrEventType.EVENT_NOTIFY_ACTIVITY_LIST, null);

            // 结束调用
            mEnableCountDown = false;
        }

        mDescLb.text = TimeMgr.ConvertTime(mRemainTime, true, false);

        mRemainTime--;
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(LPCMapping para, int index, string desc = "")
    {
        if (para == null)
            return;

        mData = para;

        mDesc = desc;

        mIndex = index;

        Redraw();
    }

    /// <summary>
    /// 设置图标背景光效
    /// </summary>
    public void SetEffect(bool enable)
    {
        mEffect.gameObject.SetActive(enable);
        mEffect.enabled = enable;
    }

    /// <summary>
    /// 设置红点提示
    /// </summary>
    public void SetRedPoint(bool enable, int type)
    {
        if (type.Equals(SystemFunctionConst.RED_POINT))
        {
            mRedPoint.SetActive(enable);
            mNewTips.SetActive(false);
        }
        else
        {
            mRedPoint.SetActive(false);
            mNewTips.SetActive(enable);
        }
    }
}
