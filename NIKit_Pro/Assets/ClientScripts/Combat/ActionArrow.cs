/// <summary>
/// ActionArrow.cs
/// Created by wangxw 2014-12-10
/// 简单投射实体
/// 最简单的射箭，碰撞即触发Hit
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using LPC;

public class ActionArrow : ActionBase
{
    #region 成员变量

    // 光效对象
    private GameObject mEffectObj = null;

    // 光效资源
    private string mPrefebName = string.Empty;

    // 光效缩放
    private Vector3 mEffectScale = Vector3.one;

    // 起点
    private Vector3 mStartPos = Vector3.zero;
    private Vector3 mEndPos = Vector3.zero;

    // mSpeed速度
    private float mSpeed = 0f;

    // 旋转初始方向
    public Vector3 tDirection = Vector3.right;

    // track移动
    private TrackInfo mTrack;
    private string mTrackName = string.Empty;
    private float mDuration = 0f;
    private float mElapseTime = 0f;

    // 延迟消失时间
    private float mDelayTime = 0f;

    // 移动结束标识
    private bool mIsMoveEnd = false;

    // 阴影对象
    private Transform mShadow = null;

    // 光效目标位置
    Vector3 mPos = Vector3.zero;

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
        mEffectObj = EffectMgr.CreateEffect(CombatConfig.EFF_PRE_ARROW + ActionSet.Cookie, mPrefebName);
        if (mEffectObj == null)
        {
            LogMgr.Trace("角色 {0} 在释放 {1} 时，创建ActionArrow节点的 {2} 光效失败，\n请检查资源是否存在", 
                Actor.ActorName, ActionSet.ActionSetDataName, mPrefebName);
            return false;
        }

        // 初始化光效
        mEffectObj.transform.position = mStartPos;
        mEffectObj.transform.localScale = Vector3.Scale(mEffectObj.transform.localScale, mEffectScale);

        // 如果光效是序列光效, 需要设置光效的播放速度
        Animator aor = mEffectObj.GetComponent<Animator>();
        if (aor != null)
            aor.speed = Actor.TimeScaleMap[ActionSet.TimeType];

        // 获取光效粒子残留时间
        mDelayTime = EffectMgr.GetEffectLifeTime(Path.GetFileName(mPrefebName));

        // 激活光效
        mEffectObj.SetActive(true);

        return true;
    }

    /// <summary>
    /// Track驱动节点
    /// </summary>
    private void DoDriveTrack(TimeDeltaInfo info)
    {
        // 累计流逝时间
        mElapseTime += info.DeltaTime;

        // 根据mTrackAnimation计算适时位置
        float rate = mElapseTime / mDuration;

        // 计算位置差值
        mPos = mTrack.Evaluate(rate, mStartPos, mEndPos);

        // 获取光效当前位置
        Vector3 currentPos = mEffectObj.transform.position;

        // 设置光效位置
        mEffectObj.transform.position = mPos;

        // 设置光效旋转
        Vector3 moveOffset = mPos - currentPos;
        if (moveOffset.sqrMagnitude > 0)
            mEffectObj.transform.rotation = Quaternion.FromToRotation(tDirection, moveOffset) * Actor.GetRotation();

        // 设置阴影位置
        if (mShadow != null)
        {
            Vector3 offset = mTrack.Evaluate(rate);
            mShadow.transform.position = new Vector3(mPos.x, mPos.y - offset.y, mPos.z);
        }

        // 时间还没有结束
        if (Game.FloatGreat(mDuration, mElapseTime))
            return;

        // 标识已经移动结束
        mIsMoveEnd = true;
    }

    /// <summary>
    /// 匀速驱动节点
    /// </summary>
    private void DoDriveSpeed(TimeDeltaInfo info)
    {
        // 本帧移动的距离
        // info.DeltaTime已经是速度缩放之后的值了，直接乘基准常量即可
        // 流失的时间 * 速度百分比
        float distance = info.DeltaTime * mSpeed;

        // 到达目标
        Vector3 currentPos = mEffectObj.transform.position;
        if ((mEndPos - currentPos).sqrMagnitude < (distance * distance))
        {
            // 设置光效位置
            mPos = mEndPos;

            // 标识已经移动结束
            mIsMoveEnd = true;
        }
        else
        {
            mPos = Vector3.MoveTowards(currentPos, mEndPos, distance);
        }

        // 设置光效位置
        mEffectObj.transform.position = mPos;
    }

    /// <summary>
    /// 设置光效残留
    /// </summary>
    private void SetEffectResidual()
    {
        // 光效不存在
        if (mEffectObj == null)
            return;

        // 设置光效的全部SpriteRenderer为隐藏状态
        SpriteRenderer[] sRenderers = mEffectObj.GetComponentsInChildren<SpriteRenderer>();
        foreach(SpriteRenderer renderer in sRenderers)
        {
            // 设置为透明效果
            renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, 0);
        }

        // 停止光效的全部ParticleSystem
        ParticleSystem[] pSystems = mEffectObj.GetComponentsInChildren<ParticleSystem>();
        foreach(ParticleSystem system in pSystems)
        {
            system.Stop();
        }
    }

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionArrow(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        mPrefebName = para.GetProperty<string>("prefeb", string.Empty);
        mSpeed = para.GetProperty<float>("speed", 0f); // 移动速度，xml中配置的是逻辑速度
        mTrackName = para.GetProperty<string>("track", string.Empty);
        mDuration = para.GetProperty<float>("duration", 0f);
        mStartPos = para.GetProperty<Vector3>("start_pos", Actor.GetPosition()) +
        para.GetProperty<Vector3>("offset", Vector3.zero);
        mEndPos = para.GetProperty<Vector3>("end_pos", Actor.GetPosition());
        mEffectScale = para.GetProperty<Vector3>("scale", Vector3.one);

        // 获取光效旋转参数
        tDirection = (Actor.GetDirectoion2D() == ObjectDirection2D.RIGHT) ? Vector3.right : Vector3.left;
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public override void Start()
    {
        base.Start();

        // 创建飞行光效
        if (! CreateEffect())
        {
            IsFinished = true;
            return;
        }

        // 获取轨迹文件
        if (!string.IsNullOrEmpty(mTrackName))
        {
            mTrack = TrackMgr.GetTrack(mTrackName);

            // 没有配置的轨迹文件
            if (mTrack == null)
            {
                LogMgr.Trace("轨迹文件{0}，不存在。", mTrackName);

                IsFinished = true;
                return;
            }

            // 获取阴影组件
            mShadow = mEffectObj.transform.Find("EntityShadow");
        }
        else
        {
            mEffectObj.transform.rotation = Quaternion.FromToRotation(tDirection, mEndPos - mStartPos) * Actor.GetRotation();
        }
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

        // 如果移动结束
        if (mIsMoveEnd)
        {
            // 扣除延迟时间
            mDelayTime -= info.DeltaTime;

            // 如果延迟时间还没有到，不能消失
            if (Game.FloatGreat(mDelayTime, 0))
                return;

            // 标识end
            IsFinished = true;

            return;
        }

        // 驱动移动，轨迹移动和匀速移动只能选择其中一种
        if (mTrack == null)
            DoDriveSpeed(info);
        else
            DoDriveTrack(info);

        // 如果移动结束，抛出HIT时间处理光效残留
        if (mIsMoveEnd)
        {
            // 抛出Hit
            Actor.TriggerEvent("ARROW_HIT", ActionSet.Cookie);

            // 设置光效残留效果
            SetEffectResidual();
        }
    }
}
