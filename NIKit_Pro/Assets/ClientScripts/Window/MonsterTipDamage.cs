/// <summary>
/// MonsterTipDamage.cs
/// Created by lic 2016-8-12
/// 普通伤害飘血框
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterTipDamage : BloodTip
{

    #region 公共字段

    public UILabel mNumber;

    #endregion

    #region 私有字段

    private bool isDeadly = false;

    private TweenAlpha[] tas;

    private TweenScale[] tss;

    private TweenPosition[] tps;

    #endregion


    void Awake()
    {
        tas = mNumber.GetComponents<TweenAlpha>();
        tss = mNumber.GetComponents<TweenScale>();
        tps = mNumber.GetComponents<TweenPosition>();

        tas[1].AddOnFinished(OnTweenFinish);
    }

    #region 外部接口

    public override void BindData(object[] args)
    {
        mNumber.text = (string)args[0];

        if(args.Length >= 3)
            this.isDeadly = (bool)args[2];

        SetPosition();

        ReStart();
    }

    #endregion

    #region 内部方法

    /// <summary>
    /// 重新开始飘血的数据重置.
    /// </summary>
    private void ReStart()
    {
        for(int i = 0; i < tas.Length; i++)
            tas[i].PlayForward();

        if (isDeadly)
            tss[1].PlayForward();
        else
            tss[0].PlayForward();

        for(int i = 0; i < tps.Length; i++)
            tps[i].PlayForward();
    }

    /// <summary>
    /// 动画播放完毕,回收资源
    /// </summary>
    private void OnTweenFinish()
    {
        for(int i = 0; i < tas.Length; i++)
            tas[i].ResetToBeginning();

        for(int i = 0; i < tss.Length; i++)
            tss[i].ResetToBeginning();

        for(int i = 0; i < tps.Length; i++)
            tps[i].ResetToBeginning();

        BloodTipMgr.Recycle(gameObject, TipsWndType.DamageTip);
    }

    /// <summary>
    /// 限制当前物体不出屏幕范围
    /// </summary>
    private void SetPosition()
    {
        float halfItemWidth = mNumber.width / 2f;

        // x轴偏移
        float x_offset = 10f;

        // UI根节点
        Transform uiRoot = WindowMgr.UIRoot;
        if (uiRoot == null)
            return;

        UIPanel panel = uiRoot.GetComponent<UIPanel>();
        if (panel == null)
            return;

        // UI根节点panel四角的坐标
        Vector3[] pos = panel.localCorners;

        this.transform.localPosition = new Vector3(
            Mathf.Clamp(this.transform.localPosition.x,
                pos[0].x + halfItemWidth + x_offset,
                pos[2].x - halfItemWidth - x_offset),
            this.transform.localPosition.y,
            this.transform.localPosition.z);
    }


    #endregion
}
