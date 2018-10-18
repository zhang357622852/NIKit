/// <summary>
/// UserMailItemWnd.cs
/// Created by fengsc 2016/11/03
/// 玩家发送的邮件基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class UserMailItemWnd : WindowBase<UserMailItemWnd>
{
    #region 成员变量

    // 玩家头像
    public UITexture mIcon;

    // 玩家名称
    public UILabel mName;

    public UILabel mDesc;

    // 邮件有效剩余时间
    public UILabel mRemainTime;

    // 增加的友情点
    public UILabel mAmount;

    // 邮件基础数据
    [HideInInspector]
    public LPCMapping mMailData;

    [HideInInspector]
    public int mTime;

    #endregion

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        LPCValue t = mMailData.GetValue<LPCValue>("from_name");
        LPCValue message = null;

        if (t != null && t.IsString)
            message = LPCRestoreString.RestoreFromString(t.AsString);

        mName.text = LocalizationMgr.GetServerDesc(message);

        mDesc.text = LocalizationMgr.Get("MailWnd_9");

        LPCArray list = mMailData.GetValue<LPCArray>("belonging_list");

        if (list == null || list.Count < 1)
            return;

        string fields = FieldsMgr.GetFieldInMapping(list[0].AsMapping);

        mAmount.text = "+ " + list[0].AsMapping.GetValue<int>(fields);

        // 邮件的失效时间
        int expire = mMailData.GetValue<int>("expire");

        mTime = expire - TimeMgr.GetServerTime();

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

        // 加载玩家头像
        LPCValue iconValue = mMailData.GetValue<LPCValue>("from_icon");
        if (iconValue != null && iconValue.IsString)
            mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/monster/{0}.png", iconValue.AsString));
        else
            mIcon.mainTexture = null;
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
