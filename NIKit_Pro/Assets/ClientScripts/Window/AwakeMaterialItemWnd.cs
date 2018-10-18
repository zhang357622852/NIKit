/// <summary>
/// AwakeMaterialItemWnd.cs
/// Created by fengsc 2016/10/29
/// 宠物觉醒材料基础格子
/// </summary>
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class AwakeMaterialItemWnd : WindowBase<AwakeMaterialItemWnd>
{
    /// <summary>
    /// 材料图标
    /// </summary>
    public UITexture mIcon;

    /// <summary>
    /// 觉醒宠物需要的材料数量
    /// </summary>
    public UILabel mAmount;

    LPCMapping mData = new LPCMapping();

    [HideInInspector]
    public int mClassId;

    /// <summary>
    /// 绘制窗口
    /// </summary>
    void Redraw()
    {
        string iconName = mData.GetValue<int>("icon").ToString();
        mIcon.mainTexture = ResourceMgr.LoadTexture(string.Format("Assets/Art/UI/Icon/item/{0}.png", iconName));

        if (mAmount == null)
            return;

        mAmount.gameObject.SetActive(true);

        int amount = mData.GetValue<int>("amount");
        int totalAmount = mData.GetValue<int>("total_amount");

        mAmount.text = string.Format("{0}/{1}", amount, totalAmount);

        if (amount < totalAmount)
            mAmount.color = new Color(255f / 255, 0f / 255, 0f / 255);
        else
            mAmount.color = new Color(253f / 255, 227f / 255, 200f / 255);
    }

    public void Bind(LPCMapping data, int classId)
    {
        mData = data;

        mClassId = classId; 

        if (mData == null)
            return;

        // 绘制窗口
        Redraw();
    }
}
