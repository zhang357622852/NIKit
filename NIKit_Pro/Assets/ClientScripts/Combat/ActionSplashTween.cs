/// <summary>
/// ActionSplashTween.cs
/// Created by zhaozy 2016-12-6
/// 渐入渐出序列帧光效
/// </summary>

using UnityEngine;
using System.Collections;

public class ActionSplashTween : ActionBase
{
    #region 成员变量

    // 资源名
    private string mPrefebName = string.Empty;

    // 时间长度
    private float mAppearTime = 0f;
    private float mDisappearTime = 0f;
    private float mEffectTime = 0f;

    // 位置偏移
    private Vector3 mOffset = Vector3.zero;
    private Vector3 mTargetPos = Vector3.zero;

    // 缩放
    private Vector3 mScale = Vector3.one;

    // 光效对象
    private GameObject mEffectObj = null;

    // 获取光效渲染randerer
    SpriteRenderer mRenderer;
    float mCurAlpha = 0f;
    float mAppearRate;
    float mDisappearRate;

    #endregion

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="actor">角色对象，允许为null.</param>
    /// <param name="para">属性数据，已引入静态属性.</param>
    public ActionSplashTween(CombatActor actor, CombatActionSet actionSet, PropertiesParameter para)
        : base(actor, actionSet, para)
    {
        mPrefebName = para.GetProperty<string>("prefeb", string.Empty);
        mTargetPos = para.GetProperty<Vector3>("target_pos", Vector3.zero);
        mOffset = para.GetProperty<Vector3>("offset", Vector3.zero);
        mScale = para.GetProperty<Vector3>("scale", Vector3.zero);
        mAppearTime = para.GetProperty<float>("appear_time", 0.5f);
        mDisappearTime = para.GetProperty<float>("disappear_time", 0.5f);
        mEffectTime = para.GetProperty<float>("time", 0f);

        // 计算光效渐入渐出alpha rate
        mAppearRate = 1f / mAppearTime;
        mDisappearRate = 1f / mDisappearTime;
    }

    /// <summary>
    /// 开始节点
    /// </summary>
    public override void Start()
    {
        base.Start();

        // 创建光效节点
        if (string.IsNullOrEmpty(mPrefebName))
        {
            IsFinished = true;
            return;
        }

        // 创建光效
        mEffectObj = EffectMgr.CreateEffect(CombatConfig.EFF_PRE_SPRITE + ActionSet.Cookie, mPrefebName);
        if (mEffectObj == null)
        {
            LogMgr.Trace("角色 {0} 在释放 {1} 时，创建ActionEffectSprite节点的 {2} 光效失败，\n请检查资源是否存在", 
                Actor.ActorName, ActionSet.ActionSetDataName, mPrefebName);
            IsFinished = true;
            return;
        }

        // 不挂接指定位置
        mEffectObj.transform.position = mTargetPos + mOffset;

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
            aor.speed = Actor.TimeScaleMap[ActionSet.TimeType];

        // 激活光效
        mEffectObj.SetActive(true);

        // 获取光效的渲染
        mRenderer = mEffectObj.GetComponent<SpriteRenderer>();
        if (mRenderer != null)
            mCurAlpha = mRenderer.color.a;
        else
            mCurAlpha = 0f;
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
        // 计算alpha
        float alpha = 0;

        // 渐入阶段
        if (Game.FloatGreat(mAppearTime, 0))
        {
            // 计算alpha
            alpha = Mathf.Clamp01(1f - mAppearTime * mAppearRate);

            // 扣除渐入时间
            mAppearTime -= info.DeltaTime;
        }
        else if (Game.FloatGreat(mEffectTime, 0))
        {
            alpha = 1f;
            mEffectTime -= info.DeltaTime;
        }
        else if (Game.FloatGreat(mDisappearTime, 0))
        {
            // 渐出阶段
            // 计算alpha
            alpha = Mathf.Clamp01(mDisappearTime * mDisappearRate);

            // 扣除渐出时间
            mDisappearTime -= info.DeltaTime;
        }
        else
        {
            IsFinished = true;
            return;
        }

        // 如果alpha没有发生任何变化
        if (Game.FloatEqual(mCurAlpha, alpha))
            return;

        // 记录当前alpha
        mCurAlpha = alpha;

        // mRenderer对象不存在
        if (mRenderer == null)
            return;

        // 重新设置alpha, alpha范围修正
        Color color = mRenderer.color;
        color.a = mCurAlpha;
        mRenderer.color = color;
    }
}
