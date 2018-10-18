/// <summary>
/// SystemMailItemWnd.cs
/// Created by fengsc 2016/11/03
/// 系统邮件基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class SystemMailItemWnd : WindowBase<SystemMailItemWnd>
{
    #region 成员变量

    // 邮件标题
    public UILabel mTitle;

    // 邮件剩余有效时间
    public UILabel mRemainTime;

    public UILabel mPoint;

    public GameObject mBox;

    [HideInInspector]
    public int mTime;

    // 邮件简易数据
    [HideInInspector]
    public LPCMapping mMailData;

    #endregion

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        LPCValue title = mMailData.GetValue<LPCValue>("title");

        // 数据异常
        if (title == null)
            return;

        // 尝试还原title数据
        title = LPCRestoreString.SafeRestoreFromString(title.AsString);

        // 邮件标题
        mTitle.text = LocalizationMgr.GetServerDesc(title);

        // 获取附件列表
        LPCValue belogingList = mMailData.GetValue<LPCValue>("belonging_list");

        // 没有附件的邮件不显示宝箱图标
        if (belogingList == null
            || !belogingList.IsArray
            || belogingList.AsArray.Count == 0)
            mBox.SetActive(false);
        else
            mBox.SetActive(true);

        // 邮件的失效时间
        int expire = mMailData.GetValue<int>("expire");

        // 剩余的有效时间
        mTime = expire - TimeMgr.GetServerTime();

        int state = mMailData.GetValue<int>("state");
        if (state == ExpressStateType.EXPRESS_STATE_READ)
        {
            mPoint.text = LocalizationMgr.Get("MainWnd_15");
        }
        else
        {
            mPoint.text = LocalizationMgr.Get("MainWnd_16");
        }

        // 邮件的有效期
        int day = mTime / 86400;

        if (day > 0)
        {
            if (mTime % 86400 != 0)
                day += 1;

            // 剩余天数
            mRemainTime.text = string.Format(LocalizationMgr.Get("MailWnd_3"), day);
        }
        else
        {
            // 剩余多少小时
            if (mTime >= 3600)
                mRemainTime.text = string.Format(LocalizationMgr.Get("MailWnd_13"), mTime / 3600);
            else
                mRemainTime.text = string.Format(LocalizationMgr.Get("MailWnd_14"), Mathf.Max(1, mTime / 60));
        }
    }

    #region 外部接口

    public void Bind(LPCMapping data)
    {
        mMailData = data;

        if (mMailData == null)
            return;

        // 绘制窗口
        Redraw();
    }

    #endregion
}
