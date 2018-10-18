/// <summary>
/// ActionBoneTrail.cs
/// Created by zhaozy 2018/02/09
/// 绑定骨骼效果
/// </summary>

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Spine;
using Spine.Unity;
using LPC;

public class ActionBoneTrail : ActionBase
{
    #region 成员变量

    // 光效对象
    private GameObject mEffectObj = null;

    // 光效资源
    private string mPrefebName = string.Empty;

    // 缩放
    private Vector3 mScale = Vector3.one;

    // 跟随bone旋转
    private bool mRotation = false;

    // 延迟消失时间
    private float mDelayTime = 0f;
    private bool mIsDelayEnd = false;

    // 获取骨头名称
    private string mBoneName = string.Empty;
    private SkeletonRenderer sRender;
    private Bone mBone;

    // 持续时间
    private float mEffectTime = -1f;

    #endregion

    #region 内部函数

    /// <summary>
    /// 创建光效
    /// </summary>
    /// <returns><c>true</c>, if effect was created, <c>false</c> otherwise.</returns>
    private bool CreateEffect()
    {
        // 获取角色
        GameObject targetOb = Actor.gameObject;
        if (targetOb == null)
            return false;

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

        // 挂接到角色身上，offset应用的local坐标系下
        mEffectObj.transform.localScale = Vector3.Scale(targetOb.transform.localScale, mScale);
        mEffectObj.transform.parent = targetOb.transform;

        // 获取光效粒子残留时间
        mDelayTime = EffectMgr.GetEffectLifeTime(Path.GetFileName(mPrefebName));

        // 如果没有指定播放时间长度，则获取光效实际长度信息
        if (mEffectTime < 0)
            mEffectTime = EffectMgr.GetEffectLength(Path.GetFileName(mPrefebName));

        // 更新光效位置
        UpdateTrailPostion();

        // 激活光效
        mEffectObj.SetActive(true);

        return true;
    }

    /// <summary>
    /// Updates the postion.
    /// </summary>
    private void UpdateTrailPostion()
    {
        // 没有对应的mBone，不需要更新
        if (mBone == null)
            return;

        // 获取光效transform对象
        Transform effectTransform = mEffectObj.transform;

        // 更新光效位置
        effectTransform.localPosition = new Vector3(
            mBone.worldX,
            mBone.worldY,
            effectTransform.localPosition.z);

        // 跟随bone旋转
        if (mRotation)
        {
            Skeleton skeleton = sRender.skeleton;
            float flipRotation = (skeleton.flipX ^ skeleton.flipY) ? -1f : 1f;
            Vector3 rotation = effectTransform.localRotation.eulerAngles;
            effectTransform.localRotation = Quaternion.Euler(rotation.x, rotation.y, mBone.WorldRotationX * flipRotation);
        }
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
    public ActionBoneTrail(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        mPrefebName = para.GetProperty<string>("prefeb", string.Empty);
        mBoneName = para.GetProperty<string>("bone", string.Empty);
        mScale = para.GetProperty<Vector3>("scale", Vector3.one);
        mRotation = para.GetProperty<bool>("rotation_bone", false);
        mEffectTime = para.GetProperty<float>("time", -1f);
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public override void Start()
    {
        // 基类Start
        base.Start();

        // 查找mBoneName
        sRender = Actor.gameObject.GetComponent<SkeletonRenderer>();
        if (sRender != null)
            mBone = sRender.skeleton.FindBone(mBoneName);

        // 创建飞行光效
        if (! CreateEffect())
        {
            IsFinished = true;
            return;
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

        // 如果光效还没有结束, 在延迟结束中
        if (mIsDelayEnd)
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

        // mDuration减去当前时间
        mEffectTime -= info.DeltaTime;

        // 时间还没有结束
        if (mEffectTime > 0)
        {
            // 更新拖尾光效位置
            UpdateTrailPostion();
            return;
        }

        // 设置光效残留效果
        SetEffectResidual();

        // 标记结束
        mIsDelayEnd = true;
    }
}
