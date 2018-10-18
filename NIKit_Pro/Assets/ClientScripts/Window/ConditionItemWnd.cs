/// <summary>
/// ConditionItemWnd.cs
/// Created by fengsc 2018/01/25
/// </summary>
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LPC;

public class ConditionItemWnd : WindowBase<ConditionItemWnd>
{
    public UISprite[] mStars;

    public UILabel mItemLb;

    [HideInInspector]
    public int mStep = -2;

    void Redraw()
    {
        if (mStep == -1 && mItemLb != null)
            mItemLb.text = LocalizationMgr.Get("CreateGuildWnd_20");

        CsvRow row = ArenaMgr.TopBonusCsv.FindByKey(mStep);
        if (row == null)
            return;

        for (int i = 0; i < row.Query<int>("star"); i++)
            mStars[i].spriteName = row.Query<string>("star_name");
    }

    /// <summary>
    /// 绑定数据
    /// </summary>
    public void Bind(int step)
    {
        mStep = step;

        // 绘制窗口
        Redraw();
    }
}
