/// <summary>
/// ActionLaserSwing.cs
/// Created by zhaozy 2016/12/21(冬至)
/// 激光扫射效果节点
/// </summary>

using System;
using UnityEngine;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using LPC;

public class ActionLaserSwing : ActionBase
{
    #region 成员变量

    // 光效对象
    private GameObject mEffectObj = null;

    // 光效资源
    private string mPrefebName = string.Empty;

    // 起点
    private Vector3 mStartPos = Vector3.zero;
    private Vector3 mEndPos = Vector3.zero;

    // 光效缩放
    private Vector3 mScale = Vector3.one;

    // 光效标准长度
    public float mBasicLength = 0f;

    // 光效播放速度
    private float mSpeedScale = 1f;

    // 时间长度(-1表示自动时长)
    private float mEffectTime = -1f;

    // 存活时间
    private float mLiveTime = 0f;

    #endregion

    #region 属性

    #endregion

    #region 内部函数

    /// <summary>
    /// 创建光效
    /// </summary>
    /// <returns><c>true</c>, if effect was created, <c>false</c> otherwise.</returns>
    private bool CreateEffect()
    {
        // 创建光效节点
        if (string.IsNullOrEmpty(mPrefebName))
        {
            LogMgr.Trace("角色 {0} 在释放 {1} 时，ActionArrow节点没有获取到PrefebName资源名", 
                Actor.ActorName, ActionSet.ActionSetDataName);
            return false;
        }

        // 创建光效
        mEffectObj = EffectMgr.CreateEffect(CombatConfig.EFF_PRE_SPRITE + ActionSet.Cookie, mPrefebName);
        if (mEffectObj == null)
        {
            LogMgr.Trace("角色 {0} 在释放 {1} 时，创建ActionArrow节点的 {2} 光效失败，\n请检查资源是否存在", 
                Actor.ActorName, ActionSet.ActionSetDataName, mPrefebName);
            return false;
        }

        // 初始化光效
        mEffectObj.transform.position = mStartPos;

        // 设置光效旋转方向
        Vector3 dir = (Actor.GetDirectoion2D() == ObjectDirection2D.RIGHT) ? Vector3.right : Vector3.left;
        mEffectObj.transform.rotation = Quaternion.FromToRotation(dir, mEndPos - mStartPos) * Actor.GetRotation();

        // 获取光效原始缩放比例
        Vector3 scale = mEffectObj.transform.localScale;

        // 设置光效X轴缩放
        if (mBasicLength > 0)
        {
            float scaleX = Vector3.Distance(mStartPos, mEndPos) / (mBasicLength * mScale.x);
            mEffectObj.transform.localScale = Vector3.Scale(new Vector3(scaleX, scale.y, scale.z), mScale);
        }

        // 如果光效是序列光效, 需要设置光效的播放速度
        Animator aor = mEffectObj.GetComponent<Animator>();
        if (aor != null)
        {
            mSpeedScale = ActionSet.TimeScaleFactor;
            aor.speed = mSpeedScale;
        }

        // 如果没有指定播放时间长度，则获取光效实际长度信息
        mEffectTime = EffectMgr.GetEffectLength(Path.GetFileName(mPrefebName));

        // 激活光效
        mEffectObj.SetActive(true);

        return true;
    }

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionLaserSwing(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        mPrefebName = para.GetProperty<string>("prefeb", string.Empty);
        mBasicLength = para.GetProperty<float>("basic_length", 0f);
        mScale = para.GetProperty<Vector3>("scale", Vector3.one);
        mEndPos = para.GetProperty<Vector3>("end_pos", Actor.GetPosition());
        mStartPos = para.GetProperty<Vector3>("start_pos", Actor.GetPosition()) +
            para.GetProperty<Vector3>("offset", Vector3.zero);
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public override void Start()
    {
        // base对象Start
        base.Start();

        // 创建飞行光效
        if (! CreateEffect())
        {
            LogMgr.Trace("创建{0}光效失败。", mPrefebName);
            IsFinished = true;
            return;
        }

        // 设置光效旋转方向
        // 获取光效旋转参数
        Vector3 dir = (Actor.GetDirectoion2D() == ObjectDirection2D.RIGHT) ? Vector3.right : Vector3.left;
        mEffectObj.transform.rotation = Quaternion.FromToRotation(dir, mEndPos - mStartPos) * Actor.GetRotation();
    }

    /// <summary>
    /// 结束节点
    /// </summary>
    /// <param name="isCancel">是否cancel方式结束</param>
    public override void End(bool isCancel = false)
    {
        // 销毁光效
        if (mEffectObj != null)
        {
            mEffectObj.SetActive(false);
            EffectMgr.DestroyEffect(mEffectObj);
            mEffectObj = null;
        }

        base.End(isCancel);
    }

    /// <summary>
    /// 节点更新
    /// </summary>
    /// <param name="info">时间参数信息</param>
    public override void Update(TimeDeltaInfo info)
    {
        if (mEffectObj == null)
        {
            LogMgr.Trace("mEffectObj = null");
            IsFinished = true;
            return;
        }

        // 记录光效LiveTime
        // 原则上光效播放是需要*播放缩放
        mLiveTime = mLiveTime + info.DeltaTime * mSpeedScale;

        // 如果光效时间播放结束
        if (mLiveTime < mEffectTime)
            return;

        // 标识播放结束
        IsFinished = true;
    }
}
