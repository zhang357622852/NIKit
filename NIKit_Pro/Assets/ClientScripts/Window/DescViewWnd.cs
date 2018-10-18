/// <summary>
/// DescViewWnd.cs
/// Created by fengsc 2017/03/31
/// 描述悬浮窗口
/// </summary>
using UnityEngine;
using System.Collections;
using LPC;

public class DescViewWnd : WindowBase<DescViewWnd>
{
    public UISprite mBg;

    public UILabel mDescLb;

    // 控件宽度的偏移量
    int mOffsetWidth = 40;

    int mOffsetHeight = 27;

    int mFrameHeight = 14;

    LPCMapping mData = LPCMapping.Empty;

    GameObject mTarget;

    // 是否显示
    private bool mEnableCountDown = false;

    private float mLastTime = 0f;

    private int mRemainTime = 0;

    private void Update()
    {
        if (mEnableCountDown)
        {
            if (Time.realtimeSinceStartup > mLastTime + 1.0f)
            {
                mLastTime = Time.realtimeSinceStartup;

                // 倒计时
                CountDown();
            }
        }
    }

    /// <summary>
    /// 倒计时
    /// </summary>
    private void CountDown()
    {
        if (mRemainTime <= 0)
        {
            HideView();

            return;
        }

        mRemainTime--;
    }

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        string desc = string.Empty;

        if (mData.ContainsKey("task_id"))
        {
            desc = TaskMgr.GetTaskDesc(ME.user, mData.GetValue<int>("task_id"));
        }
        else
        {
            desc = LocalizationMgr.Get(mData.GetValue<string>("desc"));
        }

        mDescLb.text = desc;

        mDescLb.UpdateNGUIText();

        // 计算背景图片的大小
        mBg.width = mOffsetWidth + Mathf.RoundToInt(NGUIText.CalculatePrintedSize(desc).x);
        mBg.height = mOffsetHeight + mDescLb.height + mFrameHeight;

        mDescLb.transform.localPosition = new Vector3(
            mDescLb.transform.localPosition.x,
            mBg.transform.localPosition.y - (mBg.height - mDescLb.height) * 0.5f,
            mDescLb.transform.localPosition.z);

        // 设置当前对象transform的相对位置
        UITexture bg = mTarget.GetComponent<UITexture>();
        if (bg == null)
            return;

        if (SystemFunctionConst.SCREEN_LEFT.Equals(mData.GetValue<int>("show_pos")))
        {
            Vector3 worldTarget = transform.parent.InverseTransformPoint(mTarget.transform.position);
            this.transform.localPosition = new Vector3(
                worldTarget.x + bg.width * 0.5f,
                worldTarget.y - bg.height * 0.5f,
                worldTarget.z);
        }
        else
        {
            Vector3 worldTarget = transform.parent.InverseTransformPoint(mTarget.transform.position);
            this.transform.localPosition = new Vector3(
                worldTarget.x - bg.width * 0.5f,
                worldTarget.y + bg.height * 0.5f,
                worldTarget.z);
        }
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void ShowView(LPCMapping data, GameObject target)
    {
        if (mData != null && data != null)
        {
            if (mData.GetValue<int>("id").Equals(data.GetValue<int>("id")))
            {
                if (mEnableCountDown)
                    return;
            }
        }

        mData = data;

        if (mData == null)
            return;

        mTarget = target;

        // 绘制窗口
        Redraw();

        mLastTime = 0;

        mRemainTime = 2;

        mEnableCountDown = true;

        gameObject.SetActive(true);
    }

    public void HideView()
    {
        mEnableCountDown = false;

        gameObject.SetActive(false);
    }
}
