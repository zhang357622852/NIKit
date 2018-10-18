/// <summary>
/// ActionEffectSprite.cs
/// Created by wangxw 2014-12-5
/// 序列帧光效
/// </summary>

using UnityEngine;
using System.Collections;
using System.IO;

public class ActionEffectSprite : ActionBase
{
    #region 成员变量

    // 资源名
    private string mPrefebName = string.Empty;

    // 时间长度(-1表示自动时长)
    private float mEffectTime = -1f;

    // 目标角色
    private string mTargetRid = string.Empty;

    // 是否挂接
    private bool mIsAttach = false;

    // 延迟消失时间
    private float mDelayTime = 0f;
    private bool mIsDelayEnd = false;

    // 位置偏移
    private Vector3 mOffset = Vector3.zero;
    private Vector3 mTargetPos = Vector3.zero;

    // 缩放
    private Vector3 mScale = Vector3.one;

    // 光效对象
    private GameObject mEffectObj = null;

    // 存活时间
    private float mLiveTime = 0f;

    // 是否循环
    private bool mIsLoop = false;

    // 光效播放速度
    private float mSpeedScale = 1f;

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
            return false;

        // 查找CombatActor
        CombatActor actor = CombatActorMgr.GetCombatActor(mTargetRid);

        // 创建光效
        mEffectObj = EffectMgr.CreateEffect(CombatConfig.EFF_PRE_SPRITE + ActionSet.Cookie, mPrefebName);
        if (mEffectObj == null)
        {
            LogMgr.Trace("角色 {0} 在释放 {1} 时，创建ActionEffectSprite节点的 {2} 光效失败，\n请检查资源是否存在",
                Actor.ActorName, ActionSet.ActionSetDataName, mPrefebName);
            return false;
        }

        // 挂接
        if (mIsAttach)
        {
            // 没有光效挂接对象
            if (actor == null)
                return false;

            // 挂接到角色身上，offset应用的local坐标系下
            mEffectObj.transform.localScale = actor.gameObject.transform.localScale;
            mEffectObj.transform.parent = actor.gameObject.transform;
            mEffectObj.transform.localPosition = new Vector3(mOffset.x, mOffset.y, mOffset.z * Actor.GetDirectoion2D());
        }
        else
        {
            if (! mTargetPos.Equals(Vector3.zero))
            {
                // 继承targetOb的缩放scale
                if (actor != null)
                    mEffectObj.transform.localScale = actor.gameObject.transform.localScale;

                // 挂接指定位置
                mEffectObj.transform.position = mTargetPos + mOffset;
            }
            else if (actor != null)
            {
                // 不挂接到角色，以角色为原点
                mEffectObj.transform.localScale = actor.gameObject.transform.localScale;
                mOffset.Scale(actor.gameObject.transform.localScale);
                mEffectObj.transform.position = actor.gameObject.transform.position + mOffset;
            }
            else
            {
                // 不挂接到角色，以0 0 0为原点
                mEffectObj.transform.position = mOffset;
            }
        }

        // 设置模型的方向
        if (Actor.GetDirectoion2D() == ObjectDirection2D.RIGHT)
            mEffectObj.transform.eulerAngles = Vector3.zero;
        else
            mEffectObj.transform.eulerAngles = new Vector3(0, 180, 0);

        // 缩放
        mEffectObj.transform.localScale = Vector3.Scale(mEffectObj.transform.localScale, mScale);

        // 如果光效是序列光效, 需要设置光效的播放速度
        Animator aor = mEffectObj.GetComponent<Animator>();
        if (aor != null)
        {
            mSpeedScale = ActionSet.TimeScaleFactor;
            aor.speed = mSpeedScale;
        }

        // 如果没有指定播放时间长度，则获取光效实际长度信息
        if (mEffectTime < 0)
            mEffectTime = EffectMgr.GetEffectLength(Path.GetFileName(mPrefebName));

        // 获取光效粒子残留时间
        mDelayTime = EffectMgr.GetEffectLifeTime(Path.GetFileName(mPrefebName));

        // 激活光效
        mEffectObj.SetActive(true);

        // 创建光效成功
        return true;
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
    public ActionEffectSprite(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        mPrefebName = para.GetProperty<string>("prefeb", string.Empty);
        mEffectTime = para.GetProperty<float>("time", -1);
        mTargetRid = para.GetProperty<string>("target_rid", Actor.ActorName);
        mIsAttach = para.GetProperty<bool>("is_attach", true);
        mOffset = para.GetProperty<Vector3>("offset", Vector3.zero);
        mTargetPos = para.GetProperty<Vector3>("target_pos", Vector3.zero);
        mScale = para.GetProperty<Vector3>("scale", Vector3.one);
        mIsLoop = para.GetProperty<bool>("is_loop", false);
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public override void Start()
    {
        base.Start();

        // 创建光效
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
        // 光效对象不存在直接标识IsFinished为true
        // 光效节点是attach到角色身上的，而光效节点是stop_when_cancel="false"
        // 当角色析构时光效对象已经不存在导致本节点无法end
        if (mEffectObj == null)
        {
            IsFinished = true;
            return;
        }

        // 循环动画也无需处理什么，等到cancel操作之后自然会结束节点
        if (mIsLoop)
            return;

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

        // 记录光效LiveTime
        // 原则上光效播放是需要*播放缩放
        mLiveTime = mLiveTime + info.DeltaTime * mSpeedScale;

        // 如果光效时间播放结束
        if (mLiveTime < mEffectTime)
            return;

        // 标识音效播放结束
        mIsDelayEnd = true;
    }
}
