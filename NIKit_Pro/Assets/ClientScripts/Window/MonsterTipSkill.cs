﻿/// <summary>
/// MonsterTipSkill.cs
/// Created by lic 2016-8-12
/// 技能飘血框
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MonsterTipSkill : BloodTip
{

    #region 公共字段

    public UILabel mDesc;
    public UITexture mIcon;

    public UISprite mBg;

    #endregion

    #region 私有字段

    private TweenAlpha[] tas;

    private TweenScale[] tss;

    private TweenPosition[] tps;

    #endregion

    void Awake()
    {
        tas = mDesc.GetComponents<TweenAlpha>();
        tss = mDesc.GetComponents<TweenScale>();
        tps = mDesc.GetComponents<TweenPosition>();

        tas[1].AddOnFinished(OnTweenFinish);
    }

    #region 外部接口

    public override void BindData(object[] args)
    {
        mDesc.text = (string)args[0];

        string resPath = string.Format("Assets/Art/UI/Icon/skill/{0}.png", (string)args[1]);
        mIcon.mainTexture = ResourceMgr.LoadTexture(resPath);

        // 设置位置
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

        for(int i = 0; i < tss.Length; i++)
            tss[i].PlayForward();

        for(int i = 0; i < tps.Length; i++)
            tps[i].PlayForward();
    }

    /// <summary>
    /// 回收窗口
    /// </summary>
    private void OnTweenFinish()
    {
        for(int i = 0; i < tas.Length; i++)
            tas[i].ResetToBeginning();

        for(int i = 0; i < tss.Length; i++)
            tss[i].ResetToBeginning();

        for(int i = 0; i < tps.Length; i++)
            tps[i].ResetToBeginning();
        
        BloodTipMgr.Recycle(gameObject, TipsWndType.SkillTip);
    }

    /// <summary>
    /// 限制当前物体不出屏幕范围
    /// </summary>
    private void SetPosition()
    {
        float itemWidth = mDesc.width + mBg.width;

        float halfItemWidth = itemWidth / 2f;

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
